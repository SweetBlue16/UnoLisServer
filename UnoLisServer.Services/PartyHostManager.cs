using System;
using System.Collections.Generic;
using System.ServiceModel;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;

namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class PartyHostManager : IPartyHostManager
    {
        private readonly IPartyHostCallback _callback;

        // Simulamos almacenamiento temporal de partidas activas
        private static readonly Dictionary<int, Lobby> ActiveParties = new Dictionary<int, Lobby>();
        private static int _nextPartyId = 1;

        public PartyHostManager()
        {
            _callback = OperationContext.Current.GetCallbackChannel<IPartyHostCallback>();
        }

        public void CreateParty(string hostNickname)
        {
            int partyId = _nextPartyId++;
            string joinCode = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();

            var lobby = new Lobby
            {
                PartyId = partyId,
                JoinCode = joinCode,
                HostNickname = hostNickname,
                Players = new List<PlayerState> { new PlayerState(hostNickname) }
            };

            ActiveParties[partyId] = lobby;

            Console.WriteLine($"[HOST] {hostNickname} creó la partida {partyId} (Código: {joinCode})");
            _callback.PartyCreated(partyId, joinCode);
        }

        public void StartMatch(int partyId, string hostNickname)
        {
            if (!ActiveParties.TryGetValue(partyId, out var lobby))
            {
                Console.WriteLine($"[HOST] No existe la partida {partyId}");
                _callback.PartyCancelled();
                return;
            }

            Console.WriteLine($"[HOST] {hostNickname} inició la partida {partyId}");
            _callback.MatchStarted();
        }

        public void CancelParty(int partyId)
        {
            if (ActiveParties.Remove(partyId))
            {
                Console.WriteLine($"[HOST] Partida {partyId} cancelada por el host.");
                _callback.PartyCancelled();
            }
        }

        // Clase auxiliar interna (no se serializa, solo para simular datos en memoria)
        private class Lobby
        {
            public int PartyId { get; set; }
            public string JoinCode { get; set; }
            public string HostNickname { get; set; }
            public List<PlayerState> Players { get; set; } = new List<PlayerState>();

        }

        private class PlayerState
        {
            public string Nickname { get; }
            public bool IsReady { get; set; }

            public PlayerState(string nickname)
            {
                Nickname = nickname;
                IsReady = false;
            }
        }
    }
}
