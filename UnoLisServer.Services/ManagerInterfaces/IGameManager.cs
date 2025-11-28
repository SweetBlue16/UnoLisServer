using System.Collections.Generic;
using System.Threading.Tasks;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Services.ManagerInterfaces
{
    public interface IGameManager
    {
        bool InitializeGame(string lobbyCode, List<string> playerNicknames);
        void ConnectPlayer(string lobbyCode, string nickname);
        void DisconnectPlayer(string lobbyCode, string nickname);
        Task PlayCardAsync(string lobbyCode, string nickname, string cardId, int? selectedColorId);
        Task DrawCardAsync(string lobbyCode, string nickname);
        Task SayUnoAsync(string lobbyCode, string nickname);
    }
}