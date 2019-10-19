using Newtonsoft.Json.Linq;
using Solr.BusinessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Solr.Controllers
{
    [RoutePrefix("api/Setup")]
    public class SetupController : ApiController
    {
        [Route("GetSchemaFields")]
        public List<Schema> GetSchemaFields()
        {
            List<Schema> schemaFields = new List<Schema>();
            try
            {
                schemaFields = Schema.Get();
            }
            catch (Exception ex)
            {
                Utilities.SolrException.WriteError(ex);
            }
            return schemaFields;
        }

        [Route("AddSchemaField")]
        public bool PostAddSchemaField(JObject value)
        {
            try
            {
                Schema schema = new Schema();
                var schemaName = value["Name"]?.Value<string>();

                if (schemaName.Substring(0, 3) == "LU_")
                {
                    schema.Name = schemaName;
                    schema.Type = value["Type"]?.Value<string>();
                    schema.Stored = value["Stored"]?.Value<string>();
                    schema.Indexed = value["Indexed"]?.Value<string>();
                    schema.DefaultValue = value["DefaultValue"]?.Value<string>();
                    schema.AddFieldInLucene();
                    schema.SaveInDB();
                }
                else if (schemaName.Substring(0, 3) == "DB_")
                {
                    schema.Name = schemaName;
                    schema.Type = value["Type"]?.Value<string>();
                    schema.Stored = "Yes";
                    schema.Indexed = "No";
                    schema.SaveInDB();
                }
                else
                {
                    schema.Name = schemaName;
                    schema.Type = value["Type"]?.Value<string>();
                    schema.Stored = value["Stored"]?.Value<string>();
                    schema.Indexed = value["Indexed"]?.Value<string>();
                    schema.DefaultValue = value["DefaultValue"]?.Value<string>();
                    schema.AddFieldInLucene();
                    schema.SaveInDB();
                }
                return true;
            }
            catch (Exception ex)
            {
                Utilities.SolrException.WriteError(ex);
            }
            return false;
        }

        [Route("DeleteSchemaField")]
        public bool PostDeleteSchemaField(JObject value)
        {
            try
            {
                int id = value["ID"].Value<int>();
                Schema schema = Schema.Get(id);
                schema.Name = value["Name"].Value<string>();
                schema.Delete();
                return true;
            }
            catch (Exception ex)
            {
                Utilities.SolrException.WriteError(ex);
            }
            return false;
        }

        [Route("IngestFiles")]
        public void PostIngestFiles(JObject value)
        {
            try
            {
                string folderPath = value["Directory"].Value<string>();
                string searchPattern = value["SearchPattern"].Value<string>();
                BusinessLayer.SolrOperation.IngestFiles(folderPath, searchPattern);
            }
            catch (Exception ex)
            {
                Utilities.SolrException.WriteError(ex);
            }
        }

        [Route("DeleteFiles")]
        public void PostDeleteFiles(JObject value)
        {
            try
            {
                var requestString = $"http://localhost:8983/solr/gettingStarted/update?stream.body=<delete><query>*:*</query></delete>&commit=true";
                SolrOperation.LuceneRequest(requestString);
            }
            catch (Exception ex)
            {
                Utilities.SolrException.WriteError(ex);
            }
        }

    }
}
