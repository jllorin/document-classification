using Solr.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solr.BusinessLayer
{
    public class Rule
    {
        public int ID { get; set; }
        public string RuleName { get; set; }
        public bool IsQueryShow { get; set; }
        public bool IsClassifyShow { get; set; }
        public bool IsSimilarityShow { get; set; }
        public string QueryString { get; set; }
        public string Sort { get; set; }
        public string Fields { get; set; }
        public int? Start { get; set; }
        public int? Rows { get; set; }
        public List<RuleClassify> ClassifyFields { get; set; }
        public Rule() { }

        private Rule(int id, string ruleName, bool isQueryShow, bool isClassifyShow, bool isSimilarityShow, string queryString, string sort, string fields, int? start, int? rows)
        {
            this.ID = id;
            this.RuleName = ruleName;
            this.IsQueryShow = isQueryShow;
            this.IsClassifyShow = isClassifyShow;
            this.IsSimilarityShow = isSimilarityShow;
            this.QueryString = queryString;
            this.Sort = sort;
            this.Fields = fields;
            this.Start = start;
            this.Rows = rows;
            this.ClassifyFields = new List<RuleClassify>();
        }

        public static List<Rule> Get()
        {
            List<Rule> rules = new List<Rule>();
            try
            {
                ClassificationDataContext context = new ClassificationDataContext(System.Configuration.ConfigurationManager.ConnectionStrings["ClassificationConnectionStringProd"].ConnectionString);
                var query = from entity in context.tbl_Rules
                            where entity.Status == true
                            orderby entity.RuleName
                            select entity;
                foreach (var item in query)
                {
                    rules.Add(new Rule(item.record_id, item.RuleName, item.IsQueryShow, item.IsClassifyShow, item.IsSimilarityShow, item.QueryString, item.Sort, item.Fields, 
                        item.Start, item.Rows));
                }

                var ruleClassifyList = RuleClassify.Get()
                    .GroupBy(u => u.RuleIDFk)
                    .Select(grp => new { RuleIDFk = grp.Key, classifyList = grp.ToList() })
                    .ToList();

                var joinQuery = from entity1 in rules
                                join entity2 in ruleClassifyList
                                on entity1.ID equals entity2.RuleIDFk
                                select new { entity1, entity2 };
                foreach (var item in joinQuery)
                {
                    item.entity1.ClassifyFields = item.entity2.classifyList;
                }
            }
            catch (Exception ex)
            {
                Utilities.SolrException.WriteError(ex);
            }
            return rules;
        }

        public static Rule Get(int id)
        {
            try
            {
                if (id == -1)
                {
                    return new Rule(-1, "[New Rule]", false, true, false, "*.*", string.Empty, string.Empty, null, null);
                }
                else
                {
                    ClassificationDataContext context = new ClassificationDataContext(System.Configuration.ConfigurationManager.ConnectionStrings["ClassificationConnectionStringProd"].ConnectionString);
                    var query = from entity in context.tbl_Rules
                                where entity.record_id == id
                                select entity;
                    foreach (var item in query)
                    {
                        return new Rule(item.record_id, item.RuleName, item.IsQueryShow, item.IsClassifyShow, item.IsSimilarityShow, item.QueryString, item.Sort, item.Fields, 
                            item.Start, item.Rows);
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.SolrException.WriteError(ex);
            }
            return null;
        }

        private void UpdateEntity(tbl_Rule item)
        {
            item.RuleName = this.RuleName;
            item.IsQueryShow = this.IsQueryShow;
            item.IsClassifyShow = this.IsClassifyShow;
            item.IsSimilarityShow = this.IsSimilarityShow;
            item.QueryString = this.QueryString;
            item.Sort = this.Sort;
            item.Fields = this.Fields;
            item.Start = this.Start;
            item.Rows = this.Rows;
        }

        public bool Save()
        {
            try
            {
                ClassificationDataContext context = new ClassificationDataContext(System.Configuration.ConfigurationManager.ConnectionStrings["ClassificationConnectionStringProd"].ConnectionString);
                if (this.ID == -1)
                {                    
                    tbl_Rule item = new tbl_Rule();
                    UpdateEntity(item);
                    item.ModifiedBy = "jllorin";
                    item.ModifiedDate = DateTime.Now;
                    item.Status = true;
                    context.tbl_Rules.InsertOnSubmit(item);
                    context.SubmitChanges();
                    this.ID = item.record_id;
                }
                else
                {
                    var query = from entity in context.tbl_Rules
                                where entity.record_id == this.ID
                                select entity;
                    foreach (var item in query)
                    {
                        UpdateEntity(item);
                    }
                    context.SubmitChanges();
                    return true;
                }
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
                ClassificationDataContext context = new ClassificationDataContext(System.Configuration.ConfigurationManager.ConnectionStrings["ClassificationConnectionStringProd"].ConnectionString);
                var query = from entity in context.tbl_Rules
                            where entity.record_id == this.ID
                            select entity;
                foreach (var item in query)
                {
                    item.Status = false;
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
