namespace WebRegistro.ViewModels
{
    public class UserViewModel
    {
        public string Cpf { get; set; } // CPF do usuário
        public string NomeCompleto { get; set; } // Nome completo do usuário
        public string Email { get; set; } // Email do usuário
        public DateTime DataAdmissao { get; set; } // Data de admissão do usuário
        public string Cargo { get; set; } // Cargo do usuário
        public string Role { get; set; } // Função/Perfil do usuário
    }
}
