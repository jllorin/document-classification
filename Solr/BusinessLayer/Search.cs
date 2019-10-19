using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Solr.BusinessLayer
{
    public class Search
    {
        public static string Select(string query, string rows, string sort, string start, string fields)
        {
            try
            {
                // Create a request for the URL. 		                
                string queryString = $"&q={System.Web.HttpUtility.UrlEncode(query)}";
                rows = string.IsNullOrEmpty(rows) ? "" : $"&rows={rows}";
                sort = string.IsNullOrEmpty(sort) ? "" : $"&sort={sort}";
                start = string.IsNullOrEmpty(start) ? "" : $"&start={start}";
                fields = string.IsNullOrEmpty(fields) ? "" : $"&fl={fields}";
                var requestString = $"http://localhost:8983/solr/gettingStarted/select?indent=on{queryString}{rows}{sort}{start}{fields}&wt=json";
                return SolrOperation.LuceneRequest(requestString);
            }
            catch (Exception ex)
            {
                Solr.Utilities.SolrException.WriteError(ex);
            }
            return null;
        }
    }
}
