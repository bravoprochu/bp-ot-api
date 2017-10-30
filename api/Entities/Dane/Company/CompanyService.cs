using bp.ot.s.API.Entities.Context;
using bp.ot.s.API.Entities.Dane.Address;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Entities.Dane.Company
{
    public class CompanyService
    {
        private readonly OfferTransDbContextDane _db;

        public CompanyService(OfferTransDbContextDane db)
        {
            this._db = db;
        }

        public Company CompanyMapperDTO(CompanyDTO cDTO)
        {
            var company = new Company();
            company.Email = cDTO.Email;
            company.Fax = cDTO.Fax;
            company.Legal_name = cDTO.Legal_name;
            company.Native_name = cDTO.Native_name;
            company.Short_name = cDTO.Short_name;
            company.Telephone = cDTO.Telephone;
            company.TransId = cDTO.Trans_id;
            company.Url = cDTO.Url;
            company.Vat_id = cDTO.Vat_id;

            return company;
        }

        public Address.Address AddresMapperDTO(Address.AddressDTO aDTO)
        {
            var address = new Address.Address();
            address.Address_type = aDTO.Address_type;
            address.Country = aDTO.Country;
            address.Locality = aDTO.Locality;
            address.Postal_code = aDTO.Postal_code;
            address.Street_address = aDTO.Street_address;
            address.Street_number = aDTO.Street_number;

            return address;
        }

        public CompanyEmployee CompanyEmployeeMapperDTO(CompanyEmployeeDTO empDTO)
        {
            var emp = new CompanyEmployee();
            emp.Email = empDTO.Email;
            emp.Entitled = empDTO.Entitled;
            emp.Family_name = empDTO.Family_name;
            emp.Given_name = empDTO.Given_name;
            emp.Hidden = empDTO.Hidden;
            emp.Is_driver = empDTO.Is_driver;
            emp.Is_moderator = empDTO.Is_moderator;
            emp.Telephone = empDTO.Telephone;
            emp.Trans_id = empDTO.Trans_id;

            return emp;
        }

        public BankAccount BankAccountMapperDTO(BankAccountDTO bankDTO)
        {
            var bank = new BankAccount();
            bank.Account_no = bankDTO.Account_no;
            bank.Swift = bankDTO.Swift;
            bank.Type = bankDTO.Type;

            return bank;
        }

        public CompanyDTO EntityToDTOCompany(Company company )
        {
            return new CompanyDTO()
            {
                AddressList = company.AddressList.Select(sa => new AddressDTO
                {
                    AddressId = sa.AddressId,
                    Address_type = sa.Address_type,
                    Country = sa.Country,
                    Locality = sa.Locality,
                    Postal_code = sa.Postal_code,
                    Street_address = sa.Street_address,
                    Street_number = sa.Street_number,
                }).ToList(),
                BankAccountList = company.BankAccountList.Select(sb => new BankAccountDTO
                {
                    Account_no = sb.Account_no,
                    BankAccountId = sb.BankAccountId,
                    Swift = sb.Swift,
                    Type = sb.Type
                }).ToList(),
                CompanyId = company.CompanyId,
                Email = company.Email,
                EmployeeList = company.EmployeeList.Select(se => new CompanyEmployeeDTO
                {
                    CompanyEmployeeId = se.CompanyEmployeeId,
                    Email = se.Email,
                    Entitled = se.Entitled,
                    Family_name = se.Family_name,
                    Given_name = se.Given_name,
                    Hidden = se.Hidden,
                    Is_driver = se.Is_driver,
                    Is_moderator = se.Is_moderator,
                    Telephone = se.Telephone,
                    Trans_id = se.Trans_id,
                }).ToList(),
                Fax = company.Fax,
                Legal_name = company.Legal_name,
                Native_name = company.Native_name,
                Short_name = company.Short_name,
                Telephone = company.Telephone,
                Trans_id = company.TransId,
                Url = company.Url,
                Vat_id = company.Vat_id,
            };

        }

        public CompanyDTO GetCompanyDTOById(int id)
        {
            var res = this._db.Comapny
                .Include(i => i.AddressList)
                .Include(i => i.BankAccountList)
                .Include(i => i.EmployeeList)
                .Where(w => w.CompanyId == id)
                .FirstOrDefault();

            if (res != null)
            {
                return this.EntityToDTOCompany(res);
            }
            else {
                return null;
            }
        }

        public Company GetCompanyById(int id)
        {
            var res = this._db.Comapny
                .Include(i => i.AddressList)
                .Include(i => i.BankAccountList)
                .Include(i => i.EmployeeList)
                .Where(w => w.CompanyId == id)
                .FirstOrDefault();

            if (res != null)
            {
                return res;
            }
            else
            {
                return null;
            }
        }


    }
}
