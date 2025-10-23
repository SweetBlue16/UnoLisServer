using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Common.Enums
{
    public enum MessageCode
    {
        // 1000-1999: Success Codes
        Success = 1000,
        RegistrationSuccessful = 1001,
        LoginSuccessful = 1002,
        LogoutSuccessful = 1003,
        ProfileDataRetrieved = 1004,
        ProfileUpdated = 1005,
        FriendRequestSent = 1006,
        FriendRequestAccepted = 1007,
        FriendRemoved = 1008,
        ChatMessageSent = 1009,
        ChatMessageRetrieved = 1010,
        LobbyCreated = 1011,
        LobbyJoined = 1012,
        LobbyLeft = 1013,
        MatchStarted = 1014,
        MatchEnded = 1015,

        // 2000-2999: Client Error Codes
        BadRequest = 2000,
        InvalidData = 2001,
        InvalidCredentials = 2002,
        NicknameAlreadyTaken = 2003,
        EmailAlreadyRegistered = 2004,
        InvalidEmailFormat = 2005,
        WeakPassword = 2006,
        PlayerNotFound = 2007,
        SamePassword = 2008,
        EmptyMessage = 2009,
        MessageTooLong = 2010,
        InappropriateContent = 2011,
        AlreadyFriends = 2012,
        PendingFriendRequest = 2013,
        InvalidSocialUrl = 2014,
        BlockedUser = 2015,
        LobbyNotFound = 2016,
        LobbyFull = 2017,
        OperationNotSupported = 2018,
        ValidationFailed = 2019,
        EmptyFields = 2020,

        // 3000-3999: Session/Auth Error Codes
        SessionExpired = 3000,
        UnauthorizedAccess = 3001,
        InvalidToken = 3002,
        MissingToken = 3003,
        DuplicateSession = 3004,
        UserNotConnected = 3005,
        LoginInternalError = 3006,
        LogoutInternalError = 3007,

        // 4000-4999: Internal Server Error Codes
        DatabaseError = 4000,
        TransactionFailed = 4001,
        SqlError = 4002,
        ConcurrencyConflict = 4003,
        SerializationError = 4004,
        UnhandledException = 4005,
        CallbackError = 4006,
        ProfileUpdateFailed = 4007,
        ProfileFetchFailed = 4008,
        ChatInternalError = 4009,
        FriendsInternalError = 4010,
        LobbyInternalError = 4011,
        GeneralServerError = 4012,

        // 5000-5999: Network/Communication Error Codes
        ConnectionLost = 5000,
        Timeout = 5001,
        ConnectionFailed = 5002,
        ConnectionRejected = 5003,
        UnstableConnection = 5004,
        ClientDisconnected = 5005,

        // 6000-6999: Game Logic Error Codes
        FriendActionCompleted = 6000,
        PlayerBlocked = 6001,
        PlayerUnblocked = 6002,
        PlayerHasActiveLobby = 6003,
        PlayerNotInLobby = 6004,
        PlayerAlreadyReady = 6005,
        PlayerNotReady = 6006,
        MatchAlreadyStarted = 6007,
        MatchCancelled = 6008,
        MatchNotFound = 6009,
        MatchAlreadyEnded = 6010,
        PlayerKicked = 6011,
        PlayerBanned = 6012,
        LobbyClosed = 6013,
        NoPermissions = 6014,
        LobbyInconsistentState = 6015,
        PlayerDisconnected = 6016,
        PlayerReconnected = 6017,
        MatchResultsRecorded = 6018,
        RewardProcessingError = 6019,
        PurchaseProcessingError = 6020
    }
}
