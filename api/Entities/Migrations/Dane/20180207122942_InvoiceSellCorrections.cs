using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace bp.ot.s.API.Entities.Migrations.Dane
{
    public partial class InvoiceSellCorrections : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BaseInvoiceId",
                table: "InvoiceSell",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCorrection",
                table: "InvoiceSell",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsInactive",
                table: "InvoiceSell",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsInactive",
                table: "InvoicePos",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseInvoiceId",
                table: "InvoiceSell");

            migrationBuilder.DropColumn(
                name: "IsCorrection",
                table: "InvoiceSell");

            migrationBuilder.DropColumn(
                name: "IsInactive",
                table: "InvoiceSell");

            migrationBuilder.DropColumn(
                name: "IsInactive",
                table: "InvoicePos");
        }
    }
}
