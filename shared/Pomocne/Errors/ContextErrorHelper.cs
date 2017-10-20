using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.Pomocne.ErrorsHelper
{
    public class ContextErrorHelper
    {
        public ContextErrorHelper()
        {
            this.ContextErrorCollection = new Dictionary<string, string>();
        }

        public Dictionary<string, string> ContextErrorCollection { get; private set; }
        public void AddError(string groupName, string description) {
                this.ContextErrorCollection.Add(groupName, description);
        }
    }
}
