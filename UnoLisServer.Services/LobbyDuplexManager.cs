using System;
using System.ServiceModel;
using System.Threading.Tasks;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Services.ManagerInterfaces;

namespace UnoLisServer.Services
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession, 
        ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class LobbyDuplexManager : ILobbyDuplexManager, IDisposable
    {
        private readonly ILobbyManager _lobbyManager;

        private string _currentLobbyCode;
        private string _currentNickname;
        private ILobbyDuplexCallback _cachedCallback;

        private bool _disposed = false;

        public LobbyDuplexManager() : this(new LobbyManager())
        {
            var context = OperationContext.Current;
            if (context != null)
            {
                _cachedCallback = context.GetCallbackChannel<ILobbyDuplexCallback>();
                context.Channel.Closed += Channel_Closed;
                context.Channel.Faulted += Channel_Closed;
            }
        }

        public LobbyDuplexManager(ILobbyManager lobbyManager)
        {
            _lobbyManager = lobbyManager;
        }

        public void ConnectToLobby(string lobbyCode, string nickname)
        {
            _currentLobbyCode = lobbyCode;
            _currentNickname = nickname;

            if (_cachedCallback == null && OperationContext.Current != null)
            {
                _cachedCallback = OperationContext.Current.GetCallbackChannel<ILobbyDuplexCallback>();
            }

            _lobbyManager.RegisterConnection(lobbyCode, nickname);
        }

        public void DisconnectFromLobby(string lobbyCode, string nickname)
        {
            CleanUp(lobbyCode, nickname);
        }

        public void SetReadyStatus(string lobbyCode, string nickname, bool isReady)
        {
            Task.Run(async () =>
            {
                await _lobbyManager.HandleReadyStatusAsync(lobbyCode, nickname, isReady);
            });
        }

        private void Channel_Closed(object sender, EventArgs e)
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (!string.IsNullOrEmpty(_currentLobbyCode) && !string.IsNullOrEmpty(_currentNickname))
            {
                Logger.Log($"[DUPLEX] Detected abrupt disconnection for {_currentNickname}");
                CleanUp(_currentLobbyCode, _currentNickname);
            }

            _disposed = true;
        }

        private void CleanUp(string code, string nick)
        {
            try
            {
                _lobbyManager.RemoveConnection(code, nick, _cachedCallback);
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[DUPLEX] Communication error during cleanup for {nick}: {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[DUPLEX] Timeout during cleanup for {nick}: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[DUPLEX] Unexpected error during cleanup for {nick}", ex);
            }
            finally
            {
                _currentLobbyCode = null;
                _currentNickname = null;
            }
        }
    }
}