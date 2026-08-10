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

app.MapControllers();

app.Run();