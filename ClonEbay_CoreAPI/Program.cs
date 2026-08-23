using System.Text;
using ClonEbay_CoreAPI.Middlewares;
using ClonEbay_CoreAPI.Models;
using ClonEbay_CoreAPI.Services.Implementations;
using ClonEbay_CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Bắt đầu khởi chạy ứng dụng ClonEbay_CoreAPI...");
    var builder = WebApplication.CreateBuilder(args);

    // Cấu hình Serilog đọc từ appsettings.json
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // 1. Cấu hình DbContext
    builder.Services.AddDbContext<CloneEbayDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=.\\SQLEXPRESS;Database=CloneEbayDB;User Id=sa;Password=123;TrustServerCertificate=True";
        options.UseSqlServer(connectionString);
    });

    // 2. Đăng ký Repositories (Data Access Layer)
    builder.Services.AddScoped(typeof(ClonEbay_CoreAPI.Repositories.Interfaces.IGenericRepository<>), typeof(ClonEbay_CoreAPI.Repositories.Implementations.GenericRepository<>));
    builder.Services.AddScoped<ClonEbay_CoreAPI.Repositories.Interfaces.IUserRepository, ClonEbay_CoreAPI.Repositories.Implementations.UserRepository>();

    // 3. Đăng ký Services (Business Logic Layer)
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<IJwtService, JwtService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IUserService, UserService>();

    // 4. Cấu hình JWT Authentication
    var jwtSecretKey = builder.Configuration["JwtSettings:SecretKey"] 
        ?? "YourSuperSecretCloneEbayPRN232SecretKeyForJwtAuthenticationWhichIsVeryLongAndSecure!";
    var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "CloneEbay_CoreAPI";
    var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "CloneEbay_Clients";

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddAuthorization();

    // 4. Cấu hình CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // Add controllers
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // 5. Cấu hình Swagger với JWT Bearer Authorization
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "CloneEbay Core API",
            Version = "v1",
            Description = "API hệ thống E-Commerce CloneEbay (Auth, JWT, Products, Orders,...)"
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "Nhập token theo định dạng: Bearer {token}",
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
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    // Đăng ký Global Exception Handler Middleware đầu tiên
    app.UseGlobalExceptionHandler();

    // Configure HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "CloneEbay API v1");
            c.RoutePrefix = "swagger";
        });
    }

    app.UseCors("AllowAll");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Ứng dụng bị dừng đột ngột do lỗi khởi động!");
}
finally
{
    Log.CloseAndFlush();
}
