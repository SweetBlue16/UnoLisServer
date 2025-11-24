using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}