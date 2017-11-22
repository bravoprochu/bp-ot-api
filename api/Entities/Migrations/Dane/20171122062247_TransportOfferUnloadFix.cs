using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace bp.ot.s.API.Entities.Migrations.Dane
{
    public partial class TransportOfferUnloadFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransportOffer_TransportOfferAddress_UnloadId",
                table: "TransportOffer");

            migrationBuilder.DropIndex(
                name: "IX_TransportOffer_UnloadId",
                table: "TransportOffer");

            migrationBuilder.CreateIndex(
                name: "IX_TransportOfferAddress_UnloadId",
                table: "TransportOfferAddress",
                column: "UnloadId",
                unique: true,
                filter: "[UnloadId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_TransportOfferAddress_TransportOffer_UnloadId",
                table: "TransportOfferAddress",
                column: "UnloadId",
                principalTable: "TransportOffer",
                principalColumn: "TransportOfferId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransportOfferAddress_TransportOffer_UnloadId",
                table: "TransportOfferAddress");

            migrationBuilder.DropIndex(
                name: "IX_TransportOfferAddress_UnloadId",
                table: "TransportOfferAddress");

            migrationBuilder.CreateIndex(
                name: "IX_TransportOffer_UnloadId",
                table: "TransportOffer",
                column: "UnloadId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TransportOffer_TransportOfferAddress_UnloadId",
                table: "TransportOffer",
                column: "UnloadId",
                principalTable: "TransportOfferAddress",
                principalColumn: "TransportOfferAddressId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
