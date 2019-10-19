using Solr.Models;
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
    public class Schema
    {
        public int ID { get; private set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Stored { get; set; }
        public string Indexed { get; set; }
        public string DefaultValue { get; set; }

        public Schema() { this.ID = -1; }

        private Schema(int id, string name, string description, string type, string stored, string indexed, string defaultValue)
        {
            this.ID = id;
            this.Name = name;
            this.Description = description;
            this.Type = type;
            this.Stored = stored;
            this.Indexed = indexed;
            this.DefaultValue = defaultValue;
        }

        public static List<Schema> Get()
        {
            List<Schema> schemaFields = new List<Schema>();
            try
            {
                ClassificationDataContext context = new ClassificationDataContext(System.Configuration.ConfigurationManager.ConnectionStrings["ClassificationConnectionStringProd"].ConnectionString);
                var query = from entity in context.tbl_Schemas
                            where string.Compare(entity.Status, "Deleted", true) != 0
                            orderby entity.Name
                            select entity;
                foreach (var item in query)
                {
                    var schema = new Schema(item.record_id, item.Name, item.Description, item.Type, item.Stored, item.Indexed, item.DefaultValue);
                    schemaFields.Add(schema);
                }
            }
            catch (Exception ex)
            {
                Solr.Utilities.SolrException.WriteError(ex);
            }
            return schemaFields;
        }

        public static Schema Get(int id)
        {
            try
            {
                ClassificationDataContext context = new ClassificationDataContext(System.Configuration.ConfigurationManager.ConnectionStrings["ClassificationConnectionStringProd"].ConnectionString);
                var query = from entity in context.tbl_Schemas
                            where entity.record_id == id
                            select entity;
                foreach (var item in query)
                {
                    return new Schema(item.record_id, item.Name, item.Description, item.Type, item.Stored, item.Indexed, item.DefaultValue);
                }
            }
            catch (Exception ex)
            {
                Solr.Utilities.SolrException.WriteError(ex);
            }
            return null;
        }

        private void UpdateEntity(tbl_Schema item)
        {
            item.Name = this.Name;
            item.Description = this.Description;
            item.Type = this.Type;
            item.Indexed = this.Indexed;
            item.Stored = this.Stored;
            item.DefaultValue = this.DefaultValue;
            item.ModifiedDate = DateTime.Now;
            item.ModifiedBy = "jllorin";
        }

        public bool AddFieldInLucene()
        {
            try
            {
                var requestString = $"http://localhost:8983/solr/gettingStarted/schema";
                string json = string.Format(@"{{
                 add-field: {{
                    name: '{0}',
                    type: '{1}',
                    stored: '{2}',
                    indexed: '{3}',
                    default: '{4}' 
                 }}
                }}", this.Name, this.Type, this.Stored == "Yes" ? "true" : "false", this.Indexed == "Yes" ? "true" : "false", this.DefaultValue);
                SolrOperation.LucenePostJson(requestString, json);
            }
            catch (Exception ex)
            {
                Solr.Utilities.SolrException.WriteError(ex);
            }
            return false;
        }

        private bool DeleteFieldInLucene()
        {
            try
            {
                var requestString = $"http://localhost:8983/solr/gettingStarted/schema";
                string json = string.Format(@"{{
                 delete-field: {{
                    name: '{0}'
                 }}
                }}", this.Name);
                SolrOperation.LucenePostJson(requestString, json);
            }
            catch (Exception ex)
            {
                Solr.Utilities.SolrException.WriteError(ex);
            }
            return false;
        }

        public bool SaveInDB()
        {
            try
            {                
                ClassificationDataContext context = new ClassificationDataContext(System.Configuration.ConfigurationManager.ConnectionStrings["ClassificationConnectionStringProd"].ConnectionString);
                if (this.ID == -1)
                {
                    tbl_Schema item = new tbl_Schema();
                    UpdateEntity(item);
                    item.Status = "New";
                    context.tbl_Schemas.InsertOnSubmit(item);
                    context.SubmitChanges();
                    this.ID = item.record_id;
                }
                else
                {
                    var query = from entity in context.tbl_Schemas
                                where entity.record_id == this.ID
                                select entity;
                    foreach (var item in query)
                    {
                        UpdateEntity(item);
                    }
                    context.SubmitChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                Solr.Utilities.SolrException.WriteError(ex);
            }
            return false;
        }

        public bool Delete()
        {
            try
            {
                DeleteFieldInLucene();
                ClassificationDataContext context = new ClassificationDataContext(System.Configuration.ConfigurationManager.ConnectionStrings["ClassificationConnectionStringProd"].ConnectionString);
                var query = from entity in context.tbl_Schemas
                            where entity.record_id == this.ID
                            select entity;
                foreach (var item in query)
                {
                    item.Status = "Deleted";
                }
                context.SubmitChanges();
                return true;
            }
            catch (Exception ex)
            {
                Solr.Utilities.SolrException.WriteError(ex);
            }
            return false;
        }
    }
}
