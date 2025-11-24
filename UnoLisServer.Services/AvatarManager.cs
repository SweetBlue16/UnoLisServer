using System;
using System.Collections.Generic;
using System.ServiceModel;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Exceptions;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data.Repositories;
using UnoLisServer.Data.RepositoryInterfaces;
using UnoLisServer.Services.Validators;

namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class AvatarManager : IAvatarManager
    {
        private readonly IAvatarCallback _callback;
        private readonly IPlayerRepository _playerRepository;

        public AvatarManager() : this(new PlayerRepository())
        {
        }

        public AvatarManager(IPlayerRepository repository, IAvatarCallback callback = null)
        {
            _playerRepository = repository;
            _callback = callback ?? OperationContext.Current?.GetCallbackChannel<IAvatarCallback>();
        }

        public async void GetPlayerAvatars(string nickname)
        {
            ResponseInfo<List<PlayerAvatar>> responseInfo;
            try
            {
                var avatars = await _playerRepository.GetPlayerAvatarsAsync(nickname);

                if (avatars == null)
                {
                    throw new ValidationException(MessageCode.PlayerNotFound, "Jugador no encontrado.");
                }

                responseInfo = new ResponseInfo<List<PlayerAvatar>>(
                    MessageCode.Success, true, "Avatares obtenidos.", avatars);
            }
            catch (ValidationException valEx)
            {
                responseInfo = new ResponseInfo<List<PlayerAvatar>>(valEx.ErrorCode, false, valEx.Message);
            }
            catch (Exception ex)
            {
                responseInfo = new ResponseInfo<List<PlayerAvatar>>(MessageCode.DatabaseError, false, "Error interno.");
                Logger.Error($"[AVATAR] Error Get: {nickname}", ex);
            }

            if (_callback != null)
                ResponseHelper.SendResponse(_callback.AvatarsDataReceived, responseInfo);
        }

        public async void SetPlayerAvatar(string nickname, int newAvatarId)
        {
            ResponseInfo<object> responseInfo;
            try
            {
                var unlockedAvatars = await _playerRepository.GetPlayerAvatarsAsync(nickname);

                if (unlockedAvatars == null)
                    throw new ValidationException(MessageCode.PlayerNotFound, "Jugador no encontrado.");

                AvatarValidator.ValidateSelection(newAvatarId, unlockedAvatars);

                await _playerRepository.UpdateSelectedAvatarAsync(nickname, newAvatarId);

                responseInfo = new ResponseInfo<object>(MessageCode.AvatarChanged, true, "Avatar actualizado.");
            }
            catch (ValidationException valEx)
            {
                responseInfo = new ResponseInfo<object>(valEx.ErrorCode, false, valEx.Message);
            }
            catch (Exception ex)
            {
                responseInfo = new ResponseInfo<object>(MessageCode.ProfileUpdateFailed, false, "Error al cambiar avatar.");
                Logger.Error($"[AVATAR] Error Set: {nickname} -> {newAvatarId}", ex);
            }

            if (_callback != null)
                ResponseHelper.SendResponse(_callback.AvatarUpdateResponse, responseInfo);
        }
    }
}