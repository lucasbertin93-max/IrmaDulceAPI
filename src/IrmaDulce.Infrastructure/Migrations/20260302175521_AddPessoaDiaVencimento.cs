using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IrmaDulce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPessoaDiaVencimento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiaVencimento",
                table: "Pessoas",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiaVencimento",
                table: "Pessoas");
        }
    }
}
