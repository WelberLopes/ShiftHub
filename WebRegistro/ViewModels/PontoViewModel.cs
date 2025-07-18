using WebRegistro.Models;
using System.Collections.Generic;

namespace WebRegistro.ViewModels
{
    public class PontoViewModel
    {
        public string NomeUsuario { get; set; }
        public string NomeCompleto { get; set; }
        public List<RegistroPonto> RegistrosDeHoje { get; set; }
    }
}