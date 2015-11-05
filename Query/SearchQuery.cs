using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OkayCloudSearch.Contract;
using OkayCloudSearch.Helper;
using OkayCloudSearch.Query.Boolean;
using OkayCloudSearch.Query.Facets;

namespace OkayCloudSearch.Query
{
    public class SearchQuery<T> where T : SearchDocument, new()
    {
        public BooleanQuery BooleanQuery { get; set; }

        public string Keyword { get; set; }

        public List<Facet> Facets { get; private set; }

        public List<string> Fields { get; private set; }

        public int? Start { get; set; }

        public int? Size { get; set; }

        public string PublicSearchQueryString { get; set; }

        public SearchQuery(bool buildFieldsFromType = true)
        {
            Facets = new List<Facet>();
            Fields = new List<string>();

            if (buildFieldsFromType)
            {
                BuildPropertiesArray(new ListProperties<T>().GetProperties());
            }
        }

        private void BuildPropertiesArray(List<PropertyInfo> properties)
        {
            Fields.AddRange(properties.Select(x => x.Name).ToList());
        }
    }
}