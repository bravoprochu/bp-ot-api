namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceTotalGroupDTO
    {
        public InvoiceTotalDTO Corrections { get {
                var res = new InvoiceTotalDTO();
                if (this.Current != null && this.Original != null) {
                    res.Total_brutto = this.Current.Total_brutto - this.Original.Total_brutto;
                    res.Total_netto = this.Current.Total_netto - this.Original.Total_netto;
                    res.Total_tax = this.Current.Total_tax - this.Original.Total_tax;
                }
                return res;
            } set {
            } }
        public InvoiceTotalDTO Current { get; set; }
        public InvoiceTotalDTO Original { get; set; }
    }
}