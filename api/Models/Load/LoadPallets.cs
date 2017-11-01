namespace bp.ot.s.API.Models.Load
{
    public class LoadPallets
    {
        public string Type { get; set; }
        public string Dimmension { get; set; }
        public int Amount { get; set; }
        public bool? IsStackable { get; set; }
        public bool? IsExchangeable { get; set; }
        public string Info { get; set; }
    }
}