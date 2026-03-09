using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IrmaDulce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMultaJurosConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TurmaDisciplinas_Disciplinas_DisciplinaId1",
                table: "TurmaDisciplinas");

            migrationBuilder.DropForeignKey(
                name: "FK_TurmaDisciplinas_Turmas_TurmaId1",
                table: "TurmaDisciplinas");

            migrationBuilder.DropIndex(
                name: "IX_TurmaDisciplinas_DisciplinaId1",
                table: "TurmaDisciplinas");

            migrationBuilder.DropIndex(
                name: "IX_TurmaDisciplinas_TurmaId1",
                table: "TurmaDisciplinas");

            migrationBuilder.DropColumn(
                name: "DisciplinaId1",
                table: "TurmaDisciplinas");

            migrationBuilder.DropColumn(
                name: "TurmaId1",
                table: "TurmaDisciplinas");

            migrationBuilder.AddColumn<decimal>(
                name: "JurosMensalPercent",
                table: "ConfiguracoesEscolares",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MultaAtrasoPercent",
                table: "ConfiguracoesEscolares",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "ConfiguracoesEscolares",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "JurosMensalPercent", "MultaAtrasoPercent" },
                values: new object[] { 1.0m, 2.0m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JurosMensalPercent",
                table: "ConfiguracoesEscolares");

            migrationBuilder.DropColumn(
                name: "MultaAtrasoPercent",
                table: "ConfiguracoesEscolares");

            migrationBuilder.AddColumn<int>(
                name: "DisciplinaId1",
                table: "TurmaDisciplinas",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TurmaId1",
                table: "TurmaDisciplinas",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TurmaDisciplinas_DisciplinaId1",
                table: "TurmaDisciplinas",
                column: "DisciplinaId1");

            migrationBuilder.CreateIndex(
                name: "IX_TurmaDisciplinas_TurmaId1",
                table: "TurmaDisciplinas",
                column: "TurmaId1");

            migrationBuilder.AddForeignKey(
                name: "FK_TurmaDisciplinas_Disciplinas_DisciplinaId1",
                table: "TurmaDisciplinas",
                column: "DisciplinaId1",
                principalTable: "Disciplinas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TurmaDisciplinas_Turmas_TurmaId1",
                table: "TurmaDisciplinas",
                column: "TurmaId1",
                principalTable: "Turmas",
                principalColumn: "Id");
        }
    }
}
