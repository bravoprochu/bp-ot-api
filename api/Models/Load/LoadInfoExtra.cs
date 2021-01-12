using bp.shared.DTO;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace bp.ot.s.API.Models.Load
{
    public class LoadInfoExtra
    {
        public int LoadInfoExtraId { get; set; }
        public bool? IsLtl { get; set; }
        public bool? IsLiftRequired { get; set; }
        public bool? IsTruckCraneRequired { get; set; }
        public bool? IsTirCableRequired { get; set; }
        public bool? IsTrackingSystemRequired { get; set; }
        public bool? IsForClearence { get; set; }

        public List<LoadInfoExtraAddrClassess> RequiredAddrClassess { get; set; }
        public List<LoadInfoExtraWaysOfLoad> RequiredWaysOfLoading { get; set; }

        public ViewValueDictionary RequiredTruckBody { get; set; }
        public int RequiredTruckBodyId { get; set; }
        public ViewValueDictionary TypeOfLoad { get; set; }
        public int? TypeOfLoadId { get; set; }

        public LoadInfo LoadInfo { get; set; }
        public int LoadInfoId { get; set; }
    }
}