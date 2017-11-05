using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using bp.ot.s.API.Models.Load;
using bp.ot.s.API.Entities.Dane.Invoice;
using System.ComponentModel.DataAnnotations.Schema;

namespace bp.ot.s.API.Entities.Dane.Company
{
    public class Company
    {
        //public Company()
        //{
        //    this.AddressList = new List<Address.Address>();
        //    this.EmployeeList = new List<CompanyEmployee>();
        //    this.BankAccountList = new List<BankAccount>();
        //}
        public int CompanyId { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public string Fax { get; set; }
        public int TransId { get; set; }
        public string Legal_name { get; set; }
        public string Native_name { get; set; }
        public string Short_name { get; set; }
        public string Telephone { get; set; }
        [DataType(DataType.Url)]
        public string Url { get; set; }
        public string Vat_id { get; set; }
        public List<Address.Address> AddressList { get; set; }
        public List<CompanyEmployee> EmployeeList { get; set; }
        public List<BankAccount> BankAccountList { get; set; }


        public List<TradeInfo> TradeInfoList { get; set; }
        public List<LoadSell> LoadSellList { get; set; }
        public List<InvoiceBuy> InvoiceBuyList { get; set; }
        public List<InvoiceSell> InvoiceSellBuyerList { get;set; }
        public List<InvoiceSell> InvoiceSellSellerlList { get; set; }

    }
}
