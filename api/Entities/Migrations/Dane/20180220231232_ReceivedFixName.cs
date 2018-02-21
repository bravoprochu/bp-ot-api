using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace bp.ot.s.API.Entities.Migrations.Dane
{
    public partial class ReceivedFixName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceReciveDate",
                table: "InvoiceBuy");

            migrationBuilder.DropColumn(
                name: "InvoiceRecived",
                table: "InvoiceBuy");

            migrationBuilder.AddColumn<bool>(
                name: "InvoiceReceived",
                table: "InvoiceBuy",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "InvoiceReceivedDate",
                table: "InvoiceBuy",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceReceived",
                table: "InvoiceBuy");

            migrationBuilder.DropColumn(
                name: "InvoiceReceivedDate",
                table: "InvoiceBuy");

            migrationBuilder.AddColumn<DateTime>(
                name: "InvoiceReciveDate",
                table: "InvoiceBuy",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "InvoiceRecived",
                table: "InvoiceBuy",
                nullable: false,
                defaultValue: false);
        }
    }
}
