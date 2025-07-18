using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebRegistro.Models
{
    public class RegistroLog
    {
        [Key]
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
        public long? NsrAntigo { get; set; } // Número Sequencial de Registro
        public long? Nsr { get; set; } // Novo campo para o NSR atualizado
        public string? Motivo { get; set; } // Ação realizada (ex: "Criado", "Atualizado", "Excluído")

    }
}
