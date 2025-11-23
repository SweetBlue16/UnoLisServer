using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Data.RepositoryInterfaces
{
    /// <summary>
    /// Data repository interface for Player
    /// </summary>
    public interface IPlayerRepository
    {
        Task<Player> GetPlayerProfileByNicknameAsync(string nickname);
    }
}