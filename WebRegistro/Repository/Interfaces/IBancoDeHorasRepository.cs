using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebRegistro.Models;

namespace WebRegistro.Repository.Interfaces
{
    /// <summary>
    /// Define o contrato para as operações de acesso a dados
    /// relacionadas ao sistema de Banco de Horas.
    /// </summary>
    public interface IBancoDeHorasRepository
    {
        // --- Operações Básicas (CRUD) ---

        /// <summary>
        /// Adiciona uma nova movimentação (crédito ou débito) no banco de horas.
        /// </summary>
        /// <param name="movimentacao">O objeto BancoDeHoras a ser adicionado.</param>
        Task AdicionarMovimentacaoAsync(BancoDeHoras movimentacao);

        /// <summary>
        /// Atualiza uma movimentação existente. Útil para correções administrativas.
        /// </summary>
        /// <param name="movimentacao">O objeto BancoDeHoras com os dados atualizados.</param>
        Task AtualizarMovimentacaoAsync(BancoDeHoras movimentacao);

        /// <summary>
        /// Busca uma movimentação específica pelo seu Id.
        /// </summary>
        /// <param name="id">O Id da movimentação.</param>
        /// <returns>A movimentação encontrada ou null se não existir.</returns>
        Task<BancoDeHoras> GetMovimentacaoByIdAsync(int id);

        // --- Consultas Específicas para o Controller ---

        /// <summary>
        /// Busca todas as movimentações de um usuário específico, ordenadas por data.
        /// Ideal para exibir um extrato completo.
        /// </summary>
        /// <param name="userId">O Id do usuário.</param>
        /// <returns>Uma coleção de movimentações do usuário.</returns>
        Task<IEnumerable<BancoDeHoras>> GetMovimentacoesPorUsuarioAsync(string userId);

        /// <summary>
        /// Busca as movimentações de um usuário dentro de um período específico (ex: um mês).
        /// </summary>
        /// <param name="userId">O Id do usuário.</param>
        /// <param name="dataInicio">A data de início do período.</param>
        /// <param name="dataFim">A data de fim do período.</param>
        /// <returns>Uma coleção de movimentações do usuário no período especificado.</returns>
        Task<IEnumerable<BancoDeHoras>> GetMovimentacoesPorUsuarioNoPeriodoAsync(string userId, DateTime dataInicio, DateTime dataFim);


        // --- Lógica de Negócio (Abstrai cálculos complexos) ---

        /// <summary>
        /// Calcula o saldo de horas atual de um usuário, somando todos os créditos e subtraindo todos os débitos.
        /// </summary>
        /// <param name="userId">O Id do usuário.</param>
        /// <returns>Um TimeSpan representando o saldo de horas (pode ser positivo ou negativo).</returns>
        Task<TimeSpan> CalcularSaldoAtualAsync(string userId);

        /// <summary>
        /// Busca todos os créditos de horas que estão prestes a expirar e que ainda não foram compensados.
        /// Essencial para a rotina de pagamento de horas vencidas.
        /// </summary>
        /// <param name="dataLimite">A data de corte para a expiração.</param>
        /// <returns>Uma coleção de movimentações de crédito que irão expirar.</returns>
        Task<IEnumerable<BancoDeHoras>> GetCreditosExpirandoAsync(DateTime dataLimite);

        Task<IEnumerable<BancoDeHoras>> GetAllMovimentacoesPeriodo(DateTime competencia);
    }
}