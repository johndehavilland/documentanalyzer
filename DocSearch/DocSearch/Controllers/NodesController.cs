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
    public class NodesController : ApiController
    {
        [HttpGet]
        public JObject Get(string q)
        {
            string AZURE_SEARCH_SERVICE_NAME = ConfigurationManager.AppSettings["SearchServiceName"];
            string AZURE_SEARCH_ADMIN_KEY = ConfigurationManager.AppSettings["SearchServiceKey"];
            // Calculate nodes for 3 levels
            JObject dataset = new JObject();
            int MaxEdges = 20;
            int MaxLevels = 3;
            int CurrentLevel = 1;
            int CurrentNodes = 0;

            var FDEdgeList = new List<FDGraphEdges>();
            // Create a node map that will map a facet to a node - nodemap[0] always equals the q term
            var NodeMap = new Dictionary<string, int>();
            NodeMap[q] = CurrentNodes;

            // If blank search, assume they want to search everything
            if (string.IsNullOrWhiteSpace(q))
                q = "*";

            var NextLevelTerms = new List<string>();
            NextLevelTerms.Add(q);

            // Iterate through the nodes up to 3 levels deep to build the nodes or when I hit max number of nodes
            while ((NextLevelTerms.Count() > 0) && (CurrentLevel <= MaxLevels) && (FDEdgeList.Count() < MaxEdges))
            {
                q = NextLevelTerms.First();
                NextLevelTerms.Remove(q);
                if (NextLevelTerms.Count() == 0)
                    CurrentLevel++;
                SearchServiceClient serviceClient = new SearchServiceClient(AZURE_SEARCH_SERVICE_NAME, new SearchCredentials(AZURE_SEARCH_ADMIN_KEY));
                ISearchIndexClient indexClient = serviceClient.Indexes.GetClient("docs");

                SearchParameters sp = new SearchParameters()
                {
                    SearchMode = SearchMode.Any,
                    Top = 0,
                    Select = new List<String>() { "id" },
                    Facets = new List<String>() { "entities, count:" + 30 },
                    QueryType = QueryType.Full
                };

                var response = indexClient.Documents.Search(q, sp);

                if (response != null)
                {
                    var facetVals = ((FacetResults)response.Facets)["entities"];
                    foreach (var facet in facetVals)
                    {
                        int node = -1;
                        if (NodeMap.TryGetValue(facet.Value.ToString(), out node) == false)
                        {
                            // This is a new node
                            CurrentNodes++;
                            node = CurrentNodes;
                            NodeMap[facet.Value.ToString()] = node;
                        }
                        // Add this facet to the fd list
                        if (NodeMap[q] != NodeMap[facet.Value.ToString()])
                        {
                            FDEdgeList.Add(new FDGraphEdges { source = NodeMap[q], target = NodeMap[facet.Value.ToString()] });
                            if (CurrentLevel < MaxLevels)
                                NextLevelTerms.Add(facet.Value.ToString());
                        }
                    }
                }
            }

            JArray nodes = new JArray();
            foreach (var entry in NodeMap)
            {
                nodes.Add(JObject.Parse("{name: \"" + entry.Key.Replace("\"", "") + "\"}"));
            }

            JArray edges = new JArray();
            foreach (var entry in FDEdgeList)
            {
                edges.Add(JObject.Parse("{source: " + entry.source + ", target: " + entry.target + "}"));
            }

            dataset.Add(new JProperty("edges", edges));
            dataset.Add(new JProperty("nodes", nodes));

            // Create the fd data object to return
            return dataset;
        }
    }
}
