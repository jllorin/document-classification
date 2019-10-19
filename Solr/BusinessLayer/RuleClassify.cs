using Solr.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solr.BusinessLayer
{
    public class RuleClassify
    {
        public int ID { get; set; }
        public int RuleIDFk { get; set; }
        public int SchemaIDFk { get; set; }
        public string SchemaName { get; set; }
        public string Value { get; set; }
        public bool IsDirty { get; set; }


        public RuleClassify() { this.ID = -1; }

        private RuleClassify(int id, int ruleIDFk, int schemaIDFk, string schemaName, string value, bool isDirty)
        {
            this.ID = id;
            this.RuleIDFk = ruleIDFk;
            this.SchemaIDFk = schemaIDFk;
            this.SchemaName = schemaName;
            this.Value = value;
            this.IsDirty = IsDirty;
        }

        public static List<RuleClassify> Get()
        {
            List<RuleClassify> attributes = new List<RuleClassify>();
            try
            {
                ClassificationDataContext context = new ClassificationDataContext(System.Configuration.ConfigurationManager.ConnectionStrings["ClassificationConnectionStringProd"].ConnectionString);
                var query = from entity1 in context.tbl_RuleClassifies
                            join entity2 in context.tbl_Schemas
                            on entity1.SchemaID_fk equals entity2.record_id
                            where entity1.Status == true && string.Compare(entity2.Status, "Deleted", true) != 0
                            select new { entity1.record_id, entity1.RuleID_fk, entity1.SchemaID_fk, entity1.Value, entity2.Name };
                foreach (var item in query)
                {
                    attributes.Add(new RuleClassify(item.record_id, item.RuleID_fk, item.SchemaID_fk, item.Name, item.Value, false));
                }
            }
            catch (Exception ex)
            {
                Utilities.SolrException.WriteError(ex);                
            }
            return attributes;
        }

        private void UpdateEntity(tbl_RuleClassify item)
        {
            item.RuleID_fk = this.RuleIDFk;
            item.SchemaID_fk = this.SchemaIDFk;
            item.Value = this.Value;
            item.ModifiedBy = "jllorin";
            item.ModifiedDate = DateTime.Now;            
        }

        public void Save()
        {
            try
            {
                ClassificationDataContext context = new ClassificationDataContext(System.Configuration.ConfigurationManager.ConnectionStrings["ClassificationConnectionStringProd"].ConnectionString);
                if (this.ID == -1)
                {
                    tbl_RuleClassify item = new tbl_RuleClassify();
                    UpdateEntity(item);
                    item.Status = true;
                    context.tbl_RuleClassifies.InsertOnSubmit(item);
                    context.SubmitChanges();
                    this.ID = item.record_id;                   
                }
                else
                {
                    var query = from entity in context.tbl_RuleClassifies
                                where entity.record_id == this.ID
                                select entity;
                    foreach (var item in query)
                    {
                        UpdateEntity(item);
                    }
                    context.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                Utilities.SolrException.WriteError(ex);                
            }
        }
    }
}
