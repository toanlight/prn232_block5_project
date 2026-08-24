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
    builder.Services.AddScoped<ClonEbay_CoreAPI.Repositories.Interfaces.IAddressRepository, ClonEbay_CoreAPI.Repositories.Implementations.AddressRepository>();
    builder.Services.AddScoped<ClonEbay_CoreAPI.Repositories.Interfaces.IReturnRequestRepository, ClonEbay_CoreAPI.Repositories.Implementations.ReturnRequestRepository>();
    builder.Services.AddScoped<ClonEbay_CoreAPI.Repositories.Interfaces.IReviewRepository, ClonEbay_CoreAPI.Repositories.Implementations.ReviewRepository>();
    builder.Services.AddScoped<ClonEbay_CoreAPI.Repositories.Interfaces.ICouponRepository, ClonEbay_CoreAPI.Repositories.Implementations.CouponRepository>();

    // 3. Đăng ký Services (Business Logic Layer)
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<IJwtService, JwtService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IAddressService, AddressService>();
    builder.Services.AddScoped<IReturnRequestService, ReturnRequestService>();
    builder.Services.AddScoped<IReviewService, ReviewService>();
    builder.Services.AddScoped<ICouponService, CouponService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();

    // 4. Đăng ký SignalR Real-time Services
    builder.Services.AddSignalR();

    // 5. Cấu hình JWT Authentication (hỗ trợ cả HTTP Headers và SignalR WebSocket Query String)
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

        // Đọc token từ query string `access_token` khi kết nối SignalR WebSockets
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notification"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAuthorization();

    // 6. Cấu hình CORS (AllowCredentials cho SignalR)
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });

    // Add controllers
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // 7. Cấu hình Swagger với JWT Bearer Authorization
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "CloneEbay Core API",
            Version = "v1",
            Description = "API hệ thống E-Commerce CloneEbay (Auth, JWT, Products, Orders, SignalR Notifications,...)"
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
    app.MapHub<ClonEbay_CoreAPI.Hubs.NotificationHub>("/hubs/notification");

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
