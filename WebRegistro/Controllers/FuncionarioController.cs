using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebRegistro.Repository.Interfaces;
using WebRegistro.ViewModels;

namespace WebRegistro.Controllers
{
    [Authorize]
    public class FuncionarioController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IPontoRepository _pontoRepo;
        public FuncionarioController(IUserRepository userRepo, IPontoRepository pontoRepo)
        {
            _userRepo = userRepo;
            _pontoRepo = pontoRepo;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> MinhaConta()
        {
            var cpf = User.FindFirst("CPF")?.Value;

            if (string.IsNullOrEmpty(cpf))
            {
                return Forbid("CPF não encontrado para o usuário autenticado.");
            }
            var user = await _userRepo.GetUser(cpf);
            if (user == null)
            {
                return NotFound("Usuário não encontrado no banco de dados.");
            }
            var funcionario = new UserViewModel
            {
                Cpf = user.Cpf,
                NomeCompleto = user.NomeCompleto,
                Email = user.Email,
                DataAdmissao = user.DataAdmissao,
                Cargo = user.Cargo,
                Role = user.Role
            };

            return View(funcionario);

        }
        public async Task<IActionResult> MeuEspelhoDePonto(string competencia)
        {
            var cpf = User.FindFirst("CPF")?.Value;
            
            if (string.IsNullOrEmpty(cpf))
            {
                return Forbid("CPF não encontrado para o usuário autenticado.");
            }

            var funcionario = await _userRepo.GetUser(cpf);

            if (funcionario == null)
            {
                return NotFound();
            }

            if (!DateTime.TryParse(competencia + "-01", out var competenciaDate))
            {
                competenciaDate = DateTime.UtcNow;
            }

            var registros = await _pontoRepo.GetRegistrosDoMesAsync(cpf, competenciaDate.Year, competenciaDate.Month);

            var registrosAgrupados = registros
                .GroupBy(r => r.Horario.Day)
                .ToDictionary(g => g.Key, g => g.ToList());

            var totalHoras = new TimeSpan(registros
                .Where(r => r.Tipo == "Saída" && r.HorasTrabalhadasDia.HasValue)
                .Sum(r => r.HorasTrabalhadasDia.Value.Ticks));

            var viewModel = new RelatorioMensalViewModel
            {
                Funcionario = funcionario,
                RegistrosAgrupadosPorDia = registrosAgrupados,
                TotalHorasTrabalhadas = totalHoras,
                Competencia = competenciaDate
            };

            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarMinhaSenha(AlterarSenhaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Se a validação falhar (ex: senhas não batem), você pode retornar um erro.
                // Como estamos em um modal, o ideal é mostrar o erro via TempData ou outra abordagem.
                // Por simplicidade, vamos apenas redirecionar com uma mensagem de erro genérica.
                TempData["ErrorMessage"] = "Ocorreu um erro ao validar os dados. Verifique se as senhas coincidem e tente novamente.";
                return RedirectToAction("MeuPerfil"); // Redireciona de volta para a página de perfil
            }

            // Pega o usuário logado (o CPF é uma forma comum de identificar o usuário)
            var cpfUsuarioLogado = User.FindFirst("CPF")?.Value;
            if (string.IsNullOrEmpty(cpfUsuarioLogado))
            {
                return Unauthorized(); // Ou outra forma de tratar usuário não encontrado
            }

            var usuario = await _userRepo.GetUser(cpfUsuarioLogado); // Você precisará deste método no seu repositório
            if (usuario == null)
            {
                return NotFound();
            }


            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NovaSenha); // Use BCrypt ou outro método de hash seguro
             _userRepo.Update(usuario); 

            TempData["SuccessMessage"] = "Senha alterada com sucesso!";
            return RedirectToAction("MinhaConta");
        }
        public async Task<IActionResult> MeuDashboard()
        {
            var cpfUsuarioLogado = User.FindFirst("CPF")?.Value;
            if (string.IsNullOrEmpty(cpfUsuarioLogado))
            {
                return Unauthorized();
            }

            var funcionario = await _userRepo.GetUser(cpfUsuarioLogado);
            if (funcionario == null)
            {
                return NotFound();
            }

            // A data/hora atual é capturada uma vez para consistência
            var hoje = DateTime.Now;
            var registrosDoMes = await _pontoRepo.GetRegistrosDoMesAsync(cpfUsuarioLogado, hoje.Year, hoje.Month);

            // Agrupa os registros por dia para facilitar os cálculos
            var registrosAgrupados = registrosDoMes
                .GroupBy(r => r.Horario.Day)
                .ToDictionary(g => g.Key, g => g.ToList());

            int diasTrabalhados = 0;
            int faltasJustificadas = 0; // Pode ser renomeado para "diasComJustificativa"
            TimeSpan totalHoras = TimeSpan.Zero;
            var labelsGrafico = new List<string>();
            var dadosGrafico = new List<double>();

            // Processa todos os dias do início do mês até hoje
            for (int dia = 1; dia <= hoje.Day; dia++)
            {
                labelsGrafico.Add($"{dia:D2}/{hoje.Month:D2}"); // Adiciona label para o gráfico

                if (registrosAgrupados.TryGetValue(dia, out var registrosDoDia))
                {
                    // --- LÓGICA CORRIGIDA ---

                    // 1. Verificamos se houve horas trabalhadas no dia.
                    var horasDia = registrosDoDia.FirstOrDefault(r => r.Tipo == "Saída")?.HorasTrabalhadasDia ?? TimeSpan.Zero;

                    if (horasDia > TimeSpan.Zero)
                    {
                        diasTrabalhados++;
                        totalHoras += horasDia;
                    }

                    // 2. Verificamos INDEPENDENTEMENTE se há uma justificativa no dia.
                    //    Isso permite que um dia trabalhado também tenha uma justificativa.
                    if (registrosDoDia.Any(r => r.Justificativa != string.Empty))
                    {
                        faltasJustificadas++;
                    }

                    // Adiciona as horas efetivamente trabalhadas ao gráfico.
                    dadosGrafico.Add(horasDia.TotalHours);
                }
                else // Dia sem nenhum registro de ponto
                {
                    var dataVerificada = new DateTime(hoje.Year, hoje.Month, dia);

                    // Considera falta apenas se for um dia útil (lógica simplificada)
                    if (dataVerificada.DayOfWeek != DayOfWeek.Sunday && dataVerificada.DayOfWeek != DayOfWeek.Saturday)
                    {
                        // Aqui você pode adicionar lógica para verificar feriados ou a escala específica do funcionário
                        // Se for confirmado que é uma falta, você pode incrementar um contador de "Faltas Não Justificadas"
                    }
                    dadosGrafico.Add(0); // 0 horas em dias sem registro
                }
            }

            var viewModel = new DashboardViewModel
            {
                Funcionario = funcionario,
                DiasTrabalhados = diasTrabalhados,
                FaltasJustificadas = faltasJustificadas, // ou DiasComJustificativa
                TotalHorasTrabalhadas = $"{(int)totalHoras.TotalHours:D2}:{totalHoras.Minutes:D2}",
                LabelsGraficoHoras = labelsGrafico,
                DadosGraficoHoras = dadosGrafico
            };

            return View(viewModel);
        }
    }
}
