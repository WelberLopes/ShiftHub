using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebRegistro.Migrations
{
    /// <inheritdoc />
    public partial class AlteracaoNsr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Nsr",
                table: "RegistrosPonto",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Contadores",
                columns: table => new
                {
                    NomeContador = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UltimoValor = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contadores", x => x.NomeContador);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contadores");

            migrationBuilder.DropColumn(
                name: "Nsr",
                table: "RegistrosPonto");
        }
    }
}
