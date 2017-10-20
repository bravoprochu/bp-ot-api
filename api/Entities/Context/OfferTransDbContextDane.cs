﻿using Microsoft.EntityFrameworkCore;
using bp.ot.s.API.Entities.Dane.Kontrahent;
using System;
using JetBrains.Annotations;
using bp.ot.s.API.Entities.Dane.Address;
using bp.ot.s.API.Entities.Dane.Company;

namespace bp.ot.s.API.Entities.Context
{
    public class OfferTransDbContextDane : DbContext
    {
        public OfferTransDbContextDane(DbContextOptions<OfferTransDbContextDane> options) : base(options)
        {

        }

        public DbSet<Address> Address { get; set; }
        public DbSet<Company> Comapny { get; set; }
        public DbSet<CompanyEmployee> CompanyEmployee { get; set; }



        //public DbSet<Kontrahent> Kontrahent { get; set; }
        //public DbSet<KontrahentAdres> KontrahentAdres { get; set; }
        //public DbSet<KontrahentBank> KontrahentBank { get; set; }
        //public DbSet<KontrahentKontakt> KontrahentKontakt { get; set; }







        //protected override void OnModelCreating(DbModelBuilder modelBuilder)
        //{
        //    modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

        //    modelBuilder.Entity<Kontrahent>()
        //        .HasOptional(o => o.KontrahentKontakt)
        //        .WithRequired(r => r.Kontrahent);
        //}
    }


    
}
