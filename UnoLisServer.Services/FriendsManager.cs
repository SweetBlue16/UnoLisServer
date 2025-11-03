using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Contracts;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data;
using UnoLisServer.Services;
using UnoLisServer.Common.Helpers;

namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class FriendsManager : IFriendsManager
    {
        private readonly IFriendsCallback _callback;
        private readonly UNOContext _context;
        // Asumo que SessionManager es estático y no requiere una instancia inyectada
        // private readonly SessionManager _sessionManager; 

        public FriendsManager()
        {
            _callback = OperationContext.Current.GetCallbackChannel<IFriendsCallback>();
            _context = new UNOContext();
            // _sessionManager = SessionManager.Instance; // Si no es estático, úsalo aquí.
        }

        // --- I. OBTENER AMIGOS (ESTADO = 1) ---
        public void GetFriendsList(string nickname)
        {
            try
            {
                var playerId = _context.Player.FirstOrDefault(p => p.nickname == nickname)?.idPlayer;

                if (!playerId.HasValue)
                {
                    _callback.FriendRequestResult(false, "Error: El usuario no existe.");
                    return;
                }

                // Busca relaciones donde el estado es 'Aceptado' (true/1)
                var friendsList = _context.FriendList
                    .Where(fl => fl.friendRequest == true &&
                                 (fl.Player_idPlayer == playerId.Value || fl.Player_idPlayer1 == playerId.Value))
                    .ToList();

                var friendDataList = new List<FriendData>();

                foreach (var fl in friendsList)
                {
                    var friendId = (fl.Player_idPlayer == playerId.Value) ? fl.Player_idPlayer1 : fl.Player_idPlayer;
                    var friendPlayer = _context.Player.Find(friendId);

                    if (friendPlayer != null)
                    {
                        friendDataList.Add(new FriendData
                        {
                            FriendNickname = friendPlayer.nickname,
                            // Usa la clase estática SessionManager para el estado online
                            IsOnline = SessionManager.IsOnline(friendPlayer.nickname),
                            StatusMessage = "Amigo"
                        });
                    }
                }

                _callback.FriendsListReceived(friendDataList);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error en GetFriendsList para {nickname}: {ex.Message}");
                _callback.FriendRequestResult(false, "Error interno al obtener amigos.");
            }
        }

        // --- II. OBTENER SOLICITUDES PENDIENTES (ESTADO = 0, YO SOY EL RECEPTOR) ---
        public void GetPendingRequests(string nickname)
        {
            try
            {
                var targetPlayer = _context.Player.FirstOrDefault(p => p.nickname == nickname);

                if (targetPlayer == null)
                {
                    _callback.FriendRequestResult(false, "Error: Usuario no válido.");
                    return;
                }

                // Busca las filas donde el Player_idPlayer1 (Receptor) es el usuario actual 
                // Y el estado es Pendiente (friendRequest == false / 0).
                var pendingRequests = _context.FriendList
                    .Where(fl => fl.Player_idPlayer1 == targetPlayer.idPlayer && fl.friendRequest == false)
                    .ToList();

                var requestDataList = new List<FriendRequestData>();

                foreach (var request in pendingRequests)
                {
                    var requester = _context.Player.Find(request.Player_idPlayer);

                    if (requester != null)
                    {
                        requestDataList.Add(new FriendRequestData
                        {
                            RequesterNickname = requester.nickname,
                            TargetNickname = nickname
                        });
                    }
                }

                _callback.PendingRequestsReceived(requestDataList);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error en GetPendingRequests para {nickname}: {ex.Message}");
                _callback.FriendRequestResult(false, "Error interno al obtener solicitudes pendientes.");
            }
        }


        // --- III. ENVIAR SOLICITUD (CREAR FILA CON ESTADO = 0) ---
        public void SendFriendRequest(FriendRequestData request)
        {
            try
            {
                var requester = _context.Player.FirstOrDefault(p => p.nickname == request.RequesterNickname);
                var target = _context.Player.FirstOrDefault(p => p.nickname == request.TargetNickname);

                if (requester == null || target == null || requester.idPlayer == target.idPlayer)
                {
                    _callback.FriendRequestResult(false, "El usuario objetivo no es válido.");
                    return;
                }

                // Verificar si ya existe una relación (pendiente o aceptada) en ambas direcciones
                var existingRequest = _context.FriendList
                    .FirstOrDefault(fl => (fl.Player_idPlayer == requester.idPlayer && fl.Player_idPlayer1 == target.idPlayer) ||
                                          (fl.Player_idPlayer1 == requester.idPlayer && fl.Player_idPlayer == target.idPlayer));

                if (existingRequest != null)
                {
                    if (existingRequest.friendRequest)
                        _callback.FriendRequestResult(false, "Ya son amigos.");
                    else
                        _callback.FriendRequestResult(false, "Ya existe una solicitud pendiente.");
                    return;
                }

                // Crear la solicitud (Requester -> Target, friendRequest = false/0)
                var newRequest = new FriendList
                {
                    Player_idPlayer = requester.idPlayer,
                    Player_idPlayer1 = target.idPlayer,
                    friendRequest = false // 0 = Pendiente
                };

                _context.FriendList.Add(newRequest);
                _context.SaveChanges();

                _callback.FriendRequestResult(true, $"Solicitud enviada a {request.TargetNickname}.");

                // Notificar al jugador objetivo si está online (asumiendo que SessionManager.NotifyPlayer existe)
                var targetCallback = SessionManager.GetSession(target.nickname) as IFriendsCallback;
                if (targetCallback != null)
                {
                    targetCallback.FriendRequestReceived(requester.nickname);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error en SendFriendRequest: {ex.Message}");
                _callback.FriendRequestResult(false, "Error interno al enviar solicitud.");
            }
        }

        // --- IV. ACEPTAR SOLICITUD (ACTUALIZAR FILA A ESTADO = 1) ---
        public void AcceptFriendRequest(FriendRequestData request)
        {
            try
            {
                var target = _context.Player.FirstOrDefault(p => p.nickname == request.TargetNickname); // El que ACEPTA
                var requester = _context.Player.FirstOrDefault(p => p.nickname == request.RequesterNickname); // El que ENVIÓ

                if (target == null || requester == null) return;

                // Encontrar la solicitud PENDIENTE (Requester -> Target, friendRequest = 0)
                var pendingRequest = _context.FriendList
                    .FirstOrDefault(fl => fl.Player_idPlayer == requester.idPlayer &&
                                          fl.Player_idPlayer1 == target.idPlayer &&
                                          fl.friendRequest == false);

                if (pendingRequest == null)
                {
                    _callback.FriendRequestResult(false, "No se encontró la solicitud pendiente.");
                    return;
                }

                // Cambiar el estado a ACEPTADO (true/1)
                pendingRequest.friendRequest = true;
                _context.SaveChanges();

                _callback.FriendRequestResult(true, $"Has aceptado la solicitud de {request.RequesterNickname}.");

                // Notificar al jugador solicitante (Requester) sobre la aceptación
                var requesterCallback = SessionManager.GetSession(requester.nickname) as IFriendsCallback;
                if (requesterCallback != null)
                {
                    requesterCallback.FriendRequestResult(true, $"{request.TargetNickname} ha aceptado tu solicitud.");
                    // Opcional: forzar una actualización de lista si está en la página de amigos
                    requesterCallback.FriendListUpdated(new List<FriendData>());
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error en AcceptFriendRequest: {ex.Message}");
                _callback.FriendRequestResult(false, "Error interno al aceptar solicitud.");
            }
        }

        // --- V. RECHAZAR SOLICITUD (ELIMINAR FILA CON ESTADO = 0) ---
        public void RejectFriendRequest(FriendRequestData request)
        {
            try
            {
                var target = _context.Player.FirstOrDefault(p => p.nickname == request.TargetNickname); // El que RECHAZA
                var requester = _context.Player.FirstOrDefault(p => p.nickname == request.RequesterNickname); // El que ENVIÓ

                if (target == null || requester == null) return;

                // Encontrar la solicitud PENDIENTE (Requester -> Target, friendRequest = 0)
                var pendingRequest = _context.FriendList
                    .FirstOrDefault(fl => fl.Player_idPlayer == requester.idPlayer &&
                                          fl.Player_idPlayer1 == target.idPlayer &&
                                          fl.friendRequest == false);

                if (pendingRequest != null)
                {
                    _context.FriendList.Remove(pendingRequest);
                    _context.SaveChanges();
                    _callback.FriendRequestResult(true, $"Solicitud de {request.RequesterNickname} rechazada.");
                }
                else
                {
                    _callback.FriendRequestResult(false, "No se encontró la solicitud pendiente.");
                }

                // Opcional: Notificar al solicitante que fue rechazada (o simplemente no hacer nada)
            }
            catch (Exception ex)
            {
                Logger.Log($"Error en RejectFriendRequest: {ex.Message}");
                _callback.FriendRequestResult(false, "Error interno al rechazar solicitud.");
            }
        }

        // --- VI. ELIMINAR AMIGO (ELIMINAR FILA CON ESTADO = 1) ---
        public void RemoveFriend(FriendRequestData request)
        {
            try
            {
                var user1 = _context.Player.FirstOrDefault(p => p.nickname == request.RequesterNickname); // Usuario activo
                var user2 = _context.Player.FirstOrDefault(p => p.nickname == request.TargetNickname); // Amigo a eliminar

                if (user1 == null || user2 == null) return;

                // Buscar la relación ACEPTADA (Estado = 1) en cualquier dirección
                var acceptedRelationship = _context.FriendList
                    .FirstOrDefault(fl => fl.friendRequest == true &&
                                          ((fl.Player_idPlayer == user1.idPlayer && fl.Player_idPlayer1 == user2.idPlayer) ||
                                           (fl.Player_idPlayer1 == user1.idPlayer && fl.Player_idPlayer == user2.idPlayer)));

                if (acceptedRelationship != null)
                {
                    _context.FriendList.Remove(acceptedRelationship);
                    _context.SaveChanges();

                    _callback.FriendRequestResult(true, $"{request.TargetNickname} ha sido eliminado de tu lista.");
                }
                else
                {
                    _callback.FriendRequestResult(false, "Error: No se encontró la amistad.");
                }

                // Notificar al amigo eliminado
                var friendCallback = SessionManager.GetSession(user2.nickname) as IFriendsCallback;
                if (friendCallback != null)
                {
                    friendCallback.FriendListUpdated(new List<FriendData>()); // Señal para que actualice/refresque
                    friendCallback.FriendRequestResult(true, $"{user1.nickname} te ha eliminado de su lista.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error en RemoveFriend: {ex.Message}");
                _callback.FriendRequestResult(false, "Error interno al eliminar amigo.");
            }
        }
    }
}