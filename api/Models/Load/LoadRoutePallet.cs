namespace bp.ot.s.API.Models.Load
{
    public class LoadRoutePallet
    {
        public int LoadRoutePalletId { get; set; }
        //IsEuroType == "EURO" : "Other"
        public bool IsEuroType { get; set; }
        public string Dimmension { get; set; }
        public int Amount { get; set; }
        public bool? IsStackable { get; set; }
        public bool? IsExchangeable { get; set; }
        public string Info { get; set; }

        public LoadRoute LoadRoute { get; set; }
        public int LoadRouteId { get; set; }
    }
}