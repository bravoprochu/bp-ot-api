using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace bp.ot.s.API.Entities.Migrations.Dane
{
    public partial class InvoiceExtraInfoCmr : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInvoice",
                table: "Load");

            migrationBuilder.AddColumn<string>(
                name: "CmrName",
                table: "InvoiceExtraInfo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CmrRecived",
                table: "InvoiceExtraInfo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CmrRecivedDate",
                table: "InvoiceExtraInfo",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InvoiceRecivedDate",
                table: "InvoiceExtraInfo",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "InvoiceSent",
                table: "InvoiceExtraInfo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceSentNo",
                table: "InvoiceExtraInfo",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CmrName",
                table: "InvoiceExtraInfo");

            migrationBuilder.DropColumn(
                name: "CmrRecived",
                table: "InvoiceExtraInfo");

            migrationBuilder.DropColumn(
                name: "CmrRecivedDate",
                table: "InvoiceExtraInfo");

            migrationBuilder.DropColumn(
                name: "InvoiceRecivedDate",
                table: "InvoiceExtraInfo");

            migrationBuilder.DropColumn(
                name: "InvoiceSent",
                table: "InvoiceExtraInfo");

            migrationBuilder.DropColumn(
                name: "InvoiceSentNo",
                table: "InvoiceExtraInfo");

            migrationBuilder.AddColumn<bool>(
                name: "IsInvoice",
                table: "Load",
                nullable: false,
                defaultValue: false);
        }
    }
}
