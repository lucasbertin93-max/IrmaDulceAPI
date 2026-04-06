using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IrmaDulce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCronogramaEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TipoDescontoPontualidade",
                table: "Mensalidades",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEstagio",
                table: "Disciplinas",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Ordem",
                table: "DisciplinaCursos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsEstagio",
                table: "CronogramaAulas",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DisponibilidadesDocentes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DocenteId = table.Column<int>(type: "INTEGER", nullable: false),
                    DiaSemana = table.Column<int>(type: "INTEGER", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    HoraFim = table.Column<TimeSpan>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisponibilidadesDocentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DisponibilidadesDocentes_Pessoas_DocenteId",
                        column: x => x.DocenteId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TurmaDiasLetivos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TurmaId = table.Column<int>(type: "INTEGER", nullable: false),
                    DiaSemana = table.Column<int>(type: "INTEGER", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    HoraFim = table.Column<TimeSpan>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TurmaDiasLetivos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TurmaDiasLetivos_Turmas_TurmaId",
                        column: x => x.TurmaId,
                        principalTable: "Turmas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DisponibilidadesDocentes_DocenteId",
                table: "DisponibilidadesDocentes",
                column: "DocenteId");

            migrationBuilder.CreateIndex(
                name: "IX_TurmaDiasLetivos_TurmaId_DiaSemana",
                table: "TurmaDiasLetivos",
                columns: new[] { "TurmaId", "DiaSemana" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DisponibilidadesDocentes");

            migrationBuilder.DropTable(
                name: "TurmaDiasLetivos");

            migrationBuilder.DropColumn(
                name: "IsEstagio",
                table: "Disciplinas");

            migrationBuilder.DropColumn(
                name: "Ordem",
                table: "DisciplinaCursos");

            migrationBuilder.DropColumn(
                name: "IsEstagio",
                table: "CronogramaAulas");

            migrationBuilder.AlterColumn<int>(
                name: "TipoDescontoPontualidade",
                table: "Mensalidades",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
