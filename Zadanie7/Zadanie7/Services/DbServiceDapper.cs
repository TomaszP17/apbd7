using System.Data;
using System.Data.SqlClient;
using Dapper;
using Zadanie7.Models;

namespace Zadanie7.Services;

public interface IDbServiceDapper
{
    Task<Product?> GetProduct(int id);
    Task<Warehouse?> GetWarehouse(int id);
    Task<Order?> GetOrder(int idProduct, int amount, DateTime date);
    Task<IResult> AddProductToWarehouse(ProductWarehouse productWarehouse);
    Task<ProductWarehouse?> GetProductWarehouseById(int id);
    Task UpdateOrderFulfilledAt(int id);
    Task<int> InsertProductWarehouse(ProductWarehouse productWarehouse, int idOrder);
}

public class DbServiceDapper(IConfiguration configuration) : IDbServiceDapper
{
    private async Task<SqlConnection> GetConnection()
    {
        var connection = new SqlConnection(configuration.GetConnectionString("Default"));
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }
        return connection;
    }

    public async Task<Product?> GetProduct(int id)
    {
        await using var connection = await GetConnection();
        var result = await connection.QueryFirstOrDefaultAsync<Product>("SELECT * FROM Product WHERE IdProduct = @id", new {id});
        return result;
    }

    public async Task<Warehouse?> GetWarehouse(int id)
    {
        await using var connection = await GetConnection();
        var result = await connection.QueryFirstOrDefaultAsync<Warehouse>("SELECT * FROM Warehouse WHERE IdWarehouse = @id", new {id});
        return result;
    }

    public async Task<Order?> GetOrder(int idProduct, int amount, DateTime date)
    {
        await using var connection = await GetConnection();
        var result = await connection.QueryFirstOrDefaultAsync<Order>(
            "SELECT * FROM [Order] WHERE IdProduct = @idProduct AND Amount >= @amount AND CreatedAt < @date",
            new {idProduct, amount, date});
        return result;
    }
    
    public async Task<ProductWarehouse?> GetProductWarehouseById(int id)
    {
        await using var connection = await GetConnection();
        var result = await connection.QueryFirstOrDefaultAsync<ProductWarehouse>("SELECT * FROM Product_Warehouse WHERE IdOrder = @id", new {id});
        return result;
    }

    public async Task UpdateOrderFulfilledAt(int idOrder)
    {
        await using var connection = await GetConnection();
        await connection.ExecuteAsync("UPDATE [Order] SET FulfilledAt = @date WHERE IdOrder = @idOrder", new {date = DateTime.Now,idOrder});
    }

    public async Task<int> InsertProductWarehouse(ProductWarehouse productWarehouse, int idOrder)
    {
        await using var connection = await GetConnection();

        var product = await GetProduct(productWarehouse.IdProduct);
        
        productWarehouse.Price = product.Price * productWarehouse.Amount;
        productWarehouse.CreatedAt = DateTime.Now;
        
        var newId = await connection.QuerySingleAsync<int>(
            "INSERT INTO Product_Warehouse (IdProduct, IdWarehouse, Amount, Price, CreatedAt, IdOrder) " +
            "VALUES (@IdProduct, @IdWarehouse, @Amount, @Price, @CreatedAt, @IdOrder); " +
            "SELECT CAST(SCOPE_IDENTITY() AS INT)", 
            new { 
                productWarehouse.IdProduct, 
                productWarehouse.IdWarehouse, 
                productWarehouse.Amount, 
                productWarehouse.Price, 
                productWarehouse.CreatedAt, 
                idOrder 
            });

        productWarehouse.IdProductWarehouse = newId;
        
        return newId;
    }

    public async Task<IResult> AddProductToWarehouse(ProductWarehouse productWarehouse)
    {
        await using var connection = await GetConnection();
        try
        {
            //1
            var product = await GetProduct(productWarehouse.IdProduct);
            if (product == null)
            {
                return Results.NotFound("Product does not exist");
            }
            var warehouse = await GetWarehouse(productWarehouse.IdWarehouse);
            if (warehouse == null)
            {
                return Results.NotFound("Warehouse does not exist");
            }
            //2 
            var order = await GetOrder(productWarehouse.IdProduct, productWarehouse.Amount, productWarehouse.CreatedAt);
            if (order == null)
            {
                return Results.NotFound("Order does not exist");
            }
            //3
            var orderOnWarehouse = await GetProductWarehouseById(productWarehouse.IdOrder);
            if (orderOnWarehouse != null)
            {
                return Results.Conflict("Order already exists");
            }
            //4
            await UpdateOrderFulfilledAt(productWarehouse.IdOrder);
            //5
            var productWarehouseId = await InsertProductWarehouse(productWarehouse, order.IdOrder);
            //6
            return Results.Created("ProductWarehouse added", productWarehouseId);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Results.Problem("Problem with adding new ProductWarehouse");
        }
    }
}

