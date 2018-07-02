using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace bp.ot.s.API.entities.migrations.dane
{
    public partial class currencyNbpDateRateNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "RateDate",
                table: "CurrencyNbp",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "RateDate",
                table: "CurrencyNbp",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
