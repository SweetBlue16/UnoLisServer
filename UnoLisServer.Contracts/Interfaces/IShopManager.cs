using System.Collections.Generic;
using System.ServiceModel;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Contracts.Interfaces
{
    [ServiceContract(CallbackContract = typeof(IShopCallback), SessionMode = SessionMode.Required)]
    public interface IShopManager
    {
        [OperationContract(IsOneWay = true)]
        void GetShopItems();

        [OperationContract(IsOneWay = true)]
        void PurchaseItem(PurchaseRequest request);
    }

    [ServiceContract]
    public interface IShopCallback : ISessionCallback
    {
        [OperationContract]
        void ShopItemsReceived(List<ShopItem> items);

        [OperationContract]
        void PurchaseResponse(bool success, string itemName);
    }
}
