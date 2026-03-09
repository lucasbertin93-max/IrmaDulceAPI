using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IrmaDulce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDescontoPontualidade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DescontoPontualidade",
                table: "Mensalidades",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoDescontoPontualidade",
                table: "Mensalidades",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescontoPontualidade",
                table: "Mensalidades");

            migrationBuilder.DropColumn(
                name: "TipoDescontoPontualidade",
                table: "Mensalidades");
        }
    }
}
