using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Data.RepositoryInterfaces
{
    /// <summary>
    /// Data repository interface for Player
    /// </summary>
    public interface IPlayerRepository
    {
        Task<Player> GetPlayerProfileByNicknameAsync(string nickname);
        Task UpdatePlayerProfileAsync(ProfileData data);
        Task<bool> IsNicknameTakenAsync(string nickname);
        Task<bool> IsEmailRegisteredAsync(string email);
        Task CreatePlayerAsync(RegistrationData data);
        Task CreatePlayerFromPendingAsync(string email, PendingRegistration pendingData);
        Task<List<PlayerAvatar>> GetPlayerAvatarsAsync(string nickname);
        Task UpdateSelectedAvatarAsync(string nickname, int newAvatarId);
        Task<List<PlayerStatistics>> GetTopPlayersByGlobalScoreAsync(int topCount);
        Task<Player> GetPlayerWithDetailsAsync(string nickname);
        Task UpdateMatchResultAsync(string nickname, bool isWinner, int pointsEarned);
    }
}