using WebRegistro.Models;
using System;
using System.Collections.Generic;

namespace WebRegistro.ViewModels
{
    public class RelatorioMensalViewModel
    {
        public User Funcionario { get; set; }
        public DateTime Competencia { get; set; }
        public Dictionary<int, List<RegistroPonto>> RegistrosAgrupadosPorDia { get; set; }
        public TimeSpan TotalHorasTrabalhadas { get; set; }
    }
}
