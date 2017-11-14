namespace bp.ot.s.API.Models.Load
{
    public class LoadInfo
    {
        public int LoadInfoId { get; set; }
        public string Description { get; set; }
        public LoadInfoExtra ExtraInfo { get; set; }
        public double? LoadHeight { get; set; }
        public double? LoadLength { get; set; }
        public double? LoadVolume { get; set; }
        public double LoadWeight { get; set; } //required

        public LoadBuy LoadBuy { get; set; }
        public int LoadBuyId { get; set; }
    }
}