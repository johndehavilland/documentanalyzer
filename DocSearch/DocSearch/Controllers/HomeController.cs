using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DocSearch.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }


        public ActionResult Upload()
        {
            ViewBag.Title = "Upload Page";
            ViewBag.Status = "Add a file";
            return View();
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase[] uploadedFiles)
        {

            if (uploadedFiles != null)
            {
                try
                {
                    List<string> filesUploaded = new List<string>();
                    foreach (HttpPostedFileBase file in uploadedFiles)
                    {
                        UploadBlob(file);
                        filesUploaded.Add(file.FileName);
                    }
                    ViewBag.filesUploaded = filesUploaded;
                }
                catch (Exception ex)
                {
                    ViewBag.Status = ex.Message;
                }
            }
            else
            {
                ViewBag.Status = "No files selected";
            }

            return View("Upload");
        }

        private CloudBlobContainer GetCloudBlobContainer(string containerName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageAccountConnectionString"]);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            container.CreateIfNotExists();

            return container;
        }


        public void UploadBlob(HttpPostedFileBase file)
        {

            CloudBlobContainer container = GetCloudBlobContainer("documents");
            CloudBlockBlob blob = container.GetBlockBlobReference(file.FileName);
            blob.UploadFromStream(file.InputStream);

        }

        public void DownloadBlob(string name)
        {
            string orig_name = name.Replace(".txt","");
            CloudBlobContainer container = GetCloudBlobContainer("pdf");
            var blob = container.GetBlobReferenceFromServer(orig_name);

            var memStream = new MemoryStream();
            blob.DownloadToStream(memStream);

            Response.ContentType = blob.Properties.ContentType;
            Response.AddHeader("Content-Disposition", "Attachment;filename=" + orig_name);
            Response.AddHeader("Content-Length", blob.Properties.Length.ToString());
            Response.BinaryWrite(memStream.ToArray());
        }

        public ActionResult GetPdf(string name)
        {
            try
            {
                string orig_name = name.Replace(".txt", "");
                CloudBlobContainer container = GetCloudBlobContainer("pdf");
                var blob = container.GetBlobReferenceFromServer(orig_name);

                var memStream = new MemoryStream();
                blob.DownloadToStream(memStream);
                var doc = memStream.ToArray();


                string mimeType = "application/pdf";
                Response.AppendHeader("Content-Disposition", "inline; filename=" + orig_name);
                return File(doc, mimeType);
            }
            catch(Exception ex)
            {
                if (ex.Message.Contains("404"))
                {
                    return new HttpNotFoundResult();
                }
                else
                {
                    return new HttpStatusCodeResult(500, "An error occurred trying to get the file");
                }

            }
        }
    }
}
