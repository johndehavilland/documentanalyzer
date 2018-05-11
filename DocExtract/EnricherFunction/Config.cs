using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnricherFunction
{
    public static class Config
    {
        /**************  UPDATE THESE CONSTANTS WITH YOUR SETTINGS  **************/

        // Azure Blob Storage used to store extracted page images
        public static string IMAGE_AZURE_STORAGE_ACCOUNT_NAME = ConfigurationManager.AppSettings["IMAGE_AZURE_STORAGE_ACCOUNT_NAME"];
        public static string IMAGE_BLOB_STORAGE_ACCOUNT_KEY = ConfigurationManager.AppSettings["IMAGE_BLOB_STORAGE_ACCOUNT_KEY"];

        // Cognitive Services Vision API used to process images
        public static string VISION_API_KEY = ConfigurationManager.AppSettings["VISION_API_KEY"];
        // The region URL base should match where you deployed your cognitive service to.  default is westus.
        // For list of region urls see https://westus.dev.cognitive.microsoft.com/docs/services/56f91f2d778daf23d8ec6739/operations/56f91f2e778daf14a499e1fa
        public static string VISION_API_REGION = ConfigurationManager.AppSettings["VISION_API_REGION"];

        // Cognitive Entity Linking Service
        public static string ENTITY_LINKING_API_KEY = ConfigurationManager.AppSettings["ENTITY_LINKING_API_KEY"];
        public static string ENTITY_LINKING_API_ENDPOINT = ConfigurationManager.AppSettings["ENTITY_LINKING_API_ENDPOINT"];

        // Azure Search service used to index documents
        public static string AZURE_SEARCH_SERVICE_NAME = ConfigurationManager.AppSettings["AZURE_SEARCH_SERVICE_NAME"];
        public static string AZURE_SEARCH_ADMIN_KEY = ConfigurationManager.AppSettings["AZURE_SEARCH_ADMIN_KEY"];

        /*************************************************************************/

        // settings you can change if you want but the defaults should work too
        public static string IMAGE_BLOB_STORAGE_CONTAINER = ConfigurationManager.AppSettings["IMAGE_BLOB_STORAGE_CONTAINER"];
        public const string LIBRARY_BLOB_STORAGE_CONTAINER = "imagedocs";
        public static string AZURE_SEARCH_INDEX_NAME = ConfigurationManager.AppSettings["AZURE_SEARCH_INDEX_NAME"];
    }
}
