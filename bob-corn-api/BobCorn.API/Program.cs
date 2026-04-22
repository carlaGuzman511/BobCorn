using BobCorn.Application.Abstractions.Persistence;
using BobCorn.Application.Abstractions.Time;
using BobCorn.Application.UseCases.PurchaseCorn;
using BobCorn.Infrastructure.Adapters;
using BobCorn.Infrastructure.Time;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<ICornPurchaseRepository, CornPurchaseRepository>();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<PurchaseCornHandler>();
builder.Services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>(); 

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseHttpsRedirection();

app.UseCors("AllowWebApp"); 

app.UseAuthorization();

app.MapControllers();
app.UseAuthorization();

app.MapControllers();

app.Run();
