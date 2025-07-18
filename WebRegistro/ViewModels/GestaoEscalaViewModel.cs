using WebRegistro.Models;
using System;
using System.Collections.Generic;

namespace WebRegistro.ViewModels
{
    public class GestaoEscalaViewModel
    {
        public DateTime DataReferencia { get; set; }
        public List<User> Funcionarios { get; set; }
        // Dicionário para acesso rápido: <FuncionarioId, <Dia, Escala>>
        public Dictionary<string, Dictionary<int, Escala>> Escalas { get; set; }

        public int? Unidade { get; set; }
        public string? NomeUnidade { get; set; }
    }
}