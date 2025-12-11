using System.Collections.Generic;
using System.Threading.Tasks;
using UnoLisServer.Common.Enums;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Services.GameLogic.Models;

namespace UnoLisServer.Services.ManagerInterfaces
{
    public interface IGameManager
    {
        Task <bool> InitializeGameAsync(string lobbyCode, List<string> playerNicknames);
        void ConnectPlayer(string lobbyCode, string nickname);
        void DisconnectPlayer(string lobbyCode, string nickname);
        Task PlayCardAsync(PlayCardContext context);
        Task DrawCardAsync(string lobbyCode, string nickname);
        Task SayUnoAsync(string lobbyCode, string nickname);
        void UseItem(ItemUsageContext context);
    }
}