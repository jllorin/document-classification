using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Solr.BusinessLayer
{
    public class SolrOperation
    {
        private static void ProcessDirs(string folderPath, string searchPattern)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = false;
                startInfo.UseShellExecute = false;
                startInfo.FileName = @"C:\temp\curl-7.48.0-win64-mingw\bin\curl.exe";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                foreach (string file in Directory.GetFiles(folderPath, searchPattern))
                {
                    System.IO.File.AppendAllText(@"c:\temp\solr.log", file + "\n");
                    StringBuilder myFileBuilder = new StringBuilder(file);
                    string myFile = myFileBuilder.Replace(@"\", @"/").ToString();

                    string filePath = System.Web.HttpUtility.UrlEncode(file);
                    //string fileName = System.Web.HttpUtility.UrlEncode(System.IO.Path.GetFileName(file));

                    // http://localhost:8983/solr/gettingStarted/update/extract?literal.id=25&commit=true" -F "myfile=@/Users/jllorin/Documents/oaktown.pdf
                    startInfo.Arguments = $"\"http://localhost:8983/solr/gettingStarted/update/extract?literal.id={filePath}\" -F \"myfile=@{myFile}\"";
                    try
                    {
                        // Start the process with the info we specified.
                        // Call WaitForExit and then the using statement will close.
                        using (Process exeProcess = Process.Start(startInfo))
                        {
                            exeProcess.WaitForExit();
                        }
                    }
                    catch (Exception ex)
                    {
                        Utilities.SolrException.WriteError(ex);
                    }
                }

                foreach (string dir in Directory.GetDirectories(folderPath))
                {
                    SolrOperation.ProcessDirs(dir, searchPattern);
                }
            }
            catch (Exception ex)
            {
                Solr.Utilities.SolrException.WriteError(ex);
            }
        }

        public static bool IngestFiles(string folderPath, string searchPattern)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = false;
                startInfo.UseShellExecute = false;
                startInfo.FileName = @"C:\temp\curl-7.48.0-win64-mingw\bin\curl.exe";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                ProcessDirs(folderPath, searchPattern);

                startInfo.Arguments = "\"http://localhost:8983/solr/gettingStarted/update?commit=true\"";
                try
                {
                    // Start the process with the info we specified.
                    // Call WaitForExit and then the using statement will close.
                    using (Process exeProcess = Process.Start(startInfo))
                    {
                        exeProcess.WaitForExit();
                    }
                }
                catch (Exception ex)
                {
                    Utilities.SolrException.WriteError(ex);                    
                }


                return true;
            }
            catch (Exception ex)
            {
                Utilities.SolrException.WriteError(ex);
            }
            return false;
        }


        public static bool LucenePostJson(string requestString, string json)
        {
            try
            {
                byte[] rdata = (new ASCIIEncoding()).GetBytes(json);                
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
                        return false;
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


        public static string LuceneRequest(string requestString)
        {
            try
            {
                WebRequest request = WebRequest.Create(requestString);
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
                    return responseFromServer;
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
                        return text;
                    }
                }
                throw;
            }
            catch (Exception ex)
            {
                Utilities.SolrException.WriteError(ex);
                return string.Empty;
            }
        }
    }
}

