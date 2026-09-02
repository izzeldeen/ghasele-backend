using Ghasele.Application.Interfaces;
using Ghasele.Application.Services;
using Ghasele.Domain.Interfaces;
using Ghasele.Infrastructure.Data;
using Ghasele.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models; // Corrected namespace
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;
using Ghasele.API.Middleware;
using Ghasele.API.Localization;
using Ghasele.Application.Localization;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        // Without this, Arabic message text is emitted as \uXXXX escapes.
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });

// Localization: resolves the caller's language from Accept-Language and turns ErrorCodes into text.
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IErrorLocalizer, ErrorLocalizer>();
builder.Services.AddScoped<IRequestLocalizer, RequestLocalizer>();
// Same resolution logic, exposed under the Application-layer abstraction so services there
// (DriverService, CleanerService, ItemTypeService, OrderService, TripService) can pick a
// bilingual name without depending on the API project.
builder.Services.AddScoped<ICurrentLanguageProvider, RequestLocalizer>();
// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Ghasele API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Database Configuration with dynamic provider selection
var databaseProvider = builder.Configuration["DatabaseSettings:Provider"] ?? "SqlServer";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    switch (databaseProvider.ToLower())
    {
        case "postgresql":
        case "postgres":
            var postgresConnection = builder.Configuration["DatabaseSettings:PostgreSqlConnection"];
            options.UseNpgsql(postgresConnection, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "public");
            });
            break;

        case "sqlserver":
        default:
            var sqlServerConnection = builder.Configuration["DatabaseSettings:SqlServerConnection"];
            options.UseSqlServer(sqlServerConnection);
            break;
    }
});


// Dependency Injection
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPendingRegistrationRepository, PendingRegistrationRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ITripRepository, TripRepository>();
builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<IItemTypeRepository, ItemTypeRepository>();
builder.Services.AddScoped<IItemTypeService, ItemTypeService>();
builder.Services.AddScoped<ICleanerRepository, CleanerRepository>();
builder.Services.AddScoped<ICleanerService, CleanerService>();
builder.Services.AddScoped<IUserLocationRepository, UserLocationRepository>();
builder.Services.AddScoped<IUserLocationService, UserLocationService>();
builder.Services.AddScoped<IRouteOptimizationService, RouteOptimizationService>();
builder.Services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
builder.Services.AddScoped<ISupportTicketService, SupportTicketService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IUserNotificationService, UserNotificationService>();
builder.Services.AddSingleton<INotificationService, NotificationService>();
builder.Services.AddScoped<IDriverRepository, DriverRepository>();
builder.Services.AddScoped<IDriverService, DriverService>();
builder.Services.AddScoped<IMarketingCodeRepository, MarketingCodeRepository>();
builder.Services.AddScoped<IMarketingCodeService, MarketingCodeService>();
builder.Services.AddScoped<IAppSettingsRepository, AppSettingsRepository>();
builder.Services.AddScoped<IAppSettingsService, AppSettingsService>();
builder.Services.AddScoped<IDeliveryWindowRepository, DeliveryWindowRepository>();
builder.Services.AddScoped<IDeliveryWindowService, DeliveryWindowService>();
builder.Services.AddSingleton<IFileStorageService, Ghasele.API.Services.LocalFileStorageService>();
// ---------------------------------------------------------------------------
// Firebase Admin SDK
// ---------------------------------------------------------------------------
// Created once for the whole process, here, before anything resolves it. Two services share this
// single default app: NotificationService (FCM push) and FirebaseAuthService (phone-auth token
// verification). FirebaseApp.Create throws if a default app already exists, which is why this is
// centralized rather than done inside each service.
var firebaseCredentialsPath = builder.Configuration["Firebase:CredentialsPath"];

if (!string.IsNullOrWhiteSpace(firebaseCredentialsPath))
{
    // Resolve against the content root so one relative setting works whether the API is launched
    // from the project folder or from a published output directory.
    var resolvedFirebasePath = Path.IsPathRooted(firebaseCredentialsPath)
        ? firebaseCredentialsPath
        : Path.Combine(builder.Environment.ContentRootPath, firebaseCredentialsPath);

    if (File.Exists(resolvedFirebasePath))
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(resolvedFirebasePath)
            });
        }
    }
    else
    {
        // Deliberately not fatal: FCM push already degrades to simulated sends without it, and
        // failing startup would take the whole API down over an optional file. Firebase login
        // returns 401 while this is unresolved, and FirebaseAuthService logs the reason.
        Console.WriteLine($"[FIREBASE] Credentials file not found at '{resolvedFirebasePath}'. " +
                          "Firebase login will be unavailable and FCM notifications simulated.");
    }
}

// Verifies Firebase ID tokens for the phone-auth login endpoint. Registered independently of the
// WhatsApp OTP services above, which stay in place as the fallback sign-up path.
builder.Services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();

builder.Services.AddHttpClient<Ghasele.API.Controllers.WhatsAppTestController>();
var whatsAppAccessToken = builder.Configuration["WhatsApp:AccessToken"];

if (!string.IsNullOrWhiteSpace(whatsAppAccessToken))
{
    builder.Services.AddHttpClient<IWhatsAppService, Ghasele.Infrastructure.Services.WhatsAppGraphApiService>(
        client => client.Timeout = TimeSpan.FromSeconds(15));
}
else
{
    // No WhatsApp credentials configured (e.g. local development) - fall back to console logging.
    builder.Services.AddScoped<IWhatsAppService, Ghasele.Infrastructure.Services.MockWhatsAppService>();
}

// JWT Authentication Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret is missing");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

var app = builder.Build();

// Initialize Database
using (var scope = app.Services.CreateScope())
{
    try 
    {
        var context = scope.ServiceProvider.GetRequiredService<Ghasele.Infrastructure.Data.ApplicationDbContext>();
        Console.WriteLine("Applying database migrations...");
        context.Database.Migrate();
        Console.WriteLine("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization failed: {ex.Message}");
    }
}


// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");

// Serves customer-uploaded support photos from wwwroot/uploads/**.
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
