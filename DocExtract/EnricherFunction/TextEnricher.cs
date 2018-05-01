using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ProjectOxford.EntityLinking;
using Microsoft.ProjectOxford.EntityLinking.Contract;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace EnricherFunction
{
    public static class TextEnricher
    {
        static EntityLinkingServiceClient linkedEntityClient;
        static ISearchIndexClient indexClient;
        private static HttpClient _httpClient;

        [FunctionName("TextEnricher")]
        public static void Run([BlobTrigger("txt/{name}", Connection = "blobstorage")]string myBlob, string name, TraceWriter log, ExecutionContext context)
        {
            try
            {
                log.Info($"C# Blob trigger function Processed blob\n Name:{name}");
                var searchServiceName = System.Environment.GetEnvironmentVariable("AZURE_SEARCH_SERVICE_NAME");
                var searchServiceKey = System.Environment.GetEnvironmentVariable("AZURE_SEARCH_ADMIN_KEY");
                var searchIndexName = System.Environment.GetEnvironmentVariable("AZURE_SEARCH_INDEX_NAME");
                var entityAPIKey = System.Environment.GetEnvironmentVariable("ENTITY_LINKING_API_KEY");
                _httpClient = new HttpClient();
                _httpClient.DefaultRequestHeaders.Add("api-key", searchServiceKey);
                var serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(searchServiceKey));

                if (serviceClient.Indexes.Exists(searchIndexName) == false)
                {
                    var json = File.ReadAllText(Path.Combine(context.FunctionAppDirectory, "CreateIndex.json"));

                    var serviceUri = new Uri("https://" + searchServiceName + ".search.windows.net/indexes/" + searchIndexName);
                    SendRequest(serviceUri, json, HttpMethod.Put, "2016-09-01-Preview");
                }
                indexClient = serviceClient.Indexes.GetClient(searchIndexName);
                linkedEntityClient = new EntityLinkingServiceClient(entityAPIKey, "https://api.labs.cognitive.microsoft.com");

                var texts = ChunksUpto(myBlob, 10000);

                var entityColl = new Dictionary<string, int>();
                foreach (var pageText in texts)
                {
                    var entities = GetLinkedEntitiesAsync(pageText).GetAwaiter().GetResult();
                    foreach (var entity in entities)
                    {
                        if (entityColl.ContainsKey(entity.Name))
                        {
                            entityColl[entity.Name]++;
                        }
                        else
                        {
                            entityColl.Add(entity.Name, 1);
                        }

                    }
                }

                var searchDocument = new SearchDocument(name)
                {
                    Metadata = "",
                    Text = myBlob,
                    Entities = entityColl
                        .OrderByDescending(g => g.Value)
                        .Select(f => f.Key)
                        .ToList(),
                    File=name
                };
                log.Info("name: " + searchDocument.Id);
                AddToIndex(name, searchDocument).GetAwaiter().GetResult();
            }
            catch(Exception e)
            {
                log.Error(e.Message, e);
                string error = e.Message + Environment.NewLine + e.ToString();
                log.Error(e.Message, e);
                //upload to error bucket
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("blobstorage"));
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("error-docs");
                container.CreateIfNotExists();
                CloudBlockBlob blobErr = container.GetBlockBlobReference(name + "-errdetails-" + DateTime.Now.ToFileTimeUtc().ToString() + ".txt");
                blobErr.UploadText(error);
            
        }
        }

        private static async Task AddToIndex(string name, SearchDocument searchDocument)
        {
            var batch = IndexBatch.MergeOrUpload(new[] { searchDocument });
            var result = await indexClient.Documents.IndexAsync(batch);

            if (!result.Results[0].Succeeded)
                Console.WriteLine($"index failed for {name}: {result.Results[0].ErrorMessage}");
        }

        static IEnumerable<string> ChunksUpto(string str, int maxChunkSize)
        {
            for (int i = 0; i < str.Length; i += maxChunkSize)
                yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
        }

        private static async Task<EntityLink[]> GetLinkedEntitiesAsync(params string[] txts)
        {
            var txt = string.Join(Environment.NewLine, txts);

            
           

            return await linkedEntityClient.LinkAsync(txt);
        }

        public static string SendRequest(Uri uri, string json, HttpMethod method, string version)
        {
            
            UriBuilder builder = new UriBuilder(uri);
            string separator = string.IsNullOrWhiteSpace(builder.Query) ? string.Empty : "&";
            builder.Query = builder.Query.TrimStart('?') + separator + "api-version=" + version;

            var request = new HttpRequestMessage(method, builder.Uri);

            if (json != null)
            {
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = _httpClient.SendAsync(request).Result;

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
    }
}
