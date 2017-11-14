using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace bp.ot.s.API.Entities.Migrations.Dane
{
    public partial class LoadInfoRemoveForeignKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrencyNbpId",
                table: "LoadTradeInfo");

            migrationBuilder.DropColumn(
                name: "PaymentTermsId",
                table: "LoadTradeInfo");

            migrationBuilder.DropColumn(
                name: "LoadInfoExtraId",
                table: "LoadInfo");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrencyNbpId",
                table: "LoadTradeInfo",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PaymentTermsId",
                table: "LoadTradeInfo",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LoadInfoExtraId",
                table: "LoadInfo",
                nullable: false,
                defaultValue: 0);
        }
    }
}
