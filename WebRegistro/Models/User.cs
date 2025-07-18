using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace WebRegistro.Models
{
    public class User
    {
        [Key]
        [Display(Name = "CPF")]
        public string Cpf { get; set; }

        public string NomeCompleto { get; set; } // Nome do usuário
        [EmailAddress]
        public string Email { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime DataAdmissao { get; set; } 

        [Required]
        public string PasswordHash { get; set; } // Armazena a senha hash

        [Required]
        [Display(Name = "Cargo")]
        public string Cargo { get; set; }

        [Required]
        [Display(Name = "Função")]
        public string Role { get; set; }
        public byte[]? BiometricTemplate { get; set; }

        [Required(ErrorMessage = "É obrigatório selecionar um departamento.")]
        [Display(Name = "Departamento")]
        public int DepartamentoId { get; set; } // Chave estrangeira para o departamento
        [ValidateNever]
        public Departamento Departamento { get; set; } // Navegação para o departamento
    }
}
