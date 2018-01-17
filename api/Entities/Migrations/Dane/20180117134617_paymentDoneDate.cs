using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace bp.ot.s.API.Entities.Migrations.Dane
{
    public partial class paymentDoneDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDate",
                table: "InvoiceSell",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PaymentIsDone",
                table: "InvoiceSell",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDate",
                table: "InvoiceBuy",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PaymentIsDone",
                table: "InvoiceBuy",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "InvoiceSell");

            migrationBuilder.DropColumn(
                name: "PaymentIsDone",
                table: "InvoiceSell");

            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "InvoiceBuy");

            migrationBuilder.DropColumn(
                name: "PaymentIsDone",
                table: "InvoiceBuy");
        }
    }
}
