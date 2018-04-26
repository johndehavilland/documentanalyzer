using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;

using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace EnricherFunction
{
    public static class PDFExtraction
    {
        [FunctionName("PDFExtraction")]
        public static void Run([BlobTrigger("pdf/{name}", Connection = "blobstorage")]Stream myBlob, [Blob("txt/{name}.txt", Connection = "blobstorage")]out string myOutputBlob, string name, TraceWriter log)
        {
            log.Info($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            List<string> texts = new List<string>();

            PdfReader _pdfReader = new PdfReader(myBlob);
            for (int page = 1; page <= _pdfReader.NumberOfPages; page++)
            {
                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                string currentText = PdfTextExtractor.GetTextFromPage(_pdfReader, page, strategy);
                string pattern = "^[0-9[\r\n]+";   // or @" ^\d+";

                string pageText = Regex.Replace(currentText, pattern, "");
                texts.Add(pageText);

            }

            string fullText = string.Join("\r\n", texts);

            myOutputBlob = fullText;

        }
      
    }
}
