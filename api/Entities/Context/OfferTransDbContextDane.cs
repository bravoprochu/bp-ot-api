using Microsoft.EntityFrameworkCore;
using bp.ot.s.API.Entities.Dane.Kontrahent;
using System;
using JetBrains.Annotations;
using bp.ot.s.API.Entities.Dane.Address;
using bp.ot.s.API.Entities.Dane.Company;
using bp.ot.s.API.Entities.Dane.Invoice;
using bp.ot.s.API.Models.Load;

namespace bp.ot.s.API.Entities.Context
{
    public class OfferTransDbContextDane : DbContext
    {
        public OfferTransDbContextDane(DbContextOptions<OfferTransDbContextDane> options) : base(options)
        {

        }

       

        public DbSet<Address> Address { get; set; }
        public DbSet<BankAccount> BankAccount { get; set; }
        public DbSet<Company> Company { get; set; }
        public DbSet<CompanyEmployee> CompanyEmployee { get; set; }
        public DbSet<Currency> Currency { get; set; }
        public DbSet<CurrencyNbp> CurrencyNbp { get; set; }
        public DbSet<InvoiceBuy> InvoiceBuy { get; set; }
        public DbSet<InvoiceExtraInfo> InvoiceExtraInfo { get; set; }
        public DbSet<InvoiceSell> InvoiceSell { get; set; }
        public DbSet<InvoicePos> InvoicePos { get; set; }
        public DbSet<RateValue> InvoiceRatesValues { get; set; }
        public DbSet<InvoiceTotal> InvoiceTotal { get; set; }

        public DbSet<Load> Load { get; set; }
        public DbSet<LoadBuy> LoadBuy { get; set; }
        public DbSet<LoadInfo> LoadInfo { get; set; }
        
        public DbSet<LoadInfoExtraAddrClassess> LoadInfoExtraAddrClassess { get; set; }
        public DbSet<LoadInfoExtraWaysOfLoad> LoadInfoExtraWaysOfLoad { get; set; }


        public DbSet<LoadRoute> LoadRoute { get; set; }
        public DbSet<LoadRoutePallet> LoadRoutePallet { get; set; }

        public DbSet<LoadSell> LoadSell { get; set; }
        public DbSet<LoadSellContactPersons> LoadSellContactsPersons { get;set;}

        public DbSet<LoadTransEu> LoadTransEu { get; set; }
        public DbSet<LoadTransEuContactPerson> LoadTransEuContactsPersons { get; set; }

        public DbSet<PaymentTerm> PaymentTerm { get; set; }
        public DbSet<PaymentTerms> PaymentTerms { get; set; }

