using Microsoft.EntityFrameworkCore;
using bp.ot.s.API.Entities.Dane.Kontrahent;
using System;
using JetBrains.Annotations;
using bp.ot.s.API.Entities.Dane.Address;
using bp.ot.s.API.Entities.Dane.Company;
using bp.ot.s.API.Entities.Dane.Invoice;

namespace bp.ot.s.API.Entities.Context
{
    public class OfferTransDbContextDane : DbContext
    {
        public OfferTransDbContextDane(DbContextOptions<OfferTransDbContextDane> options) : base(options)
        {

        }

        public DbSet<Address> Address { get; set; }
        public DbSet<BankAccount> BankAccount { get; set; }
        public DbSet<Company> Comapny { get; set; }
        public DbSet<CompanyEmployee> CompanyEmployee { get; set; }
        public DbSet<Currency> Currency { get; set; }
        public DbSet<InvoiceBuy> InvoiceBuy { get; set; }
        public DbSet<InvoiceSell> InvoiceSell { get; set; }
        public DbSet<InvoicePos> InvoicePos { get; set; }
        public DbSet<RateValue> InvoiceRatesValues { get; set; }
        public DbSet<PaymentTerm> PaymentTerm { get; set; }




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            //modelBuilder.Entity<PaymentTerms>()
            //    .HasOne(o => o.InvoiceBuy)
            //    .WithOne(o => o.PaymentTerms)
            //    .OnDelete(DeleteBehavior.SetNull);


            //modelBuilder.Entity<PaymentTerms>()
            //    .HasOne(o => o.InvoiceSell)
            //    .WithOne(o => o.PaymentTerms)
            //    .OnDelete(DeleteBehavior.SetNull);

            //modelBuilder.Entity<InvoiceSell>()
            //    .HasOne(o => o.PaymentTerms)
            //    .WithOne(o => o.InvoiceSell);

            //modelBuilder.Entity<InvoiceBuy>()
            //    .HasOne(o => o.PaymentTerms)
            //    .WithOne(o => o.InvoiceBuy);
        }
        

    }



    
}
