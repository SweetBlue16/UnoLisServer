using UnoLisServer.Contracts.Interfaces;

namespace UnoLisServer.Services.Helpers
{
    internal struct ConnectionResolutionResult
    {
        public ILobbyDuplexCallback Callback { get; }
        public bool IsSuccess { get; }
        public string StatusInfo { get; }

        public ConnectionResolutionResult(ILobbyDuplexCallback callback, string source)
        {
            Callback = callback;
            IsSuccess = true;
            StatusInfo = $"Resolved via {source}";
        }

        public static ConnectionResolutionResult Failure(string reason)
        {
            return new ConnectionResolutionResult(null, reason, false);
        }

        private ConnectionResolutionResult(ILobbyDuplexCallback callback, string info, bool success)
        {
            Callback = callback;
            StatusInfo = info;
            IsSuccess = success;
        }
    }
}
