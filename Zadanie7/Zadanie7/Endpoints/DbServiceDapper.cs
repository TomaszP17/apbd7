using System.Data;
using System.Data.SqlClient;
using Dapper;
using Zadanie7.Models;

namespace Zadanie7.Endpoints;

public interface IDbServiceDapper
{
    Task<Product?> GetProduct(int id);
    Task<Warehouse?> GetWarehouse(int id);
    Task<Order?> GetOrder(int idProduct, int amount, DateTime date);
    Task<Product_Warehouse> AddProductToWarehouse(Product_Warehouse productWarehouse);
    Task<Product_Warehouse?> GetProductWarehouseById(int id);
    Task UpdateOrderFulfilledAt(int id);
    Task InsertProductWareHouse(Product_Warehouse productWarehouse);
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
        var result = await connection.QueryFirstOrDefaultAsync<Order>("SELECT * FROM Order WHERE IdProduct = @idProduct AND Amount >= @amount AND CreatedAt <= @date", new {idProduct, amount, date});
        return result;
    }
    
    public async Task<Product_Warehouse?> GetProductWarehouseById(int id)
    {
        await using var connection = await GetConnection();
        var result = await connection.QueryFirstOrDefaultAsync<Product_Warehouse>("SELECT * FROM Product_Warehouse WHERE IdOrder = @id", new {id});
        return result;
    }

    public async Task UpdateOrderFulfilledAt(int idOrder)
    {
        await using var connection = await GetConnection();
        await connection.ExecuteAsync("UPDATE Order SET FulfilledAt = @date WHERE IdOrder = @idOrder", new {date = DateTime.Now,idOrder});
    }

    public async Task InsertProductWarehouse(Product_Warehouse productWarehouse)
    {
        await using var connection = await GetConnection();
        var product = await GetProduct(productWarehouse.IdProduct);
        productWarehouse.Price = product.Price * productWarehouse.Amount;
        productWarehouse.CreatedAt = DateTime.Now;
        await connection.ExecuteAsync("INSERT INTO Product_Warehouse (IdProduct, IdWarehouse, Amount, Price, CreatedAt) VALUES (@IdProduct, @IdWarehouse, @Amount, @Price, @CreatedAt)", productWarehouse);
    }

    public async Task<Product_Warehouse> AddProductToWarehouse(Product_Warehouse productWarehouse)
    {
        await using var connection = await GetConnection();

        try
        {
            //1
            var product = await GetProduct(productWarehouse.IdProduct);
            if (product == null)
            {
                throw new Exception("Product does not exist");
            }
            
            var warehouse = await GetWarehouse(productWarehouse.IdWarehouse);
            if (warehouse == null)
            {
                throw new Exception("Warehouse does not exist");
            }
            
            var amount = productWarehouse.Amount;
            if (amount <= 0)
            {
                throw new Exception("Amount must be greater than 0");
            }
            //2 
            if (await GetOrder(productWarehouse.IdProduct, productWarehouse.Amount, productWarehouse.CreatedAt) == null)
            {
                throw new Exception("Order does not exist");
            }
            //3
            var orderOnWarehouse = await GetProductWarehouseById(productWarehouse.IdOrder);
            if (orderOnWarehouse != null)
            {
                throw new Exception("Order already exists");
            }
            //4
            await UpdateOrderFulfilledAt(productWarehouse.IdOrder);
            //5
            await InsertProductWarehouse(productWarehouse);
            //6
            
        }
        catch(Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    
}

