using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Data;
using UnoLisServer.Data.RepositoryInterfaces;
namespace UnoLisServer.Data.Repositories
{
    public class ShopRepository
    {
        private readonly Func<UNOContext> _contextFactory;
        private readonly Random _random;

        public ShopRepository() : this(() => new UNOContext())
        {
        }

        public ShopRepository(Func<UNOContext> contextFactory)
        {
            _contextFactory = () => new UNOContext();
            _random = new Random();
        }

        public async Task<ShopPurchaseResult> PurchaseLootBoxAsync(string nickname, int boxId)
        {
            if (string.IsNullOrWhiteSpace(nickname)) return CreateFailureResult("InvalidRequest");

            using (var context = _contextFactory())
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    var player = await context.Player
                        .FirstOrDefaultAsync(p => p.nickname == nickname);

                    if (player == null)
                    {
                        return CreateFailureResult("PlayerNotFound");
                    }

                    var boxInfo = await context.LootBoxType
                        .FirstOrDefaultAsync(b => b.idLootBoxType == boxId);

                    if (boxInfo == null)
                    {
                        return CreateFailureResult("BoxNotFound");
                    }

                    if (!CanAffordBox(player, boxInfo))
                    {
                        return CreateFailureResult("InsufficientFunds", player.revoCoins);
                    }

                    var boxAvatarIds = await context.Avatar
                        .Where(a => a.LootBoxType_idLootBoxType == boxId)
                        .Select(a => a.idAvatar)
                        .ToListAsync();

                    var ownedAvatarIds = await context.AvatarsUnlocked
                        .Where(au => au.Player_idPlayer == player.idPlayer)
                        .Select(au => au.Avatar_idAvatar)
                        .ToListAsync();

                    int winnerId = SelectRandomNewAvatarId(boxAvatarIds, ownedAvatarIds);

                    if (winnerId == 0) 
                    {
                        return CreateFailureResult("AllContentOwned", player.revoCoins);
                    }

                    ApplyCostToPlayer(player, boxInfo.price);

                    var newUnlock = CreateUnlockRecord(player.idPlayer, winnerId);
                    context.AvatarsUnlocked.Add(newUnlock);

                    await context.SaveChangesAsync();
                    transaction.Commit();

                    var winnerAvatar = await context.Avatar.FindAsync(winnerId);

                    return CreateSuccessResult(winnerAvatar, player.revoCoins);
                }
                catch (SqlException sqlEx)
                {
                    transaction.Rollback();
                    SqlErrorHandler.HandleAndThrow(sqlEx);
                    throw;
                }
                catch (DbUpdateException dbEx)
                {
                    transaction.Rollback();
                    Logger.Error($"[SHOP-CONSTRAINT] Transaction failed for {nickname}.", dbEx);
                    throw new Exception("Data_Conflict", dbEx);
                }
                catch (EntityCommandExecutionException entityCmdEx)
                {
                    transaction.Rollback();
                    Logger.Error($"[EF-CRITICAL] Provider failed purchasing item.", entityCmdEx);
                    throw new Exception("DataStore_Unavailable", entityCmdEx);
                }
                catch (TimeoutException timeoutEx)
                {
                    transaction.Rollback();
                    Logger.Warn($"[DATA-TIMEOUT] Purchase timed out.");
                    throw new Exception("Server_Busy", timeoutEx);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Logger.Error($"[CRITICAL] Unexpected error in shop transaction", ex);
                    throw new Exception("Server_Internal_Error", ex);
                }
            }
        }

        private bool CanAffordBox(Player player, LootBoxType box)
        {
            return player.revoCoins >= box.price;
        }

        private void ApplyCostToPlayer(Player player, int cost)
        {
            player.revoCoins -= cost;
        }

        private int SelectRandomNewAvatarId(List<int> boxIds, List<int> ownedIds)
        {
            var candidates = boxIds.Except(ownedIds).ToList();

            if (!candidates.Any()) return 0;

            return candidates[_random.Next(candidates.Count)];
        }

        private AvatarsUnlocked CreateUnlockRecord(int playerId, int avatarId)
        {
            return new AvatarsUnlocked
            {
                Player_idPlayer = playerId,
                Avatar_idAvatar = avatarId,
                unlockedDate = DateTime.Now
            };
        }

        private ShopPurchaseResult CreateSuccessResult(Avatar avatarEntity, int remainingCoins)
        {
            return new ShopPurchaseResult
            {
                IsSuccess = true,
                MessageCode = "PurchaseSuccess",
                RemainingCoins = remainingCoins,
                WonAvatar = new PlayerAvatar 
                {
                    AvatarId = avatarEntity.idAvatar,
                    AvatarName = avatarEntity.avatarName,
                    Description = avatarEntity.avatarDescription,
                    Rarity = avatarEntity.avatarRarity,
                    IsUnlocked = true,
                    IsSelected = false
                }
            };
        }

        private ShopPurchaseResult CreateFailureResult(string code, int currentCoins)
        {
            return new ShopPurchaseResult
            {
                IsSuccess = false,
                MessageCode = code,
                RemainingCoins = currentCoins,
                WonAvatar = null
            };
        }

        private ShopPurchaseResult CreateFailureResult(string code)
        {
            return new ShopPurchaseResult
            {
                IsSuccess = false,
                MessageCode = code,
                RemainingCoins = 0,
                WonAvatar = null
            };
        }

        public async Task<int> GetPlayerCoinsAsync(string nickname)
        {
            using (var context = _contextFactory())
            {
                var coins = await context.Player
                    .Where(player => player.nickname == nickname)
                    .Select(player => player.revoCoins)
                    .FirstOrDefaultAsync();

                return coins;
            }
        }
    }
}
