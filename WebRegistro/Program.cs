using Microsoft.EntityFrameworkCore;
using WebRegistro.Data;
using WebRegistro.Models;
using WebRegistro.Repository;
using WebRegistro.Repository.Interfaces;
using WebRegistro.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using WebRegistro.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("AppSecrets/emailSecrets.json", optional: true, reloadOnChange: true);

builder.Services.Configure<EmailConfig>(config =>
{
    builder.Configuration.GetSection("EmailConfig").Bind(config);
    config.Senha = builder.Configuration["EmailSecrets:Senha"];
});

builder.Services.AddScoped<EmailService>();

// Configuração de cache e sessão
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Caminho para onde o [Authorize] redirecionará se o usuário não estiver logado
        options.LoginPath = "/Login/Login";
        options.AccessDeniedPath = "/Login/AccessDenied"; // Página para acesso negado
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<NsrService>();
builder.Services.AddScoped<IRegistroLogRepository, RegistroLogRepository>();
builder.Services.AddScoped<IPontoRepository, PontoRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILoginRepository, LoginRepository>();
builder.Services.AddScoped<IEscalaRepository, EscalaRepository>();
builder.Services.AddScoped<IDepartamentoRepository, DepartamentoRepository>();
builder.Services.AddScoped<IFechamentoMensalService, FechamentoMensalService>();
builder.Services.AddTransient<EmailService>();
builder.Services.AddScoped<IBancoDeHorasRepository, BancoDeHorasRepository>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        
        // Pega o contexto do banco de dados que você configurou
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Aplica qualquer migração pendente. Cria o banco se ele não existir.
        context.Database.Migrate();

         await DbInitializer.InitializeAsync(services); 
    }
    catch (Exception ex)
    {
        // Se algo der errado, registra o erro.
        // Em um app real, você usaria um sistema de log mais robusto.
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocorreu um erro durante a migração do banco de dados.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();
