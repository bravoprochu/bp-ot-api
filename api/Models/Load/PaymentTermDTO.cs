namespace bp.ot.s.API.Models.Load
{
    public class PaymentTermDTO
    {
        public int PaymentTermId { get; set; }
        public string Name { get; set; }
        public bool IsDescription { get; set; }
        public bool IsPaymentDate { get; set; }
    }
}