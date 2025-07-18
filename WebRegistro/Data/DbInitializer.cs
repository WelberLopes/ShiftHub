using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebRegistro.Models;
using WebRegistro.Repository;
using WebRegistro.Repository.Interfaces; // Certifique-se de que o namespace do seu modelo de usuário está correto

namespace WebRegistro.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            // Pega os serviços necessários para gerenciar usuários e roles

            // --- CRIAÇÃO DAS ROLES (FUNÇÕES) ---
            // Nomes das roles que sua aplicação usará
            var userRepository = serviceProvider.GetRequiredService<IUserRepository>();
            string[] roleNames = { "Admin", "Funcionario" };


            // --- CRIAÇÃO DO USUÁRIO ADMIN ---
            // Defina aqui o email e a senha padrão para o seu usuário Admin
            string adminEmail = "admin@shifthub.com";
            string adminPassword = "Password@123"; // Use uma senha forte!

            // Verifica se o usuário Admin já existe
      
                User adminUser = new User
                {
                    Cpf = "00000000000",
                    Email = adminEmail,
                    NomeCompleto = "Administrador do Sistema",
                    Cargo = "Admin",
                    Role = "Admin",
                    DataAdmissao = DateTime.Now,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword) // Hash da senha
                };

                // Cria o usuário no banco com a senha definida
                UserRepository.ReferenceEquals(userRepository.Create(adminUser), true);
            // Se a criação do usuário for bem-sucedida, atribui a ele a role "Admin"

        }
        }
    
}