using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebRegistro.Migrations
{
    /// <inheritdoc />
    public partial class AttUserTableDepartamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departamentos_Users_ResponsavelCpf",
                table: "Departamentos");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Departamentos_DepartamentoId",
                table: "Users");

            migrationBuilder.AlterColumn<int>(
                name: "DepartamentoId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Departamentos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_Departamentos_Users_ResponsavelCpf",
                table: "Departamentos",
                column: "ResponsavelCpf",
                principalTable: "Users",
                principalColumn: "Cpf",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Departamentos_DepartamentoId",
                table: "Users",
                column: "DepartamentoId",
                principalTable: "Departamentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departamentos_Users_ResponsavelCpf",
                table: "Departamentos");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Departamentos_DepartamentoId",
                table: "Users");

            migrationBuilder.AlterColumn<int>(
                name: "DepartamentoId",
                table: "Users",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Departamentos",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddForeignKey(
                name: "FK_Departamentos_Users_ResponsavelCpf",
                table: "Departamentos",
                column: "ResponsavelCpf",
                principalTable: "Users",
                principalColumn: "Cpf",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Departamentos_DepartamentoId",
                table: "Users",
                column: "DepartamentoId",
                principalTable: "Departamentos",
                principalColumn: "Id");
        }
    }
}
