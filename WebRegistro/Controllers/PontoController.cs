using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebRegistro.Models;
using WebRegistro.Repository.Interfaces;
using WebRegistro.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

// Adicione os namespaces do SDK da Digital Persona e dos seus ViewModels
using DPFP;
using DPFP.Verification;
using WebRegistro.ViewModels;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace WebRegistro.Controllers
{
    //[Authorize]
    public class PontoController : Controller
    {
        private readonly EmailService _emailService;
        private readonly IPontoRepository _pontoRepo;
        private readonly IUserRepository _userRepo;
        private readonly NsrService _nsrService;
        private readonly IRegistroLogRepository _registroLogRepository;

        // *** NOTA IMPORTANTE SOBRE O ERRO "Multiple Constructors" ***
        // O sistema de Injeção de Dependência exige que haja apenas UM construtor público
        // nesta classe. Este é o único construtor que deve existir no seu PontoController.
        // Certifique-se de que não há nenhum outro, como um construtor sem parâmetros.
        public PontoController(IPontoRepository pontoRepo, IUserRepository userRepository, NsrService nsrService, EmailService emailService, IRegistroLogRepository registroLogRepository)
        {
            _pontoRepo = pontoRepo;
            _userRepo = userRepository;
            _nsrService = nsrService;
            _emailService = emailService;
            _registroLogRepository = registroLogRepository;
        }
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var cpf = User.FindFirst("CPF")?.Value;
            if (string.IsNullOrEmpty(cpf))
            {
                return Forbid("CPF não encontrado para o utilizador autenticado.");
            }

            var user = await _userRepo.GetUser(cpf);
            if (user == null)
            {
                return NotFound("Utilizador não encontrado no banco de dados.");
            }
            ViewData["UserNome"] = user?.NomeCompleto;
            ViewData["UserCargo"] = user?.Cargo;

            var viewModel = new PontoViewModel
            {
                NomeUsuario = user.NomeCompleto,
                RegistrosDeHoje = await _pontoRepo.GetRegistrosDoDiaAsync(user.Cpf)
            };
            return View(viewModel);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerificarBiometria([FromBody] BiometriaVerificacaoRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.BiometricData))
            {
                return Json(new { success = false, message = "Dados da digital não recebidos." });
            }

            try
            {
                var cpf = User.FindFirst("CPF")?.Value;
                var utilizador = await _userRepo.GetUser(cpf);

                if (utilizador == null || utilizador.BiometricTemplate == null || utilizador.BiometricTemplate.Length == 0)
                {
                    return Json(new { success = false, message = "Utilizador não encontrado ou não possui biometria cadastrada." });
                }

                // A sua lógica de verificação biométrica...
                Template templateSalvo = new Template();
                templateSalvo.DeSerialize(utilizador.BiometricTemplate);

                FeatureSet featureSetCapturado = new FeatureSet();
                byte[] featureSetBytes = Convert.FromBase64String(request.BiometricData);
                featureSetCapturado.DeSerialize(featureSetBytes);

                Verification verificator = new Verification();
                Verification.Result result = new Verification.Result();
                verificator.Verify(featureSetCapturado, templateSalvo, ref result);

                if (!result.Verified)
                {
                    return Json(new { success = false, message = "Impressão digital não corresponde. Tente novamente." });
                }

                // *** LÓGICA ROBUSTA PARA DETERMINAR O TIPO DE PONTO ***
                var newNsr = await _nsrService.GerarProximoNsrAsync();
                var registrosDeHoje = await _pontoRepo.GetRegistrosDoDiaAsync(utilizador.Cpf);
                var ultimoRegisto = registrosDeHoje.OrderBy(r => r.Horario).LastOrDefault();

                string tipoAtual;

                if (ultimoRegisto == null)
                {
                    // Se não houver registos, a próxima batida é "Entrada"
                    tipoAtual = "Entrada";
                }
                else
                {
                    // Verifica o tipo do último registo para determinar o próximo
                    switch (ultimoRegisto.Tipo)
                    {
                        case "Entrada":
                            tipoAtual = "Saída Almoço";
                            break;
                        case "Saída Almoço":
                            tipoAtual = "Volta Almoço";
                            break;
                        case "Volta Almoço":
                            tipoAtual = "Saída";
                            break;
                        case "Saída":
                            return Json(new { success = false, message = "Todos os pontos do dia já foram registados." });
                        default:
                            // Caso de segurança para um tipo inesperado
                            return Json(new { success = false, message = "Sequência de ponto inválida. Contacte o administrador." });
                    }
                }

                TimeSpan? horasTrabalhadas = null;
                DateTime horarioDoPonto = DateTime.Now;

                if (tipoAtual == "Saída")
                {
                    var todosRegistosParaCalculo = new List<RegistroPonto>(registrosDeHoje);
                    todosRegistosParaCalculo.Add(new RegistroPonto { Horario = horarioDoPonto, Tipo = "Saída" });
                    horasTrabalhadas = CalcularHorasDoDia(todosRegistosParaCalculo);
                }

                // Chamada única e corrigida para adicionar o registo
                await _pontoRepo.AddRegistroPontoAsync(new RegistroPonto
                {
                    UsuarioId = utilizador.Cpf,
                    Horario = horarioDoPonto,
                    Tipo = tipoAtual,
                    HorasTrabalhadasDia = horasTrabalhadas,
                    Nsr = newNsr
                });

                await SendRecipt(null, tipoAtual, newNsr, horarioDoPonto);

                return Json(new
                {
                    success = true,
                    message = $"Ponto '{tipoAtual}' registado com sucesso!",
                    novoRegisto = new
                    {
                        tipo = tipoAtual,
                        horario = horarioDoPonto.ToLocalTime().ToString("HH:mm:ss")
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Ocorreu um erro no servidor: {ex.Message}" });
            }
        }

        [HttpPost] // Rota personalizada para o JavaScript
        [AllowAnonymous] // Permite que esta ação seja chamada sem login
        [ValidateAntiForgeryToken] // Protege contra CSRF
        public async Task<IActionResult> RegistrarComBiometria([FromBody] BiometriaVerificacaoRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.BiometricData))
            {
                return Json(new { success = false, message = "Dados da digital não recebidos." });
            }

            try
            {
                FeatureSet featureSetCapturado = new FeatureSet();
                byte[] featureSetBytes = Convert.FromBase64String(request.BiometricData);
                featureSetCapturado.DeSerialize(featureSetBytes);

                var todosOsUsuariosComBiometria = await _userRepo.GetAllUsersWithBiometricsAsync();

                User utilizadorIdentificado = null;
                Verification verificator = new Verification();

                foreach (var utilizador in todosOsUsuariosComBiometria)
                {
                    if (utilizador.BiometricTemplate == null || utilizador.BiometricTemplate.Length == 0) continue;

                    Template templateSalvo = new Template();
                    templateSalvo.DeSerialize(utilizador.BiometricTemplate);

                    Verification.Result result = new Verification.Result();
                    verificator.Verify(featureSetCapturado, templateSalvo, ref result);

                    if (result.Verified)
                    {
                        utilizadorIdentificado = utilizador;
                        break;
                    }
                }

                if (utilizadorIdentificado == null)
                {
                    return Json(new { success = false, message = "Impressão digital não reconhecida." });
                }

                var newNsr = await _nsrService.GerarProximoNsrAsync();
                var registrosDeHoje = await _pontoRepo.GetRegistrosDoDiaAsync(utilizadorIdentificado.Cpf);
                var ultimoRegisto = registrosDeHoje.OrderBy(r => r.Horario).LastOrDefault();

                string tipoAtual;
                if (ultimoRegisto == null)
                {
                    tipoAtual = "Entrada";
                }
                else
                {
                    switch (ultimoRegisto.Tipo)
                    {
                        case "Entrada": tipoAtual = "Saída Almoço"; break;
                        case "Saída Almoço": tipoAtual = "Volta Almoço"; break;
                        case "Volta Almoço": tipoAtual = "Saída"; break;
                        case "Saída":
                            return Json(new { success = false, message = $"Jornada de trabalho finalizada por hoje, {utilizadorIdentificado.NomeCompleto}." });
                        default:
                            return Json(new { success = false, message = "Sequência de ponto inválida. Contacte o RH." });
                    }
                }

                DateTime horarioDoPonto = DateTime.Now;
                TimeSpan? horasTrabalhadas = null;
                if (tipoAtual == "Saída")
                {
                    var todosRegistosParaCalculo = new List<RegistroPonto>(registrosDeHoje)
                    {
                        new RegistroPonto { Horario = horarioDoPonto, Tipo = "Saída" }
                    };
                    horasTrabalhadas = CalcularHorasDoDia(todosRegistosParaCalculo);
                }

                await _pontoRepo.AddRegistroPontoAsync(new RegistroPonto
                {
                    UsuarioId = utilizadorIdentificado.Cpf,
                    Horario = horarioDoPonto,
                    Tipo = tipoAtual,
                    HorasTrabalhadasDia = horasTrabalhadas,
                    Nsr = newNsr
                });

                _ = SendRecipt(utilizadorIdentificado, tipoAtual, newNsr, horarioDoPonto);

                return Json(new { success = true, message = $"Ponto '{tipoAtual}' registado para {utilizadorIdentificado.NomeCompleto} às {horarioDoPonto:HH:mm:ss}!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Ocorreu um erro inesperado no servidor." });
            }
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
        private async Task SendRecipt(User? funcionario, string tipoBatida, long nsr, DateTime horario)
        {
            try
            {
                var cpf = User.FindFirst("CPF")?.Value;
                if(string.IsNullOrEmpty(cpf))
                {
                    cpf = funcionario.Cpf;
                }
                var user = await _userRepo.GetUser(cpf);
                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "ReciboPonto.html");

                if (!System.IO.File.Exists(templatePath))
                {
                    Console.WriteLine($"Erro: Template de e-mail não encontrado em {templatePath}");
                    TempData["ErrorMessage"] = "Ponto registado, mas o template de e-mail não foi encontrado.";
                    return;
                }

                string htmlBody = await System.IO.File.ReadAllTextAsync(templatePath);

                htmlBody = htmlBody.Replace("{{NOME_EMPRESA}}", "Nome da Sua Empresa")
                                    .Replace("{{NOME_FUNCIONARIO}}", user.NomeCompleto)
                                    .Replace("{{CPF_FUNCIONARIO}}", user.Cpf)
                                    .Replace("{{DATA_REGISTRO}}", horario.ToString("dd/MM/yyyy"))
                                    .Replace("{{HORA_REGISTRO}}", horario.ToString("HH:mm:ss"))
                                    .Replace("{{TIPO_REGISTRO}}", tipoBatida)
                                    .Replace("{{NSR_REGISTRO}}", nsr.ToString())
                                    .Replace("{{DATA_ANO}}", DateTime.Now.Year.ToString());

                await _emailService.SendEmailSmtpAsync(new List<string> { user.Email }, "Seu Recibo de Registro de Ponto", htmlBody);
                TempData["SuccessMessage"] = "Ponto registado e recibo enviado com sucesso!";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar recibo de ponto: {ex.Message}");
                TempData["ErrorMessage"] = "Ponto registado, mas houve um erro ao enviar o recibo por e-mail.";
            }
        }
    }
}
