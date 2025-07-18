using WebRegistro.Models;
using System;
using System.Collections.Generic;

namespace WebRegistro.ViewModels
{
    public class RhDashboardViewModel
    {
        public IEnumerable<User> Funcionarios { get; set; }
        public Dictionary<string, TimeSpan> SaldosIndividuais { get; set; } // Dicionário para guardar o saldo de cada um
        public int TotalFuncionarios { get; set; }
        public TimeSpan TotalHorasPositivas { get; set; }
        public TimeSpan TotalHorasNegativas { get; set; }
        public int FuncionariosComHorasExpirando { get; set; }
    }
}