using WebRegistro.Models;

namespace WebRegistro.Repository.Interfaces
{
    public interface IRegistroLogRepository
    {
        Task<List<RegistroLog>> GetRegistrosLogAsync(string usuarioId);
        Task<List<RegistroLog>> GetTodosRegistrosLogAsync();
        Task<RegistroLog> GetRegistroLogByIdAsync(int id);
        Task AddRegistroLogAsync(RegistroPonto registro, long? nsrAntigo, string Motivo);
        Task UpdateRegistroLogAsync(RegistroLog registro);
        Task DeleteRegistroLogAsync(int id);
        Task<List<RegistroLog>> GetRegistrosPorDataAsync(DateTime data, string usuarioId);
    }
}
