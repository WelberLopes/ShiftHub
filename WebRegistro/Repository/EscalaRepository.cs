using Microsoft.EntityFrameworkCore;
using WebRegistro.Data;
using WebRegistro.Models;
using WebRegistro.Repository.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebRegistro.Repository
{
    public class EscalaRepository : IEscalaRepository
    {
        private readonly ApplicationDbContext _context;

        public EscalaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Escala>> GetEscalasDoMesAsync(int ano, int mes)
        {
            return await _context.Escalas
                .Where(e => e.Data.Year == ano && e.Data.Month == mes)
                .ToListAsync();
        }

        public async Task<Escala?> GetEscalaAsync(int escalaId)
        {
            return await _context.Escalas.FindAsync(escalaId);
        }

        public async Task<Escala?> GetEscalaPorDataEFuncionarioAsync(string funcionarioId, DateTime data)
        {
            return await _context.Escalas
                .FirstOrDefaultAsync(e => e.FuncionarioId == funcionarioId && e.Data.Date == data.Date);
        }

        public async Task AdicionarOuAtualizarEscalaAsync(Escala escala)
        {
            var escalaExistente = await GetEscalaPorDataEFuncionarioAsync(escala.FuncionarioId, escala.Data);

            if (escalaExistente != null)
            {
                escalaExistente.TipoExame = escala.TipoExame;
                escalaExistente.HoraInicio = escala.HoraInicio;
                escalaExistente.HoraFim = escala.HoraFim;
                escalaExistente.Turno = escala.Turno;
                _context.Escalas.Update(escalaExistente);
            }
            else
            {
                _context.Escalas.Add(escala);
            }
            await _context.SaveChangesAsync();
        }

        public async Task RemoverEscalaAsync(Escala escala)
        {

            _context.Escalas.Remove(escala);
            await _context.SaveChangesAsync();
        }
        public async Task<List<Escala>> GetEscalaPorUnidade(int unidadeId, int ano, int mes)
        {
            return await _context.Escalas
                .Where(e => e.Unidade == unidadeId && e.Data.Year == ano && e.Data.Month == mes)
                .ToListAsync();
        }
    }
}
