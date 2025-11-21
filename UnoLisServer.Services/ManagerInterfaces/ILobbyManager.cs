using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Services.ManagerInterfaces
{
    /// <summary>
    /// Internal logic interface for managing in-memory lobbies.
    /// This allows for mocking and unit testing (xUnit).
    /// </summary>
    public interface ILobbyManager
    {
        CreateMatchResponse CreateLobby(MatchSettings settings);

        JoinMatchResponse JoinLobby(string lobbyCode, string nickname);

        bool SetLobbyBackground(string lobbyCode, string backgroundName);

        LobbySettings GetLobbySettings(string lobbyCode);
    }
}
