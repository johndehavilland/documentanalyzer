using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Azure.WebJobs.Extensions;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Cognitive.Skills;
using EnricherFunction;
using System.Net.Http;
using System.Text;

namespace DataEnricher
{
    public static class Program
    {
        static ConsoleLogger log = new ConsoleLogger(TraceLevel.Info);

        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Initializing Services");
                    InitializeServices();

                    Console.WriteLine("Services have been successfully Initialized");
                }
                else
                {
                    Console.WriteLine("Initializing Services");
                    InitializeServices();

                    Console.WriteLine("Services have been successfully Initialized");
                    //bool deleteall = false;
                    //var serviceClient = new SearchServiceClient(Config.AZURE_SEARCH_SERVICE_NAME, new SearchCredentials(Config.AZURE_SEARCH_ADMIN_KEY));
                    //var indexClient = serviceClient.Indexes.GetClient(Config.AZURE_SEARCH_INDEX_NAME);

                    //if (deleteall)
                    //{
                    //    var sp = new SearchParameters() { Select = new[] { "id" }.ToList() };
                    //    var ids = indexClient.Documents.Search("*", sp).Results.Select(s => s.Document).ToArray();

                    //    if (ids.Length > 0)
                    //    {
                    //        var batch = IndexBatch.Delete(ids);
                    //        var result = indexClient.Documents.IndexAsync(batch).Result;
                    //    }
                    //}

                    Console.WriteLine("Indexing images under " + args[0]);
                    var files = Directory.GetFiles(args[0]);

                    // advance to the high water mark if needed
                    int start = 0;
                    string hwmFile = "hwm.txt";
                    if (File.Exists(hwmFile))
                    {
                        var hwm = File.ReadAllText("hwm.txt");
                        start = Array.IndexOf(files.Select(f => f.ToLowerInvariant()).ToArray(), hwm.ToLowerInvariant().Trim());
                        if (start < 0)
                            start = 0;
                    }

                    Dictionary<string, Exception> errors = new Dictionary<string, Exception>();
                    for (var i = start; i < files.Length; i++)
                    {
                        var filepath = files[i];

                        // write the hwm
                        File.WriteAllText(hwmFile, filepath);

                        // get the document record number for the filename
                        string name = Path.GetFileName(filepath).Replace(" ", "_").Replace(".", "_");

                        Console.WriteLine("Processing file {0} : ID={1}  [{2} of {3}]", Path.GetFileName(filepath), name, i + 1, files.Length);

                        using (var file = File.OpenRead(filepath))
                        {
                            try
                            {
                                EnrichFunction.Run(file, name, log).Wait();
                            }
                            catch(Exception e)
                            {
                                errors.Add(filepath, e);
                                Console.WriteLine("ERROR: " + e.ToString());
                            }
                        }
                    }

                    if (errors.Count > 0)
                    {
                        Console.WriteLine("files with errors:");
                        foreach (var err in errors)
                        {
                            Console.WriteLine(errors.Keys);
                        }

                        Console.WriteLine();
                        Console.WriteLine();
                        Console.WriteLine("errors:");
                        foreach (var err in errors)
                        {
                            Console.WriteLine(err.Key + " : " + err.Value.ToString().Substring(0, Math.Min(300, err.Value.ToString().Length)));
                        }
                    }

                    // remove the hwm since we are done
                    if (File.Exists(hwmFile))
                        File.Delete(hwmFile);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An Error has occured: " + e.ToString());
            }

            Console.WriteLine();
            Console.WriteLine("Done.");
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        static void InitializeServices()
        {
            // create the storage containers if needed
            CloudBlobClient blobClient = CloudStorageAccount.Parse($"DefaultEndpointsProtocol=https;AccountName={Config.IMAGE_AZURE_STORAGE_ACCOUNT_NAME};AccountKey={Config.IMAGE_BLOB_STORAGE_ACCOUNT_KEY};EndpointSuffix=core.windows.net").CreateCloudBlobClient();
            blobClient.GetContainerReference(Config.IMAGE_BLOB_STORAGE_CONTAINER).CreateIfNotExists();
            blobClient.GetContainerReference(Config.LIBRARY_BLOB_STORAGE_CONTAINER).CreateIfNotExists();

            var serviceClient = new SearchServiceClient(Config.AZURE_SEARCH_SERVICE_NAME, new SearchCredentials(Config.AZURE_SEARCH_ADMIN_KEY));

            
            var searchServiceName = Config.AZURE_SEARCH_SERVICE_NAME;
            var searchServiceKey = Config.AZURE_SEARCH_ADMIN_KEY;
            var searchIndexName = Config.AZURE_SEARCH_INDEX_NAME;
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("api-key", searchServiceKey);
            

            if (serviceClient.Indexes.Exists(searchIndexName) == false)
            {
                var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CreateIndex.json"));

                var serviceUri = new Uri("https://" + searchServiceName + ".search.windows.net/indexes/" + searchIndexName);
                SendRequest(serviceUri, json, HttpMethod.Put, "2016-09-01-Preview", httpClient);
            }
        }

        public static string SendRequest(Uri uri, string json, HttpMethod method, string version, HttpClient httpClient)
        {

            UriBuilder builder = new UriBuilder(uri);
            string separator = string.IsNullOrWhiteSpace(builder.Query) ? string.Empty : "&";
            builder.Query = builder.Query.TrimStart('?') + separator + "api-version=" + version;

            var request = new HttpRequestMessage(method, builder.Uri);

            if (json != null)
            {
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = httpClient.SendAsync(request).Result;

            EnsureSuccessfulSearchResponse(response);

            return response.Content.ReadAsStringAsync().Result;
        }

        private static void EnsureSuccessfulSearchResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                string error = response.Content == null ? null : response.Content.ReadAsStringAsync().Result;
                throw new Exception("Search request failed: " + error);
            }
        }



        public class ConsoleLogger : TraceMonitor
        {
            public ConsoleLogger(TraceLevel level) : base(level)
            {
            }
            public override void Trace(TraceEvent traceEvent)
            {
                Console.WriteLine(traceEvent);
            }
        }

    }
}
