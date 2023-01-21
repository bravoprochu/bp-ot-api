using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace bp.ot.s.API.Entities.Migrations.Dane
{
    public partial class invoicePosquantityasfloat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Quantity",
                table: "InvoicePos",
                type: "float",
                nullable: false,
                oldClrType: typeof(int));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "InvoicePos",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }
    }
}
