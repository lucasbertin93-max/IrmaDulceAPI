using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IrmaDulce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigPrazoParcelas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrazoMaximoParcelamento",
                table: "ConfiguracoesEscolares",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "ConfiguracoesEscolares",
                keyColumn: "Id",
                keyValue: 1,
                column: "PrazoMaximoParcelamento",
                value: 26);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrazoMaximoParcelamento",
                table: "ConfiguracoesEscolares");
        }
    }
}
