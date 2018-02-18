using bp.ot.s.API.Entities.Dane.Company;
using bp.ot.s.API.Models.Load;
using bp.Pomocne.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceSellDTO
    {
        public InvoiceSellDTO()
        {
            this.InvoiceLines = new List<InvoiceLinesGroupDTO>();
            this.Rates = new List<InvoiceRatesGroupDTO>();
        }
        public int InvoiceSellId { get; set; }
        public int BaseInvoiceId { get; set; }
        public CompanyDTO CompanyBuyer { get; set; }
        public CompanyDTO CompanySeller { get; set; }
        public int? CorrectionId { get; set; }
        public string CorrectionTotalInfo { get; set; }
        public CreationInfo CreationInfo { get; set; }
        public CurrencyDTO Currency { get; set; }
        public DateTime DateOfIssue { get; set; }
        public DateTime DateOfSell { get; set; }

        public InvoiceExtraInfoDTO ExtraInfo { get; set; }
        public string GetInvoiceNo { get {
                string type = this.IsCorrection ? "Faktura korygująca " : "Faktura VAT ";
                return $"{type} {this.InvoiceNo}";
                }
        }
        public double GetInvoiceValue { get {
                double res = 0;
                if (IsCorrection)
                {
                    if (InvoiceOriginalPaid && this.InvoiceTotal.Corrections.Total_brutto < 0)
                    {
                        res = this.InvoiceTotal.Corrections.Total_brutto;
                    }
                    if (InvoiceOriginalPaid && this.InvoiceTotal.Corrections.Total_brutto >= 0)
                    {

                        res = this.InvoiceTotal.Corrections.Total_brutto;
                    }
                    if (!InvoiceOriginalPaid)
                    {
                        res = this.InvoiceTotal.Current.Total_brutto;
                    }
                }
                else
                {
                    res = this.InvoiceTotal.Current.Total_brutto;
                }
                return res;
            } }
        public string GetCorrectionPaymenntInfo { get {
                string isToPayOrToReturn = this.IsPaymentToReturn ? "Do zwrotu" : "Do zapłaty";
                return $"{isToPayOrToReturn} {GetInvoiceValue.ToString("# ##0.00")} {Currency.Name}";
            } }
        public string Info { get; set; }
        public string InvoiceNo { get; set; }
        public string InvoiceOriginalNo { get; set; }
        public bool InvoiceOriginalPaid { get; set; }
        public InvoiceTotalGroupDTO InvoiceTotal { get; set; }
        public bool IsCorrection { get; set; }
        public bool IsPaymentToReturn => this.InvoiceOriginalPaid && this.InvoiceTotal.Corrections.Total_brutto < 0;
        public bool PaymentIsDone { get; set; }
        public DateTime? PaymentDate { get; set; }
        public PaymentTermsDTO PaymentTerms { get; set; }
        public List<InvoiceLinesGroupDTO> InvoiceLines { get; set; }
        public List<InvoiceRatesGroupDTO> Rates { get; set; }

    }
}
