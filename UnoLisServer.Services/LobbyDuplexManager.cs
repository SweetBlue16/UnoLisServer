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
        private readonly object _lock = new object();

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
            if (string.IsNullOrWhiteSpace(lobbyCode) || string.IsNullOrWhiteSpace(nickname))
            {
                return;
            }

            lock (_lock)
            {
                _currentLobbyCode = lobbyCode;
                _currentNickname = nickname;

                if (_cachedCallback == null && OperationContext.Current != null)
                {
                    _cachedCallback = OperationContext.Current.GetCallbackChannel<ILobbyDuplexCallback>();

                    var channel = (ICommunicationObject)_cachedCallback;
                    channel.Closed += Channel_Closed;
                    channel.Faulted += Channel_Closed;
                }
            }

            try
            {
                _lobbyManager.RegisterConnection(lobbyCode, nickname);
                Logger.Log($"[DUPLEX] Connected to lobby {lobbyCode}");
            }
            catch (CommunicationException commEx)
            {
                Logger.Error($"[DUPLEX] Communication error registering connection: {commEx.Message}", commEx);
            }
            catch (TimeoutException timeEx)
            {
                Logger.Error($"[DUPLEX] Timeout error registering connection: {timeEx.Message}", timeEx);
            }
            catch (Exception ex)
            {
                Logger.Error($"[DUPLEX] Error registering connection", ex);
            }
        }

        public void DisconnectFromLobby(string lobbyCode, string nickname)
        {
            CleanUp(lobbyCode, nickname);
        }

        public void SetReadyStatus(string lobbyCode, string nickname, bool isReady)
        {
            Task.Run(async () =>
            {
                try
                {
                    await _lobbyManager.HandleReadyStatusAsync(lobbyCode, nickname, isReady);
                }
                catch (CommunicationException commEx)
                {
                    Logger.Error($"[DUPLEX-ASYNC] Communication error setting ready status: {commEx.Message}", commEx);
                }
                catch (TimeoutException timeEx)
                {
                    Logger.Error($"[DUPLEX-ASYNC] Timeout error setting ready status: {timeEx.Message}", timeEx);
                }
                catch (Exception ex)
                {
                    Logger.Error($"[DUPLEX-ASYNC] Error setting ready status for {nickname}", ex);
                }
            });
        }

        private void Channel_Closed(object sender, EventArgs e)
        {
            Logger.Warn($"[DUPLEX] Channel closed/faulted detected for session.");
            Dispose();
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(_currentLobbyCode) && !string.IsNullOrEmpty(_currentNickname))
                {
                    Logger.Log($"[DUPLEX] Detected abrupt disconnection");
                    CleanUp(_currentLobbyCode, _currentNickname);
                }

                if (_cachedCallback is ICommunicationObject channel)
                {
                    try
                    {
                        channel.Closed -= Channel_Closed;
                        channel.Faulted -= Channel_Closed;
                    }
                    catch (ObjectDisposedException)
                    {
                        Logger.Warn($"[DUPLEX] Callback channel already disposed when detaching event handlers.");
                    }
                    catch (Exception)
                    {
                        Logger.Warn($"[DUPLEX] Error detaching event handlers from callback channel.");
                    }
                }

                _disposed = true;
            }
        }

        private void CleanUp(string code, string nick)
        {
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(nick))
            {
                return;
            }

            try
            {
                _lobbyManager.RemoveConnection(code, nick, _cachedCallback);
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[DUPLEX] Communication error during cleanup: {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[DUPLEX] Timeout during cleanup: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[DUPLEX] Unexpected error during cleanup", ex);
            }
            finally
            {
                lock (_lock)
                {
                    _currentLobbyCode = null;
                    _currentNickname = null;
                }
            }
        }
    }
}