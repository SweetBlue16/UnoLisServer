using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Exceptions;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data;
using UnoLisServer.Services.Validators;

namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class AvatarManager : IAvatarManager
    {
        private readonly IAvatarCallback _callback;

        public AvatarManager()
        {
            _callback = OperationContext.Current.GetCallbackChannel<IAvatarCallback>();
        }

        public void GetPlayerAvatars(string nickname)
        {
            ResponseInfo<List<PlayerAvatar>> responseInfo;
            try
            {
                using (var context = new UNOContext())
                {
                    var player = AvatarValidator.ValidatePlayerExists(context, nickname);
                    var unlockedIds = player.AvatarsUnlocked.Select(au => au.Avatar_idAvatar).ToHashSet();
                    int? selectedAvatarId = player.SelectedAvatar_Avatar_idAvatar;

                    var allAvatars = context.Avatar.Select(avatar => new PlayerAvatar
                    {
                        AvatarId = avatar.idAvatar,
                        AvatarName = avatar.avatarName,
                        Description = avatar.avatarDescription,
                        Rarity = avatar.avatarRarity,
                        IsUnlocked = unlockedIds.Contains(avatar.idAvatar),
                        IsSelected = (avatar.idAvatar == selectedAvatarId)
                    }).ToList();

                    responseInfo = new ResponseInfo<List<PlayerAvatar>>(
                        MessageCode.Success,
                        true,
                        $"[INFO] Avatares obtenidos para el jugador '{nickname}'.",
                        allAvatars
                    );
                }
            }
            catch (ValidationException validationEx)
            {
                responseInfo = new ResponseInfo<List<PlayerAvatar>>(
                    validationEx.ErrorCode,
                    false,
                    $"[WARNING] Validación durante la obtención de avatares para '{nickname}': {validationEx.Message}"
                );
            }
            catch (CommunicationException communicationEx)
            {
                responseInfo = new ResponseInfo<List<PlayerAvatar>>(
                    MessageCode.ConnectionFailed,
                    false,
                    $"[ERROR] Comunicación con '{nickname}'. Error: {communicationEx.Message}"
                );
            }
            catch (TimeoutException timeoutEx)
            {
                responseInfo = new ResponseInfo<List<PlayerAvatar>>(
                    MessageCode.Timeout,
                    false,
                    $"[ERROR] Tiempo de espera agotado para '{nickname}'. Error: {timeoutEx.Message}"
                );
            }
            catch (SqlException dbEx)
            {
                responseInfo = new ResponseInfo<List<PlayerAvatar>>(
                    MessageCode.DatabaseError,
                    false,
                    $"[ERROR] Base de datos durante la obtención de avatares para '{nickname}': {dbEx.Message}"
                );
            }
            catch (Exception ex)
            {
                responseInfo = new ResponseInfo<List<PlayerAvatar>>(
                    MessageCode.LoginInternalError,
                    false,
                    $"[ERROR] Excepción no controlada durante la obtención de avatares para '{nickname}': {ex.Message}"
                );
            }
            ResponseHelper.SendResponse(_callback.AvatarsDataReceived, responseInfo);
        }

        public void SetPlayerAvatar(string nickname, int newAvatarId)
        {
            ResponseInfo<object> responseInfo;
            try
            {
                using (var context = new UNOContext())
                {
                    var player = AvatarValidator.ValidatePlayerExists(context, nickname);
                    AvatarValidator.ValidateAvatarIsUnlocked(player, newAvatarId);

                    player.SelectedAvatar_Player_idPlayer = player.idPlayer;
                    player.SelectedAvatar_Avatar_idAvatar = newAvatarId;
                    context.SaveChanges();

                    responseInfo = new ResponseInfo<object>(
                        MessageCode.AvatarChanged,
                        true,
                        $"[INFO] Avatar actualizado para el jugador '{nickname}'."
                    );
                }
            }
            catch (ValidationException validationEx)
            {
                responseInfo = new ResponseInfo<object>(
                    validationEx.ErrorCode,
                    false,
                    $"[WARNING] Validación fallida para actualización del avatar de '{nickname}'. Error: {validationEx.Message}"
                );
            }
            catch (DbUpdateException dbUpdateEx)
            {
                responseInfo = new ResponseInfo<object>(
                    MessageCode.DatabaseError,
                    false,
                    $"[FATAL] Error de base de datos al actualizar el avatar de '{nickname}'. Error: {dbUpdateEx.Message}"
                );
            }
            catch (SqlException sqlEx)
            {
                responseInfo = new ResponseInfo<object>(
                    MessageCode.DatabaseError,
                    false,
                    $"[FATAL] Error SQL al actualizar el avatar de '{nickname}'. Error: {sqlEx.Message}"
                );
            }
            catch (CommunicationException communicationEx)
            {
                responseInfo = new ResponseInfo<object>(
                    MessageCode.ConnectionFailed,
                    false,
                    $"[ERROR] Comunicación al actualizar el avatar de '{nickname}'. Error: {communicationEx.Message}"
                );
            }
            catch (TimeoutException timeoutEx)
            {
                responseInfo = new ResponseInfo<object>(
                    MessageCode.Timeout,
                    false,
                    $"[ERROR] Tiempo de espera al actualizar el avatar de '{nickname}'. Error: {timeoutEx.Message}"
                );
            }
            catch (Exception ex)
            {
                responseInfo = new ResponseInfo<object>(
                    MessageCode.ProfileUpdateFailed,
                    false,
                    $"[FATAL] Error inesperado al actualizar el avatar de '{nickname}'. Error: {ex.Message}"
                );
            }
            ResponseHelper.SendResponse(_callback.AvatarUpdateResponse, responseInfo);
        }
    }
}
