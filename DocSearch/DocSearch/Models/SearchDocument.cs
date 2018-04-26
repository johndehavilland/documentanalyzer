using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DocSearch.Models
{
    public class SearchDocument
    {

        public SearchDocument(string name)
        {
            this.Id = name;
        }

        public SearchDocument()
        {
        }


        // Fields that are in the index
        private string id;

        [System.ComponentModel.DataAnnotations.Key]
        [IsFilterable]
        public string Id { get { return id; } set { id = value.Replace(".", "_").Replace(" ", "_"); } }


        [IsSearchable]
        public string Metadata
        {
            get;
            set;
        }

        [IsRetrievable(false)]
        [IsSearchable]
        public string Text
        {
            get;
            set;
        }

        [IsRetrievable(true)]
        public string File
        {
            get;
            set;
        }

        [IsFilterable]
        [IsFacetable]
        [JsonProperty("entities")]
        public List<string> LinkedEntities { get; set; } = new List<string>();
    }

    public class Docs
    {
        public FacetResults Facets { get; set; }
        public IList<SearchResult> Results { get; set; }
        public int? Count { get; set; }
    }

    public class FDGraphEdges
    {
        public int source { get; set; }
        public int target { get; set; }

    }
}