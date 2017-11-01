using System.Collections.Generic;

namespace bp.ot.s.API.Models.Load
{
    public class LoadBuy
    {
        public LoadBuy()
        {
            this.Routes = new List<LoadRoutes>();
        }
        public BuyingInfo BuyingInfo { get; set; }
        public LoadInfo LoadInfo { get; set; }
        public List<LoadRoutes> Routes { get; set; }
    }
}