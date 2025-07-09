using BlocoNotas.Data;
using BlocoNotas.Models;
using BlocoNotas.ApiEmail.Entities;
using BlocoNotas.ApiEmail.Services;
using BlocoNotas.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configurações do SMTP para envio de emails
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddSingleton<ISendEmail, SendEmail>();

// MVC com Views
builder.Services.AddControllersWithViews();

// Configurar DbContext com SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"), sqliteOptions =>
    {
        sqliteOptions.MigrationsAssembly("BlocoNotas");
        // Não usar UseForeignKeys() pois não existe nesse contexto
    }));

// Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Muda para true se quiseres confirmação de email
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configuração de autenticação JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Autorização
builder.Services.AddAuthorization();

// Serviço de Tokens
builder.Services.AddScoped<TokenService>();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi(); // Se estiveres a usar algum pacote OpenAPI extra

// Build da app
var app = builder.Build();

// Escopo para DB (testar conexão)
using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

if (await context.Database.CanConnectAsync())
{
    Console.WriteLine("✅ Conexão à base SQLite OK");
}
else
{
    Console.WriteLine("❌ Falha na conexão à base SQLite");
}

// Pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
