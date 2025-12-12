using System.Linq;
using System.ServiceModel;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data;

namespace UnoLisServer.Services
{
    /// <summary>
    /// Class to manage shop
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ShopManager : IShopManager
    {
        private readonly UNOContext _context;
        private readonly IShopCallback _callback;

        public ShopManager()
        {
            _context = new UNOContext();
            _callback = OperationContext.Current.GetCallbackChannel<IShopCallback>();
        }

        public void GetShopItems()
        {
            var items = _context.LootBoxType
                .Select(lootBoxType => new ShopItem
                {
                    BoxId = lootBoxType.idLootBoxType,
                    Name = lootBoxType.boxName,
                    Price = lootBoxType.price,
                    Description = lootBoxType.description
                }).ToList();

            _callback.ShopItemsReceived(items);
        }

        public void PurchaseItem(PurchaseRequest request)
        {
            _callback.PurchaseResponse(true, $"Compra realizada: Item {request.ItemId}");
        }
    }
}
