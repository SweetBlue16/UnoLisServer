using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data; 
using UnoLisServer.Data.Repositories; 

namespace UnoLisServer.Services
{
    /// <summary>
    /// Class to manage shop for getting avatars
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ShopManager : IShopManager
    {
        private readonly ShopRepository _shopRepository;
        private readonly UNOContext _context;
        private readonly IShopCallback _callback;

        public ShopManager()
        {
            _shopRepository = new ShopRepository();
            _context = new UNOContext();
            _callback = OperationContext.Current.GetCallbackChannel<IShopCallback>();
        }

        public void GetShopItems()
        {
            try
            {
                var items = _context.LootBoxType
                    .Select(lootBoxType => new ShopItem
                    {
                        BoxId = lootBoxType.idLootBoxType,
                        Name = lootBoxType.boxName,
                        Price = lootBoxType.price,
                        Description = lootBoxType.description,
                        Rarity = lootBoxType.boxRarity
                    }).ToList();

                _callback.ShopItemsReceived(items);
            }
            catch (Exception ex) when (ex.Message == "DataStore_Unavailable")
            {
                Logger.Error($"[CRITICAL] Shop fetch failed. Data Store unavailable.", ex);
                _callback.ShopItemsReceived(new System.Collections.Generic.List<ShopItem>());
            }
            catch (Exception ex) when (ex.Message == "Server_Busy")
            {
                Logger.Warn($"[WARN] Shop fetch timeout.");
                _callback.ShopItemsReceived(new System.Collections.Generic.List<ShopItem>());
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[WCF] Communication error fetching Shop: {commEx.Message}");
                _callback.ShopItemsReceived(new System.Collections.Generic.List<ShopItem>());
            }
            catch (TimeoutException timeoutEx)
            {
                Logger.Warn($"[WCF] Timeout fetching Shop: {timeoutEx.Message}");
                _callback.ShopItemsReceived(new System.Collections.Generic.List<ShopItem>());
            }
            catch (Exception ex)
            {
                Logger.Warn($"Error fetching items: {ex.Message}");
                _callback.ShopItemsReceived(new System.Collections.Generic.List<ShopItem>());
            }
        }

        public void PurchaseItem(PurchaseRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Nickname))
            {
                return;
            }

            Task.Run(async () =>
            {
                try
                {
                    var result = await _shopRepository.PurchaseLootBoxAsync(request.Nickname, request.ItemId);

                    _callback.PurchaseResponse(result);
                }
                catch(CommunicationException commEx)
                {
                    Logger.Warn($"[WCF] Communication error during purchase: {commEx.Message}");
                    var errorResult = new ShopPurchaseResult
                    {
                        IsSuccess = false,
                        MessageCode = "Communication_Error",
                        RemainingCoins = 0
                    };
                    _callback.PurchaseResponse(errorResult);
                }
                catch (TimeoutException timeoutEx)
                {
                    Logger.Warn($"[WCF] Timeout during purchase: {timeoutEx.Message}");
                    var errorResult = new ShopPurchaseResult
                    {
                        IsSuccess = false,
                        MessageCode = "Timeout_Error",
                        RemainingCoins = 0
                    };
                    _callback.PurchaseResponse(errorResult);
                }
                catch (Exception ex)
                {
                    Logger.Error($"[CRITICAL] Unexpected error during purchase: { ex.Message}");
                    var errorResult = new ShopPurchaseResult
                    {
                        IsSuccess = false,
                        MessageCode = "Server_Internal_Error",
                        RemainingCoins = 0
                    };
                    _callback.PurchaseResponse(errorResult);
                }
            });
        }

        public void GetPlayerBalance(string nickname)
        {
            Task.Run(async () =>
            {
                try
                {
                    int coins = await _shopRepository.GetPlayerCoinsAsync(nickname);
                    _callback.PlayerBalanceReceived(coins);
                }
                catch (CommunicationException commEx)
                {
                    Logger.Warn($"[WCF] Communication error fetching player balance: {commEx.Message}");
                    _callback.PlayerBalanceReceived(0);
                }
                catch (TimeoutException timeoutEx)
                {
                    Logger.Warn($"[WCF] Timeout fetching player balance: {timeoutEx.Message}");
                    _callback.PlayerBalanceReceived(0);
                }
                catch (Exception ex)
                {
                    Logger.Error($"[CRITICAL] Error fetching player balance: {ex.Message}");
                    _callback.PlayerBalanceReceived(0);
                }
            });
        }
    }
}
