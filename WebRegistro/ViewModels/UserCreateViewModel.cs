using WebRegistro.Models;

namespace WebRegistro.ViewModels
{
    public class UserCreateViewModel
    {
       public User Usuario { get; set; }
       public IEnumerable<Departamento> DepartamentosDisponiveis { get; set; }


        public UserCreateViewModel()
        {
            // Inicializa as propriedades para evitar erros de referência nula
            Usuario = new User();
            DepartamentosDisponiveis = new List<Departamento>();
        }
    }
}
