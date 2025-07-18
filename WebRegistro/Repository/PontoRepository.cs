using Microsoft.EntityFrameworkCore;
using WebRegistro.Data;
using WebRegistro.Models;
using WebRegistro.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace WebRegistro.Repository
{
    public class PontoRepository : IPontoRepository
    {
        private readonly ApplicationDbContext _context;

        public PontoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddRegistroPontoAsync(RegistroPonto registro)
        {
            _context.RegistrosPonto.Add(registro);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Implementação CORRIGIDA e ROBUSTA do método para obter os registos do dia.
        /// </summary>
        /// <param name="usuarioId">O CPF do utilizador.</param>
        /// <returns>Uma lista dos registos de ponto do dia atual, devidamente ordenada.</returns>
        public async Task<List<RegistroPonto>> GetRegistrosDoDiaAsync(string usuarioId)
        {
            // Define o intervalo de datas para o dia de hoje (na hora local do servidor).
            // Isto evita problemas de fuso horário e de tradução para SQL.
            var hoje = DateTime.Today; // Ex: 26/06/2025 00:00:00
            var amanha = hoje.AddDays(1);   // Ex: 27/06/2025 00:00:00

            // A consulta filtra os registos cujo horário é MAIOR OU IGUAL a hoje
            // E MENOR QUE amanhã. A ordenação é crucial para a lógica de sequência.
            return await _context.RegistrosPonto
                                 .Where(r => r.UsuarioId == usuarioId && r.Horario >= hoje && r.Horario < amanha)
                                 .OrderBy(r => r.Horario)
                                 .ToListAsync();
        }

        /// <summary>
        /// Método corrigido para usar a mesma lógica de intervalo de datas.
        /// </summary>
        public async Task<List<RegistroPonto>> GetTodosRegistrosDoDiaAsync()
        {
            var hoje = DateTime.Today;
            var amanha = hoje.AddDays(1);

            return await _context.RegistrosPonto
                .Include(r => r.Usuario) // Inclui os dados do funcionário
                .Where(r => r.Horario >= hoje && r.Horario < amanha)
                .OrderByDescending(r => r.Horario)
                .ToListAsync();
        }

        public async Task<List<RegistroPonto>> GetRegistrosDoMesAsync(string usuarioId, int ano, int mes)
        {
            return await _context.RegistrosPonto
                .Where(r => r.UsuarioId == usuarioId && r.Horario.Year == ano && r.Horario.Month == mes)
                .OrderBy(r => r.Horario)
                .ToListAsync();
        }

        public async Task UpdateRegistroPonto(RegistroPonto ponto)
        {
            var existingRegistro = await _context.RegistrosPonto.FindAsync(ponto.Id);
            if (existingRegistro != null)
            {
                existingRegistro.Horario = ponto.Horario;
                existingRegistro.Tipo = ponto.Tipo;
                existingRegistro.HorasTrabalhadasDia = ponto.HorasTrabalhadasDia;
                existingRegistro.Justificativa = ponto.Justificativa;
                existingRegistro.Nsr = ponto.Nsr;
                _context.RegistrosPonto.Update(existingRegistro);
                await _context.SaveChangesAsync();


            }
            else
            {
                throw new Exception("Registro de ponto não encontrado.");
            }
        }
        public async Task<List<RegistroPonto?>> GetAjusteRegistroAsync(string usuarioId, DateTime dataregistro)
        {
            // Busca o registro de ponto específico para o usuário e a data fornecida
            return await _context.RegistrosPonto
                 .Where(r => r.UsuarioId == usuarioId && r.Horario.Date == dataregistro.Date).OrderBy(r => r.Horario)
                                  .ToListAsync();
        }
        public async Task<List<RegistroPonto>> GetTodosRegistrosEmDataAsync(DateTime data)
        {
            // Garante que estamos comparando apenas a parte da data
            var inicioDoDia = data.Date;
            var fimDoDia = inicioDoDia.AddDays(1);

            return await _context.RegistrosPonto
                .Include(r => r.Usuario) // Inclui os dados do funcionário para evitar múltiplas buscas
                .Where(r => r.Horario >= inicioDoDia && r.Horario < fimDoDia)
                .OrderBy(r => r.UsuarioId) // Ordenar pode otimizar o agrupamento posterior
                .ThenBy(r => r.Horario)
                .ToListAsync();
        }
        public async Task<int> GetDiasJustificados(string usuarioId, int ano, int mes)
        {
            // Obtém os registros de ponto do usuário para o mês e ano especificados
            var registros = await _context.RegistrosPonto
                .Where(r => r.UsuarioId == usuarioId && r.Horario.Year == ano && r.Horario.Month == mes)
                .ToListAsync();

            var diasUteisJustificados = registros
        .Where(r =>
            !string.IsNullOrEmpty(r.Justificativa) &&
            r.Horario.DayOfWeek != DayOfWeek.Sunday)
        .Select(r => r.Horario.Date)
        .Distinct()
        .Count();

            return diasUteisJustificados;

        }
        public async Task<int> GetDiasDeFerias(string usuarioId, int ano, int mes)
        {
            // Obtém os registros de ponto do usuário para o mês e ano especificados
            var registros = await _context.RegistrosPonto
                .Where(r => r.UsuarioId == usuarioId && r.Horario.Year == ano && r.Horario.Month == mes)
                .ToListAsync();
            // Conta quantos registros são do tipo "Férias"
            return registros.Count(r => r.Tipo == "Férias");
        }
    }
}
