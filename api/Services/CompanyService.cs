﻿using bp.kpir.DAO.Addresses;
using bp.kpir.DAO.Contractor;
using bp.ot.s.API.Entities.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Services
{
    public class CompanyService
    {
        private readonly BpKpirContextDane _db;

        public CompanyService(BpKpirContextDane db)
        {
            this._db = db;
        }





        public async Task<Company> CompanyMapper(Company db, CompanyDTO cDTO)
        {
            if (db == null)
            {
                db = new Company();
            }
            if (db.CompanyId != cDTO.CompanyId)
            {
                return await this._db.Company.FindAsync(cDTO.CompanyId);
            }
            else
            {
                return db;
            }
        }

        public CompanyCardDTO CompanyCardMapper(Company db)
        {
            var res = new CompanyCardDTO();

            if (db.AddressList.Count > 0)
            {
                var address = db.AddressList.FirstOrDefault();

                res.Address = bp.sharedLocal.EntitiesConv.CompanyAddressCombined(address);
            }
            if (db.BankAccountList.Count > 0)
            {
                foreach (var bank in db.BankAccountList)
                {
                    res.BankAccounts.Add(bp.sharedLocal.EntitiesConv.CompanyBankAccountCombined(bank));
                }
            }
            res.CompanyId = db.CompanyId;
            res.Contact = bp.sharedLocal.EntitiesConv.CompayContactCombined(db);
            res.ShortName = db.Short_name;
            res.VatId = db.Vat_id;
            return res;
        }

        public void AddresMapperDTO(Address address, AddressDTO aDTO)
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

        public IQueryable<Company> CompanyQueryableFull()
        {
            return this._db.Company
                .Include(i => i.AddressList)
                .Include(i => i.BankAccountList)
                .Include(i => i.EmployeeList)
                .Include(i => i.TradeInfoList)
                .Include(i => i.LoadSellList)
                .Include(i => i.LoadTransEuList)
                .Include(i => i.InvoiceBuyList)
                .Include(i => i.InvoiceSellBuyerList)
                .Include(i => i.InvoiceSellSellerlList)
                .Include(i => i.TransportOfferList);
        }


        public BankAccount BankAccountMapperDTO(BankAccountDTO bankDTO)
        {
            var bank = new BankAccount();
            bank.Account_no = bankDTO.Account_no;
            bank.Swift = bankDTO.Swift;
            bank.Type = bankDTO.Type;

            return bank;
        }


        public async Task Delete(int id)
        {



            var comp = await this.CompanyQueryable().FirstOrDefaultAsync(c => c.CompanyId == id);


            if (comp.AddressList.Count > 0)
            {
                comp.AddressList.ForEach(address =>
                {
                    this._db.Entry(address).State = EntityState.Deleted;
                });
            }


            if (comp.BankAccountList.Count > 0)
            {
                comp.AddressList.ForEach(bankAccount =>
                {
                    this._db.Entry(bankAccount).State = EntityState.Deleted;
                });
            }


            if (comp.EmployeeList.Count > 0)
            {
                comp.EmployeeList.ForEach(employee =>
                {
                    this._db.Entry(employee).State = EntityState.Deleted;
                });
            }


            this._db.Entry(comp).State = EntityState.Deleted;


            try
            {
                await this._db.SaveChangesAsync();
                Debug.WriteLine("ok saving....");
            }
            catch (DbException e)
            {

                throw e;
            }


        }



        public AddressDTO EtDTOAddress(Address addres)
        {
            var res = new AddressDTO();
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

        public CompanyDTO EtDTOCompany(Company company)
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

        public CompanyListDTO EtoDTOCompanyList(Company company)
        {
            var cDTO = this.EtDTOCompany(company);
            var res = new CompanyListDTO();
            res.Id = company.CompanyId;
            res.Adres = cDTO.AddressList.FirstOrDefault().AddressCombined;
            res.NIP = cDTO.Vat_id;
            res.Skrot = cDTO.Short_name;
            res.Telefon = cDTO.Telephone;

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
            else
            {
                return null;
            }
        }

        public async Task<CompanyDTO> GetCompanyById(int id)
        {
            var res = await this.CompanyQueryable()
                .FirstOrDefaultAsync(f => f.CompanyId == id);

            if (res != null)
            {
                return this.EtDTOCompany(res);
            }
            else
            {
                return null;
            }
        }

        public async Task<Company> Owner()
        {
            //first company in a base is "Owner"
            return await this.CompanyQueryable().FirstOrDefaultAsync(f => f.CompanyId == 1);
        }

        public async Task<CompanyDTO> OwnerDTO()
        {
            //first company in a base is "Owner"
            var owner = await this.Owner();
            return this.EtDTOCompany(owner);
        }

    }
}
