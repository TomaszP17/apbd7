using FluentValidation;
using Zadanie7.DTOs;
using Zadanie7.Models;
using Zadanie7.Services;

namespace Zadanie7.Endpoints;

public static class ProductWarehouseEndpoints
{
    public static void RegisterProductWarehouseEndpoints(this WebApplication application)
    {
        var productWarehouse = application.MapGroup("productwarehouse-dapper");
        productWarehouse.MapPost("", AddProductWarehouse);
    }

    private static async Task<IResult> AddProductWarehouse(
        ProductWarehouseDTO productWarehouseDTO,
        IDbServiceDapper dbServiceDapper,
        IValidator<ProductWarehouseDTO> validator)
    {
        var validate = await validator.ValidateAsync(productWarehouseDTO);
        if (!validate.IsValid)
        {
            return Results.ValidationProblem(validate.ToDictionary());
        }
        return await dbServiceDapper.AddProductToWarehouse(
            new ProductWarehouse
            {
                IdProduct = productWarehouseDTO.IdProduct,
                IdWarehouse = productWarehouseDTO.IdWarehouse,
                Amount = productWarehouseDTO.Amount,
                CreatedAt = productWarehouseDTO.CreatedAt
            });
    }
}