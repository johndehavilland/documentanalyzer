using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace EnricherFunction
{
    [SerializePropertyNamesAsCamelCase]
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
        public List<string> Entities { get; set; } = new List<string>();
    }
}