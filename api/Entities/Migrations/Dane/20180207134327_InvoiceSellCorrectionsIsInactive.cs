using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace bp.ot.s.API.Entities.Migrations.Dane
{
    public partial class InvoiceSellCorrectionsIsInactive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInactive",
                table: "InvoiceTotal",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsInactive",
                table: "InvoiceRatesValues",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CorrectionInfo",
                table: "InvoicePos",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInactive",
                table: "InvoiceTotal");

            migrationBuilder.DropColumn(
                name: "IsInactive",
                table: "InvoiceRatesValues");

            migrationBuilder.DropColumn(
                name: "CorrectionInfo",
                table: "InvoicePos");
        }
    }
}
