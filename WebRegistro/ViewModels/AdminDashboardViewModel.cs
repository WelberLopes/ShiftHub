using WebRegistro.Models;
using System.Collections.Generic;

namespace WebRegistro.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalFuncionarios { get; set; }
        public int FuncionariosPresentes { get; set; }
        public int FuncionariosFaltas { get; set; }
        public int FuncionariosAtrasados { get; set; }
        public int FuncionariosJustificativas { get; set; }
        public List<RegistroPonto> UltimosRegistros { get; set; }
        public List<User> ListaFuncionarios { get; set; }
        public List<FuncionarioRelatorioViewModel> ListaPresentes { get; set; }
        public List<FuncionarioRelatorioViewModel> ListaAtrasados { get; set; }
        public List<FuncionarioRelatorioViewModel> ListaAusentes { get; set; }
        public List<FuncionarioRelatorioViewModel> ListaJustificadas { get; set; }
        public IEnumerable<Departamento> Departamentos { get; set; } // Lista de departamentos para o filtro
        public int? DepartamentoSelecionadoId { get; set; }


    }
    public class FuncionarioRelatorioViewModel
    {
        public string Cpf { get; set; }
        public string NomeCompleto { get; set; }
        public string Cargo { get; set; }
        public TimeSpan TempoAtraso { get; set; } // Apenas para a lista de atrasados
        public List<RegistroPontoViewModel> RegistrosDoDia { get; set; }
    }

    public class RegistroPontoViewModel
    {
        public string Tipo { get; set; }
        public DateTime Horario { get; set; }
        public string? Justificativa { get; set; } // Novo campo para justificativa
    }

}