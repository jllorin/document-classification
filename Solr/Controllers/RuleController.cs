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
    [RoutePrefix("api/Rule")]
    public class RuleController : ApiController
    {
        [Route("GetSelect")]
        public string GetSelect(string value)
        {
            try
            {
                JObject objValue = JObject.Parse(value);
                var rule = objValue.ToObject<Rule>();
                string result = Search.Select(rule.QueryString, rule.Rows?.ToString(), rule.Sort, rule.Start?.ToString(), rule.Fields);
                return result;
            }
            catch (Exception ex)
            {
                Utilities.SolrException.WriteError(ex);
            }
            return string.Empty;
        }

        [Route("ClassifyItem")]
        public bool PostClassifyItem(JObject value)
        {
            try
            {
                var rule = value.ToObject<Rule>();
                Classify classify = Classify.SetupRule(rule);
                classify.UpdateDBLucene();                
                return true;
            }
            catch (Exception ex)
            {
                Utilities.SolrException.WriteError(ex);
            }
            return false;
        }
        
        [Route("GetSearchDatabases")]
        public string GetSearchDatabases()
        {
            List<SolrCore> cores = new List<SolrCore>();
            try
            {
                return SolrCore.GetCores();
            }
            catch (Exception ex)
            {
                Utilities.SolrException.WriteError(ex);
            }
            return string.Empty;
        }

        [Route("GetFileBytes")]
        public string GetFileBytes(string fileID)
        {
            try
            {                
                byte[] fileBytes = System.IO.File.ReadAllBytes(fileID);
                string content = Convert.ToBase64String(fileBytes);
                return content;
            }
            catch (Exception ex)
            {
                Utilities.SolrException.WriteError(ex);
            }
            return null;
        }

        [Route("GetRules")]
        public List<Rule> GetRules(bool includeBlank)
        {
            List<Rule> rules = new List<Rule>();
            try
            {
                if (includeBlank)
                {
                    rules.Add(Rule.Get(-1));
                }
                rules.AddRange(Rule.Get());
            }
            catch (Exception ex)
            {
                Utilities.SolrException.WriteError(ex);
            }
            return rules;
        }

        [Route("SaveRule")]
        public bool PostSaveRule(JObject value)
        {
            try
            {
                var rule = value.ToObject<Rule>();
                rule.Save();
                foreach (var item in rule.ClassifyFields)
                {
                    item.RuleIDFk = rule.ID;
                    item.Save();
                }
                return true;
            }
            catch (Exception ex)
            {
                Utilities.SolrException.WriteError(ex);
            }
            return false;
        }

        [Route("DeleteRule")]
        public bool PostDeleteRule(JObject value)
        {
            try
            {
                var rule = value.ToObject<Rule>();
                rule.Delete();
                return true;
            }
            catch (Exception ex)
            {
                Utilities.SolrException.WriteError(ex);
            }
            return false;
        }

        [Route("SimilarityCheck")]
        public bool PostSimilarityCheck(JObject value)
        {
            try
            {
                var rule = value["Rule"].ToObject<Rule>();                
                return true;
            }
            catch (Exception ex)
            {
                Utilities.SolrException.WriteError(ex);
            }
            return false;
        }
    }
}
