using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebRegistro.Migrations
{
    /// <inheritdoc />
    public partial class DuracaoConversao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Adiciona uma coluna temporária para armazenar os Ticks
            migrationBuilder.AddColumn<long>(
                name: "Duracao_temp",
                table: "BancoDeHoras",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            // Converte os valores de TIME para Ticks (BIGINT) e popula a nova coluna
            // A fórmula converte o tempo em nanossegundos e divide por 100 (pois 1 tick = 100 nanossegundos)
            migrationBuilder.Sql(
                "UPDATE [BancoDeHoras] SET [Duracao_temp] = DATEDIFF_BIG(NANOSECOND, '00:00:00', [Horas]) / 100"); // Use [Duracao] se o nome da sua coluna for Duracao

            // Remove a coluna antiga do tipo TIME
            migrationBuilder.DropColumn(
                name: "Horas", // Use "Duracao" se o nome da sua coluna for Duracao
                table: "BancoDeHoras");

            // Renomeia a coluna temporária para o nome original
            migrationBuilder.RenameColumn(
                name: "Duracao_temp",
                table: "BancoDeHoras",
                newName: "Horas"); // Use "Duracao" se o nome da sua coluna for Duracao
        }
    }
}
