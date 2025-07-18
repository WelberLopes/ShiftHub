using WebRegistro.Models;
using System.ComponentModel.DataAnnotations;
using static WebRegistro.Models.BancoDeHoras;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WebRegistro.ViewModels
{
    public class AdicionarMovimentacaoViewModel
    {
        [Required]
        public string UserId { get; set; }
        [ValidateNever]
        public string NomeFuncionario { get; set; }

        [Required(ErrorMessage = "O tipo da movimentação é obrigatório.")]
        public TipoMovimentacaoHoras TipoMovimentacao { get; set; }

        [Required(ErrorMessage = "A duração é obrigatória.")]      
        [Display(Name = "Duração (formato HH:mm)")]
        public string DuracaoInput { get; set; } // Usamos string para facilitar o input do usuário

        [Required(ErrorMessage = "A data da ocorrência é obrigatória.")]
        [DataType(DataType.Date)]
        public DateTime DataRegistro { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "A origem/justificativa é obrigatória.")]
        [MaxLength(200)]
        public string Descricao { get; set; }
    }
}