using DocSearch.Models;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DocSearch.Controllers
{
    public class SearchController : ApiController
    {
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        private DocSearch _docSearch = new DocSearch();

        [HttpPost]
        public JObject Post([FromBody]JObject body)
        {
            //var json = await Request.Content.ReadAsStringAsync();
            dynamic input = body as dynamic;
            var facetsIn = body.GetValue("facets") as JArray;
            var it = facetsIn.Where(t => t.Value<string>().StartsWith("terms")).FirstOrDefault();
            if (it != null)
                it.Remove();

            var result = _docSearch.Search(body.ToString());


            if (it != null)
            {
                dynamic dynResult = result as dynamic;

                string query = input.search;
                var facets = _docSearch.GetFacets(query, 10);

                foreach (var facet in facets.Facets)
                {
                    var facetList = result.GetValue("@search.facets") as JObject;
                    facetList.Add(facet.Key, JArray.FromObject(facet.Value));
                }
            }

          

            return result;
        }




        public Docs Get([FromUri]string value, [FromUri]string facet)
        {
            string AZURE_SEARCH_SERVICE_NAME = ConfigurationManager.AppSettings["SearchServiceName"];
            string AZURE_SEARCH_ADMIN_KEY = ConfigurationManager.AppSettings["SearchServiceKey"];

            List<SearchDocument> docsToReturn = new List<SearchDocument>();

            string filter = null;
            if (facet != "" && facet != "undefined")
                filter = "entities/any(e: e eq '" + facet + "')";

            var parameters = new SearchParameters();

            //List<string> orderby = new List<string>();
            //orderby.Add(sort);
            List<string> highlight = new List<string>();
            highlight.Add("text");

            //parameters.OrderBy = orderby.AsReadOnly();
            parameters.HighlightFields = highlight.AsReadOnly();
            parameters.HighlightPreTag = "<strong>";
            parameters.HighlightPostTag = "</strong>";
            parameters.IncludeTotalResultCount = true;
            parameters.Facets = new List<String>() { "entities" };
            if (filter != null)
            {
                parameters.Filter = filter;
            }
            SearchServiceClient serviceClient = new SearchServiceClient(AZURE_SEARCH_SERVICE_NAME, new SearchCredentials(AZURE_SEARCH_ADMIN_KEY));
            ISearchIndexClient indexClient = serviceClient.Indexes.GetClient("docs");
            DocumentSearchResult response = indexClient.Documents.Search(value, parameters);

            

            return new Docs() { Facets = response.Facets, Count = Convert.ToInt32(response.Count), Results = response.Results };

        
    }

        
    }
}
