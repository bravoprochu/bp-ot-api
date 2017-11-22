using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace bp.ot.s.API.Entities.Migrations.Dane
{
    public partial class TransportOfferLoadIdRemove : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoadId",
                table: "TransportOffer");

            migrationBuilder.DropColumn(
                name: "UnloadId",
                table: "TransportOffer");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LoadId",
                table: "TransportOffer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UnloadId",
                table: "TransportOffer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
