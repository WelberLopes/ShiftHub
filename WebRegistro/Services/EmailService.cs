using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using WebRegistro.Models;

namespace WebRegistro.Services
{
    public class EmailService 
    {

            private readonly EmailConfig _config;
            private readonly string _credentialsFullPath;
            private readonly string _tokenPath;

            public EmailService(IOptions<EmailConfig> config)
            {
                _config = config.Value ?? throw new ArgumentNullException(nameof(config));
                _credentialsFullPath = Path.Combine(AppContext.BaseDirectory, _config.CredentialsPath);
                _tokenPath = Path.Combine(
                   Environment.GetEnvironmentVariable("HOME") ?? AppContext.BaseDirectory,
                   "AppSecrets", "token.json"
               );
            }
            public async Task SendEmailSmtpAsync(List<string> destinatarios, string assunto, string corpo, List<string> anexos = null)
            {
                try
                {
                    var mail = new MailMessage
                    {
                        From = new MailAddress(_config.Email),
                        Subject = assunto,
                        Body = corpo,
                        IsBodyHtml = true
                    };

                    foreach (var to in destinatarios)
                    {
                        mail.To.Add(to);
                    }

                    if (anexos != null)
                    {
                        foreach (var anexo in anexos)
                        {
                            if (File.Exists(anexo))
                                mail.Attachments.Add(new Attachment(anexo));
                        }
                    }

                    using var smtp = new SmtpClient("smtp.gmail.com", 587)
                    {
                        EnableSsl = true,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(_config.Email, _config.Senha) // senha de app
                    };

                    await smtp.SendMailAsync(mail);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao enviar e-mail por SMTP: {ex.Message}");
                    throw;
                }
            }

        }
    }