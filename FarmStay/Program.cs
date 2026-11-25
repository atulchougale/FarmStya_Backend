using FarmStay.API.DependencyInjection;
using FarmStay.API.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------
// Logging
// -------------------------------------
builder.Host.AddSerilogServices();

// -------------------------------------
// Framework Services
// -------------------------------------
builder.Services.AddControllers();
builder.Services.AddRateLimiter(options => { options.RejectionStatusCode=429; options.AddPolicy("Accounts", context =>
    System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
        context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
        _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions {
            PermitLimit = 120, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 })); });

builder.Services.AddCorsServices();

builder.Services.AddFluentValidationServices();

builder.Services.AddSwaggerServices();

// -------------------------------------
// Application & Infrastructure
// -------------------------------------
builder.Services.AddApplicationServices();

builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddAuthenticationServices(builder.Configuration);

var app = builder.Build();

// -------------------------------------
// Swagger
// -------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

// -------------------------------------
// Logging
// -------------------------------------
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
});

// -------------------------------------
// Middleware Pipeline
// -------------------------------------
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("AllowAngular");

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthentication();

app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

app.Run();
