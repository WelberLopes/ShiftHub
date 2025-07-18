using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace WebRegistro.Models
{
    public class BancoDeHoras
    {
        public enum TipoMovimentacaoHoras
        {
            Credito = 1,
            Debito = 2
        }

        [Key]
        public int Id { get; set; }
        // Identificador único do registro de horas
        [Required]
        public string UserId { get; set; }
        // Chave estrangeira para o usuário associado

        [ValidateNever]
        public User User { get; set; } // Navegação para o usuário associado
        public DateTime Data { get; set; } // Data do registro de horas

        [Required(ErrorMessage = "O tipo de movimentação é obrigatório.")]
        [Display(Name = "Tipo de Movimentação")]
        public TipoMovimentacaoHoras TipoMovimentacao { get; set; } // Horas trabalhadas no dia
        public TimeSpan Horas { get; set; } // Quantidade de horas registradas
        public string Descricao { get; set; } // Descrição do registro de horas

        [Display(Name = "Data de Registro")]
        public DateTime DataRegistro { get; set; } // Data em que o registro foi criado

        [Display(Name = "Data de Expiração")]
        public DateTime? DataExpiracao { get; set; }
    }
}
