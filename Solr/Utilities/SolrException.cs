using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solr.Utilities
{
    public class SolrException
    {
        private const string EVENT_SOURCE = "Classification";

        public static void WriteError(string message)
        {
            using (System.Web.Hosting.HostingEnvironment.Impersonate())
            {
                EventLog.WriteEntry(EVENT_SOURCE, string.Format("{0}", message), EventLogEntryType.Error);
            }
        }

        public static void WriteError(System.Exception exception)
        {
            using (System.Web.Hosting.HostingEnvironment.Impersonate())
            {
                EventLog.WriteEntry(EVENT_SOURCE, string.Format("{0}", exception.Message), EventLogEntryType.Error);
                EventLog.WriteEntry(EVENT_SOURCE, exception.StackTrace, EventLogEntryType.Error);
            }
        }

        public static void WriteInfo(string message)
        {
            using (System.Web.Hosting.HostingEnvironment.Impersonate())
            {
                EventLog.WriteEntry(EVENT_SOURCE, string.Format("{0}", message), EventLogEntryType.Information);
            }
        }

        public static void WriteWarning(string message)
        {
            using (System.Web.Hosting.HostingEnvironment.Impersonate())
            {
                EventLog.WriteEntry(EVENT_SOURCE, string.Format("{0}", message), EventLogEntryType.Warning);
            }
        }
    }
}
