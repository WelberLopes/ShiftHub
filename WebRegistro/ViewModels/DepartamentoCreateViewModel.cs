using WebRegistro.Models;
using System.Collections.Generic;

namespace WebRegistro.ViewModels
{
    public class DepartamentoCreateViewModel
    {
        // O objeto Departamento que será preenchido pelo formulário
        public Departamento Departamento { get; set; }

        // A lista de usuários disponíveis para popular o dropdown
        public IEnumerable<User> UsuariosDisponiveis { get; set; }

        public DepartamentoCreateViewModel()
        {
            // Inicializa as propriedades para evitar erros de referência nula
            Departamento = new Departamento();
            UsuariosDisponiveis = new List<User>();
        }
    }
}