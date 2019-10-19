using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Solr.BusinessLayer
{
    public class UpdateJSON
    {
        //// This one uses curl
        //public static bool UpdateItem(string id, string fieldName)
        //{
        //    try
        //    {
        //        ProcessStartInfo startInfo = new ProcessStartInfo();
        //        startInfo.CreateNoWindow = false;
        //        startInfo.UseShellExecute = false;
        //        startInfo.FileName = @"C:\temp\curl-7.48.0-win64-mingw\bin\curl.exe";
        //        startInfo.WindowStyle = ProcessWindowStyle.Hidden;

        //        //string json = @"{
        //        // add: {
        //        //  doc: {
        //        //    id: ""1"",
        //        //    Test: {set : '1.Cyber'}                  
        //        //  }
        //        // },
        //        //        commit: {},
        //        //        optimize: { waitSearcher:false }
        //        //}";

        //        // this is the best it removes all the "" and ' and it worked on JObject, using json string single quote worked here too
        //        //string json = @"{add: {doc: {id: '1',Test: {set : '\'\'Cumber'}  }},commit: {},optimize: { waitSearcher:false }}";
        //        // this is the best it removes all the "" and ' and it worked on JObject, using json string single quote worked here too, double quotes with \ works too if you 
        //        // used json rather than valueString
        //        //string json = @"{add: {doc: {id: '1',Test: {set : '\""\'adfa.Cum33ber'}  }},commit: {},optimize: { waitSearcher:false }}";
        //        StringBuilder jsonBuilder = new StringBuilder(@"{add: {doc: {id: '1',Test: {set : ',adfa.Cum33ber'}");
        //        jsonBuilder.Append(@"  }},commit: {},optimize: { waitSearcher:false }}");

        //        // don't use this, tried and it's not good
        //        //var json = "{\"add\": {\"doc\": {\"id\": \"1\",\"Test\": {\"set\" : \"Cyber3\"}  }},\"commit\": {},\"optimize\": { \"waitSearcher\":false }}";
        //        //json = "{\"add\": {\"doc\": {\"id\": \"1\",\"Test\": {\"set\" : \"Cyberpunk3\"}  }},\"commit\": {},\"optimize\": { \"waitSearcher\":false }}";
        //        //string json = "{\"add\": {\"doc\": {\"id\": \"1\",\"Test\": {\"set\" : \"\"\"Cyberpunk3\"\"\"}  }},\"commit\": {},\"optimize\": { \"waitSearcher\":false }}";

        //        JObject value = JObject.Parse(jsonBuilder.ToString());
        //        var valueString = value.ToString(Formatting.None);

        //        startInfo.Arguments = $"http://localhost:8983/solr/gettingStarted/update -d \"{jsonBuilder.ToString()}\"";                
        //        try
        //        {
        //            // Start the process with the info we specified.
        //            // Call WaitForExit and then the using statement will close.
        //            using (Process exeProcess = Process.Start(startInfo))
        //            {
        //                exeProcess.WaitForExit();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            string message = ex.Message;
        //            // Log error.
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        string message = ex.Message;
        //    }
        //    return false;
        //}





        public static bool UpdateItem(string id, string fieldName, string value)
        {
            id = id.Replace("\\", "\\\\");
            string json = string.Format(@"{{
                 add: {{
                  doc: {{
                    id: '{0}',
                    {1}: {{set : '{2}'}}
                  }}
                 }},
                        commit: {{}},
                        optimize: {{ waitSearcher:false }}
                }}", id, fieldName, value);


            //StringBuilder jsonBuilder = new StringBuilder("{add: {doc: {id: '1',Test: {set : '\"\\\'adfa.Cum33ber'}");
            //jsonBuilder.Append("  }},commit: {},optimize: { waitSearcher:false }}");
            //var json = jsonBuilder.ToString();


            try
            {
                byte[] rdata = (new ASCIIEncoding()).GetBytes(json);

                var requestString = $"http://localhost:8983/solr/gettingStarted/update";
                WebRequest request = WebRequest.Create(requestString);
                request.ContentLength = rdata.Length;
                request.ContentType = "application/json";
                request.Method = "POST";                

                Stream reqStream = request.GetRequestStream();
                reqStream.Write(rdata, 0, rdata.Length);

                using (WebResponse response = request.GetResponse())
                {
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
                    return true;
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    var message = $"Error code: {httpResponse.StatusCode}";
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        return true;
                    }
                }
                throw;
            }
            catch (Exception ex)
            {
                Utilities.SolrException.WriteError(ex);
            }
            return false;
        }
    }
}
