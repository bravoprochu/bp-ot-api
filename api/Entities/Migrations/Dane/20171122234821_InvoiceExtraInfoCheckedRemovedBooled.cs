using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace bp.ot.s.API.Entities.Migrations.Dane
{
    public partial class InvoiceExtraInfoCheckedRemovedBooled : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "InvoiceExtraInfoChecked",
                columns: table => new
                {
                    InvoiceExtraInfoCheckedId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Checked = table.Column<bool>(type: "bit", nullable: true),
                    CmrCheckedId = table.Column<int>(type: "int", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Info = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecivedCheckedId = table.Column<int>(type: "int", nullable: true),
                    SentCheckedId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceExtraInfoChecked", x => x.InvoiceExtraInfoCheckedId);
                    table.ForeignKey(
                        name: "FK_InvoiceExtraInfoChecked_InvoiceExtraInfo_CmrCheckedId",
                        column: x => x.CmrCheckedId,
                        principalTable: "InvoiceExtraInfo",
                        principalColumn: "InvoiceExtraInfoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceExtraInfoChecked_InvoiceExtraInfo_RecivedCheckedId",
                        column: x => x.RecivedCheckedId,
                        principalTable: "InvoiceExtraInfo",
                        principalColumn: "InvoiceExtraInfoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceExtraInfoChecked_InvoiceExtraInfo_SentCheckedId",
                        column: x => x.SentCheckedId,
                        principalTable: "InvoiceExtraInfo",
                        principalColumn: "InvoiceExtraInfoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceExtraInfoChecked_CmrCheckedId",
                table: "InvoiceExtraInfoChecked",
                column: "CmrCheckedId",
                unique: true,
                filter: "[CmrCheckedId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceExtraInfoChecked_RecivedCheckedId",
                table: "InvoiceExtraInfoChecked",
                column: "RecivedCheckedId",
                unique: true,
                filter: "[RecivedCheckedId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceExtraInfoChecked_SentCheckedId",
                table: "InvoiceExtraInfoChecked",
                column: "SentCheckedId",
                unique: true,
                filter: "[SentCheckedId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceExtraInfoChecked");

            migrationBuilder.AddColumn<string>(
                name: "CmrName",
                table: "InvoiceExtraInfo",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CmrRecived",
                table: "InvoiceExtraInfo",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CmrRecivedDate",
                table: "InvoiceExtraInfo",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InvoiceRecivedDate",
                table: "InvoiceExtraInfo",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "InvoiceSent",
                table: "InvoiceExtraInfo",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceSentNo",
                table: "InvoiceExtraInfo",
                nullable: true);
        }
    }
}
