using System.ComponentModel.DataAnnotations;

namespace WebRegistro.Models;
public class Contador
{
    [Key]
    public string NomeContador { get; set; }
    public long UltimoValor { get; set; }
}