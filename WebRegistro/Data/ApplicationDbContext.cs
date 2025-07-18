using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebRegistro.Models;

namespace WebRegistro.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Minhas Tabelas
        public DbSet<RegistroPonto> RegistrosPonto { get; set; }
        public DbSet<Escala> Escalas { get; set; } 
        public DbSet<User> Users { get; set; }
        public DbSet<Contador> Contadores { get; set; } 
        public DbSet<RegistroLog> RegistrosLog { get; set; }
        public DbSet<Departamento> Departamentos { get; set; }
        public DbSet<BancoDeHoras> BancoDeHoras { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Configuração da Relação 1: Funcionários do Departamento ---
            // Diz que um Departamento tem muitos Funcionarios (User) e que
            // cada User tem um Departamento, usando a chave estrangeira DepartamentoId em User.
            modelBuilder.Entity<Departamento>()
                .HasMany(d => d.Funcionarios)
                .WithOne(u => u.Departamento)
                .HasForeignKey(u => u.DepartamentoId)
                .OnDelete(DeleteBehavior.Restrict); // Impede que um departamento seja deletado se tiver funcionários

            // --- Configuração da Relação 2: Responsável pelo Departamento ---
            // Diz que um Departamento tem um Responsavel (User). A outra ponta da relação
            // não tem uma propriedade de navegação em User, então usamos .WithMany() vazio.
            // A chave estrangeira é ResponsavelCpf na tabela Departamentos.
            modelBuilder.Entity<Departamento>()
                .HasOne(d => d.Responsavel)
                .WithMany() // Um User pode ser responsável por muitos departamentos
                .HasForeignKey(d => d.ResponsavelCpf)
                .OnDelete(DeleteBehavior.Restrict); // Impede que um usuário seja deletado se for responsável por um depto

            modelBuilder.Entity<BancoDeHoras>(entity =>
            {
                // Esta linha diz ao EF para converter a propriedade 'Duracao' (ou 'Horas')
                // do tipo TimeSpan para um 'long' (BIGINT no SQL) ao salvar no banco.
                entity.Property(b => b.Horas) // ou b.Horas, dependendo do nome na sua classe
                      .HasConversion<long>();
            });
        }

    }
}
