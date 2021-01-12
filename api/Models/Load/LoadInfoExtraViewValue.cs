using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Models.Load
{
    public class LoadInfoExtraViewValue
    {

        public ViewValueDictionary ViewValueDictionary { get; set; }
        public int? ViewValueDictionaryId { get; set; }

        public LoadInfoExtra LoadInfoExtra { get; set; }
        public int? LoadInfoExtraId { get; set; }
    }
}
