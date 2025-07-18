using WebRegistro.Models;

namespace WebRegistro.ViewModels;
public class ResumoMensalRhViewModel
{
    public User Funcionario { get; set; }
    public DateTime Competencia { get; set; }

    // KPIs para os cards
    public TimeSpan TotalHorasTrabalhadas { get; set; }
    public TimeSpan TotalHorasExtra { get; set; }
    public int DiasTrabalhados { get; set; }
    public int TotalFaltas { get; set; }
    public int TotalAtrasos { get; set; }

    // Lista para o detalhamento diário
    public List<DetalheDiaViewModel> DetalhesDiarios { get; set; }
}

// ViewModel para cada linha do detalhamento diário
public class DetalheDiaViewModel
{
    public DateTime Dia { get; set; }
    public string Status { get; set; } // Ex: "Trabalhado", "Falta", "Fim de Semana", "Falta Justificada"
    public TimeSpan HorasTrabalhadas { get; set; }
    public TimeSpan HorasExtra { get; set; }
    public TimeSpan TempoAtraso { get; set; }
    public string Justificativa { get; set; }
    public List<RegistroPonto> RegistrosDoDia { get; set; }
}