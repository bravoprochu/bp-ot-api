using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace bp.ot.s.API.entities.migrations.dane
{
    public partial class TransEu : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LoadTransEuId",
                table: "CurrencyNbp",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LoadTransEu",
                columns: table => new
                {
                    LoadTransEuId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    LoadId = table.Column<int>(type: "int", nullable: false),
                    SellingCompanyId = table.Column<int>(type: "int", nullable: false),
                    TransEuId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadTransEu", x => x.LoadTransEuId);
                    table.ForeignKey(
                        name: "FK_LoadTransEu_Load_LoadId",
                        column: x => x.LoadId,
                        principalTable: "Load",
                        principalColumn: "LoadId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoadTransEu_Company_SellingCompanyId",
                        column: x => x.SellingCompanyId,
                        principalTable: "Company",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoadTransEuContactsPersons",
                columns: table => new
                {
                    CompanyEmployeeId = table.Column<int>(type: "int", nullable: false),
                    LoadTransEuId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadTransEuContactsPersons", x => new { x.CompanyEmployeeId, x.LoadTransEuId });
                    table.ForeignKey(
                        name: "FK_LoadTransEuContactsPersons_CompanyEmployee_CompanyEmployeeId",
                        column: x => x.CompanyEmployeeId,
                        principalTable: "CompanyEmployee",
                        principalColumn: "CompanyEmployeeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoadTransEuContactsPersons_LoadTransEu_LoadTransEuId",
                        column: x => x.LoadTransEuId,
                        principalTable: "LoadTransEu",
                        principalColumn: "LoadTransEuId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyNbp_LoadTransEuId",
                table: "CurrencyNbp",
                column: "LoadTransEuId",
                unique: true,
                filter: "[LoadTransEuId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LoadTransEu_LoadId",
                table: "LoadTransEu",
                column: "LoadId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoadTransEu_SellingCompanyId",
                table: "LoadTransEu",
                column: "SellingCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadTransEuContactsPersons_LoadTransEuId",
                table: "LoadTransEuContactsPersons",
                column: "LoadTransEuId");

            migrationBuilder.AddForeignKey(
                name: "FK_CurrencyNbp_LoadTransEu_LoadTransEuId",
                table: "CurrencyNbp",
                column: "LoadTransEuId",
                principalTable: "LoadTransEu",
                principalColumn: "LoadTransEuId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CurrencyNbp_LoadTransEu_LoadTransEuId",
                table: "CurrencyNbp");

            migrationBuilder.DropTable(
                name: "LoadTransEuContactsPersons");

            migrationBuilder.DropTable(
                name: "LoadTransEu");

            migrationBuilder.DropIndex(
                name: "IX_CurrencyNbp_LoadTransEuId",
                table: "CurrencyNbp");

            migrationBuilder.DropColumn(
                name: "LoadTransEuId",
                table: "CurrencyNbp");
        }
    }
}
