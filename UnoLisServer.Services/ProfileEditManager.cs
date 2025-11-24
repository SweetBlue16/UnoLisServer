using System;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
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
    public class ProfileEditManager : IProfileEditManager
    {
        private readonly IProfileEditCallback _callback;
        private readonly IPlayerRepository _playerRepository;

        public ProfileEditManager() : this(new PlayerRepository())
        {
        }

        public ProfileEditManager(IPlayerRepository playerRepository, IProfileEditCallback callbackTest = null)
        {
            _playerRepository = playerRepository;
            _callback = callbackTest ?? OperationContext.Current?.GetCallbackChannel<IProfileEditCallback>();
        }

        public async void UpdateProfileData(ProfileData data)
        {
            string userNickname = data?.Nickname ?? "Unknown";
            ResponseInfo<ProfileData> responseInfo;

            try
            {
                ProfileEditValidator.ValidateProfileFormats(data);

                var currentPlayer = await _playerRepository.GetPlayerProfileByNicknameAsync(userNickname);

                if (currentPlayer == null)
                {
                    throw new ValidationException(MessageCode.PlayerNotFound, "Jugador no encontrado.");
                }

                if (!string.IsNullOrWhiteSpace(data.Password))
                {
                    string currentHash = currentPlayer.Account.FirstOrDefault()?.password;
                    if (PasswordHelper.VerifyPassword(data.Password, currentHash))
                    {
                        throw new ValidationException(MessageCode.SamePassword, "La nueva contraseña no puede ser igual a la anterior.");
                    }
                }

                await _playerRepository.UpdatePlayerProfileAsync(data);

                responseInfo = new ResponseInfo<ProfileData>(
                    MessageCode.ProfileUpdated,
                    true,
                    $"[INFO] Perfil de '{userNickname}' actualizado."
                );
            }
            catch (ValidationException valEx)
            {
                responseInfo = new ResponseInfo<ProfileData>(valEx.ErrorCode, false, valEx.Message);
                Logger.Warn($"[VALIDATION] Error al editar perfil: {valEx.Message}");
            }
            catch (Exception ex)
            {
                responseInfo = new ResponseInfo<ProfileData>(MessageCode.ProfileUpdateFailed, false, "Error interno al actualizar perfil.");
                Logger.Error($"[ERROR] Fallo al actualizar perfil de {userNickname}", ex);
            }

            if (_callback != null)
            {
                ResponseHelper.SendResponse(_callback.ProfileUpdateResponse, responseInfo);
            }
        }
    }
}