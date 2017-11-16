using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace bp.ot.s.API.Entities.Migrations.Dane
{
    public partial class LoadInvoiceSell : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LoadId",
                table: "InvoiceSell",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceSell_LoadId",
                table: "InvoiceSell",
                column: "LoadId",
                unique: true,
                filter: "[LoadId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceSell_Load_LoadId",
                table: "InvoiceSell",
                column: "LoadId",
                principalTable: "Load",
                principalColumn: "LoadId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceSell_Load_LoadId",
                table: "InvoiceSell");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceSell_LoadId",
                table: "InvoiceSell");

            migrationBuilder.DropColumn(
                name: "LoadId",
                table: "InvoiceSell");
        }
    }
}
