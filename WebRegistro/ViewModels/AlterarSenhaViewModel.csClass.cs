using System.ComponentModel.DataAnnotations;

namespace WebRegistro.ViewModels
{
    public class AlterarSenhaViewModel
    {
        [Required(ErrorMessage = "O campo Nova Senha é obrigatório.")]
        [StringLength(100, ErrorMessage = "A {0} deve ter no mínimo {2} e no máximo {1} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NovaSenha { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Senha")]
        [Compare("NovaSenha", ErrorMessage = "A nova senha e a confirmação não coincidem.")]
        public string ConfirmacaoSenha { get; set; }
    }
}