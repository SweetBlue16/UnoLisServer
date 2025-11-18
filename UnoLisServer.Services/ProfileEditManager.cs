using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data;
using UnoLisServer.Services.Validators;
using UnoLisServer.Common.Exceptions;

namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ProfileEditManager : IProfileEditManager
    {
        private const string FacebookSocialNetworkType = "Facebook";
        private const string InstagramSocialNetworkType = "Instagram";
        private const string TikTokSocialNetworkType = "TikTok";

        private readonly UNOContext _context;
        private readonly IProfileEditCallback _callback;
        private ResponseInfo<ProfileData> _responseInfo;

        public ProfileEditManager()
        {
            _context = new UNOContext();
            _callback = OperationContext.Current.GetCallbackChannel<IProfileEditCallback>();
        }

        public void UpdateProfileData(ProfileData data)
        {
            string userNickname = data?.Nickname ?? "Unknown";
            try
            {
                Logger.Log($"[INFO] Solicitud de actualización de perfil para '{userNickname}'...");
                ProfileEditValidator.ValidateProfileUpdate(data);

                UpdateProfile(data);
                _responseInfo = new ResponseInfo<ProfileData>(
                    MessageCode.ProfileUpdated,
                    true,
                    $"[INFO] Perfil de '{userNickname}' actualizado correctamente."
                );
            }
            catch (ValidationException validationEx)
            {
                _responseInfo = new ResponseInfo<ProfileData>(
                    validationEx.ErrorCode,
                    false,
                    $"[WARNING] Validación fallida para actualización de perfil de '{userNickname}'. Error: {validationEx.Message}"
                );
            }
            catch (DbUpdateException dbUpdateEx)
            {
                _responseInfo = new ResponseInfo<ProfileData>(
                    MessageCode.DatabaseError,
                    false,
                    $"[FATAL] Error de base de datos al actualizar perfil de '{userNickname}'. Error: {dbUpdateEx.Message}"
                );
            }
            catch (SqlException sqlEx)
            {
                _responseInfo = new ResponseInfo<ProfileData>(
                    MessageCode.DatabaseError,
                    false,
                    $"[FATAL] Error SQL al actualizar perfil de '{userNickname}'. Error: {sqlEx.Message}"
                );
            }
            catch (CommunicationException communicationEx)
            {
                _responseInfo = new ResponseInfo<ProfileData>(
                    MessageCode.ConnectionFailed,
                    false,
                    $"[ERROR] Comunicación al actualizar perfil de '{userNickname}'. Error: {communicationEx.Message}"
                );
            }
            catch (TimeoutException timeoutEx)
            {
                _responseInfo = new ResponseInfo<ProfileData>(
                    MessageCode.Timeout,
                    false,
                    $"[ERROR] Tiempo de espera al actualizar perfil de '{userNickname}'. Error: {timeoutEx.Message}"
                );
            }
            catch (Exception ex)
            {
                _responseInfo = new ResponseInfo<ProfileData>(
                    MessageCode.ProfileUpdateFailed,
                    false,
                    $"[FATAL] Error inesperado al actualizar perfil de '{userNickname}'. Error: {ex.Message}"
                );
            }
            ResponseHelper.SendResponse(_callback.ProfileUpdateResponse, _responseInfo);
        }

        private void UpdateProfile(ProfileData data)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    Logger.Log($"[DEBUG] Buscando player: {data.Nickname}");
                    var player = _context.Player.FirstOrDefault(p => p.nickname == data.Nickname);

                    if (player == null)
                    {
                        Logger.Log($"[ERROR] Player no encontrado: {data.Nickname}");
                        throw new ValidationException(MessageCode.PlayerNotFound, "Jugador no encontrado");
                    }

                    var account = _context.Account.FirstOrDefault(a => a.Player.idPlayer == player.idPlayer);

                    if (account == null)
                    {
                        throw new ValidationException(MessageCode.AccountNotVerified, "Cuenta no encontrada");
                    }

                    player.fullName = data.FullName;
                    account.email = data.Email;

                    if (!string.IsNullOrWhiteSpace(data.Password))
                    {
                        account.password = PasswordHelper.HashPassword(data.Password);
                    }

                    var socialNetworks = _context.SocialNetwork
                        .Where(sn => sn.Player_idPlayer == player.idPlayer)
                        .ToList();

                    UpdateOrAddNetwork(player.idPlayer, socialNetworks, new NetworkUpdateData
                    {
                        Type = FacebookSocialNetworkType,
                        Url = data.FacebookUrl
                    });

                    UpdateOrAddNetwork(player.idPlayer, socialNetworks, new NetworkUpdateData
                    {
                        Type = InstagramSocialNetworkType,
                        Url = data.InstagramUrl
                    });

                    UpdateOrAddNetwork(player.idPlayer, socialNetworks, new NetworkUpdateData
                    {
                        Type = TikTokSocialNetworkType,
                        Url = data.TikTokUrl
                    });

                    _context.SaveChanges();

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    Logger.Log($"[DEBUG] ERROR en UpdateProfile: {ex.Message}");
                    Logger.Log($"[DEBUG] Inner Exception: {ex.InnerException?.Message}");
                    Logger.Log($"[DEBUG] StackTrace: {ex.StackTrace}");

                    transaction.Rollback();
                    throw;
                }
            }
        }

        private void UpdateOrAddNetwork(int playerId, List<SocialNetwork> existingNetworks, NetworkUpdateData data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.Type))
            {
                return;
            }

            var existing = existingNetworks.FirstOrDefault(sn => sn.tipoRedSocial == data.Type);
            if (existing != null)
            {
                existing.linkRedSocial = data.Url;
            }
            else
            {
                var newNetwork = new SocialNetwork
                {
                    tipoRedSocial = data.Type,
                    linkRedSocial = data.Url,
                    Player_idPlayer = playerId
                };
                _context.SocialNetwork.Add(newNetwork);
            }
        }
    }
}
