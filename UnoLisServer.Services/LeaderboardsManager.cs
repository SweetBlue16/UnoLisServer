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
using UnoLisServer.Data.RepositoryInterfaces;

namespace UnoLisServer.Services
{
    /// <summary>
    /// Class to manage logic for leaderboards
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class LeaderboardsManager : ILeaderboardsManager
    {
        private readonly IPlayerRepository _playerRepository;
        private const int LeaderboardSize = 15;

        public LeaderboardsManager() : this(new PlayerRepository()) 
        { 
        }

        public LeaderboardsManager(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public async Task<ServiceResponse<List<LeaderboardEntry>>> GetGlobalLeaderboardAsync()
        {
            try
            {
                var topStats = await _playerRepository.GetTopPlayersByGlobalScoreAsync(LeaderboardSize);

                if (topStats == null || !topStats.Any())
                {
                    Logger.Warn("[LEADERBOARD] No stats found in DB.");
                    return new ServiceResponse<List<LeaderboardEntry>>
                    {
                        Code = MessageCode.Success,
                        Success = true,
                        Data = new List<LeaderboardEntry>()
                    };
                }

                var leaderboardList = topStats.Select((stat, index) => new LeaderboardEntry
                {
                    Rank = index + 1,
                    Nickname = stat.Player.nickname,
                    GlobalPoints = stat.globalPoints ?? 0,
                    MatchesPlayed = stat.matchesPlayed ?? 0,
                    Wins = stat.wins ?? 0,
                    WinRate = CalculateWinRate(stat)
                }).ToList();

                return new ServiceResponse<List<LeaderboardEntry>>
                {
                    Code = MessageCode.LeaderboardDataRetrieved,
                    Success = true,
                    Data = leaderboardList
                };
            }
            catch (Exception ex) when (ex.Message == "DataStore_Unavailable")
            {
                Logger.Error($"[CRITICAL] Leaderboard fetch failed. Data Store unavailable.", ex);
                return new ServiceResponse<List<LeaderboardEntry>>
                {
                    Code = MessageCode.DatabaseError,
                    Success = false,
                };
            }
            catch (Exception ex) when (ex.Message == "Server_Busy")
            {
                Logger.Warn($"[WARN] Leaderboard fetch timeout.");
                return new ServiceResponse<List<LeaderboardEntry>>
                {
                    Code = MessageCode.Timeout,
                    Success = false,
                };
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[WCF] Communication error fetching leaderboard: {commEx.Message}");
                return new ServiceResponse<List<LeaderboardEntry>>
                {
                    Code = MessageCode.ConnectionFailed,
                    Success = false
                };
            }
            catch (TimeoutException timeoutEx)
            {
                Logger.Warn($"[WCF] Timeout fetching leaderboard: {timeoutEx.Message}");
                return new ServiceResponse<List<LeaderboardEntry>>
                {
                    Code = MessageCode.Timeout,
                    Success = false
                };
            }
            catch (Exception ex)
            {
                Logger.Error($"[CRITICAL] Unexpected error fetching leaderboard.", ex);
                return new ServiceResponse<List<LeaderboardEntry>>
                {
                    Code = MessageCode.LeaderboardInternalError,
                    Success = false
                };
            }
        }

        private string CalculateWinRate(PlayerStatistics stat)
        {
            int played = stat.matchesPlayed ?? 0;
            int wins = stat.wins ?? 0;

            return (played > 0)
                ? $"{(double)wins / played:P0}"
                : "0%";
        }
    }
}
