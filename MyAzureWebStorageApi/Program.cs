using Microsoft.EntityFrameworkCore;
using MyAzureWebStorageApi.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
string connection = builder.Configuration.GetConnectionString("ConnectionStringAzure");

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AzureTestContext>(options => { options.UseSqlServer(connection); });
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
