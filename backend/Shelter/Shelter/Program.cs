using Application.Interfaces;
using Application.Services;
using Application.Settings;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Abstractions;
using Infrastructure.Repositories.Abstractions.Adoptions;
using Infrastructure.Repositories.Adoptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shelter.API.Middleware;
using System.Text;
using System.Text.Json.Serialization;

//stripe listen --forward-to https://localhost:7191/api/stripe/webhook

var builder = WebApplication.CreateBuilder(args);

// === Controllers + JSON (enumy jako stringi)
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// === DbContext (SQL Server)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// === Stripe
//To rejestracja konfiguracji Stripe w DI
builder.Services.Configure<StripeSettings>(
    //pobiera cala sekcje Stripe z konfiguracji (appsettings.json) (czyli te 3 klucze)
    //mapuje te sekcje na Twoj¹ klasê StripeSettings
    builder.Configuration.GetSection("Stripe"));
//To ustawia globalny klucz API Stripe SDK (.NET) —
//czyli mówi bibliotece Stripe, jakim kluczem autoryzowaæ wszystkie ¿¹dania
Stripe.StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];


// === DI
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAnimalService, AnimalService>();
builder.Services.AddScoped<ISpeciesService, SpeciesService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMedicalRecordService, MedicalRecordService>();
builder.Services.AddScoped<IDonationService, DonationService>();
// Adopcja
builder.Services.AddScoped<IAdoptionApplicationRepository, AdoptionApplicationRepository>();
builder.Services.AddScoped<IAdoptionStatusRepository, AdoptionStatusRepository>();
builder.Services.AddScoped<IHomeVisitResultRepository, HomeVisitResultRepository>();
builder.Services.AddScoped<IAdoptionContractRepository, AdoptionContractRepository>();
builder.Services.AddScoped<IAdoptionUnitOfWork, AdoptionUnitOfWork>();
builder.Services.AddScoped<IAdoptionProcessService, AdoptionProcessService>();

// === OpenAPI
builder.Services.AddOpenApi();

// === CORS dla Angular (lepiej jawnie z originem)
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("dev", p => p
        .AllowAnyHeader()
        .AllowAnyMethod()
        .WithOrigins("http://localhost:4200")); 
});

// === Identity
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(opt =>
    {
        opt.Password.RequireDigit = false;
        opt.Password.RequireNonAlphanumeric = false;
        opt.Password.RequireUppercase = false;
        opt.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// === JWT
var jwt = builder.Configuration.GetSection("Jwt");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // PROD: true
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateLifetime = true
        };
    });
builder.Environment.WebRootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
var app = builder.Build();

// === Migracje + seedy (domena + identity)
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var db = sp.GetRequiredService<AppDbContext>();
    var env = sp.GetRequiredService<IHostEnvironment>();

    await db.Database.MigrateAsync(); // <- najpierw migracje

    // jeœli pliki le¿¹ w Infrastructure/Data/SeedData
    var seedDir = Path.GetFullPath(Path.Combine(env.ContentRootPath, "..", "Infrastructure", "Data", "SeedData"));
    await Seeder.SeedAsync(db, seedDir);

    // role + admin
    await IdentitySeeder.SeedAsync(app.Services);
}

// === Pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseStaticFiles(); 

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("dev");

app.UseAuthentication();
app.UseAuthorization();

// Globalna obs³uga wyj¹tków
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();
