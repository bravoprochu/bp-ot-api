using bp.ot.s.API.Entities.Dane.Company;
using bp.shared.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceCommonDTO: CreationInfo
    {
        public InvoiceCommonDTO()
        {
            this.InvoiceLines = new List<InvoiceLinesGroupDTO>();
        }


        public CurrencyDTO Currency { get; set; }

        private DateTime _dateOfIssue { get; set; }
        public DateTime DateOfIssue
        {
            get
            {
                return _dateOfIssue.AddHours(1);
                //return bp.shared.DateHelp.DateHelpful.DataStalaGodzina(_dateOfIssue, 2);
            }
            set { _dateOfIssue = value; }
        }
        private DateTime _dateOfSell { get; set; }
        public DateTime DateOfSell
        {
            get
            {
                return _dateOfSell.AddHours(1);
                //return bp.shared.DateHelp.DateHelpful.DataStalaGodzina(_dateOfSell, 2);
            }
            set { _dateOfSell = value; }
        }




        //public DateTime DateOfIssue { get; set; }
        //public DateTime DateOfSell { get; set; }

        public string Info { get; set; }
        public string InvoiceNo { get; set; }
        public bool IsCorrection { get; set; }
        public PaymentTermsDTO PaymentTerms { get; set; }

        public List<InvoiceLinesGroupDTO> InvoiceLines { get; set; }
        public InvoiceTotalGroupDTO InvoiceTotal
        {
            get
            {
                var res = new InvoiceTotalGroupDTO();

                res.Current = new InvoiceTotalDTO();
                res.Current.Total_brutto = this.InvoiceLines.Sum(s => s.Current.Brutto_value);
                res.Current.Total_netto = this.InvoiceLines.Sum(s => s.Current.Netto_value);
                res.Current.Total_tax = this.InvoiceLines.Sum(s => s.Current.Vat_value);

                res.Original = new InvoiceTotalDTO();
                res.Original.Total_brutto = this.InvoiceLines.Sum(s => s.Original.Brutto_value);
                res.Original.Total_netto = this.InvoiceLines.Sum(s => s.Original.Netto_value);
                res.Original.Total_tax = this.InvoiceLines.Sum(s => s.Original.Vat_value);
                
                return res;
            }
        }
        public List<InvoiceRatesGroupDTO> Rates
        {
            get
            {
                var res = new List<InvoiceRatesGroupDTO>();

                var currLines = this.InvoiceLines.Select(select => select.Current).Where(w => w.Vat_rate != null).GroupBy(g => g.Vat_rate).Select(sg => new InvoiceRatesValuesDTO
                {
                    Vat_rate = sg.Key,
                    Brutto_value = sg.Sum(s => s.Brutto_value),
                    Netto_value = sg.Sum(s => s.Netto_value),
                    Vat_value = sg.Sum(s => s.Vat_value)
                }).ToList();

                var orgLines = this.InvoiceLines.Select(select => select.Original).Where(w=>w.Vat_rate!=null).GroupBy(g => g.Vat_rate).Select(sg => new InvoiceRatesValuesDTO
                {
                    Vat_rate = sg.Key,
                    Brutto_value = sg.Sum(s => s.Brutto_value),
                    Netto_value = sg.Sum(s => s.Netto_value),
                    Vat_value = sg.Sum(s => s.Vat_value)
                }).ToList();

                var totalCurrAndOrgRates = new List<InvoiceRatesValuesDTO>();
                totalCurrAndOrgRates.AddRange(currLines);
                if (this.IsCorrection)
                {
                    totalCurrAndOrgRates.AddRange(orgLines);
                }


                foreach (var rateUnique in totalCurrAndOrgRates.GroupBy(g => g.Vat_rate).Select(s => s).ToList())
                {
                    var rate = rateUnique.FirstOrDefault().Vat_rate;

                    var rGroup = new InvoiceRatesGroupDTO();
                    rGroup.VatRate = rate;
                    rGroup.Current = currLines.Find(f => f.Vat_rate == rate) ?? new InvoiceRatesValuesDTO
                    {
                        Brutto_value = 0,
                        Netto_value = 0,
                        Vat_rate = rate,
                        Vat_value = 0
                    };
                    rGroup.Original = orgLines.Find(f => f.Vat_rate == rate) ?? new InvoiceRatesValuesDTO
                    {
                        Brutto_value = 0,
                        Netto_value = 0,
                        Vat_rate = rate,
                        Vat_value = 0
                    };
                    res.Add(rGroup);
                }
                return res;
            }
        }

    }
}
