using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace bp.ot.s.API.entities.migrations.dane
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Company",
                columns: table => new
                {
                    CompanyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Legal_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Native_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Short_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telephone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransId = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Vat_id = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Company", x => x.CompanyId);
                });

            migrationBuilder.CreateTable(
                name: "Currency",
                columns: table => new
                {
                    CurrencyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currency", x => x.CurrencyId);
                });

            migrationBuilder.CreateTable(
                name: "Load",
                columns: table => new
                {
                    LoadId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Info = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsInvoice = table.Column<bool>(type: "bit", nullable: false),
                    LoadNo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Load", x => x.LoadId);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTerm",
                columns: table => new
                {
                    PaymentTermId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDescription = table.Column<bool>(type: "bit", nullable: false),
                    IsPaymentDate = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTerm", x => x.PaymentTermId);
                });

            migrationBuilder.CreateTable(
                name: "ViewValueGroupName",
                columns: table => new
                {
                    ViewValueGroupNameId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewValueGroupName", x => x.ViewValueGroupNameId);
                });

            migrationBuilder.CreateTable(
                name: "BankAccount",
                columns: table => new
                {
                    BankAccountId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Account_no = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Swift = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccount", x => x.BankAccountId);
                    table.ForeignKey(
                        name: "FK_BankAccount_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyEmployee",
                columns: table => new
                {
                    CompanyEmployeeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Entitled = table.Column<bool>(type: "bit", nullable: false),
                    Family_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Given_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Hidden = table.Column<bool>(type: "bit", nullable: false),
                    Is_driver = table.Column<bool>(type: "bit", nullable: false),
                    Is_moderator = table.Column<bool>(type: "bit", nullable: false),
                    Telephone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Trans_id = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyEmployee", x => x.CompanyEmployeeId);
                    table.ForeignKey(
                        name: "FK_CompanyEmployee_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoadBuy",
                columns: table => new
                {
                    LoadBuyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    LoadId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadBuy", x => x.LoadBuyId);
                    table.ForeignKey(
                        name: "FK_LoadBuy_Load_LoadId",
                        column: x => x.LoadId,
                        principalTable: "Load",
                        principalColumn: "LoadId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoadSell",
                columns: table => new
                {
                    LoadSellId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    LoadId = table.Column<int>(type: "int", nullable: false),
                    PrincipalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadSell", x => x.LoadSellId);
                    table.ForeignKey(
                        name: "FK_LoadSell_Load_LoadId",
                        column: x => x.LoadId,
                        principalTable: "Load",
                        principalColumn: "LoadId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoadSell_Company_PrincipalId",
                        column: x => x.PrincipalId,
                        principalTable: "Company",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ViewValueDictionary",
                columns: table => new
                {
                    ViewValueDictionaryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ViewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ViewValueGroupNameId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewValueDictionary", x => x.ViewValueDictionaryId);
                    table.ForeignKey(
                        name: "FK_ViewValueDictionary_ViewValueGroupName_ViewValueGroupNameId",
                        column: x => x.ViewValueGroupNameId,
                        principalTable: "ViewValueGroupName",
                        principalColumn: "ViewValueGroupNameId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoadInfo",
                columns: table => new
                {
                    LoadInfoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoadBuyId = table.Column<int>(type: "int", nullable: false),
                    LoadHeight = table.Column<double>(type: "float", nullable: true),
                    LoadInfoExtraId = table.Column<int>(type: "int", nullable: false),
                    LoadLength = table.Column<double>(type: "float", nullable: true),
                    LoadVolume = table.Column<double>(type: "float", nullable: true),
                    LoadWeight = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadInfo", x => x.LoadInfoId);
                    table.ForeignKey(
                        name: "FK_LoadInfo_LoadBuy_LoadBuyId",
                        column: x => x.LoadBuyId,
                        principalTable: "LoadBuy",
                        principalColumn: "LoadBuyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoadRoute",
                columns: table => new
                {
                    LoadRouteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Info = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsLoadingType = table.Column<bool>(type: "bit", nullable: false),
                    LoadBuyId = table.Column<int>(type: "int", nullable: false),
                    LoadingDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadRoute", x => x.LoadRouteId);
                    table.ForeignKey(
                        name: "FK_LoadRoute_LoadBuy_LoadBuyId",
                        column: x => x.LoadBuyId,
                        principalTable: "LoadBuy",
                        principalColumn: "LoadBuyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoadSellContactsPersons",
                columns: table => new
                {
                    CompanyEmployeeId = table.Column<int>(type: "int", nullable: false),
                    LoadSellId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadSellContactsPersons", x => new { x.CompanyEmployeeId, x.LoadSellId });
                    table.ForeignKey(
                        name: "FK_LoadSellContactsPersons_CompanyEmployee_CompanyEmployeeId",
                        column: x => x.CompanyEmployeeId,
                        principalTable: "CompanyEmployee",
                        principalColumn: "CompanyEmployeeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoadSellContactsPersons_LoadSell_LoadSellId",
                        column: x => x.LoadSellId,
                        principalTable: "LoadSell",
                        principalColumn: "LoadSellId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoadTradeInfo",
                columns: table => new
                {
                    TradeInfoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    CurrencyNbpId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LoadBuyId = table.Column<int>(type: "int", nullable: true),
                    LoadSellId = table.Column<int>(type: "int", nullable: true),
                    PaymentTermsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadTradeInfo", x => x.TradeInfoId);
                    table.ForeignKey(
                        name: "FK_LoadTradeInfo_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoadTradeInfo_LoadBuy_LoadBuyId",
                        column: x => x.LoadBuyId,
                        principalTable: "LoadBuy",
                        principalColumn: "LoadBuyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoadTradeInfo_LoadSell_LoadSellId",
                        column: x => x.LoadSellId,
                        principalTable: "LoadSell",
                        principalColumn: "LoadSellId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoadInfoExtra",
                columns: table => new
                {
                    LoadInfoExtraId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsForClearence = table.Column<bool>(type: "bit", nullable: true),
                    IsLiftRequired = table.Column<bool>(type: "bit", nullable: true),
                    IsLtl = table.Column<bool>(type: "bit", nullable: true),
                    IsTirCableRequired = table.Column<bool>(type: "bit", nullable: true),
                    IsTrackingSystemRequired = table.Column<bool>(type: "bit", nullable: true),
                    IsTruckCraneRequired = table.Column<bool>(type: "bit", nullable: true),
                    LoadInfoId = table.Column<int>(type: "int", nullable: false),
                    RequiredTruckBodyId = table.Column<int>(type: "int", nullable: false),
                    TypeOfLoadId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadInfoExtra", x => x.LoadInfoExtraId);
                    table.ForeignKey(
                        name: "FK_LoadInfoExtra_LoadInfo_LoadInfoId",
                        column: x => x.LoadInfoId,
                        principalTable: "LoadInfo",
                        principalColumn: "LoadInfoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoadInfoExtra_ViewValueDictionary_RequiredTruckBodyId",
                        column: x => x.RequiredTruckBodyId,
                        principalTable: "ViewValueDictionary",
                        principalColumn: "ViewValueDictionaryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoadInfoExtra_ViewValueDictionary_TypeOfLoadId",
                        column: x => x.TypeOfLoadId,
                        principalTable: "ViewValueDictionary",
                        principalColumn: "ViewValueDictionaryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Address",
                columns: table => new
                {
                    AddressId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Address_type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    LoadRouteId = table.Column<int>(type: "int", nullable: true),
                    Locality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Postal_code = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    Street_address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Street_number = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.AddressId);
                    table.ForeignKey(
                        name: "FK_Address_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Address_LoadRoute_LoadRouteId",
                        column: x => x.LoadRouteId,
                        principalTable: "LoadRoute",
                        principalColumn: "LoadRouteId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoadRoutePallet",
                columns: table => new
                {
                    LoadRoutePalletId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    Dimmension = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Info = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsEuroType = table.Column<bool>(type: "bit", nullable: false),
                    IsExchangeable = table.Column<bool>(type: "bit", nullable: true),
                    IsStackable = table.Column<bool>(type: "bit", nullable: true),
                    LoadRouteId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadRoutePallet", x => x.LoadRoutePalletId);
                    table.ForeignKey(
                        name: "FK_LoadRoutePallet_LoadRoute_LoadRouteId",
                        column: x => x.LoadRouteId,
                        principalTable: "LoadRoute",
                        principalColumn: "LoadRouteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CurrencyNbp",
                columns: table => new
                {
                    CurrencyNbpId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    PlnValue = table.Column<double>(type: "float", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    Rate = table.Column<double>(type: "float", nullable: false),
                    RateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TradeInfoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyNbp", x => x.CurrencyNbpId);
                    table.ForeignKey(
                        name: "FK_CurrencyNbp_Currency_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currency",
                        principalColumn: "CurrencyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CurrencyNbp_LoadTradeInfo_TradeInfoId",
                        column: x => x.TradeInfoId,
                        principalTable: "LoadTradeInfo",
                        principalColumn: "TradeInfoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTerms",
                columns: table => new
                {
                    PaymentTermsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Day0 = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvoiceBuyId = table.Column<int>(type: "int", nullable: true),
                    InvoiceSellId = table.Column<int>(type: "int", nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentDays = table.Column<int>(type: "int", nullable: true),
                    PaymentDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentTermId = table.Column<int>(type: "int", nullable: false),
                    TradeInfoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTerms", x => x.PaymentTermsId);
                    table.ForeignKey(
                        name: "FK_PaymentTerms_PaymentTerm_PaymentTermId",
                        column: x => x.PaymentTermId,
                        principalTable: "PaymentTerm",
                        principalColumn: "PaymentTermId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentTerms_LoadTradeInfo_TradeInfoId",
                        column: x => x.TradeInfoId,
                        principalTable: "LoadTradeInfo",
                        principalColumn: "TradeInfoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoadInfoExtraAddrClassess",
                columns: table => new
                {
                    LoadInfoExtraAddrClassessId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    LoadInfoExtraId = table.Column<int>(type: "int", nullable: true),
                    ViewValueDictionaryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadInfoExtraAddrClassess", x => x.LoadInfoExtraAddrClassessId);
                    table.ForeignKey(
                        name: "FK_LoadInfoExtraAddrClassess_LoadInfoExtra_LoadInfoExtraId",
                        column: x => x.LoadInfoExtraId,
                        principalTable: "LoadInfoExtra",
                        principalColumn: "LoadInfoExtraId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoadInfoExtraAddrClassess_ViewValueDictionary_ViewValueDictionaryId",
                        column: x => x.ViewValueDictionaryId,
                        principalTable: "ViewValueDictionary",
                        principalColumn: "ViewValueDictionaryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoadInfoExtraWaysOfLoad",
                columns: table => new
                {
                    LoadInfoExtraWaysOfLoadId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    LoadInfoExtraId = table.Column<int>(type: "int", nullable: true),
                    ViewValueDictionaryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadInfoExtraWaysOfLoad", x => x.LoadInfoExtraWaysOfLoadId);
                    table.ForeignKey(
                        name: "FK_LoadInfoExtraWaysOfLoad_LoadInfoExtra_LoadInfoExtraId",
                        column: x => x.LoadInfoExtraId,
                        principalTable: "LoadInfoExtra",
                        principalColumn: "LoadInfoExtraId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoadInfoExtraWaysOfLoad_ViewValueDictionary_ViewValueDictionaryId",
                        column: x => x.ViewValueDictionaryId,
                        principalTable: "ViewValueDictionary",
                        principalColumn: "ViewValueDictionaryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceBuy",
                columns: table => new
                {
                    InvoiceBuyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    DateOfIssue = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Info = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceTotalId = table.Column<int>(type: "int", nullable: false),
                    PaymentTermsId = table.Column<int>(type: "int", nullable: false),
                    SellerId = table.Column<int>(type: "int", nullable: false),
                    SellingDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceBuy", x => x.InvoiceBuyId);
                    table.ForeignKey(
                        name: "FK_InvoiceBuy_Currency_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currency",
                        principalColumn: "CurrencyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceBuy_PaymentTerms_PaymentTermsId",
                        column: x => x.PaymentTermsId,
                        principalTable: "PaymentTerms",
                        principalColumn: "PaymentTermsId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceBuy_Company_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Company",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceSell",
                columns: table => new
                {
                    InvoiceSellId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BuyerId = table.Column<int>(type: "int", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    DateOfIssue = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Info = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceTotalId = table.Column<int>(type: "int", nullable: false),
                    PaymentTermsId = table.Column<int>(type: "int", nullable: false),
                    SellerId = table.Column<int>(type: "int", nullable: false),
                    SellingDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceSell", x => x.InvoiceSellId);
                    table.ForeignKey(
                        name: "FK_InvoiceSell_Company_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Company",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceSell_Currency_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currency",
                        principalColumn: "CurrencyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceSell_PaymentTerms_PaymentTermsId",
                        column: x => x.PaymentTermsId,
                        principalTable: "PaymentTerms",
                        principalColumn: "PaymentTermsId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceSell_Company_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Company",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceExtraInfo",
                columns: table => new
                {
                    InvoiceExtraInfoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    InvoiceSellId = table.Column<int>(type: "int", nullable: false),
                    IsLoadNo = table.Column<bool>(type: "bit", nullable: false),
                    IsTaxNbpExchanged = table.Column<bool>(type: "bit", nullable: false),
                    LoadNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxExchangedInfo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceExtraInfo", x => x.InvoiceExtraInfoId);
                    table.ForeignKey(
                        name: "FK_InvoiceExtraInfo_InvoiceSell_InvoiceSellId",
                        column: x => x.InvoiceSellId,
                        principalTable: "InvoiceSell",
                        principalColumn: "InvoiceSellId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvoicePos",
                columns: table => new
                {
                    InvoicePosId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BruttoValue = table.Column<double>(type: "float", nullable: false),
                    InvoiceBuyId = table.Column<int>(type: "int", nullable: true),
                    InvoiceSellId = table.Column<int>(type: "int", nullable: true),
                    MeasurementUnit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NettoValue = table.Column<double>(type: "float", nullable: false),
                    Pkwiu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<double>(type: "float", nullable: false),
                    VatRate = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    VatUnitValue = table.Column<double>(type: "float", nullable: false),
                    VatValue = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoicePos", x => x.InvoicePosId);
                    table.ForeignKey(
                        name: "FK_InvoicePos_InvoiceBuy_InvoiceBuyId",
                        column: x => x.InvoiceBuyId,
                        principalTable: "InvoiceBuy",
                        principalColumn: "InvoiceBuyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoicePos_InvoiceSell_InvoiceSellId",
                        column: x => x.InvoiceSellId,
                        principalTable: "InvoiceSell",
                        principalColumn: "InvoiceSellId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceRatesValues",
                columns: table => new
                {
                    RateValueId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BruttoValue = table.Column<double>(type: "float", nullable: false),
                    InvoiceBuyId = table.Column<int>(type: "int", nullable: true),
                    InvoiceSellId = table.Column<int>(type: "int", nullable: true),
                    NettoValue = table.Column<double>(type: "float", nullable: false),
                    VatRate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VatValue = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceRatesValues", x => x.RateValueId);
                    table.ForeignKey(
                        name: "FK_InvoiceRatesValues_InvoiceBuy_InvoiceBuyId",
                        column: x => x.InvoiceBuyId,
                        principalTable: "InvoiceBuy",
                        principalColumn: "InvoiceBuyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceRatesValues_InvoiceSell_InvoiceSellId",
                        column: x => x.InvoiceSellId,
                        principalTable: "InvoiceSell",
                        principalColumn: "InvoiceSellId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceTotal",
                columns: table => new
                {
                    InvoiceTotalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    InvoiceBuyId = table.Column<int>(type: "int", nullable: true),
                    InvoiceSellId = table.Column<int>(type: "int", nullable: true),
                    TotalBrutto = table.Column<double>(type: "float", nullable: false),
                    TotalNetto = table.Column<double>(type: "float", nullable: false),
                    TotalTax = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceTotal", x => x.InvoiceTotalId);
                    table.ForeignKey(
                        name: "FK_InvoiceTotal_InvoiceBuy_InvoiceBuyId",
                        column: x => x.InvoiceBuyId,
                        principalTable: "InvoiceBuy",
                        principalColumn: "InvoiceBuyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceTotal_InvoiceSell_InvoiceSellId",
                        column: x => x.InvoiceSellId,
                        principalTable: "InvoiceSell",
                        principalColumn: "InvoiceSellId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Address_CompanyId",
                table: "Address",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Address_LoadRouteId",
                table: "Address",
                column: "LoadRouteId",
                unique: true,
                filter: "[LoadRouteId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccount_CompanyId",
                table: "BankAccount",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyEmployee_CompanyId",
                table: "CompanyEmployee",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyNbp_CurrencyId",
                table: "CurrencyNbp",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyNbp_TradeInfoId",
                table: "CurrencyNbp",
                column: "TradeInfoId",
                unique: true,
                filter: "[TradeInfoId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceBuy_CurrencyId",
                table: "InvoiceBuy",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceBuy_PaymentTermsId",
                table: "InvoiceBuy",
                column: "PaymentTermsId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceBuy_SellerId",
                table: "InvoiceBuy",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceExtraInfo_InvoiceSellId",
                table: "InvoiceExtraInfo",
                column: "InvoiceSellId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePos_InvoiceBuyId",
                table: "InvoicePos",
                column: "InvoiceBuyId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePos_InvoiceSellId",
                table: "InvoicePos",
                column: "InvoiceSellId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRatesValues_InvoiceBuyId",
                table: "InvoiceRatesValues",
                column: "InvoiceBuyId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRatesValues_InvoiceSellId",
                table: "InvoiceRatesValues",
                column: "InvoiceSellId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceSell_BuyerId",
                table: "InvoiceSell",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceSell_CurrencyId",
                table: "InvoiceSell",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceSell_PaymentTermsId",
                table: "InvoiceSell",
                column: "PaymentTermsId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceSell_SellerId",
                table: "InvoiceSell",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceTotal_InvoiceBuyId",
                table: "InvoiceTotal",
                column: "InvoiceBuyId",
                unique: true,
                filter: "[InvoiceBuyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceTotal_InvoiceSellId",
                table: "InvoiceTotal",
                column: "InvoiceSellId",
                unique: true,
                filter: "[InvoiceSellId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LoadBuy_LoadId",
                table: "LoadBuy",
                column: "LoadId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoadInfo_LoadBuyId",
                table: "LoadInfo",
                column: "LoadBuyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoadInfoExtra_LoadInfoId",
                table: "LoadInfoExtra",
                column: "LoadInfoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoadInfoExtra_RequiredTruckBodyId",
                table: "LoadInfoExtra",
                column: "RequiredTruckBodyId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadInfoExtra_TypeOfLoadId",
                table: "LoadInfoExtra",
                column: "TypeOfLoadId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadInfoExtraAddrClassess_LoadInfoExtraId",
                table: "LoadInfoExtraAddrClassess",
                column: "LoadInfoExtraId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadInfoExtraAddrClassess_ViewValueDictionaryId",
                table: "LoadInfoExtraAddrClassess",
                column: "ViewValueDictionaryId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadInfoExtraWaysOfLoad_LoadInfoExtraId",
                table: "LoadInfoExtraWaysOfLoad",
                column: "LoadInfoExtraId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadInfoExtraWaysOfLoad_ViewValueDictionaryId",
                table: "LoadInfoExtraWaysOfLoad",
                column: "ViewValueDictionaryId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadRoute_LoadBuyId",
                table: "LoadRoute",
                column: "LoadBuyId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadRoutePallet_LoadRouteId",
                table: "LoadRoutePallet",
                column: "LoadRouteId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadSell_LoadId",
                table: "LoadSell",
                column: "LoadId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoadSell_PrincipalId",
                table: "LoadSell",
                column: "PrincipalId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadSellContactsPersons_LoadSellId",
                table: "LoadSellContactsPersons",
                column: "LoadSellId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadTradeInfo_CompanyId",
                table: "LoadTradeInfo",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadTradeInfo_LoadBuyId",
                table: "LoadTradeInfo",
                column: "LoadBuyId",
                unique: true,
                filter: "[LoadBuyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LoadTradeInfo_LoadSellId",
                table: "LoadTradeInfo",
                column: "LoadSellId",
                unique: true,
                filter: "[LoadSellId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTerms_PaymentTermId",
                table: "PaymentTerms",
                column: "PaymentTermId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTerms_TradeInfoId",
                table: "PaymentTerms",
                column: "TradeInfoId",
                unique: true,
                filter: "[TradeInfoId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ViewValueDictionary_ViewValueGroupNameId",
                table: "ViewValueDictionary",
                column: "ViewValueGroupNameId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Address");

            migrationBuilder.DropTable(
                name: "BankAccount");

            migrationBuilder.DropTable(
                name: "CurrencyNbp");

            migrationBuilder.DropTable(
                name: "InvoiceExtraInfo");

            migrationBuilder.DropTable(
                name: "InvoicePos");

            migrationBuilder.DropTable(
                name: "InvoiceRatesValues");

            migrationBuilder.DropTable(
                name: "InvoiceTotal");

            migrationBuilder.DropTable(
                name: "LoadInfoExtraAddrClassess");

            migrationBuilder.DropTable(
                name: "LoadInfoExtraWaysOfLoad");

            migrationBuilder.DropTable(
                name: "LoadRoutePallet");

            migrationBuilder.DropTable(
                name: "LoadSellContactsPersons");

            migrationBuilder.DropTable(
                name: "InvoiceBuy");

            migrationBuilder.DropTable(
                name: "InvoiceSell");

            migrationBuilder.DropTable(
                name: "LoadInfoExtra");

            migrationBuilder.DropTable(
                name: "LoadRoute");

            migrationBuilder.DropTable(
                name: "CompanyEmployee");

            migrationBuilder.DropTable(
                name: "Currency");

            migrationBuilder.DropTable(
                name: "PaymentTerms");

            migrationBuilder.DropTable(
                name: "LoadInfo");

            migrationBuilder.DropTable(
                name: "ViewValueDictionary");

            migrationBuilder.DropTable(
                name: "PaymentTerm");

            migrationBuilder.DropTable(
                name: "LoadTradeInfo");

            migrationBuilder.DropTable(
                name: "ViewValueGroupName");

            migrationBuilder.DropTable(
                name: "LoadBuy");

            migrationBuilder.DropTable(
                name: "LoadSell");

            migrationBuilder.DropTable(
                name: "Load");

            migrationBuilder.DropTable(
                name: "Company");
        }
    }
}
