﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace bp.ot.s.API.Entities.Dane.Company
{
    public class Company
    {
        public Company()
        {
            this.AddressList = new List<Address.Address>();
            this.EmployeeList = new List<CompanyEmployee>();
            this.BankAccountList = new List<BankAccount>();
        }
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
        public IList<Address.Address> AddressList { get; set; }
        public IList<CompanyEmployee> EmployeeList { get; set; }
        public IList<BankAccount> BankAccountList { get; set; }
    }
}
