using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Contracts.Interfaces
{
    [ServiceContract(CallbackContract = typeof(IChatCallback), SessionMode = SessionMode.Required)]
    public interface IChatManager
    {
        [OperationContract(IsOneWay = true)]
        void SendMessage(ChatMessageData message);

        [OperationContract(IsOneWay = true)]
        void GetChatHistory(string channelId);
    }

    [ServiceContract]
    public interface IChatCallback : ISessionCallback
    {
        [OperationContract]
        void MessageReceived(ChatMessageData message);

        [OperationContract]
        void ChatHistoryReceived(List<ChatMessageData> messages);
    }
}
