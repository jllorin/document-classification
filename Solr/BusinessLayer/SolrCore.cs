using Newtonsoft.Json;
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
    public class SolrCore
    {
        public static string GetCores()
        {
            try
            {
                // Create a request for the URL. 		
                WebRequest request = WebRequest.Create("http://localhost:8983/solr/admin/cores?action=STATUS&wt=json");
                // Get the response.
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                // Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();
                //JObject responseObject = JObject.Parse(responseFromServer);

                

                // Cleanup the streams and the response.
                reader.Close();
                dataStream.Close();
                response.Close();
                return responseFromServer;
            }
            catch (Exception ex)
            {
                Solr.Utilities.SolrException.WriteError(ex);
            }
            return null;
        }
    }
}
