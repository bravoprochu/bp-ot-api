namespace bp.ot.s.API.Models.Load
{
    public class LoadInfoDTO
    {
        public string Description { get; set; }

        public LoadInfoExtra Extra_info { get; set; }

        public double? Load_height { get; set; }
        public double? Load_Length { get; set; }
        public double? Load_volume { get; set; }
        public double? Load_weight { get; set; }



    }
}