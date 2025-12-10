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
                    throw new ValidationException(MessageCode.PlayerNotFound, "Player not found.");
                }

                responseInfo = new ResponseInfo<List<PlayerAvatar>>(
                    MessageCode.Success, true, "Avatars retrieved successfully.", avatars);
            }
            catch (ValidationException valEx)
            {
                Logger.Warn($"[AVATAR] Validation failed fetching avatars: {valEx.Message}");
                responseInfo = new ResponseInfo<List<PlayerAvatar>>(valEx.ErrorCode, false, valEx.Message);
            }
            catch (Exception ex) when (ex.Message == "DataStore_Unavailable")
            {
                Logger.Error($"[CRITICAL] Fetching avatars failed. Data Store unavailable.", ex);
                responseInfo = new ResponseInfo<List<PlayerAvatar>>(
                    MessageCode.DatabaseError,
                    false,
                    "Service unavailable. Please try again later."
                );
            }
            catch (Exception ex) when (ex.Message == "Server_Busy")
            {
                Logger.Warn($"[WARN] Fetching avatars timeout.");
                responseInfo = new ResponseInfo<List<PlayerAvatar>>(
                    MessageCode.Timeout,
                    false,
                    "Request timed out."
                );
            }
            catch (Exception ex)
            {
                Logger.Error($"[CRITICAL] Unexpected error fetching avatars.", ex);
                responseInfo = new ResponseInfo<List<PlayerAvatar>>(
                    MessageCode.DatabaseError,
                    false,
                    "An internal error occurred."
                );
            }

            try
            {
                if (_callback != null && responseInfo != null)
                {
                    ResponseHelper.SendResponse(_callback.AvatarsDataReceived, responseInfo);
                }
            }
            catch (Exception sendEx)
            {
                Logger.Warn($"[WCF] Failed to send avatar list. '{sendEx}'.");
            }
        }

        public async void SetPlayerAvatar(string nickname, int newAvatarId)
        {
            string safeNickname = nickname ?? "Unknown";
            ResponseInfo<object> responseInfo;
            try
            {
                var unlockedAvatars = await _playerRepository.GetPlayerAvatarsAsync(safeNickname);

                if (unlockedAvatars == null)
                {
                    throw new ValidationException(MessageCode.PlayerNotFound, "Player not found.");
                }

                AvatarValidator.ValidateSelection(newAvatarId, unlockedAvatars);
                await _playerRepository.UpdateSelectedAvatarAsync(safeNickname, newAvatarId);

                responseInfo = new ResponseInfo<object>(MessageCode.AvatarChanged, true, "Avatar " +
                    "updated successfully.");
            }
            catch (ValidationException valEx)
            {
                Logger.Warn($"[AVATAR] Invalid avatar selection by '{safeNickname}': {valEx.Message}");
                responseInfo = new ResponseInfo<object>(valEx.ErrorCode, false, valEx.Message);
            }
            catch (Exception ex) when (ex.Message == "Data_Conflict")
            {
                Logger.Error($"[DATA] Constraint violation setting avatar for '{safeNickname}'. " +
                    $"Avatar ID {newAvatarId} might be invalid.", ex);
                responseInfo = new ResponseInfo<object>(
                    MessageCode.ProfileUpdateFailed,
                    false,
                    "Failed to update avatar. Invalid selection."
                );
            }
            catch (Exception ex) when (ex.Message == "DataStore_Unavailable")
            {
                Logger.Error($"[CRITICAL] Setting avatar failed for '{safeNickname}'. Data Store unavailable.", ex);
                responseInfo = new ResponseInfo<object>(
                    MessageCode.DatabaseError,
                    false,
                    "Service unavailable."
                );
            }
            catch (Exception ex) when (ex.Message == "Server_Busy")
            {
                Logger.Warn($"[WARN] Setting avatar timeout.");
                responseInfo = new ResponseInfo<object>(MessageCode.Timeout, false, "Request timed out.");
            }
            catch (Exception ex)
            {
                Logger.Error($"[CRITICAL] Unexpected error setting avatar.", ex);
                responseInfo = new ResponseInfo<object>(
                    MessageCode.ProfileUpdateFailed,
                    false,
                    "An internal error occurred."
                );
            }

            try
            {
                if (_callback != null && responseInfo != null)
                {
                    ResponseHelper.SendResponse(_callback.AvatarUpdateResponse, responseInfo);
                }
            }
            catch (Exception sendEx)
            {
                Logger.Warn($"[WCF] Failed to send avatar update response. '{sendEx}'.");
            }
        }
    }
}