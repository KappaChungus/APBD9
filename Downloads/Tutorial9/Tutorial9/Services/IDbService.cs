using Tutorial9.Model;

namespace Tutorial9.Services;

public interface IDbService
{
    Task<bool> CheckIfProductExists(int idProduct);
    Task<bool> CheckIfWarehouseExists(int idWarehouse);
    Task<int> GetMatchingOrderId(int idProduct, int amount, DateTime createdAt);
    
    Task<bool> CheckIfOrderWasRealized(int idOrder);
    
    Task UpdateOrder(int orderID);
    Task<int> InsertProductWarehouse(int idProduct, int idWarehouse, int idOrder,int amount);

    Task<int> AddProductToWarehouse(int idProduct, int idWarehouse, int amount, DateTime createdAt);


}