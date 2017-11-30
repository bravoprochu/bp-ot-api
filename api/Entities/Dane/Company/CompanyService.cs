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





        public void CompanyMapper(Company dbComp, CompanyDTO cDTO)
        {
            if (dbComp.CompanyId != cDTO.CompanyId) {
                dbComp = this._db.Company.Find(cDTO.CompanyId);
            }
        }

        public void AddresMapperDTO(Address.Address address,  Address.AddressDTO aDTO)
        {
            address.Address_type = aDTO.Address_type;
            address.Country = aDTO.Country;
            address.Locality = aDTO.Locality;
            address.Postal_code = aDTO.Postal_code;
            address.Street_address = aDTO.Street_address;
            address.Street_number = aDTO.Street_number;
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

        public IQueryable<Company> CompanyQueryable()
        {
            return this._db.Company
                .Include(i => i.AddressList)
                .Include(i => i.BankAccountList)
                .Include(i => i.EmployeeList);
        }

        public BankAccount BankAccountMapperDTO(BankAccountDTO bankDTO)
        {
            var bank = new BankAccount();
            bank.Account_no = bankDTO.Account_no;
            bank.Swift = bankDTO.Swift;
            bank.Type = bankDTO.Type;

            return bank;
        }

        public AddressDTO EtDTOAddress(Address.Address addres)
        {
            var res = new Address.AddressDTO();
            res.AddressId = addres.AddressId;
            res.Address_type = addres.Address_type;
            res.Country = addres.Country;
            res.Locality = addres.Locality;
            res.Postal_code = addres.Postal_code;
            res.Street_address = addres.Street_address;
            res.Street_number = addres.Street_number;
            return res;
        }

        public BankAccountDTO EtDTOBank(BankAccount bank)
        {
            var res = new BankAccountDTO();
            res.Account_no = bank.Account_no;
            res.BankAccountId = bank.BankAccountId;
            res.Swift = bank.Swift;
            res.Type = bank.Type;
            return res;
        }

        public CompanyDTO EtDTOCompany(Company company )
        {
            var res = new CompanyDTO();
//            if (company == null) { return res; }

            res.AddressList = new List<AddressDTO>();
            if (company.AddressList != null)
            {
                foreach (var add in company.AddressList)
                {
                    res.AddressList.Add(this.EtDTOAddress(add));
                }
            }
            res.BankAccountList = new List<BankAccountDTO>();
            if (company.BankAccountList != null)
            {
                foreach (var bank in company.BankAccountList)
                {
                    res.BankAccountList.Add(this.EtDTOBank(bank));
                }
            }
            res.CompanyId = company.CompanyId;
            res.Email = company.Email;
            res.EmployeeList = new List<CompanyEmployeeDTO>();
            if (company.EmployeeList != null)
            {
                foreach (var emp in company.EmployeeList)
                {
                    res.EmployeeList.Add(this.EtDTOEmployee(emp));
                }
            }
            res.Fax = company.Fax;
            res.Legal_name = company.Legal_name;
            res.Native_name = company.Native_name;
            res.Short_name = company.Short_name;
            res.Telephone = company.Telephone;
            res.Trans_id = company.TransId;
            res.Url = company.Url;
            res.Vat_id = company.Vat_id;
            
            return res;
        }

        public CompanyEmployeeDTO EtDTOEmployee(CompanyEmployee emp)
        {
            var res = new CompanyEmployeeDTO();
            //if (emp == null) { return res; }
            res.CompanyEmployeeId = emp.CompanyEmployeeId;
            res.Email = emp.Email;
            res.Entitled = emp.Entitled;
            res.Family_name = emp.Family_name;
            res.Given_name = emp.Given_name;
            res.Is_driver = emp.Is_driver;
            res.Telephone = emp.Telephone;
            res.Trans_id = emp.Trans_id;
            return res;
        }

        public CompanyDTO GetCompanyDTOById(int id)
        {
            var res = this._db.Company
                .Include(i => i.AddressList)
                .Include(i => i.BankAccountList)
                .Include(i => i.EmployeeList)
                .Where(w => w.CompanyId == id)
                .FirstOrDefault();

            if (res != null)
            {
                return this.EtDTOCompany(res);
            }
            else {
                return null;
            }
        }

        public Company GetCompanyById(int id)
        {
            var res = this._db.Company
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

        public async Task<Company> Owner()
        {
            //first company in a base is "Owner"
            return await this._db.Company.FirstOrDefaultAsync(f => f.CompanyId == 1);
        }


    }
}
