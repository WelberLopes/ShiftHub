using WebRegistro.Models;

namespace WebRegistro.Repository.Interfaces
{
    public interface IDepartamentoRepository
    {

        Task<IEnumerable<Departamento>> GetAllDepartamentosAsync();
        Task<Departamento> GetDepartamentoByIdAsync(int id);
        Task AddDepartamentoAsync(Departamento departamento);
        Task UpdateDepartamentoAsync(Departamento departamento);
        Task DeleteDepartamentoAsync(int id);
        Task<IEnumerable<User>> GetFuncionariosByDepartamentoIdAsync(int departamentoId);
        Task<User> GetResponsavelByDepartamentoIdAsync(int departamentoId);
    }
}
