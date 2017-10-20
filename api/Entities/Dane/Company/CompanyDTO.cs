using bp.Pomocne.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Entities.Dane.Company
{
    public class CompanyDTO
    {
        public CompanyDTO()
        {
            this.AddressList = new List<Address.AddressDTO>();
            this.EmployeeList = new List<CompanyEmployeeDTO>();

        }

        public int CompanyId { get; set; }
        public string ContactInfo
        {
            get
            {
                var result = string.IsNullOrWhiteSpace(this.Telephone) ? "" : $"tel: {this.Telephone}";
                result += result.Length > 2 ? ", " : "";
                result += string.IsNullOrWhiteSpace(this.Email) ? "" : $"email: {this.Email}";
                return result;
            }
        }
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
        public List<Address.AddressDTO> AddressList { get; set; }
        public List<CompanyEmployeeDTO> EmployeeList { get; set; }
    }
}
