using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IrmaDulce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPessoaQuantidadeParcelas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuantidadeParcelas",
                table: "Pessoas",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuantidadeParcelas",
                table: "Pessoas");
        }
    }
}
