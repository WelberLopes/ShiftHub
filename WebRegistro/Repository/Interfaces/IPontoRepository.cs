using WebRegistro.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebRegistro.Repository.Interfaces
{
    public interface IPontoRepository
    {
        Task<List<RegistroPonto>> GetRegistrosDoDiaAsync(string usuarioId);
        Task<List<RegistroPonto>> GetTodosRegistrosDoDiaAsync(); // Novo método
        Task<List<RegistroPonto>> GetAjusteRegistroAsync(string usuarioId, DateTime dataregistro);
        Task<List<RegistroPonto>> GetRegistrosDoMesAsync(string usuarioId, int ano, int mes);
        Task AddRegistroPontoAsync(RegistroPonto registro);
        Task UpdateRegistroPonto(RegistroPonto ponto);
        Task<List<RegistroPonto>> GetTodosRegistrosEmDataAsync(DateTime data); // NOVO MÉTODO
        Task<int> GetDiasJustificados(string usuarioId, int ano, int mes);
        Task<int> GetDiasDeFerias(string usuarioId, int ano, int mes);
    }
}
