using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace bp.ot.s.API.Entities.Migrations.Dane
{
    public partial class TransportOfferAddressFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CurrencyNbp_TransportOffer_TransportOfferId",
                table: "CurrencyNbp");

            migrationBuilder.DropForeignKey(
                name: "FK_CurrencyNbp_TransportOffer_TransportOfferId1",
                table: "CurrencyNbp");

            migrationBuilder.DropForeignKey(
                name: "FK_TransportOffer_TransportOfferAddress_LoadId",
                table: "TransportOffer");

            migrationBuilder.DropIndex(
                name: "IX_TransportOffer_LoadId",
                table: "TransportOffer");

            migrationBuilder.DropIndex(
                name: "IX_CurrencyNbp_TransportOfferId",
                table: "CurrencyNbp");

            migrationBuilder.DropIndex(
                name: "IX_CurrencyNbp_TransportOfferId1",
                table: "CurrencyNbp");

            migrationBuilder.DropColumn(
                name: "TransportOfferId1",
                table: "CurrencyNbp");

            migrationBuilder.AddColumn<int>(
                name: "LoadId",
                table: "TransportOfferAddress",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnloadId",
                table: "TransportOfferAddress",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransportOfferId",
                table: "PaymentTerms",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransportOfferAddress_LoadId",
                table: "TransportOfferAddress",
                column: "LoadId",
                unique: true,
                filter: "[LoadId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TransportOffer_CurrencyNbpId",
                table: "TransportOffer",
                column: "CurrencyNbpId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TransportOffer_CurrencyNbp_CurrencyNbpId",
                table: "TransportOffer",
                column: "CurrencyNbpId",
                principalTable: "CurrencyNbp",
                principalColumn: "CurrencyNbpId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransportOfferAddress_TransportOffer_LoadId",
                table: "TransportOfferAddress",
                column: "LoadId",
                principalTable: "TransportOffer",
                principalColumn: "TransportOfferId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransportOffer_CurrencyNbp_CurrencyNbpId",
                table: "TransportOffer");

            migrationBuilder.DropForeignKey(
                name: "FK_TransportOfferAddress_TransportOffer_LoadId",
                table: "TransportOfferAddress");

            migrationBuilder.DropIndex(
                name: "IX_TransportOfferAddress_LoadId",
                table: "TransportOfferAddress");

            migrationBuilder.DropIndex(
                name: "IX_TransportOffer_CurrencyNbpId",
                table: "TransportOffer");

            migrationBuilder.DropColumn(
                name: "LoadId",
                table: "TransportOfferAddress");

            migrationBuilder.DropColumn(
                name: "UnloadId",
                table: "TransportOfferAddress");

            migrationBuilder.DropColumn(
                name: "TransportOfferId",
                table: "PaymentTerms");

            migrationBuilder.AddColumn<int>(
                name: "TransportOfferId1",
                table: "CurrencyNbp",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransportOffer_LoadId",
                table: "TransportOffer",
                column: "LoadId",
                unique: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_TransportOffer_TransportOfferAddress_LoadId",
                table: "TransportOffer",
                column: "LoadId",
                principalTable: "TransportOfferAddress",
                principalColumn: "TransportOfferAddressId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