        public DbSet<TradeInfo> LoadTradeInfo { get; set; }
        public DbSet<ViewValueGroupName> ViewValueGroupName { get; set; }
        public DbSet<ViewValueDictionary> ViewValueDictionary { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            //top
            modelBuilder.Entity<Address>()
                .HasOne(o => o.Company)
                .WithMany(o => o.AddressList)
                .HasForeignKey(f => f.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<ViewValueGroupName>()
                .HasMany(m => m.ViewValueDictinaryList)
                .WithOne(o => o.ViewValueGroupName)
                .HasForeignKey(f => f.ViewValueGroupNameId);


            modelBuilder.Entity<ViewValueDictionary>()
                .HasOne(o => o.ViewValueGroupName)
                .WithMany(o => o.ViewValueDictinaryList)
                .HasForeignKey(f => f.ViewValueGroupNameId);






            // level 1

//invoiceBuy

            modelBuilder.Entity<InvoiceBuy>()
                .HasOne(o => o.Seller)
                .WithMany(m => m.InvoiceBuyList)
                .HasForeignKey(f => f.SellerId);

            modelBuilder.Entity<InvoiceBuy>()
                .HasOne(o => o.Currency)
                .WithMany(m => m.InvoiceBuyList);

            modelBuilder.Entity<InvoiceBuy>()
                .HasMany(m => m.InvoicePosList)
                .WithOne(o => o.InvoiceBuy)
                .HasForeignKey(f => f.InvoiceBuyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<InvoiceBuy>()
                .HasOne(o => o.InvoiceTotal)
                .WithOne(o => o.InvoiceBuy)
                .HasForeignKey<InvoiceTotal>(f => f.InvoiceBuyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<InvoiceBuy>()
                .HasOne(o => o.PaymentTerms)
                .WithOne(o => o.InvoiceBuy)
                .HasForeignKey<PaymentTerms>(f => f.InvoiceBuyId)
                .OnDelete(DeleteBehavior.ClientSetNull);


            modelBuilder.Entity<InvoiceBuy>()
                .HasOne(o => o.Load)
                .WithOne(o => o.InvoiceBuy);


            modelBuilder.Entity<InvoiceBuy>()
                .HasMany(m => m.RatesValuesList)
                .WithOne(o => o.InvoiceBuy)
                .HasForeignKey(f => f.InvoiceBuyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<InvoiceBuy>()
                .HasOne(o => o.Seller)
                .WithMany(m => m.InvoiceBuyList)
                .HasForeignKey(f => f.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull);


//invoiceSell

            modelBuilder.Entity<InvoiceSell>()
                .HasOne(o => o.Buyer)
                .WithMany(m => m.InvoiceSellBuyerList)
                .HasForeignKey(f => f.BuyerId)
                .OnDelete(DeleteBehavior.ClientSetNull);


            modelBuilder.Entity<InvoiceSell>()
                .HasOne(o => o.Currency)
                .WithMany(m => m.InvoiceSellList)
                .HasForeignKey(f => f.CurrencyId);

            modelBuilder.Entity<InvoiceSell>()
                .HasOne(o => o.ExtraInfo)
                .WithOne(o => o.InvoiceSell)
                .HasForeignKey<InvoiceExtraInfo>(f => f.InvoiceSellId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<InvoiceSell>()
                .HasMany(f => f.InvoicePosList)
                .WithOne(o => o.InvoiceSell)
                .HasForeignKey(f => f.InvoiceSellId);


            modelBuilder.Entity<InvoiceSell>()
                .HasOne(o => o.InvoiceTotal)
                .WithOne(o => o.InvoiceSell)
                .HasForeignKey<InvoiceTotal>(f => f.InvoiceSellId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<InvoiceSell>()
                .HasOne(o => o.Load)
                .WithOne(o => o.InvoiceSell);



            modelBuilder.Entity<InvoiceSell>()
                .HasOne(o => o.PaymentTerms)
                .WithOne(o => o.InvoiceSell)
                .HasForeignKey<PaymentTerms>(f => f.InvoiceSellId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<InvoiceSell>()
                .HasMany(m => m.RatesValuesList)
                .WithOne(o => o.InvoiceSell)
                .HasForeignKey(f => f.InvoiceSellId);

            modelBuilder.Entity<InvoiceSell>()
                .HasOne(o => o.Seller)
                .WithMany(m => m.InvoiceSellSellerlList)
                .HasForeignKey(f => f.SellerId);




            //load

            modelBuilder.Entity<Load>()
                .HasOne(o => o.LoadBuy)
                .WithOne(o => o.Load)
                .HasForeignKey<LoadBuy>(f => f.LoadId);

            modelBuilder.Entity<Load>()
                .HasOne(o => o.LoadSell)
                .WithOne(o => o.Load)
                .HasForeignKey<LoadSell>(f => f.LoadId);

            modelBuilder.Entity<Load>()
                .HasOne(o => o.InvoiceSell)
                .WithOne(o => o.Load)
                .HasForeignKey<InvoiceSell>(f => f.LoadId);


            //PaymentTerms

            modelBuilder.Entity<PaymentTerms>()
                .HasOne(o => o.InvoiceBuy)
                .WithOne(o => o.PaymentTerms)
                .HasForeignKey<InvoiceBuy>(f => f.PaymentTermsId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<PaymentTerms>()
                .HasOne(o => o.InvoiceSell)
                .WithOne(o => o.PaymentTerms)
                .HasForeignKey<InvoiceSell>(f => f.PaymentTermsId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<PaymentTerms>()
                .HasOne(o => o.PaymentTerm)
                .WithMany(m => m.PaymentTermsList)
                .HasForeignKey(f => f.PaymentTermId);

            modelBuilder.Entity<PaymentTerms>()
                .HasOne(o => o.TradeInfo)
                .WithOne(o => o.PaymentTerms);

            
            //currencyNbp
            modelBuilder.Entity<CurrencyNbp>()
                .HasOne(o => o.Currency)
                .WithMany(m => m.CurrencyNbpList)
                .HasForeignKey(f => f.CurrencyId);

            modelBuilder.Entity<CurrencyNbp>()
                .HasOne(o => o.TradeInfo)
                .WithOne(o => o.CurrencyNbp);


            modelBuilder.Entity<CurrencyNbp>()
                .HasOne(o => o.LoadTransEu)
                .WithOne(o => o.Price)
                .HasForeignKey<CurrencyNbp>(f => f.LoadTransEuId)
                .OnDelete(DeleteBehavior.ClientSetNull);



            //level 2

            modelBuilder.Entity<LoadBuy>()
                .HasOne<LoadInfo>(o => o.LoadInfo)
                .WithOne(o => o.LoadBuy)
                .HasForeignKey<LoadInfo>(f => f.LoadBuyId)
                .IsRequired();
            
            modelBuilder.Entity<LoadBuy>()
                .HasOne<TradeInfo>(o => o.BuyingInfo)
                .WithOne(o => o.LoadBuy)
                .HasForeignKey<TradeInfo>(t=>t.LoadBuyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<LoadBuy>()
                .HasMany(m => m.Routes)
                .WithOne(o => o.LoadBuy)
                .HasForeignKey(f => f.LoadBuyId);





            modelBuilder.Entity<LoadSell>()
                .HasOne(o => o.SellingInfo)
                .WithOne(o => o.LoadSell)
                .HasForeignKey<TradeInfo>(f => f.LoadSellId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<LoadSell>()
                .HasOne(o => o.Principal)
                .WithMany(o => o.LoadSellList)
                .HasForeignKey(f => f.PrincipalId);


            modelBuilder.Entity<LoadSell>()
                .HasMany(m => m.ContactPersonsList)
                .WithOne(o => o.LoadSell)
                .OnDelete(DeleteBehavior.ClientSetNull);

            
            modelBuilder.Entity<LoadSellContactPersons>()
                .HasKey(k => new { k.CompanyEmployeeId, k.LoadSellId});

            



            // level 3
            modelBuilder.Entity<LoadInfo>()
                .HasOne(o => o.ExtraInfo)
                .WithOne(o => o.LoadInfo)
                .HasForeignKey<LoadInfoExtra>(f => f.LoadInfoId)
                .IsRequired();


            modelBuilder.Entity<TradeInfo>()
                .HasOne(o => o.Company)
                .WithMany(m => m.TradeInfoList)
                .HasForeignKey(f => f.CompanyId);


            modelBuilder.Entity<TradeInfo>()
                .HasOne(o => o.PaymentTerms)
                .WithOne(o => o.TradeInfo)
                .HasForeignKey<PaymentTerms>(f => f.TradeInfoId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<TradeInfo>()
                .HasOne(o => o.CurrencyNbp)
                .WithOne(o => o.TradeInfo)
                .HasForeignKey<CurrencyNbp>(f => f.TradeInfoId)
                .OnDelete(DeleteBehavior.ClientSetNull);


            modelBuilder.Entity<LoadRoute>()
                .HasOne(o => o.Address)
                .WithOne(o => o.LoadRoute)
                .HasForeignKey<Address>(f => f.LoadRouteId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<LoadRoute>()
                .HasMany(m => m.Pallets)
                .WithOne(o => o.LoadRoute)
                .HasForeignKey(f => f.LoadRouteId)
                .IsRequired();




            // level 4
            modelBuilder.Entity<CurrencyNbp>()
                .HasOne(o => o.Currency)
                .WithMany(o => o.CurrencyNbpList)
                .HasForeignKey(f => f.CurrencyId);

            modelBuilder.Entity<PaymentTerms>()
                .HasOne(o => o.PaymentTerm)
                .WithMany(m => m.PaymentTermsList)
                .HasForeignKey(f => f.PaymentTermId);




            modelBuilder.Entity<LoadInfoExtra>()
                .HasOne(o => o.RequiredTruckBody)
                .WithMany(o => o.RequiredTruckBodyList)
                .HasForeignKey(f => f.RequiredTruckBodyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<LoadInfoExtra>()
                .HasOne(o => o.TypeOfLoad)
                .WithMany(o => o.TypeOfLoadList)
                .HasForeignKey(f => f.TypeOfLoadId)
                .OnDelete(DeleteBehavior.ClientSetNull);


            //modelBuilder.Entity<LoadInfoExtraViewValue>()
            //    .HasKey(k => new { k.LoadInfoExtraId, k.ViewValueDictionaryId });


        ///many to many -  two tables....
            modelBuilder.Entity<LoadInfoExtraWaysOfLoad>()
                .ToTable("LoadInfoExtraWaysOfLoad")
                .HasOne(o => o.LoadInfoExtra)
                .WithMany(m => m.RequiredWaysOfLoading)
                .HasForeignKey(f => f.LoadInfoExtraId);

            modelBuilder.Entity<ViewValueDictionary>()
                .HasMany(m => m.RequiredWaysOfLoading)
                .WithOne(o => o.ViewValueDictionary)
                .HasForeignKey(f => f.ViewValueDictionaryId);


            modelBuilder.Entity<LoadInfoExtraAddrClassess>()
                .ToTable("LoadInfoExtraAddrClassess")
                .HasOne(o => o.LoadInfoExtra)
                .WithMany(m => m.RequiredAddrClassess)
                .HasForeignKey(f => f.LoadInfoExtraId);

            modelBuilder.Entity<ViewValueDictionary>()
                .HasMany(m => m.RequiredAdrClasses)
                .WithOne(o => o.ViewValueDictionary)
                .HasForeignKey(f => f.ViewValueDictionaryId);



            //laodTransEu

            modelBuilder.Entity<LoadTransEu>()
                .HasOne(o => o.Price)
                .WithOne(o => o.LoadTransEu)
                .HasForeignKey<CurrencyNbp>(f => f.LoadTransEuId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<LoadTransEu>()
                .HasMany(m => m.ContactPersonsList)
                .WithOne(o => o.LoadTransEu)
                .HasForeignKey(f => f.LoadTransEuId);

            modelBuilder.Entity<LoadTransEu>()
                .HasOne(o => o.Load)
                .WithOne(o => o.LoadTransEu)
                .HasForeignKey<LoadTransEu>(f => f.LoadId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<LoadTransEu>()
                .HasOne(o => o.SellingCompany)
                .WithMany(m => m.LoadTransEuList);






            modelBuilder.Entity<LoadTransEuContactPerson>()
                .HasKey(k => new { k.CompanyEmployeeId, k.LoadTransEuId });


            modelBuilder.Entity<LoadTransEuContactPerson>()
                .HasOne(o => o.CompanyEmployee)
                .WithMany(m => m.LoadTransEuContactPersonsList);

            modelBuilder.Entity<LoadTransEuContactPerson>()
                .HasOne(o => o.LoadTransEu)
                .WithMany(m => m.ContactPersonsList);













            modelBuilder.Entity<CompanyEmployee>()
                .HasMany(m => m.LoadSellContactPersonsList)
                .WithOne(o => o.CompanyEmployee)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<CompanyEmployee>()
                .HasMany(m => m.LoadTransEuContactPersonsList)
                .WithOne(o => o.CompanyEmployee)
                .OnDelete(DeleteBehavior.ClientSetNull);








            //modelBuilder.Entity<InvoiceBuy>()
            //    .OwnsOne(o => o.Currency);

            //modelBuilder.Entity<InvoiceSell>()
            //    .OwnsOne(o => o.Currency);

            //modelBuilder.Entity<Load>()
            //    .HasOne(o => o.LoadSell)
            //    .WithOne(o => o.Load);



            //modelBuilder.Entity<LoadBuy>()
            //    .OwnsOne(o => o.LoadInfo, oi=> {
            //        oi.OwnsOne(i => i.ExtraInfo);
            //    })
            //    .OwnsOne(o => o.BuyingInfo, bio =>
            //    {
            //        bio.OwnsOne(o => o.CurrencyNbp, bio_nbp =>
            //        {
            //            bio_nbp.OwnsOne(c => c.Currency);
            //        });
            //        bio.OwnsOne(o => o.PaymentTerms);
            //    });

            //modelBuilder.Entity<LoadSell>()
            //    .OwnsOne(o => o.SellingInfo, bio =>
            //    {
            //        bio.OwnsOne(o => o.CurrencyNbp, bio_nbp =>
            //        {
            //            bio_nbp.OwnsOne(c => c.Currency);
            //        });
            //        bio.OwnsOne(o => o.PaymentTerms);
            //    });






            //modelBuilder.Entity<Load>()
            //    .OwnsOne(s => s.LoadSell, so => {
            //        so.OwnsOne(o => o.SellingInfo, bio =>
            //        {
            //            bio.OwnsOne(o => o.CurrencyNbp, bio_nbp =>
            //            {
            //                bio_nbp.OwnsOne(c => c.Currency);
            //            });
            //            bio.OwnsOne(o => o.PaymentTerms);
            //        });
            //    })
            //    .OwnsOne(b => b.LoadBuy)
            //        .OwnsOne(bi => bi.BuyingInfo, bio =>
            //        {
            //            bio.OwnsOne(o => o.CurrencyNbp, bio_nbp =>
            //            {
            //                bio_nbp.OwnsOne(c => c.Currency);
            //            });
            //            bio.OwnsOne(o => o.PaymentTerms);
            //        })
            //        .OwnsOne(li => li.LoadInfo, lio =>
            //        {
            //            lio.OwnsOne(ei => ei.ExtraInfo);
            //        });








            //modelBuilder.Entity<LoadRoute>()
            //    .OwnsOne(o => o.Geo);



            //modelBuilder.Entity<Load>()
            //    .OwnsOne(o => o.LoadSell)
            //    .OwnsOne(o => o.SellingInfo)
            //    .OwnsOne(o => o.PaymentTerms)
            //    .OwnsOne(o => o.PaymentTerm);





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
