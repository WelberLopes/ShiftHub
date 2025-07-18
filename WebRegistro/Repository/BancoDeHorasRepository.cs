using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRegistro.Data;
using WebRegistro.Models;
using WebRegistro.Repository.Interfaces;
using static WebRegistro.Models.BancoDeHoras;

namespace WebRegistro.Repository
{
    public class BancoDeHorasRepository : IBancoDeHorasRepository
    {
        // Injeção de dependência do DbContext da sua aplicação
        private readonly ApplicationDbContext _context;

        public BancoDeHorasRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adiciona uma nova movimentação no banco de dados.
        /// </summary>
        public async Task AdicionarMovimentacaoAsync(BancoDeHoras movimentacao)
        {
            if (movimentacao == null)
            {
                throw new ArgumentNullException(nameof(movimentacao));
            }

            _context.BancoDeHoras.Add(movimentacao);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Atualiza uma movimentação existente.
        /// </summary>
        public async Task AtualizarMovimentacaoAsync(BancoDeHoras movimentacao)
        {
            if (movimentacao == null)
            {
                throw new ArgumentNullException(nameof(movimentacao));
            }

            // O método Update marca a entidade inteira como modificada.
            _context.BancoDeHoras.Update(movimentacao);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Busca uma movimentação pelo seu Id.
        /// </summary>
        public async Task<BancoDeHoras> GetMovimentacaoByIdAsync(int id)
        {
            return await _context.BancoDeHoras.FindAsync(id);
        }

        /// <summary>
        /// Retorna todas as movimentações de um usuário, com as mais recentes primeiro.
        /// </summary>
        public async Task<IEnumerable<BancoDeHoras>> GetMovimentacoesPorUsuarioAsync(string userId)
        {
            return await _context.BancoDeHoras
                                 .Where(m => m.UserId == userId)
                                 .OrderByDescending(m => m.DataRegistro)
                                 .ToListAsync();
        }

        /// <summary>
        /// Retorna as movimentações de um usuário em um período, em ordem cronológica.
        /// </summary>
        public async Task<IEnumerable<BancoDeHoras>> GetMovimentacoesPorUsuarioNoPeriodoAsync(string userId, DateTime dataInicio, DateTime dataFim)
        {
            return await _context.BancoDeHoras
                                 .Where(m => m.UserId == userId && m.DataRegistro >= dataInicio && m.DataRegistro <= dataFim)
                                 .OrderBy(m => m.DataRegistro)
                                 .ToListAsync();
        }

        /// <summary>
        /// Calcula o saldo de horas de forma eficiente, direto no banco de dados.
        /// </summary>
        public async Task<TimeSpan> CalcularSaldoAtualAsync(string userId)
        {
            var creditos = await _context.BancoDeHoras
                .Where(b => b.UserId == userId && b.TipoMovimentacao == TipoMovimentacaoHoras.Credito)
                .ToListAsync(); // Materializa no C# para usar Ticks

            var debitos = await _context.BancoDeHoras
                .Where(b => b.UserId == userId && b.TipoMovimentacao == TipoMovimentacaoHoras.Debito)
                .ToListAsync();

            var creditosEmTicks = creditos.Sum(b => b.Horas.Ticks);
            var debitosEmTicks = debitos.Sum(b => b.Horas.Ticks);

            var saldoEmTicks = creditosEmTicks - debitosEmTicks;

            return TimeSpan.FromTicks(saldoEmTicks);
        }


        /// <summary>
        /// Retorna os créditos que já expiraram ou expiram até a data limite.
        /// </summary>
        public async Task<IEnumerable<BancoDeHoras>> GetCreditosExpirandoAsync(DateTime dataLimite)
        {
            // Aqui, uma lógica mais complexa poderia ser adicionada para verificar se o crédito já foi totalmente compensado.
            // Para este exemplo, retornamos todos os créditos que expiram até a data.
            return await _context.BancoDeHoras
                                 .Where(m => m.TipoMovimentacao == TipoMovimentacaoHoras.Credito &&
                                             m.DataExpiracao != null &&
                                             m.DataExpiracao <= dataLimite)
                                 .ToListAsync();
        }

        async Task<IEnumerable<BancoDeHoras>> IBancoDeHorasRepository.GetAllMovimentacoesPeriodo(DateTime competencia)
        {
            var sentence1 = $"Fechamento de Mês ({competencia.Month}/{competencia.Year}) - Saldo Devedor";
            var sentence2 = $"Fechamento de Mês ({competencia.Month}/{competencia.Year}) - Saldo Positivo";
            var lista = _context.BancoDeHoras
                .Where(m=> m.Descricao == sentence1 || m.Descricao == sentence2).Select(m => m)
                .OrderByDescending(m => m.Data)
                ;
            return lista;
        }
    }
}