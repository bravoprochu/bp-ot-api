namespace bp.ot.s.API.Models.Load
{
    public class LoadRoutePalletDTO
    {
        public int? LoadRoutePalletId { get; set; }
        public string Type { get; set; }
        public string Dimmension { get; set; }
        public int Amount { get; set; }
        public bool? Is_stackable { get; set; }
        public bool? Is_exchangeable { get; set; }
        public string Info { get; set; }
    }
}