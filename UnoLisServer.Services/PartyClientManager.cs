using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;

namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class PartyClientManager : IPartyClientManager
    {
        private readonly IPartyClientCallback _callback;

        public PartyClientManager()
        {
            _callback = OperationContext.Current.GetCallbackChannel<IPartyClientCallback>();
        }

        public void JoinParty(JoinPartyRequest request)
        {
            // Simulación de búsqueda de partida
            Console.WriteLine($"[CLIENT] {request.Nickname} intenta unirse al código {request.JoinCode}");

            bool joinSuccess = true; // Simula validación del código
            if (joinSuccess)
            {
                int partyId = new Random().Next(1000, 9999);
                _callback.JoinedSuccessfully(partyId, "HostDemo");
                Console.WriteLine($"[CLIENT] {request.Nickname} se unió exitosamente a la partida {partyId}");
            }
            else
            {
                _callback.JoinFailed("Código inválido o partida no disponible");
            }
        }

        public void LeaveParty(PartyActionData data)
        {
            Console.WriteLine($"[CLIENT] {data.Nickname} salió de la partida {data.PartyId}");
            _callback.PlayerLeft(data.Nickname);
        }

        public void SetReadyStatus(PartyActionData data)
        {
            Console.WriteLine($"[CLIENT] {data.Nickname} está {(data.IsReady == true ? "listo" : "no listo")}");
            _callback.PlayerReadyStatusChanged(data.Nickname, data.IsReady ?? false);
        }
    }
}
