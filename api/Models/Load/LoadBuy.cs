using System.Collections.Generic;

namespace bp.ot.s.API.Models.Load
{
    public class LoadBuy
    {
        public LoadBuy()
        {
            this.Routes = new List<LoadRoute>();
        }
        public int LoadBuyId { get; set; }
        public LoadInfo LoadInfo { get; set; }
        public TradeInfo BuyingInfo { get; set; }
        public List<LoadRoute> Routes { get; set; }

        public Load Load { get; set; }
        public int LoadId { get; set; }
    }
}