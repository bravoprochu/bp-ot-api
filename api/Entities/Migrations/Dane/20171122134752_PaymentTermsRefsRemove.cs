using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace bp.ot.s.API.Entities.Migrations.Dane
{
    public partial class PaymentTermsRefsRemove : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceBuyId",
                table: "PaymentTerms");

            migrationBuilder.DropColumn(
                name: "InvoiceSellId",
                table: "PaymentTerms");

            migrationBuilder.DropColumn(
                name: "TransportOfferId",
                table: "PaymentTerms");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InvoiceBuyId",
                table: "PaymentTerms",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceSellId",
                table: "PaymentTerms",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransportOfferId",
                table: "PaymentTerms",
                nullable: true);
        }
    }
}
