using System;
using System.ServiceModel;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Contracts.Interfaces
{
    [ServiceContract(
        CallbackContract = typeof(IChatCallback),
        SessionMode = SessionMode.Required)]
    public interface IChatManager
    {
        [OperationContract(IsOneWay = true)]
        void SendMessage(ChatMessageData message);

        [OperationContract(IsOneWay = true)]
        void GetChatHistory(string channelId);

        [OperationContract(IsOneWay = true)]
        void RegisterPlayer(string nickname);
    }

    [ServiceContract]
    public interface IChatCallback : ISessionCallback
    {
        [OperationContract(IsOneWay = true)]
        void MessageReceived(ChatMessageData message);

        [OperationContract(IsOneWay = true)]
        void ChatHistoryReceived(ChatMessageData[] messages);
    }
}
