using System;
using System.Linq;
using System.Threading.Tasks;
using WebRegistro.Models;
using WebRegistro.Repository.Interfaces;
using WebRegistro.Services.Interfaces;
using static WebRegistro.Models.BancoDeHoras;

namespace WebRegistro.Services
{
    public class FechamentoMensalService : IFechamentoMensalService
    {
        private readonly IUserRepository _userRepo;
        private readonly IPontoRepository _pontoRepo; // Supondo que você tenha este repositório
        private readonly IBancoDeHorasRepository _bancoHorasRepo;

        public FechamentoMensalService(
            IUserRepository userRepo,
            IPontoRepository pontoRepo,
            IBancoDeHorasRepository bancoHorasRepo)
        {
            _userRepo = userRepo;
            _pontoRepo = pontoRepo;
            _bancoHorasRepo = bancoHorasRepo;
        }

        public async Task<string> ExecutarFechamentoAsync(int ano, int mes)
        {
            var todosFuncionarios =  _userRepo.GetAllUsers();
            int funcionariosProcessados = 0;
            var CargaHorariaSemanal = 36;
            var horasDiarias = 6;
                
            foreach (var funcionario in todosFuncionarios)
            {
                if (funcionario.Cargo == "Administrador" || funcionario.Cargo == "Coordenacao" || funcionario.Cargo == "Enfermagem")
                {
                    CargaHorariaSemanal = 40;
                    horasDiarias = 8; // 40 horas semanais, 5 dias úteis
                } else if(funcionario.Cargo == "Estágio")
                {
                    CargaHorariaSemanal = 20;
                    horasDiarias = 4; // 20 horas semanais, 5 dias úteis
                }
                else
                {
                    CargaHorariaSemanal = 36; // 36 horas semanais, 6 dias úteis
                }
                    // 1. Calcular a carga horária padrão para o mês
                    var cargaHorariaPadrao = CalcularCargaHorariaMensal(ano, mes, CargaHorariaSemanal);

                // 2. Buscar os registros de ponto e somar as horas trabalhadas
                var registrosDoMes = await _pontoRepo.GetRegistrosDoMesAsync(funcionario.Cpf, ano, mes);
                var horasTrabalhadasNoMes = registrosDoMes
                    .Where(r => r.Tipo == "Entrada" || r.Tipo == "Saída") // Considera apenas registros de trabalho
                    .Aggregate(TimeSpan.Zero, (total, registro) => total + (registro.HorasTrabalhadasDia ?? TimeSpan.Zero));

                var faltasJustificadas = await _pontoRepo.GetDiasJustificados(funcionario.Cpf, ano, mes);


                var horasCompensadas = faltasJustificadas * horasDiarias;
                var faltasConvertidas = TimeSpan.FromHours(horasCompensadas);
                // 3. Calcular a diferença
                var diferenca = horasTrabalhadasNoMes - cargaHorariaPadrao;

                // Ignorar diferenças pequenas para evitar registros desnecessários
                if (Math.Abs(diferenca.TotalMinutes) < 10)
                {
                    continue; // Pula para o próximo funcionário
                }

                // 4. Criar a movimentação no banco de horas
                var movimentacao = new BancoDeHoras
                {
                    UserId = funcionario.Cpf,
                    Data = new DateTime(ano, mes, DateTime.DaysInMonth(ano, mes)),
                    DataRegistro = DateTime.Now
                };

                if (diferenca > TimeSpan.Zero) // Saldo Positivo
                {
                    movimentacao.TipoMovimentacao = TipoMovimentacaoHoras.Credito;
                    movimentacao.Horas = diferenca;
                    movimentacao.Descricao = $"Fechamento de Mês ({mes}/{ano}) - Saldo Positivo";
                    movimentacao.DataExpiracao = movimentacao.Data.AddMonths(6);
                }
                else // Saldo Devedor
                {
                    movimentacao.TipoMovimentacao = TipoMovimentacaoHoras.Debito;
                    movimentacao.Horas = diferenca.Negate(); // Armazena a duração como um valor positivo
                    movimentacao.Descricao = $"Fechamento de Mês ({mes}/{ano}) - Saldo Devedor";
                }

                await _bancoHorasRepo.AdicionarMovimentacaoAsync(movimentacao);
                funcionariosProcessados++;
            }

            return $"{funcionariosProcessados} funcionários processados com sucesso.";
        }

        private TimeSpan CalcularCargaHorariaMensal(int ano, int mes, int cargaSemanal)
        {
            var diasTrabalhados = 0.0;
            if (cargaSemanal == 40)
            {
                diasTrabalhados = 5.0; // 40 horas semanais, 5 dias úteis
            }else if(cargaSemanal == 20)
            {
                diasTrabalhados = 5.0; // 20 horas semanais, 5 dias úteis
            }
            else
            {
                diasTrabalhados = 6.0; // 36 horas semanais, 6 dias úteis
            }
                int diasUteis = 0;
            for (int dia = 1; dia <= DateTime.DaysInMonth(ano, mes); dia++)
            {
                var dataAtual = new DateTime(ano, mes, dia);
                if (dataAtual.DayOfWeek != DayOfWeek.Saturday && dataAtual.DayOfWeek != DayOfWeek.Sunday)
                {
                    diasUteis++;
                }
            }
            // Supondo uma jornada de 8h/dia para 40h semanais, ou 8.8h/dia para 44h.
            // Uma forma mais simples é usar a média diária.
            double horasDiarias = cargaSemanal / diasTrabalhados;
            return TimeSpan.FromHours(diasUteis * horasDiarias);
        }
    }
}