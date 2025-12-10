using System.Collections.Generic;
using System.Threading.Tasks;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;

namespace UnoLisServer.Services.ManagerInterfaces
{
    /// <summary>
    /// Internal logic interface for managing in-memory lobbies.
    /// This allows for mocking and unit testing (xUnit).
    /// </summary>
    public interface ILobbyManager
    {
        Task<CreateMatchResponse> CreateLobbyAsync(MatchSettings settings);
        Task<JoinMatchResponse> JoinLobbyAsync(string lobbyCode, string nickname);
        Task<bool> SetLobbyBackgroundAsync(string lobbyCode, string backgroundName);
        LobbySettings GetLobbySettings(string lobbyCode); 
        void RegisterConnection(string lobbyCode, string nickname);
        void RemoveConnection(string lobbyCode, string nickname, ILobbyDuplexCallback cachedCallback = null);
        Task HandleReadyStatusAsync(string lobbyCode, string nickname, bool isReady);
        Task<bool> SendInvitationsAsync(string lobbyCode, string senderNickname, List<string> invitedNicknames);
    }
}
