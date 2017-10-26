using bp.ot.s.API.Entities.Context;
using bp.ot.s.API.Entities.Dane.Address;
using bp.ot.s.API.Entities.Dane.Company;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bp.PomocneLocal.ModelStateHelpful;
using Microsoft.EntityFrameworkCore;
using bp.Pomocne.Constansts;

namespace bp.ot.s.API.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Spedytor")]
    [Route("api/[controller]/[action]")]
    public class CompanyController : Controller
    {
        private readonly OfferTransDbContextDane _db;

        public CompanyController(OfferTransDbContextDane db)
        {
            this._db = db;

        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var companyList = this._db.Comapny
                .Include(i=>i.AddressList)
                .Include(i => i.BankAccountList)
                .Include(i=>i.EmployeeList)
                .ToList();


            var result = new List<CompanyDTO>();
            foreach (var comp in companyList)
            {
                result.Add(this.EntityToDTO(comp));
            }
            return Ok(result);
        }

        [HttpGet("{idx}")]
        public IActionResult GetById(int idx)
        {
            var res = this._db.Comapny
                .Include(i => i.AddressList)
                .Include(i => i.BankAccountList)
                .Include(i => i.EmployeeList)
                .Where(w => w.CompanyId == idx)
                .FirstOrDefault();


            if (res == null) return BadRequest(ModelStateHelpful.ModelError("Info", $"Nie znaleziono kontrahenta o Id: {idx}"));

            return Ok(this.EntityToDTO(res));
        }

        [HttpGet("{key}")]
        public IActionResult GetByKey(string key)
        {
            var lowerKey = key.ToLower();
            //if (string.IsNullOrWhiteSpace(key)) return BadRequest(ModelStateHelpful.ModelError("Błąd danych", "Wyszukiwana fraza nie może być pusta"));
            //var result =this._db.Comapny.Where(w => w.Short_name.Contains(key) || w.Legal_name.Contains(key) || w.Vat_id.Contains(key)).ToList();
            //if (result.Count == 0) return BadRequest(ModelStateHelpful.ModelError("Dane", $"Nie znaleziono kontrahentów dla frazy {key}"));


            if (string.IsNullOrWhiteSpace(key)) return Ok(new object[]{});
            var companyList = this._db.Comapny
                .Include(i => i.AddressList)
                .Include(i => i.BankAccountList)
                .Include(i => i.EmployeeList)
                .Where(w => w.Short_name.Contains(key) || w.Legal_name.Contains(key) || w.Vat_id.Contains(key))
                .ToList();

            var result = new List<CompanyDTO>();
            foreach (var comp in companyList)
            {
                result.Add(this.EntityToDTO(comp));
            }
            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CompanyDTO companyDTO)
        {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            if (this.FindCompanyByNip(companyDTO.Vat_id) != null) {
                return BadRequest(bp.PomocneLocal.ModelStateHelpful.ModelStateHelpful.ModelError("Błąd danych", $"Firma o ID: {companyDTO.Vat_id} już istnieje w bazie"));
            }

            var compNew = new Company
            {
                 Email=companyDTO.Email,
                 Fax=companyDTO.Fax,
                 Legal_name=companyDTO.Legal_name,
                 Native_name=companyDTO.Native_name,
                 Short_name=companyDTO.Short_name,
                 Telephone=companyDTO.Telephone,
                 TransId=companyDTO.Trans_id,
                 Url=companyDTO.Url,
                 Vat_id=companyDTO.Vat_id
            };

            _db.Entry(compNew).State = Microsoft.EntityFrameworkCore.EntityState.Added;

            foreach (var aDTO in companyDTO.AddressList)
            {
                var address = new Address
                {
                    Address_type = aDTO.Address_type,
                    Company = compNew,
                    Country = aDTO.Country,
                    Locality = aDTO.Locality,
                    Postal_code = aDTO.Postal_code,
                    Street_address = aDTO.Street_address,
                    Street_number = aDTO.Street_number
                };

                _db.Entry(address).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            }

            foreach (var acc in companyDTO.BankAccountList)
            {
                var accNew = new BankAccount
                {
                    Account_no = acc.Account_no,
                    Company = compNew,
                    Swift = acc.Swift,
                    Type = acc.Type
                };

                this._db.Entry(accNew).State = EntityState.Added;
            }

            foreach (var emp in companyDTO.EmployeeList)
            {
                var companyEmployee = new CompanyEmployee
                {
                    Company=compNew,
                    Email=emp.Email,
                    Entitled=emp.Entitled,
                    Family_name=emp.Family_name,
                    Given_name=emp.Given_name,
                    Hidden=emp.Hidden,
                    Is_driver=emp.Is_driver,
                    Is_moderator=emp.Is_moderator,
                    Telephone=emp.Telephone,
                    Trans_id=emp.Trans_id
                };
                _db.Entry(companyEmployee).State = EntityState.Added;
            }


            await _db.SaveChangesAsync();
            return Ok(this.EntityToDTO(compNew));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put (int id, [FromBody] CompanyDTO cDTO)
        {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }
            var comp = this._db.Comapny
                .Include(i => i.AddressList)
                .Include(i => i.BankAccountList)
                .Include(i => i.EmployeeList)
                .Where(w => w.CompanyId == id).FirstOrDefault();
            if (comp == null) return BadRequest(bp.PomocneLocal.ModelStateHelpful.ModelStateHelpful.ModelError("Błąd", $"Nie znaleziono firmy o Id: {id}"));


            comp.Email = cDTO.Email;
            comp.Fax = cDTO.Fax;
            comp.Legal_name = cDTO.Legal_name;
            comp.Native_name = cDTO.Native_name;
            comp.Short_name = cDTO.Short_name;
            comp.Telephone = cDTO.Telephone;
            comp.TransId = cDTO.Trans_id;
            comp.Url = cDTO.Url;
            comp.Vat_id = cDTO.Vat_id;


            //check for deleted addresses
            foreach (var aBase in comp.AddressList)
            {
                var found = cDTO.AddressList.Where(w => w.AddressId == aBase.AddressId).FirstOrDefault();
                if (found == null) { this._db.Entry(aBase).State = EntityState.Deleted; }
            }

            //check for deleted employees
            foreach (var eBase in comp.EmployeeList)
            {
                var found = cDTO.EmployeeList.Where(w => w.CompanyEmployeeId == eBase.CompanyEmployeeId).FirstOrDefault();
                if (found == null) { this._db.Entry(eBase).State = EntityState.Deleted; }
            }

            //check for deleted bankAccounts
            foreach (var bAcc in comp.BankAccountList)
            {
                var found = cDTO.BankAccountList.Where(w => w.BankAccountId == bAcc.BankAccountId).FirstOrDefault();
                if (found == null) { this._db.Entry(bAcc).State = EntityState.Deleted; }
            }


            //modify or add the rest..
            foreach (var aDTO in cDTO.AddressList)
            {
                //new address
                if (aDTO.AddressId == 0)
                {
                    var newAddress = new Address
                    {
                        Address_type = aDTO.Address_type,
                        Company = comp,
                        Country = aDTO.Country,
                        Locality = aDTO.Locality,
                        Postal_code = aDTO.Postal_code,
                        Street_address = aDTO.Street_address,
                        Street_number = aDTO.Street_number
                    };
                    this._db.Entry(newAddress).State = EntityState.Added;
                }
                else
                {
                    //modify
                    var foundInBase = comp.AddressList.Where(w => w.AddressId == aDTO.AddressId).FirstOrDefault();
                    if (foundInBase != null)
                    {
                        foundInBase.Address_type = aDTO.Address_type;
                        foundInBase.Country = aDTO.Country;
                        foundInBase.Locality = aDTO.Locality;
                        foundInBase.Postal_code = aDTO.Postal_code;
                        foundInBase.Street_address = aDTO.Street_address;
                        foundInBase.Street_number = aDTO.Street_number;
                        this._db.Entry(foundInBase).State = EntityState.Modified;
                    }
                }
            }


            //modify or add employees
            foreach (var eDTO in cDTO.EmployeeList)
            {
                //new employee
                if (eDTO.CompanyEmployeeId == 0)
                {
                    var newCompanyEmployee = new CompanyEmployee
                    {
                        Company = comp,
                        Email = eDTO.Email,
                        Entitled = eDTO.Entitled,
                        Family_name = eDTO.Family_name,
                        Given_name = eDTO.Given_name,
                        Hidden = eDTO.Hidden,
                        Is_driver = eDTO.Is_driver,
                        Is_moderator = eDTO.Is_moderator,
                        Telephone = eDTO.Telephone,
                        Trans_id = eDTO.Trans_id
                    };
                    this._db.Entry(newCompanyEmployee).State = EntityState.Added;
                }
                else
                {

                    var foundInBase = comp.EmployeeList.Where(w => w.CompanyEmployeeId == eDTO.CompanyEmployeeId).FirstOrDefault();
                    if (foundInBase != null)
                    {
                        foundInBase.Email = eDTO.Email;
                        foundInBase.Entitled = eDTO.Entitled;
                        foundInBase.Family_name = eDTO.Family_name;
                        foundInBase.Given_name = eDTO.Given_name;
                        foundInBase.Hidden = eDTO.Hidden;
                        foundInBase.Is_driver = eDTO.Is_driver;
                        foundInBase.Is_moderator = eDTO.Is_moderator;
                        foundInBase.Telephone = eDTO.Telephone;
                        foundInBase.Trans_id = eDTO.Trans_id;

                        this._db.Entry(foundInBase).State = EntityState.Modified;
                    }
                }
            }

            //modify or add bankAccount
            foreach (var accDTO in cDTO.BankAccountList)
            {
                if (accDTO.BankAccountId == 0)
                {
                    var accNew = new BankAccount
                    {
                        Account_no = accDTO.Account_no,
                        Company = comp,
                        Swift = accDTO.Swift,
                        Type = accDTO.Type
                    };

                    this._db.Entry(accNew).State = EntityState.Added;
                }
                else
                {

                    var foundInBase = comp.BankAccountList.Where(w => w.BankAccountId == accDTO.BankAccountId).FirstOrDefault();
                    if (foundInBase != null)
                    {
                        foundInBase.Account_no = accDTO.Account_no;
                        foundInBase.Swift = accDTO.Swift;
                        foundInBase.Type = accDTO.Type;
                    }

                    this._db.Entry(foundInBase).State = EntityState.Modified;
                }
            }


            await this._db.SaveChangesAsync();
            
            return Ok(this.EntityToDTO(comp));
        }


        private Company FindCompanyByNip(string vatId)
        {
            return this._db.Comapny.Where(w => w.Vat_id == vatId).FirstOrDefault();
        }


        private CompanyDTO EntityToDTO(Company s) {
            return new CompanyDTO()
            {
                AddressList = s.AddressList.Select(sa => new AddressDTO
                {
                    AddressId = sa.AddressId,
                    Address_type = sa.Address_type,
                    Country = sa.Country,
                    Locality = sa.Locality,
                    Postal_code = sa.Postal_code,
                    Street_address = sa.Street_address,
                    Street_number = sa.Street_number,
                }).ToList(),
                BankAccountList = s.BankAccountList.Select(sb => new BankAccountDTO {
                    Account_no=sb.Account_no,
                    BankAccountId=sb.BankAccountId,
                    Swift=sb.Swift,
                    Type=sb.Type
                }).ToList(),
                CompanyId = s.CompanyId,
                Email = s.Email,
                EmployeeList = s.EmployeeList.Select(se => new CompanyEmployeeDTO
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
                Fax = s.Fax,
                Legal_name = s.Legal_name,
                Native_name = s.Native_name,
                Short_name = s.Short_name,
                Telephone = s.Telephone,
                Trans_id = s.TransId,
                Url = s.Url,
                Vat_id = s.Vat_id,
            };
        }
    }
}
