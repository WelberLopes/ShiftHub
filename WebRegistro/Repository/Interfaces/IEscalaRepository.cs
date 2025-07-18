using WebRegistro.Models;

namespace WebRegistro.Repository.Interfaces
{
    public interface IEscalaRepository
    {
        Task<List<Escala>> GetEscalasDoMesAsync(int ano, int mes);
        Task<Escala?> GetEscalaAsync(int escalaId);
        Task<Escala?> GetEscalaPorDataEFuncionarioAsync(string funcionarioId, DateTime data);
        Task AdicionarOuAtualizarEscalaAsync(Escala escala);
        Task RemoverEscalaAsync(Escala escala);
        Task<List<Escala>> GetEscalaPorUnidade(int unidadeId, int ano, int mes);
    }
}
