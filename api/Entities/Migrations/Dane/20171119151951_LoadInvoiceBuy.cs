using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace bp.ot.s.API.Entities.Migrations.Dane
{
    public partial class LoadInvoiceBuy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "InvoiceRecived",
                table: "InvoiceBuy",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LoadId",
                table: "InvoiceBuy",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceBuy_LoadId",
                table: "InvoiceBuy",
                column: "LoadId",
                unique: true,
                filter: "[LoadId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceBuy_Load_LoadId",
                table: "InvoiceBuy",
                column: "LoadId",
                principalTable: "Load",
                principalColumn: "LoadId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceBuy_Load_LoadId",
                table: "InvoiceBuy");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceBuy_LoadId",
                table: "InvoiceBuy");

            migrationBuilder.DropColumn(
                name: "InvoiceRecived",
                table: "InvoiceBuy");

            migrationBuilder.DropColumn(
                name: "LoadId",
                table: "InvoiceBuy");
        }
    }
}
