using WebRegistro.Models;

namespace WebRegistro.ViewModels
{
    public class DepartamentoDetailsViewModel
    {
        // O objeto Departamento que será exibido na página de detalhes
        public Departamento Departamento { get; set; }
        // Lista de funcionários associados ao departamento
        public IEnumerable<User> Funcionarios { get; set; }
        public int qtdFuncionario { get; set; } // Quantidade de funcionários no departamento   

    }
}
