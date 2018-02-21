using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace bp.ot.s.API.Entities.Migrations.Dane
{
    public partial class InvoiceCommon_InvoiceBuy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceBuy_Company_SellerId",
                table: "InvoiceBuy");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceBuy_SellerId",
                table: "InvoiceBuy");

            migrationBuilder.DropColumn(
                name: "SellerId",
                table: "InvoiceBuy");

            migrationBuilder.AddColumn<int>(
                name: "CompanySellerId",
                table: "InvoiceBuy",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceBuy_CompanySellerId",
                table: "InvoiceBuy",
                column: "CompanySellerId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceBuy_Company_CompanySellerId",
                table: "InvoiceBuy",
                column: "CompanySellerId",
                principalTable: "Company",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceBuy_Company_CompanySellerId",
                table: "InvoiceBuy");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceBuy_CompanySellerId",
                table: "InvoiceBuy");

            migrationBuilder.DropColumn(
                name: "CompanySellerId",
                table: "InvoiceBuy");

            migrationBuilder.AddColumn<int>(
                name: "SellerId",
                table: "InvoiceBuy",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceBuy_SellerId",
                table: "InvoiceBuy",
                column: "SellerId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceBuy_Company_SellerId",
                table: "InvoiceBuy",
                column: "SellerId",
                principalTable: "Company",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
