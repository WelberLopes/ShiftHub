using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebRegistro.Migrations
{
    /// <inheritdoc />
    public partial class EscalaUnidadeField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Unidade",
                table: "Escalas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Unidade",
                table: "Escalas");
        }
    }
}
