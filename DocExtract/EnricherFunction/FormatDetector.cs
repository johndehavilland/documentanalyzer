using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;

namespace EnricherFunction
{
    public static class FormatDetector
    {
        [FunctionName("FormatDetector")]
        public static void Run([BlobTrigger("documents/{name}", Connection = "blobstorage")]CloudBlockBlob myBlob, string name, TraceWriter log)
        {
            try
            {
                log.Info($"C# Blob trigger function Processed blob\n Name:{name}");
            int pageNum = 0;
            int imageCount = 0;
            string ext = Path.GetExtension(name).ToLower();
                if (ext == ".pdf")
                {


                    using (MemoryStream mem = new MemoryStream())
                    {
                        myBlob.DownloadToStream(mem);
                        PdfDocument document = PdfReader.Open(mem);

                        // Iterate pages

                        foreach (PdfPage page in document.Pages)
                        {
                            pageNum++;
                            // Get resources dictionary
                            PdfDictionary resources = page.Elements.GetDictionary("/Resources");
                            if (resources != null)
                            {
                                // Get external objects dictionary
                                PdfDictionary xObjects = resources.Elements.GetDictionary("/XObject");
                                if (xObjects != null)
                                {
                                    ICollection<PdfItem> items = xObjects.Elements.Values;
                                    // Iterate references to external objects
                                    foreach (PdfItem item in items)
                                    {
                                        PdfReference reference = item as PdfReference;
                                        if (reference != null)
                                        {
                                            PdfDictionary xObject = reference.Value as PdfDictionary;
                                            // Is external object an image?
                                            if (xObject != null && xObject.Elements.GetString("/Subtype") == "/Image")
                                            {
                                                imageCount++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                log.Info("Found " + imageCount + " images and " + pageNum + " pages");
                if (imageCount < (pageNum * 0.5))
                {
                    var destinationContainer = myBlob.Container.ServiceClient.GetContainerReference("pdffiles");
                    destinationContainer.CreateIfNotExists();
                    CloudBlockBlob outputBlob = destinationContainer.GetBlockBlobReference(name);
                    outputBlob.StartCopy(myBlob);

                }
                else
                {
                    var destinationContainer = myBlob.Container.ServiceClient.GetContainerReference("imagefiles");
                    destinationContainer.CreateIfNotExists();
                    CloudBlockBlob outputBlob = destinationContainer.GetBlockBlobReference(name);
                    outputBlob.StartCopy(myBlob); 
                }
            }
            catch(Exception e)
            {
                log.Error(e.Message, e);
            }
        }
    }
}
