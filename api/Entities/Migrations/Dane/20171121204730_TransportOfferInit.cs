using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace bp.ot.s.API.Entities.Migrations.Dane
{
    public partial class TransportOfferInit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TransportOfferId",
                table: "InvoiceSell",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransportOfferId",
                table: "CurrencyNbp",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransportOfferId1",
                table: "CurrencyNbp",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TransportOfferAddress",
                columns: table => new
                {
                    TransportOfferAddressId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Locality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransportOfferAddress", x => x.TransportOfferAddressId);
                });

            migrationBuilder.CreateTable(
                name: "TransportOffer",
                columns: table => new
                {
                    TransportOfferId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    CurrencyNbpId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Info = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceSellId = table.Column<int>(type: "int", nullable: true),
                    LoadId = table.Column<int>(type: "int", nullable: false),
                    OfferNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentTermsId = table.Column<int>(type: "int", nullable: false),
                    UnloadId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransportOffer", x => x.TransportOfferId);
                    table.ForeignKey(
                        name: "FK_TransportOffer_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransportOffer_InvoiceSell_InvoiceSellId",
                        column: x => x.InvoiceSellId,
                        principalTable: "InvoiceSell",
                        principalColumn: "InvoiceSellId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransportOffer_TransportOfferAddress_LoadId",
                        column: x => x.LoadId,
                        principalTable: "TransportOfferAddress",
                        principalColumn: "TransportOfferAddressId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransportOffer_PaymentTerms_PaymentTermsId",
                        column: x => x.PaymentTermsId,
                        principalTable: "PaymentTerms",
                        principalColumn: "PaymentTermsId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransportOffer_TransportOfferAddress_UnloadId",
                        column: x => x.UnloadId,
                        principalTable: "TransportOfferAddress",
                        principalColumn: "TransportOfferAddressId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyNbp_TransportOfferId",
                table: "CurrencyNbp",
                column: "TransportOfferId",
                unique: true,
                filter: "[TransportOfferId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyNbp_TransportOfferId1",
                table: "CurrencyNbp",
                column: "TransportOfferId1");

            migrationBuilder.CreateIndex(
                name: "IX_TransportOffer_CompanyId",
                table: "TransportOffer",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_TransportOffer_InvoiceSellId",
                table: "TransportOffer",
                column: "InvoiceSellId",
                unique: true,
                filter: "[InvoiceSellId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TransportOffer_LoadId",
                table: "TransportOffer",
                column: "LoadId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransportOffer_PaymentTermsId",
                table: "TransportOffer",
                column: "PaymentTermsId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransportOffer_UnloadId",
                table: "TransportOffer",
                column: "UnloadId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CurrencyNbp_TransportOffer_TransportOfferId",
                table: "CurrencyNbp",
                column: "TransportOfferId",
                principalTable: "TransportOffer",
                principalColumn: "TransportOfferId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CurrencyNbp_TransportOffer_TransportOfferId1",
                table: "CurrencyNbp",
                column: "TransportOfferId1",
                principalTable: "TransportOffer",
                principalColumn: "TransportOfferId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CurrencyNbp_TransportOffer_TransportOfferId",
                table: "CurrencyNbp");

            migrationBuilder.DropForeignKey(
                name: "FK_CurrencyNbp_TransportOffer_TransportOfferId1",
                table: "CurrencyNbp");

            migrationBuilder.DropTable(
                name: "TransportOffer");

            migrationBuilder.DropTable(
                name: "TransportOfferAddress");

            migrationBuilder.DropIndex(
                name: "IX_CurrencyNbp_TransportOfferId",
                table: "CurrencyNbp");

            migrationBuilder.DropIndex(
                name: "IX_CurrencyNbp_TransportOfferId1",
                table: "CurrencyNbp");

            migrationBuilder.DropColumn(
                name: "TransportOfferId",
                table: "InvoiceSell");

            migrationBuilder.DropColumn(
                name: "TransportOfferId",
                table: "CurrencyNbp");

            migrationBuilder.DropColumn(
                name: "TransportOfferId1",
                table: "CurrencyNbp");
        }
    }
}
