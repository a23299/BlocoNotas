using BlocoNotas.ApiEmail.Entities;
using BlocoNotas.ApiEmail.Services;
using BlocoNotas.Data;
using BlocoNotas.Hubs;
using BlocoNotas.Models;
using BlocoNotas.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddSingleton<ISendEmail, SendEmail>();

builder.Services.AddRazorPages();
builder.Services.AddSignalR();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"), sqliteOptions =>
    {
        sqliteOptions.MigrationsAssembly("BlocoNotas");
    }));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

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

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api") &&
            context.Response.StatusCode == 200)
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api") &&
            context.Response.StatusCode == 200)
        {
            context.Response.StatusCode = 403;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<TokenService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

var app = builder.Build();

using var scope = app.Services.CreateScope();

var services = scope.ServiceProvider;
var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

if (await context.Database.CanConnectAsync())
{
    Console.WriteLine("✅ Conexão à base SQLite OK");
}
else
{
    Console.WriteLine("❌ Falha na conexão à base SQLite");
}
await SeedRoles.CreateRolesAsync(services);
async Task CreateInitialUsers(IServiceProvider serviceProvider)
{
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    if (!await roleManager.RoleExistsAsync("Utilizador"))
        await roleManager.CreateAsync(new IdentityRole("Utilizador"));

    var users = new (string UserName, string Email, string Password, string Role)[]
    {
        ("admin", "admin@admin.com", "Admin123!", "Admin"),
        ("aluno23299", "aluno23299@ipt.pt", "Aluno23299!", "Utilizador"),
        ("aluno23037", "aluno23037@ipt.pt", "Aluno23037!", "Utilizador"),
        ("teste", "teste@teste.com", "Teste123!", "Utilizador")
    };

    foreach (var (userName, email, password, role) in users)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing != null)
        {
            Console.WriteLine($"Utilizador '{userName}' já existe.");
            continue;
        }

        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
            Console.WriteLine($"Utilizador '{userName}' criado com sucesso (role: {role}).");
        }
        else
        {
            Console.WriteLine($"Falha ao criar '{userName}':");
            foreach (var error in result.Errors)
                Console.WriteLine($" - {error.Description}");
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await CreateInitialUsers(services);

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();
app.MapRazorPages();
app.MapHub<NoteHub>("/noteHub");

app.Run();
