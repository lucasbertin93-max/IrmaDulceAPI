using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IrmaDulce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDocenteToTurmaDisciplina : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisciplinaId1",
                table: "TurmaDisciplinas",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocenteId",
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
                name: "IX_TurmaDisciplinas_DocenteId",
                table: "TurmaDisciplinas",
                column: "DocenteId");

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
                name: "FK_TurmaDisciplinas_Pessoas_DocenteId",
                table: "TurmaDisciplinas",
                column: "DocenteId",
                principalTable: "Pessoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TurmaDisciplinas_Turmas_TurmaId1",
                table: "TurmaDisciplinas",
                column: "TurmaId1",
                principalTable: "Turmas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TurmaDisciplinas_Disciplinas_DisciplinaId1",
                table: "TurmaDisciplinas");

            migrationBuilder.DropForeignKey(
                name: "FK_TurmaDisciplinas_Pessoas_DocenteId",
                table: "TurmaDisciplinas");

            migrationBuilder.DropForeignKey(
                name: "FK_TurmaDisciplinas_Turmas_TurmaId1",
                table: "TurmaDisciplinas");

            migrationBuilder.DropIndex(
                name: "IX_TurmaDisciplinas_DisciplinaId1",
                table: "TurmaDisciplinas");

            migrationBuilder.DropIndex(
                name: "IX_TurmaDisciplinas_DocenteId",
                table: "TurmaDisciplinas");

            migrationBuilder.DropIndex(
                name: "IX_TurmaDisciplinas_TurmaId1",
                table: "TurmaDisciplinas");

            migrationBuilder.DropColumn(
                name: "DisciplinaId1",
                table: "TurmaDisciplinas");

            migrationBuilder.DropColumn(
                name: "DocenteId",
                table: "TurmaDisciplinas");

            migrationBuilder.DropColumn(
                name: "TurmaId1",
                table: "TurmaDisciplinas");
        }
    }
}
