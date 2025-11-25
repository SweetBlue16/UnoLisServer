using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data;
using UnoLisServer.Data.Repositories;
using UnoLisServer.Data.RepositoryInterfaces;

namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class LeaderboardsManager : ILeaderboardsManager
    {
        private readonly IPlayerRepository _playerRepository;
        private const int LeaderboardSize = 15;

        public LeaderboardsManager() : this(new PlayerRepository()) { }

        public LeaderboardsManager(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public ServiceResponse<List<LeaderboardEntry>> GetGlobalLeaderboard()
        {
            try
            {
                var topStats = _playerRepository.GetTopPlayersByGlobalScoreAsync(LeaderboardSize);
                if (topStats == null || !topStats.Any())
                {
                    Logger.Log("[WARNING] No se encontraron jugadores en el ranking.");
                    return new ServiceResponse<List<LeaderboardEntry>>
                    {
                        Code = MessageCode.Success,
                        Success = true,
                        Data = new List<LeaderboardEntry>()
                    };
                }
                var leaderboardList = new List<LeaderboardEntry>();
                int rankCounter = 1;

                foreach (var stat in topStats)
                {
                    leaderboardList.Add(new LeaderboardEntry
                    {
                        Rank = rankCounter++,
                        Nickname = stat.Player.nickname,
                        GlobalPoints = stat.globalPoints ?? 0,
                        MatchesPlayed = stat.matchesPlayed ?? 0,
                        Wins = stat.wins ?? 0,
                        WinRate = CalculateWinRate(stat)
                    });
                }

                Logger.Log("[INFO] Datos del ranking global recuperados con éxito.");
                return new ServiceResponse<List<LeaderboardEntry>>
                {
                    Code = MessageCode.LeaderboardDataRetrieved,
                    Success = true,
                    Data = leaderboardList
                };
            }
            catch (CommunicationException commEx)
            {
                Logger.Log($"[ERROR] Error de comunicación en GetGlobalLeaderboard: {commEx}");
                return new ServiceResponse<List<LeaderboardEntry>>
                {
                    Code = MessageCode.ConnectionFailed,
                    Success = false
                };
            }
            catch (TimeoutException timeoutEx)
            {
                Logger.Log($"[ERROR] Timeout en GetGlobalLeaderboard: {timeoutEx}");
                return new ServiceResponse<List<LeaderboardEntry>>
                {
                    Code = MessageCode.Timeout,
                    Success = false
                };
            }
            catch (Exception ex)
            {
                Logger.Log($"[ERROR] Excepción inesperada en GetGlobalLeaderboard: {ex}");
                return new ServiceResponse<List<LeaderboardEntry>>
                {
                    Code = MessageCode.LeaderboardInternalError,
                    Success = false
                };
            }
        }

        private string CalculateWinRate(PlayerStatistics stat)
        {
            return (stat.matchesPlayed > 0)
                ? $"{(double)(stat.wins) / stat.matchesPlayed:P0}"
                : "0%";
        }
    }
}
