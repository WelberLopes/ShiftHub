using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebRegistro.Models;
using WebRegistro.Repository.Interfaces;
using WebRegistro.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using WebRegistro.Repository;
using Microsoft.IdentityModel.Tokens;
using WebRegistro.Data;

namespace WebRegistro.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IPontoRepository _pontoRepo;
        private readonly NsrService _nsrService;
        private readonly IRegistroLogRepository _registroLogRepository;
        private readonly IDepartamentoRepository _departamentoRepository;
        private readonly ApplicationDbContext _context;
        public AdminController(IPontoRepository pontoRepo, IUserRepository userRepository, NsrService nsrService, IDepartamentoRepository departamentoRepository, ApplicationDbContext context)
        {
            _pontoRepo = pontoRepo;
            _userRepo = userRepository;
            _nsrService = nsrService;
            _departamentoRepository = departamentoRepository;
            _context = context;
        }

        public async Task<IActionResult> Index(int? departamentoId)
        {
            var funcionariosQuery =  _context.Users
                                    .Include(u => u.Departamento)
                                    .AsQueryable();

            // Se um ID de departamento foi passado, aplica o filtro
            if (departamentoId.HasValue && departamentoId > 0 && departamentoId != null)
            {
                funcionariosQuery = funcionariosQuery.Where(u => u.DepartamentoId == departamentoId.Value);
            }

            var todosFuncionarios =  _userRepo.GetAllUsers().OrderBy(u => u.NomeCompleto).ToList();
            var registrosHoje = await _pontoRepo.GetTodosRegistrosDoDiaAsync();
            var horarioAbertura = new TimeSpan(7, 15, 0);

            var horarioFechamento = new TimeSpan(19, 0, 0);
            var presentesIds = registrosHoje.Where(r =>  string.IsNullOrEmpty(r.Justificativa)).Select(r => r.UsuarioId).Distinct().ToList();
            var departamentos = await _departamentoRepository.GetAllDepartamentosAsync();

            var viewModel = new AdminDashboardViewModel
            {
                TotalFuncionarios = todosFuncionarios.Count,
                FuncionariosPresentes = presentesIds.Count,
                FuncionariosFaltas = todosFuncionarios.Count - presentesIds.Count,
                FuncionariosAtrasados = registrosHoje
                                         .Where(r => r.Tipo == "Entrada" &&  r.Horario.TimeOfDay > horarioAbertura)
                                         .Select(r => r.UsuarioId)
                                         .Distinct()
                                         .Count(),
                UltimosRegistros = registrosHoje.Take(5).ToList(),
                ListaFuncionarios = funcionariosQuery.ToList(),
                Departamentos = departamentos, // Adiciona a lista de departamentos ao ViewModel
            };
            return View(viewModel);
        }

        public async Task<IActionResult> Relatorios(DateTime? data)
        {
            // 1. DEFINIÇÕES INICIAIS
            var dataFiltro = data ?? DateTime.Today;
            var horarioAbertura = new TimeSpan(7, 30, 0);

            // 2. BUSCA DE DADOS BRUTOS
            var todosFuncionarios = _userRepo.GetAllUsers().OrderBy(r=> r.NomeCompleto).ToList();
            var todosRegistrosDoDia = await _pontoRepo.GetTodosRegistrosEmDataAsync(dataFiltro);

            // 3. INICIALIZAÇÃO DAS LISTAS
            var listaPresentes = new List<FuncionarioRelatorioViewModel>();
            var listaAtrasados = new List<FuncionarioRelatorioViewModel>();
            var listaAusentes = new List<FuncionarioRelatorioViewModel>();
            var listaJustificadas = new List<FuncionarioRelatorioViewModel>();

            // 4. LÓGICA DE CLASSIFICAÇÃO CORRIGIDA
            foreach (var funcionario in todosFuncionarios)
            {
                var registrosDoFuncionario = todosRegistrosDoDia
                    .Where(r => r.UsuarioId == funcionario.Cpf)
                    .ToList();

                // VERIFICAÇÃO PRIORITÁRIA: O funcionário tem alguma justificativa para o dia?
                if (registrosDoFuncionario.Any(r => !string.IsNullOrEmpty(r.Justificativa)))
                {
                    // Se tem justificativa, classificamos aqui e pulamos para o próximo funcionário.
                    listaJustificadas.Add(new FuncionarioRelatorioViewModel
                    {
                        Cpf = funcionario.Cpf,
                        NomeCompleto = funcionario.NomeCompleto,
                        Cargo = funcionario.Cargo,
                        RegistrosDoDia = registrosDoFuncionario.Select(r => new RegistroPontoViewModel { Tipo = r.Tipo, Horario = r.Horario, Justificativa = r.Justificativa }).ToList()
                    });
                    continue; // Pula para o próximo funcionário do loop
                }

                // Se o código chegou aqui, o funcionário NÃO tem justificativa.
                // Agora verificamos se ele está presente, atrasado ou ausente.
                if (registrosDoFuncionario.Any())
                {
                    var primeiraEntrada = registrosDoFuncionario
                        .Where(r => r.Tipo == "Entrada")
                        .OrderBy(r => r.Horario)
                        .FirstOrDefault();

                    if (primeiraEntrada != null && primeiraEntrada.Horario.TimeOfDay > horarioAbertura)
                    {
                        // CLASSIFICADO COMO ATRASADO
                        listaAtrasados.Add(new FuncionarioRelatorioViewModel
                        {
                            Cpf = funcionario.Cpf,
                            NomeCompleto = funcionario.NomeCompleto,
                            Cargo = funcionario.Cargo,
                            TempoAtraso = primeiraEntrada.Horario.TimeOfDay - horarioAbertura,
                            RegistrosDoDia = registrosDoFuncionario.Select(r => new RegistroPontoViewModel { Tipo = r.Tipo, Horario = r.Horario }).ToList()
                        });
                    }
                    else
                    {
                        // CLASSIFICADO COMO PRESENTE (NO HORÁRIO)
                        listaPresentes.Add(new FuncionarioRelatorioViewModel
                        {
                            Cpf = funcionario.Cpf,
                            NomeCompleto = funcionario.NomeCompleto,
                            Cargo = funcionario.Cargo,
                            RegistrosDoDia = registrosDoFuncionario.Select(r => new RegistroPontoViewModel { Tipo = r.Tipo, Horario = r.Horario }).ToList()
                        });
                    }
                }
                else
                {
                    // FUNCIONÁRIO ESTÁ AUSENTE (SEM JUSTIFICATIVA)
                    listaAusentes.Add(new FuncionarioRelatorioViewModel
                    {
                        Cpf = funcionario.Cpf,
                        NomeCompleto = funcionario.NomeCompleto,
                        Cargo = funcionario.Cargo,
                        RegistrosDoDia = new List<RegistroPontoViewModel>()
                    });
                }
            }

            // 5. MONTAGEM FINAL DO VIEWMODEL
            var viewModel = new AdminDashboardViewModel
            {
                TotalFuncionarios = todosFuncionarios.Count(),
                FuncionariosPresentes = listaPresentes.Count,
                FuncionariosAtrasados = listaAtrasados.Count,
                FuncionariosFaltas = listaAusentes.Count,
                FuncionariosJustificativas = listaJustificadas.Count, // Adicione esta propriedade ao ViewModel

                ListaPresentes = listaPresentes,
                ListaAtrasados = listaAtrasados,
                ListaAusentes = listaAusentes,
                ListaJustificadas = listaJustificadas
            };

            return View(viewModel);
        }


        public async Task<IActionResult> Relatorio([FromForm] string cpf, string competencia)
        {
            if (string.IsNullOrWhiteSpace(cpf))
            {
                return BadRequest("CPF não foi fornecido.");
            }
            var funcionario =  _userRepo.GetById(cpf);
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
        public async Task<IActionResult> JustificarFalta([FromForm]string cpf, DateTime dataFalta, string justificativa)
        {
            if (string.IsNullOrEmpty(justificativa))
            {
                TempData["ErrorMessage"] = "A justificativa não pode estar vazia.";
                return Json(new { success = true, message = "Não foi possivel justificar o horário!" });
            }

            var registro = new RegistroPonto
            {
                UsuarioId = cpf,
                Horario = dataFalta,
                Tipo = "Falta Justificada",
                Justificativa = justificativa,
                HorasTrabalhadasDia = TimeSpan.Zero
            };

            await _pontoRepo.AddRegistroPontoAsync(registro);

            return RedirectToAction("Index", "Admin");

            //return Json(new { success = true, message = "Horário justificado com sucesso!" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DefinirFerias([FromForm] string cpf, DateTime dataInicio, DateTime dataFim)
        {
            // A sua lógica de validação está ótima, só precisamos de ajustar o retorno
            if (dataInicio == DateTime.MinValue || dataFim == DateTime.MinValue) // Uma verificação mais robusta para datas não preenchidas
            {
                return Json(new { success = false, message = "O período de início e fim deve ser preenchido." });
            }

            if (dataInicio > dataFim)
            {
                return Json(new { success = false, message = "A data de início não pode ser posterior à data de fim." });
            }

            // O resto da sua lógica de negócio para salvar no banco
            var funcionario = await _userRepo.GetUser(cpf);
            if (funcionario == null)
            {
                return Json(new { success = false, message = "Funcionário não encontrado." });
            }

            foreach (var data in EachDay(dataInicio, dataFim))
            {
                var registro = new RegistroPonto
                {
                    UsuarioId = cpf,
                    Horario = data,
                    Tipo = "Férias",
                    Justificativa = "Férias",
                    HorasTrabalhadasDia = TimeSpan.Zero,
                    Nsr = await _nsrService.GerarProximoNsrAsync()
                };
                await _pontoRepo.AddRegistroPontoAsync(registro);
            }

            // CORREÇÃO: Retorna um JSON de sucesso
            return Json(new { success = true, message = $"Férias definidas para {funcionario.NomeCompleto} com sucesso!" });
        }

        // O seu método auxiliar está perfeito
        public static IEnumerable<DateTime> EachDay(DateTime from, DateTime to)
        {
            for (var day = from.Date; day <= to.Date; day = day.AddDays(1))
                yield return day;
        }
        public class JustificativaFaltaDto
        {
            public string Cpf { get; set; }
            public DateTime DataFalta { get; set; }
            public string Justificativa { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Essencial para segurança
        public async Task<IActionResult> AjustarHorario([FromBody] AjustePontoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var erro = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault();
                return Json(new { success = false, message = erro?.ErrorMessage ?? "Dados inválidos." });
            }

            try
            {
                // ETAPA 1: Obter dados essenciais (igual a antes)
                var funcionario = await _userRepo.GetUser(model.Cpf);
                if (funcionario == null)
                {
                    return Json(new { success = false, message = "Funcionário não encontrado." });
                }

                var competenciaDT = DateTime.ParseExact(model.Competencia, "yyyy-MM", CultureInfo.InvariantCulture);
                var dataDoAjuste = new DateTime(competenciaDT.Year, competenciaDT.Month, model.Dia);

                // ETAPA 2: Carregar todos os registros existentes para o dia (igual a antes)
                var registrosDoDia = await _pontoRepo.GetAjusteRegistroAsync(funcionario.Cpf, dataDoAjuste);

                var horariosParaAjustar = new Dictionary<string, string>
        {
            { "Entrada", model.Entrada }, { "Saída Almoço", model.SaidaAlmoco },
            { "Volta Almoço", model.VoltaAlmoco }, { "Saída", model.Saida }
        };

                // ETAPA 3: Processar CADA horário (APENAS ATUALIZAR ou DELETAR)
                long? nsrAntigo = null; // Variável para armazenar o NSR antigo, se necessário
                foreach (var parHorario in horariosParaAjustar)
                {
                    var tipo = parHorario.Key;
                    var horarioString = parHorario.Value;
                    var registroExistente = registrosDoDia.FirstOrDefault(p => p.Tipo == tipo);
                    var novoHorario = dataDoAjuste.Date + TimeSpan.Parse(horarioString);
                    // *** MUDANÇA PRINCIPAL AQUI ***
                    if (!string.IsNullOrEmpty(horarioString))
                    {
                        // Se o campo NÃO ESTÁ VAZIO, significa que queremos CRIAR ou ATUALIZAR.

                        // Convertemos o horário aqui, de forma segura.
                        var novoHorarioIfNull = dataDoAjuste.Date + TimeSpan.Parse(horarioString);

                        if (registroExistente != null)
                        {
                            // O registro JÁ EXISTE, então apenas ATUALIZAMOS o horário.
                            registroExistente.Horario = novoHorario;
                            nsrAntigo = registroExistente.Nsr;
                            registroExistente.Nsr = await _nsrService.GerarProximoNsrAsync();
                            await _pontoRepo.UpdateRegistroPonto(registroExistente);
                            // Registra o log de atualização
                            //await _registroLogRepository.AddRegistroLogAsync(registroExistente, nsrAntigo, model.Motivo);
                        }
                        else
                        {
                            // O registro NÃO EXISTE, então CRIAMOS um novo.
                            var novoRegistro = new RegistroPonto
                            {
                                UsuarioId = funcionario.Cpf, // Ou FuncionarioId
                                Horario = novoHorarioIfNull,
                                Tipo = tipo,
                                Nsr = await _nsrService.GerarProximoNsrAsync()
                            };
                            // Adicionamos o NOVO registro ao banco.
                            await _pontoRepo.AddRegistroPontoAsync(novoRegistro);
                        }
                    }
                    else
                    {

                    }
                    //await RegistroLog(registroExistente, nsrAntigo, model.Motivo);
                }

                // ETAPA 4: Recalcular as horas totais do dia (lógica igual a antes)
                var registrosFinaisDoDia = await _pontoRepo.GetAjusteRegistroAsync(funcionario.Cpf, dataDoAjuste);

                TimeSpan? horasCalculadas = CalcularHorasDoDia(registrosFinaisDoDia);

                // Limpa as horas de todos os registros primeiro para garantir consistência
                foreach (var registro in registrosFinaisDoDia)
                {
                    if (registro.HorasTrabalhadasDia != null)
                    {
                        registro.HorasTrabalhadasDia = null;
                        await _pontoRepo.UpdateRegistroPonto(registro);
                    }
                }

                // Atribui o total de horas apenas ao registro de Saída, se ele existir
                var registroDeSaidaFinal = registrosFinaisDoDia.FirstOrDefault(r => r.Tipo == "Saída");
                if (registroDeSaidaFinal != null)
                {
                    registroDeSaidaFinal.HorasTrabalhadasDia = horasCalculadas;
                    await _pontoRepo.UpdateRegistroPonto(registroDeSaidaFinal);
                }

                return Json(new { success = true, message = "Horários existentes foram ajustados com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Ocorreu um erro no servidor: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResumoMensal([FromForm] string cpf, [FromForm] string competencia)
        {
            // ... (código inicial de validação do CPF e da competência permanece o mesmo) ...

            DateTime dataFiltro;
            if (!string.IsNullOrEmpty(competencia) && DateTime.TryParse(competencia + "-01", out var parsedDate))
            {
                dataFiltro = parsedDate;
            }
            else
            {
                dataFiltro = DateTime.Today;
            }

            var funcionario = await _userRepo.GetUser(cpf);
            if (funcionario == null)
            {
                return NotFound();
            }

            // 1. Busca no banco de dados UMA ÚNICA VEZ
            var registrosDoMes = await _pontoRepo.GetRegistrosDoMesAsync(cpf, dataFiltro.Year, dataFiltro.Month);

            // 2. Prepara os dados de férias de forma segura
            var feriasDoMes = registrosDoMes.Where(r => r.Tipo == "Férias").ToList();
            DateTime? iniFerias = null;
            DateTime? fimFerias = null;
            if (feriasDoMes.Any())
            {
                iniFerias = feriasDoMes.Min(f => f.Horario);
                fimFerias = feriasDoMes.Max(f => f.Horario);
            }

            // 3. Agrupa os registros por dia para acesso rápido
            var registrosAgrupados = registrosDoMes.GroupBy(r => r.Horario.Day)
                                                   .ToDictionary(g => g.Key, g => g.ToList());

            // 4. Inicializa os contadores e listas
            var detalhesDiarios = new List<DetalheDiaViewModel>();
            var jornadaPadrao = new TimeSpan(8, 0, 0);
            var horarioEntradaPadrao = new TimeSpan(7, 15, 0);
            int totalAtrasos = 0;
            int totalFaltas = 0;
            TimeSpan totalHorasExtra = TimeSpan.Zero;

            // 5. Loop principal com a LÓGICA RESTAURADA
            for (int i = 1; i <= DateTime.DaysInMonth(dataFiltro.Year, dataFiltro.Month); i++)
            {
                var diaAtual = new DateTime(dataFiltro.Year, dataFiltro.Month, i);
                var detalheDia = new DetalheDiaViewModel { Dia = diaAtual, RegistrosDoDia = new List<RegistroPonto>() };

                bool estaDeFerias = iniFerias.HasValue && fimFerias.HasValue &&
                                    diaAtual.Date >= iniFerias.Value.Date && diaAtual.Date <= fimFerias.Value.Date;

                if (estaDeFerias)
                {
                    detalheDia.Status = "Férias";
                }
                else if (registrosAgrupados.TryGetValue(i, out var registrosDoDia))
                {
                    // === LÓGICA DE DIAS COM REGISTRO RESTAURADA ===
                    detalheDia.RegistrosDoDia = registrosDoDia;
                    var registroPrincipal = registrosDoDia.First();

                    if (registroPrincipal.Tipo == "Falta Justificada")
                    {
                        detalheDia.Status = "Falta Justificada";
                        detalheDia.Justificativa = registroPrincipal.Justificativa;
                    }
                    else
                    {
                        detalheDia.Status = "Trabalhado";
                        var entrada = registrosDoDia.FirstOrDefault(r => r.Tipo == "Entrada");
                        var saida = registrosDoDia.LastOrDefault(r => r.Tipo == "Saída");
                        detalheDia.HorasTrabalhadas = saida?.HorasTrabalhadasDia ?? TimeSpan.Zero;

                        if (entrada != null && entrada.Horario.TimeOfDay > horarioEntradaPadrao)
                        {
                            detalheDia.TempoAtraso = entrada.Horario.TimeOfDay - horarioEntradaPadrao;
                            totalAtrasos++;
                        }

                        if (detalheDia.HorasTrabalhadas > jornadaPadrao)
                        {
                            detalheDia.HorasExtra = detalheDia.HorasTrabalhadas - jornadaPadrao;
                            totalHorasExtra += detalheDia.HorasExtra;
                        }
                    }
                }
                else
                {
                    // === LÓGICA DE DIAS SEM REGISTRO RESTAURADA ===
                    if (diaAtual.DayOfWeek == DayOfWeek.Saturday || diaAtual.DayOfWeek == DayOfWeek.Sunday)
                    {
                        detalheDia.Status = "Fim de Semana";
                    }
                    else
                    {
                        detalheDia.Status = "Falta";
                        totalFaltas++;
                    }
                }
                detalhesDiarios.Add(detalheDia);
            }

            var viewModel = new ResumoMensalRhViewModel
            {
                Funcionario = funcionario,
                Competencia = dataFiltro,
                TotalHorasTrabalhadas = detalhesDiarios.Aggregate(TimeSpan.Zero, (total, dia) => total + dia.HorasTrabalhadas),
                TotalHorasExtra = totalHorasExtra,
                DiasTrabalhados = detalhesDiarios.Count(d => d.Status == "Trabalhado"),
                TotalFaltas = totalFaltas,
                TotalAtrasos = totalAtrasos,
                DetalhesDiarios = detalhesDiarios
            };

            return View(viewModel);
        }


        public TimeSpan? CalcularHorasDoDia(List<RegistroPonto> registrosOrdenados)
        {
            var entrada = registrosOrdenados.FirstOrDefault(r => r.Tipo == "Entrada");
            var saida = registrosOrdenados.LastOrDefault(r => r.Tipo == "Saída");

            if (entrada != null && saida != null && saida.Horario > entrada.Horario)
            {
                TimeSpan tempoTrabalhado = saida.Horario - entrada.Horario;
                var saidaAlmoco = registrosOrdenados.FirstOrDefault(r => r.Tipo == "Saída Almoço");
                var voltaAlmoco = registrosOrdenados.FirstOrDefault(r => r.Tipo == "Volta Almoço");

                if (saidaAlmoco != null && voltaAlmoco != null && voltaAlmoco.Horario > saidaAlmoco.Horario)
                {
                    tempoTrabalhado -= (voltaAlmoco.Horario - saidaAlmoco.Horario);
                }
                return tempoTrabalhado;
            }
            return null;
        }
        
    }
}
