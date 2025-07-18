using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebRegistro.Models
{
    public class Escala
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FuncionarioId { get; set; }

        [ForeignKey("FuncionarioId")]
        public virtual User Funcionario { get; set; }

        [Required]
        public DateTime Data { get; set; }

        public TimeSpan? HoraInicio { get; set; }

        public TimeSpan? HoraFim { get; set; }

        [Required]
        public string Tipo { get; set; } // "Trabalho" ou "Folga"

        public string? Turno { get; set; }

        public string? TipoExame { get; set; } // Novo: "Endoscopia" ou "Ph/ mano"
        public int Unidade { get; set; } // Novo: Unidade de Saúde onde o exame será realizado
    }
}
