using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Cognitive.Skills
{
    public class Entities
    {
        private string apiKey;
        private string apiUrl;

        public Entities(string entityApiKey, string endpoint)
        {
            apiKey = entityApiKey;
            apiUrl = $"https://{endpoint}/text/analytics/v2.0/entities";
        }

        public async Task<IList<EntityDocument>> GetEntitiesAsync(string text)
        {

            HttpClient httpClient = new HttpClient();
            var requestMessage = new HttpRequestMessage();

            requestMessage.Headers.Add("Ocp-Apim-Subscription-Key", apiKey);
            //requestMessage.Headers.Add("Content-Type", "application/json");
            requestMessage.Method = HttpMethod.Post;
            requestMessage.RequestUri = new Uri(apiUrl);
            
            var entityObj = new List<Document>() { new Document() { id = "1", language = "en", text = text } };
            var doc = new Documents() { documents = entityObj };
            string json = JsonConvert.SerializeObject(doc);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            requestMessage.Content = httpContent;
            HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage);

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var results = JsonConvert.DeserializeObject<EntityDocs>(responseContent);
                return results.documents;
            }
            else
            {
                var response = new List<EntityDocument>();
                return response;
            }

            
        }


    }

    public class Documents
    {
        public IList<Document> documents { get; set; }
    }

    public class Document
    {
        public string language { get; set; }
        public string id { get; set; }
        public string text { get; set; }
    }


    public class Match
    {
        public string text { get; set; }
        public int offset { get; set; }
        public int length { get; set; }
    }

    public class Entity
    {
        public string name { get; set; }
        public IList<Match> matches { get; set; }
        public string wikipediaLanguage { get; set; }
        public string wikipediaId { get; set; }
        public string wikipediaUrl { get; set; }
        public string bingId { get; set; }
    }

    public class EntityDocs
    {
        public IList<EntityDocument> documents { get; set; }
    }

    public class EntityDocument
    {
        public string id { get; set; }
        public IList<Entity> entities { get; set; }
    }

}
