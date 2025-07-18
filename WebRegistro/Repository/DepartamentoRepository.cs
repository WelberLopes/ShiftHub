using Microsoft.EntityFrameworkCore;
using WebRegistro.Data;
using WebRegistro.Models;
using WebRegistro.Repository.Interfaces;

namespace WebRegistro.Repository
{
    public class DepartamentoRepository : IDepartamentoRepository
    {
        private readonly ApplicationDbContext _context;

        public DepartamentoRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddDepartamentoAsync(Departamento departamento)
        {
            if (departamento == null)
            {
                throw new ArgumentNullException(nameof(departamento), "Departamento não pode ser nulo.");
               
            }
            var lista = _context.Departamentos.Where(r=> r.Nome == departamento.Nome);
            if (lista.Any())
            {
                throw new ArgumentNullException(nameof(departamento), $"Já existe um Departamento chamado {departamento.Nome}." );

            }
                _context.Departamentos.Add(departamento);
                _context.SaveChangesAsync();

        }

        public Task DeleteDepartamentoAsync(int id)
        {
            var departamento = _context.Departamentos.Find(id);
            if (departamento == null)
            {
                throw new KeyNotFoundException($"Departamento com ID {id} não encontrado.");
            }
            _context.Departamentos.Remove(departamento);
            return _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Departamento>> GetAllDepartamentosAsync()
        {
            return await _context.Departamentos
                                 .Include(d => d.Responsavel) // "d" representa um Departamento
                                 .ToListAsync();
        }

        public Task<Departamento> GetDepartamentoByIdAsync(int id)
        {
            var departamento = _context.Departamentos
                .Include(d => d.Funcionarios)
                .Include(d => d.Responsavel)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (departamento == null)
            {
                throw new KeyNotFoundException($"Departamento com ID {id} não encontrado.");
            }
            return departamento;
        }

        async Task <IEnumerable<User>> IDepartamentoRepository.GetFuncionariosByDepartamentoIdAsync(int departamentoId)
        {
            var funcionarios = _context.Departamentos
                .Where(d => d.Id == departamentoId)
                .SelectMany(d => d.Funcionarios);
           
            return funcionarios;
        }

        public Task<User> GetResponsavelByDepartamentoIdAsync(int departamentoId)
        {
            var responsavel = _context.Departamentos
                .Where(d => d.Id == departamentoId)
                .Select(d => d.ResponsavelCpf)
                .FirstOrDefaultAsync();
            if (responsavel == null)
            {
                throw new KeyNotFoundException($"Responsável não encontrado para o departamento com ID {departamentoId}.");
            }
            return null; // responsavel;
        }

        public Task UpdateDepartamentoAsync(Departamento departamento)
        {
            if (departamento == null)
            {
                throw new ArgumentNullException(nameof(departamento), "Departamento não pode ser nulo.");
            }
            var existingDepartamento = _context.Departamentos.Find(departamento.Id);
            if (existingDepartamento == null)
            {
                throw new KeyNotFoundException($"Departamento com ID {departamento.Id} não encontrado.");
            }
            _context.Entry(existingDepartamento).CurrentValues.SetValues(departamento);
            return _context.SaveChangesAsync();
        }
    }
}
