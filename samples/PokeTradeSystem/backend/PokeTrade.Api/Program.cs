using PokeTrade.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<PokeTradeStore>();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ManageWorkPlay", policy => policy.RequireAssertion(_ => true));
    options.AddPolicy("ManageDelivery", policy => policy.RequireAssertion(_ => true));
});

var app = builder.Build();
app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.Run("http://localhost:5080");
