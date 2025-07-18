using WebRegistro.Data;
using WebRegistro.Migrations;
using WebRegistro.Models;
using WebRegistro.Repository.Interfaces;    
namespace WebRegistro.Repository
{
    public class RegistroLogRepository : IRegistroLogRepository
    {
        private readonly ApplicationDbContext _context;

        public RegistroLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddRegistroLogAsync(RegistroPonto registro, long? nsrAntigo, string motivo)
        {
            try
            {
                var registroLog = new RegistroLog
                {
                    UsuarioId = registro.UsuarioId,
                    Horario = registro.Horario,
                    Tipo = registro.Tipo,
                    Nsr = registro.Nsr,
                    NsrAntigo = nsrAntigo,
                    Motivo = motivo ?? "Registro de ponto atualizado.",
                };
                _context.RegistrosLog.Add(registroLog);
                await _context.SaveChangesAsync();
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao registrar log: {ex.Message}");
            }
            
        }

        public Task DeleteRegistroLogAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<RegistroLog> GetRegistroLogByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<RegistroLog>> GetRegistrosLogAsync(string usuarioId)
        {
            var registros =  _context.RegistrosLog
                .Where(r => r.UsuarioId == usuarioId)
                .OrderByDescending(r => r.Horario).ToList();
            return registros;
        }

        public async Task<List<RegistroLog>> GetRegistrosPorDataAsync(DateTime data, string usuarioId)
        {
            var registros = _context.RegistrosLog.Where(r=> r.UsuarioId == usuarioId && r.Horario.Date == data.Date)
                .OrderByDescending(r => r.Horario)
                .ToList();
            return registros;
        }

        public async Task<List<RegistroLog>> GetTodosRegistrosLogAsync()
        {
            var list = _context.RegistrosLog
                .OrderByDescending(r => r.Horario)
                .ToList();
            return list;
        }

        public Task UpdateRegistroLogAsync(RegistroLog registro)
        {
            throw new NotImplementedException();
        }
    }
}
