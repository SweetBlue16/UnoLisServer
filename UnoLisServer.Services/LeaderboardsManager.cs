using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Contracts;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data;

namespace UnoLisServer.Services.Leaderboards
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class LeaderboardsManager : ILeaderboardsManager
    {
        private readonly UNOContext _context;
        private readonly ILeaderboardsCallback _callback;

        public LeaderboardsManager()
        {
            _context = new UNOContext();
            _callback = OperationContext.Current.GetCallbackChannel<ILeaderboardsCallback>();
        }

        public void GetLeaderboard()
        {
            try
            {
                var entries = _context.Player
                    .Select(p => new LeaderboardEntry
                    {
                        Nickname = p.nickname,
                        FullName = p.fullName
                    })
                    .ToList();

                _callback.LeaderboardReceived(entries);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener leaderboard: {ex.Message}");
            }
        }

        public void GetPlayerRank(string nickname)
        {
            try
            {
                var player = _context.Player.FirstOrDefault(p => p.nickname == nickname);
                if (player == null)
                {
                    _callback.PlayerRankReceived(null);
                    return;
                }

                var entry = new LeaderboardEntry
                {
                    Nickname = player.nickname,
                    FullName = player.fullName
                };

                _callback.PlayerRankReceived(entry);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener rank del jugador: {ex.Message}");
            }
        }
    }
}
