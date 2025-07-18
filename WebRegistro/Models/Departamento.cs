using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WebRegistro.Models
{
    public class Departamento
    {
        [Key]
        public int Id { get; set; }

        // Adicione estes atributos
        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O Nome não pode ter mais de 100 caracteres.")]
        [Display(Name = "Nome do Departamento")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O campo Descrição é obrigatório.")]
        [Display(Name = "Descrição")]
        public string Descricao { get; set; }

        // Relacionamento com Funcionarios
        public ICollection<User>? Funcionarios { get; set; } = new List<User>();

        // Responsável pelo departamento
        [Required(ErrorMessage = "É obrigatório selecionar um Responsável.")]
        [Display(Name = "CPF do Responsável")]
        public string ResponsavelCpf { get; set; }

        [ValidateNever]
        public User Responsavel { get; set; }
    }
}