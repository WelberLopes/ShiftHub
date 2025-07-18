using Microsoft.AspNetCore.Mvc;
using WebRegistro.Repository.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;

// Adicione os namespaces do SDK da Digital Persona e dos seus ViewModels
using DPFP;
using DPFP.Verification;
using System;
using WebRegistro.Models;

namespace WebRegistro.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILoginRepository _context;

        public LoginController(ILoginRepository context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string cpf, string password)
        {
            if (string.IsNullOrEmpty(cpf) || string.IsNullOrEmpty(password))
            {
                return BadRequest(new { mensagem = "CPF ou senha não informados." });
            }
            var user = _context.GetUser(cpf);

            if (user == null)
            {
                return BadRequest(new { mensagem = "Cpf inválido." });
            }

            if (!_context.VerifyPassword(cpf, password))
            {
                return BadRequest(new { mensagem = "Senha incorreta." });
            }

            var claims = new List<Claim>
            {
                new Claim("CPF", user.Cpf),
                new Claim(ClaimTypes.Name, user.NomeCompleto),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("Cargo", user.Cargo)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            string redirectUrl = user.Role == "Admin" ? Url.Action("Index", "Admin") : Url.Action("Index", "Ponto");
            return Ok(new { redirectUrl });
        }

        /// <summary>
        /// NOVA AÇÃO: Recebe uma digital capturada, encontra o utilizador correspondente
        /// e realiza o login.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> VerificarBiometriaLogin([FromBody] BiometriaVerificacaoRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.BiometricData))
            {
                return Json(new { success = false, message = "Dados da digital não recebidos." });
            }

            try
            {
                var utilizadoresComBiometria = await _context.GetAllUsersWithBiometrics();

                FeatureSet featureSetCapturado = new FeatureSet();
                byte[] featureSetBytes = Convert.FromBase64String(request.BiometricData);
                featureSetCapturado.DeSerialize(featureSetBytes);

                Verification verificator = new Verification();
                foreach (var utilizador in utilizadoresComBiometria)
                {
                    if (utilizador.BiometricTemplate == null || utilizador.BiometricTemplate.Length == 0) continue;

                    Template templateSalvo = new Template();
                    templateSalvo.DeSerialize(utilizador.BiometricTemplate);

                    Verification.Result result = new Verification.Result();
                    verificator.Verify(featureSetCapturado, templateSalvo, ref result);

                    if (result.Verified)
                    {
                        // SUCESSO! Utilizador encontrado. Realiza o login.
                        var claims = new List<Claim>
                        {
                            new Claim("CPF", utilizador.Cpf),
                            new Claim(ClaimTypes.Name, utilizador.NomeCompleto),
                            new Claim(ClaimTypes.Role, utilizador.Role),
                            new Claim("Cargo", utilizador.Cargo)
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                        string redirectUrl = utilizador.Role == "Admin" ? Url.Action("Index", "Admin") : Url.Action("Index", "Ponto");
                        return Json(new { success = true, redirectUrl = redirectUrl });
                    }
                }

                return Json(new { success = false, message = "Impressão digital não reconhecida." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Ocorreu um erro no servidor: {ex.Message}" });
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}