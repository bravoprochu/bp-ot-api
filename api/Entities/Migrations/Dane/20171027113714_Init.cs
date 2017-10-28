using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace bp.ot.s.API.entities.Migrations.dane
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Comapny",
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
                    table.PrimaryKey("PK_Comapny", x => x.CompanyId);
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
                name: "Address",
                columns: table => new
                {
                    AddressId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Address_type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyRefId = table.Column<int>(type: "int", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    Locality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Postal_code = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    Street_address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Street_number = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.AddressId);
                    table.ForeignKey(
                        name: "FK_Address_Comapny_CompanyRefId",
                        column: x => x.CompanyRefId,
                        principalTable: "Comapny",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BankAccount",
                columns: table => new
                {
                    BankAccountId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Account_no = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    Swift = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccount", x => x.BankAccountId);
                    table.ForeignKey(
                        name: "FK_BankAccount_Comapny_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Comapny",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanyEmployee",
                columns: table => new
                {
                    CompanyEmployeeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CompanyRefId = table.Column<int>(type: "int", nullable: false),
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
                        name: "FK_CompanyEmployee_Comapny_CompanyRefId",
                        column: x => x.CompanyRefId,
                        principalTable: "Comapny",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTerms",
                columns: table => new
                {
                    PaymentTermsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Day0 = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentDays = table.Column<int>(type: "int", nullable: true),
                    PaymentTermId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTerms", x => x.PaymentTermsId);
                    table.ForeignKey(
                        name: "FK_PaymentTerms_PaymentTerm_PaymentTermId",
                        column: x => x.PaymentTermId,
                        principalTable: "PaymentTerm",
                        principalColumn: "PaymentTermId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceSell",
                columns: table => new
                {
                    InvoiceSellId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BuyerCompanyId = table.Column<int>(type: "int", nullable: true),
                    CurrencyId = table.Column<int>(type: "int", nullable: true),
                    DateOfIssue = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExtraInfo_IsInWords = table.Column<bool>(type: "bit", nullable: false),
                    ExtraInfo_IsLoadNo = table.Column<bool>(type: "bit", nullable: false),
                    ExtraInfo_IsTaxNbpExchanged = table.Column<bool>(type: "bit", nullable: false),
                    ExtraInfo_LoadNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtraInfo_TaxExchangedInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtraInfo_TotalBruttoInWords = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Info = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentTermsId = table.Column<int>(type: "int", nullable: true),
                    SellerCompanyId = table.Column<int>(type: "int", nullable: true),
                    SellingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalBrutto = table.Column<double>(type: "float", nullable: false),
                    TotalNetto = table.Column<double>(type: "float", nullable: false),
                    TotalTax = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceSell", x => x.InvoiceSellId);
                    table.ForeignKey(
                        name: "FK_InvoiceSell_Comapny_BuyerCompanyId",
                        column: x => x.BuyerCompanyId,
                        principalTable: "Comapny",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceSell_Currency_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currency",
                        principalColumn: "CurrencyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceSell_PaymentTerms_PaymentTermsId",
                        column: x => x.PaymentTermsId,
                        principalTable: "PaymentTerms",
                        principalColumn: "PaymentTermsId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceSell_Comapny_SellerCompanyId",
                        column: x => x.SellerCompanyId,
                        principalTable: "Comapny",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoicePos",
                columns: table => new
                {
                    InvoicePosId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BruttoValue = table.Column<double>(type: "float", nullable: false),
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
                    InvoiceSellId = table.Column<int>(type: "int", nullable: true),
                    NettoValue = table.Column<double>(type: "float", nullable: false),
                    VatRate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VatValue = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceRatesValues", x => x.RateValueId);
                    table.ForeignKey(
                        name: "FK_InvoiceRatesValues_InvoiceSell_InvoiceSellId",
                        column: x => x.InvoiceSellId,
                        principalTable: "InvoiceSell",
                        principalColumn: "InvoiceSellId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Address_CompanyRefId",
                table: "Address",
                column: "CompanyRefId");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccount_CompanyId",
                table: "BankAccount",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyEmployee_CompanyRefId",
                table: "CompanyEmployee",
                column: "CompanyRefId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePos_InvoiceSellId",
                table: "InvoicePos",
                column: "InvoiceSellId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceRatesValues_InvoiceSellId",
                table: "InvoiceRatesValues",
                column: "InvoiceSellId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceSell_BuyerCompanyId",
                table: "InvoiceSell",
                column: "BuyerCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceSell_CurrencyId",
                table: "InvoiceSell",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceSell_PaymentTermsId",
                table: "InvoiceSell",
                column: "PaymentTermsId",
                unique: true,
                filter: "[PaymentTermsId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceSell_SellerCompanyId",
                table: "InvoiceSell",
                column: "SellerCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTerms_PaymentTermId",
                table: "PaymentTerms",
                column: "PaymentTermId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Address");

            migrationBuilder.DropTable(
                name: "BankAccount");

            migrationBuilder.DropTable(
                name: "CompanyEmployee");

            migrationBuilder.DropTable(
                name: "InvoicePos");

            migrationBuilder.DropTable(
                name: "InvoiceRatesValues");

            migrationBuilder.DropTable(
                name: "InvoiceSell");

            migrationBuilder.DropTable(
                name: "Comapny");

            migrationBuilder.DropTable(
                name: "Currency");

            migrationBuilder.DropTable(
                name: "PaymentTerms");

            migrationBuilder.DropTable(
                name: "PaymentTerm");
        }
    }
}
