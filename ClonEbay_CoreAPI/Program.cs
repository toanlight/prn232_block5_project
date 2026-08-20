using ClonEbay_CoreAPI.Middlewares;
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

    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Đăng ký Global Exception Handler Middleware tại vị trí đầu tiên của HTTP pipeline
    app.UseGlobalExceptionHandler();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

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
