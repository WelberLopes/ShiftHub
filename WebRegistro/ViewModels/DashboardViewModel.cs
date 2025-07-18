using System.Collections.Generic;
using WebRegistro.Models; // Supondo que seus modelos estão aqui

namespace WebRegistro.ViewModels
{
    public class DashboardViewModel
    {
        // Informações do Perfil do Funcionário
        public User Funcionario { get; set; }

        // KPIs (Key Performance Indicators) do Mês Atual
        public int DiasTrabalhados { get; set; }
        public int Faltas { get; set; }
        public int FaltasJustificadas { get; set; }
        public string TotalHorasTrabalhadas { get; set; } // Formatado como string (ex: "160:30")

        // Dados para o Gráfico de Horas
        // Lista de dias do mês (ex: "01/07", "02/07", ...)
        public List<string> LabelsGraficoHoras { get; set; }
        // Lista de horas trabalhadas por dia (em decimal, ex: 8.5 para 8h30)
        public List<double> DadosGraficoHoras { get; set; }
    }
}