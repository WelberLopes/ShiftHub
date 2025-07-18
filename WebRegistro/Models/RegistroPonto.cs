using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebRegistro.Models
{
    public class RegistroPonto
    {[Key]
        public int Id { get; set; }

        [Required]
        public DateTime Horario { get; set; }

        [Required]
        public string Tipo { get; set; }

        public TimeSpan? HorasTrabalhadasDia { get; set; }

        public string? Justificativa { get; set; } // Novo campo

        [Required]
        public string UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual User Usuario { get; set; }
        public long? Nsr { get; set; } // Número Sequencial de Registro
        
    }
}
