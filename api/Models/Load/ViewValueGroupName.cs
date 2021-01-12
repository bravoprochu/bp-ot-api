using System.Collections.Generic;

namespace bp.ot.s.API.Models.Load
{
    public class ViewValueGroupName
    {
        public int ViewValueGroupNameId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<ViewValueDictionary> ViewValueDictinaryList { get; set; }
    }
}