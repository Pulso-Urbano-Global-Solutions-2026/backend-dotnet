using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PulsoUrbano.Net.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "SEQ_ALERTA_HISTORICO",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "SEQ_ZONA_REFERENCIA",
                incrementBy: 10);

            migrationBuilder.CreateTable(
                name: "ZONA_REFERENCIA_NET",
                columns: table => new
                {
                    ID_ZONA = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    NOME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    MUNICIPIO = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZONA_REFERENCIA_NET", x => x.ID_ZONA);
                });

            migrationBuilder.CreateTable(
                name: "ALERTA_HISTORICO",
                columns: table => new
                {
                    ID_ALERTA = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ID_ZONA = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    NIVEL_ALERTA = table.Column<string>(type: "NVARCHAR2(15)", maxLength: 15, nullable: false),
                    SCORE_REGISTRADO = table.Column<decimal>(type: "NUMBER(5,2)", nullable: false),
                    NO2_REGISTRADO = table.Column<decimal>(type: "NUMBER(8,4)", nullable: false),
                    TEXTO_RECOMENDACAO = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: false),
                    DT_ALERTA = table.Column<DateTime>(type: "DATE", nullable: false),
                    CONFIRMADO = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ALERTA_HISTORICO", x => x.ID_ALERTA);
                    table.ForeignKey(
                        name: "FK_ALERTA_HISTORICO_ZONA_REFERENCIA_NET_ID_ZONA",
                        column: x => x.ID_ZONA,
                        principalTable: "ZONA_REFERENCIA_NET",
                        principalColumn: "ID_ZONA",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ALERTA_ZONA_DT",
                table: "ALERTA_HISTORICO",
                columns: new[] { "ID_ZONA", "DT_ALERTA" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ALERTA_HISTORICO");

            migrationBuilder.DropTable(
                name: "ZONA_REFERENCIA_NET");

            migrationBuilder.DropSequence(
                name: "SEQ_ALERTA_HISTORICO");

            migrationBuilder.DropSequence(
                name: "SEQ_ZONA_REFERENCIA");
        }
    }
}
