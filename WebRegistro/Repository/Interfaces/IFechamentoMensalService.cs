using System.Threading.Tasks;

namespace WebRegistro.Services.Interfaces
{
    /// <summary>
    /// Define o contrato para o serviço de fechamento mensal do banco de horas.
    /// </summary>
    public interface IFechamentoMensalService
    {
        /// <summary>
        /// Executa o processo de fechamento para um determinado mês e ano,
        /// calculando e registrando os saldos de banco de horas para todos os funcionários.
        /// </summary>
        /// <param name="ano">O ano da competência.</param>
        /// <param name="mes">O mês da competência.</param>
        /// <returns>Um resumo do processo, como o número de funcionários processados.</returns>
        Task<string> ExecutarFechamentoAsync(int ano, int mes);
    }
}