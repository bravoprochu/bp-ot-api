using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Models.Load
{
    public class ViewValueDictionary
    {
        public int ViewValueDictionaryId { get; set; }
        public ViewValueGroupName ViewValueGroupName { get; set; }
        public int ViewValueGroupNameId { get; set; }
        
        public string Value { get; set; }
        public string ViewValue { get; set; }



        public List<LoadInfoExtraAddrClassess> RequiredAdrClasses { get; set; }

        public List<LoadInfoExtraWaysOfLoad> RequiredWaysOfLoading { get; set; }


        public List<LoadInfoExtra> RequiredTruckBodyList { get; set; }
        public List<LoadInfoExtra> TypeOfLoadList { get; set; }



    }
}
