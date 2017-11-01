using bp.Pomocne.DTO;
using System.Collections.Generic;

namespace bp.ot.s.API.Models.Load
{
    public class LoadInfoExtra
    {
        public LoadInfoExtra()
        {
            this.RequiredWaysOfLoading = new List<ValueViewValueDTO>();
            this.RequiredAdrClasses = new List<ValueViewValueDTO>();
        }

        public bool? IsLtl { get; set; }
        public bool? IsLiftRequired { get; set; }
        public bool? IsTruckCraneRequired { get; set; }
        public bool? IsTirCableRequired { get; set; }
        public bool? IsTrackingSystemRequired { get; set; }
        public bool? IsForClearence { get; set; }
        public List<ValueViewValueDTO> RequiredWaysOfLoading { get; set; }
        public List<ValueViewValueDTO> RequiredAdrClasses { get; set; }
        public ValueViewValueDTO RequiredTruckBody { get; set; }
        public ValueViewValueDTO TypeOfLoad { get; set; }
    }
}