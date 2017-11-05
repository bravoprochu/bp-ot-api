using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace bp.ot.s.API.entities.migrations.dane
{
    public partial class init2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceTotalId",
                table: "InvoiceSell");

            migrationBuilder.DropColumn(
                name: "IsLoadNo",
                table: "InvoiceExtraInfo");

            migrationBuilder.DropColumn(
                name: "IsTaxNbpExchanged",
                table: "InvoiceExtraInfo");

            migrationBuilder.DropColumn(
                name: "InvoiceTotalId",
                table: "InvoiceBuy");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InvoiceTotalId",
                table: "InvoiceSell",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsLoadNo",
                table: "InvoiceExtraInfo",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTaxNbpExchanged",
                table: "InvoiceExtraInfo",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceTotalId",
                table: "InvoiceBuy",
                nullable: false,
                defaultValue: 0);
        }
    }
}
