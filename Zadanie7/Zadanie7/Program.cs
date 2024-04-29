using FluentValidation;
using Zadanie7.Endpoints;
using Zadanie7.Services;
using Zadanie7.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IDbServiceDapper, DbServiceDapper>();

builder.Services.AddValidatorsFromAssemblyContaining<ProductWarehouseValidator>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.RegisterProductWarehouseEndpoints();
app.Run();