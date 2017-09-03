using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace bp.ot.s.API.Entities.Migrations.Dane
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kontrahent",
                columns: table => new
                {
                    KontrahentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Imie = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    NIP = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    Nazwa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Nazwisko = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Skrot = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Uwagi = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kontrahent", x => x.KontrahentId);
                });

            migrationBuilder.CreateTable(
                name: "KontrahentAdres",
                columns: table => new
                {
                    KontrahentAdresId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    KodKraju = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    KodPocztowy = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    KontrahentRefId = table.Column<int>(type: "int", nullable: false),
                    Miejscowosc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TypAdresu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ulica = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UlicaNr = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KontrahentAdres", x => x.KontrahentAdresId);
                    table.ForeignKey(
                        name: "FK_KontrahentAdres_Kontrahent_KontrahentRefId",
                        column: x => x.KontrahentRefId,
                        principalTable: "Kontrahent",
                        principalColumn: "KontrahentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KontrahentBank",
                columns: table => new
                {
                    KontrahentBankId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    KontrahentRefId = table.Column<int>(type: "int", nullable: false),
                    NumerRachunku = table.Column<string>(type: "nvarchar(28)", maxLength: 28, nullable: true),
                    Swift = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    Uwagi = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KontrahentBank", x => x.KontrahentBankId);
                    table.ForeignKey(
                        name: "FK_KontrahentBank_Kontrahent_KontrahentRefId",
                        column: x => x.KontrahentRefId,
                        principalTable: "Kontrahent",
                        principalColumn: "KontrahentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KontrahentKontakt",
                columns: table => new
                {
                    KontrahentId = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Tel = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Tel2 = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    WWW = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KontrahentKontakt", x => x.KontrahentId);
                    table.ForeignKey(
                        name: "FK_KontrahentKontakt_Kontrahent_KontrahentId",
                        column: x => x.KontrahentId,
                        principalTable: "Kontrahent",
                        principalColumn: "KontrahentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KontrahentAdres_KontrahentRefId",
                table: "KontrahentAdres",
                column: "KontrahentRefId");

            migrationBuilder.CreateIndex(
                name: "IX_KontrahentBank_KontrahentRefId",
                table: "KontrahentBank",
                column: "KontrahentRefId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KontrahentAdres");

            migrationBuilder.DropTable(
                name: "KontrahentBank");

            migrationBuilder.DropTable(
                name: "KontrahentKontakt");

            migrationBuilder.DropTable(
                name: "Kontrahent");
        }
    }
}
