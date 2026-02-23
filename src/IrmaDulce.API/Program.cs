using System.Text;
using IrmaDulce.Infrastructure;
using IrmaDulce.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ========== Services ==========

// Infrastructure (DbContext + Repositories + Application Services)
builder.Services.AddInfrastructure(builder.Configuration);

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "IrmaDulce-SecretKey-Dev-2026-SuperSegura-256bits!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "IrmaDulce.API";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "IrmaDulce.Client";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Irma Dulce API",
        Version = "v1",
        Description = "API do Sistema de Gestão Acadêmica - Escola de Enfermagem Irma Dulce"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. Exemplo: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// ========== Middleware ==========

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Apply pending migrations and seed Master user (dev only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // Seed: Cria usuário Master se não existir
    if (!db.Usuarios.Any(u => u.Perfil == IrmaDulce.Domain.Enums.PerfilUsuario.Master))
    {
        var pessoaMaster = new IrmaDulce.Domain.Entities.Pessoa
        {
            NomeCompleto = "Administrador Master",
            CPF = "000.000.000-00",
            RG = "0000000",
            IdFuncional = "AD0001",
            Perfil = IrmaDulce.Domain.Enums.PerfilUsuario.Master,
            DataNascimento = new DateTime(1990, 1, 1),
            Logradouro = "Sistema",
            Numero = "0",
            CEP = "00000-000",
            Bairro = "Sistema",
            Cidade = "Sistema",
        };
        db.Pessoas.Add(pessoaMaster);
        db.SaveChanges();

        var usuarioMaster = new IrmaDulce.Domain.Entities.Usuario
        {
            PessoaId = pessoaMaster.Id,
            Login = "admin",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Perfil = IrmaDulce.Domain.Enums.PerfilUsuario.Master,
        };
        db.Usuarios.Add(usuarioMaster);
        db.SaveChanges();

        Console.WriteLine("✅ Usuário Master criado: login='admin' senha='123456'");
    }
}

app.Run();
