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
        public static string IMAGE_AZURE_STORAGE_ACCOUNT_NAME = "jdhdocstrg";
        public static string IMAGE_BLOB_STORAGE_ACCOUNT_KEY = "yf6ArvGRNz6fSg5dT5auY3hhgAcyRxV+8AiDhdNW4PVqqVsxMw42bQVVuiwrF4ExiQIL6fdmWMfuyBx8RyjddA==";

        // Cognitive Services Vision API used to process images
        public static string VISION_API_KEY = "f2568fd765d440c498a671f7c3a94646";
        // The region URL base should match where you deployed your cognitive service to.  default is westus.
        // For list of region urls see https://westus.dev.cognitive.microsoft.com/docs/services/56f91f2d778daf23d8ec6739/operations/56f91f2e778daf14a499e1fa
        public static string VISION_API_REGION = "eastus2.api.cognitive.microsoft.com";

        // Cognitive Entity Linking Service
        public static string ENTITY_LINKING_API_KEY = "bfd3b050ac194385b1cbaf7d5faec2b3";
        public static string ENTITY_LINKING_API_ENDPOINT = "westus.api.cognitive.microsoft.com";

        // Azure Search service used to index documents
        public static string AZURE_SEARCH_SERVICE_NAME = "jdhdocsrch";
        public static string AZURE_SEARCH_ADMIN_KEY = "DE6DCEEC51DAC5B8F5B9099D1E5ED78F";

        /*************************************************************************/

        // settings you can change if you want but the defaults should work too
        public static string IMAGE_BLOB_STORAGE_CONTAINER = "images";
        public const string LIBRARY_BLOB_STORAGE_CONTAINER = "imagedocs";
        public static string AZURE_SEARCH_INDEX_NAME = "docs";
    }
}
