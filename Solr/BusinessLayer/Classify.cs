using Newtonsoft.Json.Linq;
using Solr.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Solr.BusinessLayer
{
    public class Classify
    {
        public Rule RuleToClassify { get; private set; }

        private Classify(Rule ruleToClassify)
        {
            this.RuleToClassify = ruleToClassify;
        }

        public static Classify SetupRule(Rule rule)
        {
            try
            {
                return new Classify(rule);                
            }
            catch (Exception ex)
            {
                Utilities.SolrException.WriteError(ex);
            }
            return null;
        }

        public bool UpdateDBLucene()
        {
            try
            {
                List<ItemIngested> itemIngesteds = new List<ItemIngested>();
                string resultJSON = Search.Select(this.RuleToClassify.QueryString, this.RuleToClassify.Rows?.ToString(), this.RuleToClassify.Sort, 
                    this.RuleToClassify.Start?.ToString(), this.RuleToClassify.Fields);
                JObject jsonObject = JObject.Parse(resultJSON);
                var resultValues = jsonObject["response"]["docs"];
                foreach (var resultValue in resultValues)
                {
                    string id = resultValue["id"].Value<string>();                    
                    JObject dbValue = new JObject();
                    ItemIngested itemIngested = new ItemIngested();
                    itemIngested.ID = -1;
                    itemIngested.FileID = id;
                    itemIngested.Value = string.Empty;
                    foreach (var classifyField in this.RuleToClassify.ClassifyFields)
                    {
                        if (classifyField.SchemaName.StartsWith("DB_"))
                        {
                            dbValue.Add(classifyField.SchemaName, classifyField.Value);                            
                        }
                        else if (classifyField.SchemaName.StartsWith("LU_"))
                        {
                            UpdateLuceneItem(id, classifyField.SchemaName, classifyField.Value);
                        }
                        else
                        {
                            UpdateLuceneItem(id, classifyField.SchemaName, classifyField.Value);
                            dbValue.Add(classifyField.SchemaName, classifyField.Value);
                        }                                             
                    }
                    itemIngested.Value = dbValue.ToString();
                    itemIngesteds.Add(itemIngested);
                }                
                CommitLucene();

                ClassificationDataContext context = new ClassificationDataContext(System.Configuration.ConfigurationManager.ConnectionStrings["ClassificationConnectionStringProd"].ConnectionString);
                // See if we need to update DB
                var query = from entity1 in itemIngesteds
                            join entity2 in context.tbl_ItemIngesteds
                            on entity1.FileID equals entity2.FileID into newTable
                            from entity3 in newTable.DefaultIfEmpty()
                            select new { entity1, entity3 };
                foreach (var item in query)
                {
                    if (item.entity3 == null)
                    {
                        tbl_ItemIngested newItem = new tbl_ItemIngested();
                        newItem.FileID = item.entity1.FileID;
                        newItem.Value = item.entity1.Value;
                        newItem.Status = true;
                        newItem.ModifiedBy = "jllorin";
                        newItem.ModifiedDate = DateTime.Now;
                        context.tbl_ItemIngesteds.InsertOnSubmit(newItem);
                    }
                    else
                    {
                        item.entity3.Value = item.entity1.Value;
                    }
                }
                context.SubmitChanges();
                return true;
            }
            catch (Exception ex)
            {
                Utilities.SolrException.WriteError(ex);
            }
            return false;
        }


        private bool UpdateLuceneItem(string id, string fieldName, string value)
        {
            id = id.Replace("\\", "\\\\");
            string json = string.Format(@"{{
                 add: {{
                  doc: {{
                    id: '{0}',
                    {1}: {{set : '{2}'}}
                  }}
                 }}
                }}", id, fieldName, value);
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

        private bool CommitLucene()
        {
            string json = string.Format(@"{{
                    commit: {{}},
                    optimize: {{ waitSearcher:false }}
                }}");
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
