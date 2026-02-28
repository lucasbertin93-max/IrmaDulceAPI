using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IrmaDulce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTemplateTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TemplateTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TemplateDocumentoId = table.Column<int>(type: "INTEGER", nullable: false),
                    TagNoDocumento = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CampoSistema = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateTags_TemplatesDocumentos_TemplateDocumentoId",
                        column: x => x.TemplateDocumentoId,
                        principalTable: "TemplatesDocumentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TemplatesDocumentos_Tipo",
                table: "TemplatesDocumentos",
                column: "Tipo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TemplateTags_TemplateDocumentoId_TagNoDocumento",
                table: "TemplateTags",
                columns: new[] { "TemplateDocumentoId", "TagNoDocumento" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TemplateTags");

            migrationBuilder.DropIndex(
                name: "IX_TemplatesDocumentos_Tipo",
                table: "TemplatesDocumentos");
        }
    }
}
