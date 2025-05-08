using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Tutorial9.Model;

namespace Tutorial9.Services;

public class DbService : IDbService
{
    private readonly IConfiguration _configuration;
    public DbService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task<bool> CheckIfProductExists(int idProduct)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand(
            "SELECT COUNT(*) FROM Product WHERE IdProduct = @IdProduct"
            , connection);
        command.Parameters.AddWithValue("@IdProduct", idProduct);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        int count = Convert.ToInt32(result);
        return count > 0;
    }

    public async Task<Boolean> CheckIfWarehouseExists(int idWarehouse)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand(
            "SELECT COUNT(*) FROM Warehouse WHERE @IdWareHouse = IdWareHouse ",
            connection);
        
        command.Parameters.AddWithValue("@IdWareHouse", idWarehouse);
        await connection.OpenAsync();
        var result = Convert.ToInt32(await command.ExecuteScalarAsync());
        return result > 0;
    }

    public async Task<int> GetMatchingOrderId(int idProduct, int amount, DateTime createdAt)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand(
            @"SELECT TOP 1 IdOrder 
          FROM [Order] 
          WHERE IdProduct = @IdProduct 
            AND Amount = @Amount 
            AND CreatedAt < @CreatedAt", 
            connection
        );

        command.Parameters.AddWithValue("@IdProduct", idProduct);
        command.Parameters.AddWithValue("@Amount", amount);
        command.Parameters.AddWithValue("@CreatedAt", createdAt);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        if (result != null && int.TryParse(result.ToString(), out int idOrder))
            return idOrder;

        return -1;
    }

    public async Task<bool> CheckIfOrderWasRealized(int idOrder)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand(
            "SELECT COUNT(*) FROM Product_Warehouse WHERE IdOrder = @IdOrder",
            connection
            );
        
        command.Parameters.AddWithValue("@IdOrder", idOrder);
        
        await connection.OpenAsync();
        var result = Convert.ToInt32(await command.ExecuteScalarAsync());
        
        return result > 0;
    }

    public async Task UpdateOrder(int orderID)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand("UPDATE [ORDER] SET FulfilledAt = @FulfilledAt WHERE IdOrder = @IdOrder", connection);
        await connection.OpenAsync();
        command.Parameters.AddWithValue("@IdOrder", orderID);
        command.Parameters.AddWithValue("@FulfilledAt", DateTime.Now);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<int> InsertProductWarehouse(int idProduct, int idWarehouse, int idOrder, int amount)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await connection.OpenAsync();

        await using var getPriceCommand = new SqlCommand("SELECT Price FROM Product WHERE IDProduct = @IdProduct", connection);
        getPriceCommand.Parameters.AddWithValue("@IdProduct", idProduct);
        double price = Convert.ToDouble(await getPriceCommand.ExecuteScalarAsync());

        await using var insertCommand = new SqlCommand(
            @"INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt) 
          VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt); 
          SELECT SCOPE_IDENTITY();", connection
        );

        insertCommand.Parameters.AddWithValue("@IdWarehouse", idWarehouse);
        insertCommand.Parameters.AddWithValue("@IdProduct", idProduct);
        insertCommand.Parameters.AddWithValue("@IdOrder", idOrder);
        insertCommand.Parameters.AddWithValue("@Amount", amount);
        insertCommand.Parameters.AddWithValue("@Price", price * amount);
        insertCommand.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

        var result = await insertCommand.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<int> AddProductToWarehouse(int idProduct, int idWarehouse, int amount, DateTime createdAt)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand("AddProductToWarehouse", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@IdProduct", idProduct);
        command.Parameters.AddWithValue("@IdWarehouse", idWarehouse);
        command.Parameters.AddWithValue("@Amount", amount);
        command.Parameters.AddWithValue("@CreatedAt", createdAt);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

}