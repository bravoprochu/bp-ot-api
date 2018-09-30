using bp.ot.s.API.Entities.Context;
using bp.sharedLocal.ModelStateHelpful;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bp.ot.s.API.Services;
using bp.kpir.DAO.Contractor;
using bp.kpir.DAO.Addresses;

namespace bp.ot.s.API.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Spedytor, Finanse")]
    [Route("api/[controller]/[action]")]
    public class CompanyController : Controller
    {
        private readonly BpKpirContextDane _db;
        private readonly CompanyService _companyService;

        public CompanyController(BpKpirContextDane db, CompanyService companyService)
        {
            this._db = db;
            this._companyService = companyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var companyList = await this._companyService.CompanyQueryable()
                .OrderBy(o=>o.Short_name)
                .ToListAsync();


            var result = new List<CompanyDTO>();
            foreach (var comp in companyList)
            {
                result.Add(this.EntityToDTO(comp));
            }
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var dbRes = id > 0 ? await this._companyService.CompanyQueryable().FirstOrDefaultAsync(f => f.CompanyId == id) : await this._companyService.CompanyQueryable().OrderByDescending(f => f.CompanyId).FirstOrDefaultAsync();

            if (dbRes == null) return BadRequest(ModelStateHelpful.ModelError("Info", $"Nie znaleziono kontrahenta o Id: {id}"));


            return Ok(this._companyService.EtDTOCompany(dbRes));
        }

        [HttpGet("{key}")]
        public IActionResult GetByKey(string key)
        {
            var lowerKey = key.ToLower();
            //if (string.IsNullOrWhiteSpace(key)) return BadRequest(ModelStateHelpful.ModelError("Błąd danych", "Wyszukiwana fraza nie może być pusta"));
            //var result =this._db.Comapny.Where(w => w.Short_name.Contains(key) || w.Legal_name.Contains(key) || w.Vat_id.Contains(key)).ToList();
            //if (result.Count == 0) return BadRequest(ModelStateHelpful.ModelError("Dane", $"Nie znaleziono kontrahentów dla frazy {key}"));


            if (string.IsNullOrWhiteSpace(key)) return Ok(new object[]{});
            var companyList = this._db.Company
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

        [HttpPut("{id}")]
        public async Task<IActionResult> Put (int id, [FromBody] CompanyDTO cDTO)
        {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var dbNip = await this._companyService.CompanyQueryable()
                .FirstOrDefaultAsync(f => f.Vat_id == cDTO.Vat_id);

            var db = new Company();

            if (id == 0)
            {
                if (dbNip != null)
                {
                    return Ok(new { Info = $"Firma z NIP: {dbNip.Vat_id} już istnieje w bazie, zapis anulowany" });
                }
                else
                {
                    this.CompanyMapper(db, cDTO);
                    this._db.Entry(db).State = EntityState.Added;
                }
            }
            else {
                var dbBase = await this._companyService.CompanyQueryable()
                    .FirstOrDefaultAsync(f => f.CompanyId == id);
                //found NIP in db;
                if (dbNip != null)
                {
                    //all are the same
                    if ((cDTO.Vat_id != dbBase.Vat_id) && (cDTO.Vat_id == dbNip.Vat_id))
                    {
                        return Ok(new { Info = $"Firma z NIP: {dbNip.Vat_id} już istnieje w bazie, zapis anulowany" });
                     }
                    //nip has changed...
                this.CompanyMapper(dbBase, cDTO);
                }
                else
                {
                    this.CompanyMapper(dbBase, cDTO);
                }

            }



            //if (id > 0)
            //{
            //    var dbBase = await this._companyService.CompanyQueryable()
            //    .FirstOrDefaultAsync(f => f.CompanyId == id);

            //    if (dbBase == null)
            //    {
            //        return NotFound();
            //    }
            //    if (dbNip==null || (dbNip.Vat_id==cDTO.Vat_id))
            //    {
            //        this.CompanyMapper(dbBase, cDTO);
            //    }
            //    else {
            //        return Ok(new { Info = $"Firma z NIP: {dbNip.Vat_id} już istnieje w bazie, zapis anulowany" });
            //    }
            //}
            //else {
            //    if (dbNip!=null)
            //    {
            //        return Ok(new { Info = $"Firma z NIP: {dbNip.Vat_id} już istnieje w bazie, zapis anulowany" });
            //    }

            //    this.CompanyMapper(db, cDTO);
            //    this._db.Entry(db).State = EntityState.Added;
            //}

            await this._db.SaveChangesAsync();

            return NoContent();
        }

        private void CompanyMapper(Company db, CompanyDTO dto)
        {
            db.Email = dto.Email;
            db.Fax = dto.Fax;
            db.Legal_name = dto.Legal_name;
            db.Native_name = dto.Native_name;
            db.Short_name = dto.Short_name;
            db.Telephone = dto.Telephone;
            if (dto.Trans_id.HasValue) {
                db.TransId = dto.Trans_id.Value;
            }
            db.Url = dto.Url;
            db.Vat_id = dto.Vat_id;


            //check for deleted addresses
            if (db.AddressList?.Count > 0)
            {
                foreach (var aBase in db.AddressList)
                {
                    var found = dto.AddressList.Where(w => w.AddressId == aBase.AddressId).FirstOrDefault();
                    if (found == null) { this._db.Entry(aBase).State = EntityState.Deleted; }
                }
            }

            //check for deleted employees
            if (db.EmployeeList?.Count > 0)
            {
                foreach (var eBase in db.EmployeeList)
                {
                    var found = dto.EmployeeList.Where(w => w.CompanyEmployeeId == eBase.CompanyEmployeeId).FirstOrDefault();
                    if (found == null) { this._db.Entry(eBase).State = EntityState.Deleted; }
                }
            }

            //check for deleted bankAccounts
            if (db.BankAccountList?.Count > 0)
            {
                foreach (var bAcc in db.BankAccountList)
                {
                    var found = dto.BankAccountList.Where(w => w.BankAccountId == bAcc.BankAccountId).FirstOrDefault();
                    if (found == null) { this._db.Entry(bAcc).State = EntityState.Deleted; }
                }
            }


            //modify or add the rest..
            foreach (var aDTO in dto.AddressList)
            {
                //new address
                if (aDTO.AddressId == 0)
                {
                    var newAddress = new Address
                    {
                        Address_type = aDTO.Address_type,
                        Company = db,
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
                    var foundInBase = db.AddressList.Where(w => w.AddressId == aDTO.AddressId).FirstOrDefault();
                    if (foundInBase != null)
                    {
                        foundInBase.Address_type = aDTO.Address_type;
                        foundInBase.Country = aDTO.Country;
                        foundInBase.Locality = aDTO.Locality;
                        foundInBase.Postal_code = aDTO.Postal_code;
                        foundInBase.Street_address = aDTO.Street_address;
                        foundInBase.Street_number = aDTO.Street_number;
                    }
                }
            }

            //modify or add employees
            foreach (var eDTO in dto.EmployeeList)
            {
                //new employee
                if (eDTO.CompanyEmployeeId == 0)
                {
                    var newCompanyEmployee = new CompanyEmployee
                    {
                        Company = db,
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

                    var foundInBase = db.EmployeeList.Where(w => w.CompanyEmployeeId == eDTO.CompanyEmployeeId).FirstOrDefault();
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
                    }
                }
            }

            //modify or add bankAccount
            foreach (var accDTO in dto.BankAccountList)
            {
                if (accDTO.BankAccountId == 0)
                {
                    var accNew = new BankAccount
                    {
                        Account_no = accDTO.Account_no,
                        Company = db,
                        Swift = accDTO.Swift,
                        Type = accDTO.Type
                    };

                    this._db.Entry(accNew).State = EntityState.Added;
                }
                else
                {

                    var foundInBase = db.BankAccountList.Where(w => w.BankAccountId == accDTO.BankAccountId).FirstOrDefault();
                    if (foundInBase != null)
                    {
                        foundInBase.Account_no = accDTO.Account_no;
                        foundInBase.Swift = accDTO.Swift;
                        foundInBase.Type = accDTO.Type;
                    }

                    this._db.Entry(foundInBase).State = EntityState.Modified;
                }
            }
        }


        private Company FindCompanyByNip(string vatId)
        {
            return this._db.Company.Where(w => w.Vat_id == vatId).FirstOrDefault();
        }


        public CompanyDTO EntityToDTO(Company s) {
            return this._companyService.EtDTOCompany(s);
        }
    }
}
