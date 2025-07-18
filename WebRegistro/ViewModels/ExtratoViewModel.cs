using WebRegistro.Models;
using System;
using System.Collections.Generic;

namespace WebRegistro.ViewModels
{
    public class ExtratoViewModel
    {
        public User Funcionario { get; set; }
        public TimeSpan SaldoAtual { get; set; }
        public IEnumerable<BancoDeHoras> Movimentacoes { get; set; }

        // Propriedades adicionadas para os totais
        public TimeSpan TotalCreditos { get; set; }
        public TimeSpan TotalDebitos { get; set; }
    }
}