namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceTotalGroupDTO
    {
        public InvoiceTotalDTO Corrections { get; set; }
        public InvoiceTotalDTO Current { get; set; }
        public InvoiceTotalDTO Original { get; set; }
    }
}