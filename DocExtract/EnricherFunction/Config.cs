using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnricherFunction
{
    public static class Config
    {
        /**************  UPDATE THESE CONSTANTS WITH YOUR SETTINGS  **************/

        // Azure Blob Storage used to store extracted page images
        public static string IMAGE_AZURE_STORAGE_ACCOUNT_NAME = Environment.GetEnvironmentVariable("IMAGE_AZURE_STORAGE_ACCOUNT_NAME");
        public static string IMAGE_BLOB_STORAGE_ACCOUNT_KEY = Environment.GetEnvironmentVariable("IMAGE_BLOB_STORAGE_ACCOUNT_KEY");

        // Cognitive Services Vision API used to process images
        public static string VISION_API_KEY = Environment.GetEnvironmentVariable("VISION_API_KEY");
        // The region URL base should match where you deployed your cognitive service to.  default is westus.
        // For list of region urls see https://westus.dev.cognitive.microsoft.com/docs/services/56f91f2d778daf23d8ec6739/operations/56f91f2e778daf14a499e1fa
        public static string VISION_API_REGION = Environment.GetEnvironmentVariable("VISION_API_REGION");

        // Cognitive Entity Linking Service
        public static string ENTITY_LINKING_API_KEY = Environment.GetEnvironmentVariable("ENTITY_LINKING_API_KEY");

        // Azure Search service used to index documents
        public static string AZURE_SEARCH_SERVICE_NAME = Environment.GetEnvironmentVariable("AZURE_SEARCH_SERVICE_NAME");
        public static string AZURE_SEARCH_ADMIN_KEY = Environment.GetEnvironmentVariable("AZURE_SEARCH_ADMIN_KEY");

        /*************************************************************************/

        // settings you can change if you want but the defaults should work too
        public static string IMAGE_BLOB_STORAGE_CONTAINER = Environment.GetEnvironmentVariable("IMAGE_BLOB_STORAGE_CONTAINER");
        public const string LIBRARY_BLOB_STORAGE_CONTAINER = "imagedocs";
        public static string AZURE_SEARCH_INDEX_NAME = Environment.GetEnvironmentVariable("AZURE_SEARCH_INDEX_NAME");
    }
}
