using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace bp.ot.s.API.Entities.Migrations.Dane
{
    public partial class InvoiceSellBuyTransportLoad_creationInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TransportOffer",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDateTime",
                table: "TransportOffer",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ModifyBy",
                table: "TransportOffer",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifyDateTime",
                table: "TransportOffer",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Load",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDateTime",
                table: "Load",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ModifyBy",
                table: "Load",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifyDateTime",
                table: "Load",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "InvoiceSell",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDateTime",
                table: "InvoiceSell",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ModifyBy",
                table: "InvoiceSell",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifyDateTime",
                table: "InvoiceSell",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "InvoiceBuy",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDateTime",
                table: "InvoiceBuy",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ModifyBy",
                table: "InvoiceBuy",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifyDateTime",
                table: "InvoiceBuy",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TransportOffer");

            migrationBuilder.DropColumn(
                name: "CreatedDateTime",
                table: "TransportOffer");

            migrationBuilder.DropColumn(
                name: "ModifyBy",
                table: "TransportOffer");

            migrationBuilder.DropColumn(
                name: "ModifyDateTime",
                table: "TransportOffer");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Load");

            migrationBuilder.DropColumn(
                name: "CreatedDateTime",
                table: "Load");

            migrationBuilder.DropColumn(
                name: "ModifyBy",
                table: "Load");

            migrationBuilder.DropColumn(
                name: "ModifyDateTime",
                table: "Load");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "InvoiceSell");

            migrationBuilder.DropColumn(
                name: "CreatedDateTime",
                table: "InvoiceSell");

            migrationBuilder.DropColumn(
                name: "ModifyBy",
                table: "InvoiceSell");

            migrationBuilder.DropColumn(
                name: "ModifyDateTime",
                table: "InvoiceSell");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "InvoiceBuy");

            migrationBuilder.DropColumn(
                name: "CreatedDateTime",
                table: "InvoiceBuy");

            migrationBuilder.DropColumn(
                name: "ModifyBy",
                table: "InvoiceBuy");

            migrationBuilder.DropColumn(
                name: "ModifyDateTime",
                table: "InvoiceBuy");
        }
    }
}
