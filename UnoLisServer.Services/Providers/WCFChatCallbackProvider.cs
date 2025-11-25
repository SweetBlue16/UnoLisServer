using UnoLisServer.Contracts.Interfaces;

namespace UnoLisServer.Services.Providers
{
    public interface IChatCallbackProvider
    {
        IChatCallback GetCallback();
    }
    /// <summary>
    ///  Manages the retrieval of the WCF chat callback channel, isolated for easier testing.
    public class WcfChatCallbackProvider : IChatCallbackProvider
    {
        public IChatCallback GetCallback()
        {
            return System.ServiceModel.OperationContext.Current.GetCallbackChannel<IChatCallback>();
        }
    }
}