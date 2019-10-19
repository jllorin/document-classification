using Solr.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solr.BusinessLayer
{
    public class ItemIngested
    {
        public int ID { get; set; }
        public string FileID { get; set; }
        public string Value { get; set; }

        public ItemIngested() { }

        private ItemIngested(int id, string fileID, string value)
        {
            this.ID = id;
            this.FileID = fileID;
            this.Value = value;
        }

        public static List<ItemIngested> Get()
        {
            List<ItemIngested> items = new List<ItemIngested>();
            try
            {
                ClassificationDataContext context = new ClassificationDataContext(System.Configuration.ConfigurationManager.ConnectionStrings["ClassificationConnectionStringProd"].ConnectionString);
                var query = from entity in context.tbl_ItemIngesteds
                            where entity.Status == true
                            select entity;
                foreach (var item in query)
                {
                    items.Add(new ItemIngested(item.record_id, item.FileID, item.Value));
                }
            }
            catch (Exception ex)
            {
                Utilities.SolrException.WriteError(ex);                
            }
            return items;
        }
    }
}
