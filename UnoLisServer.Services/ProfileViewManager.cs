using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Exceptions;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data;
using UnoLisServer.Data.Repositories;
using UnoLisServer.Data.RepositoryInterfaces;
using UnoLisServer.Services.Validators;

namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ProfileViewManager : IProfileViewManager
    {
        private readonly IProfileViewCallback _callback;
        private readonly IPlayerRepository _playerRepository;
        public ProfileViewManager() : this(new PlayerRepository())
        {
        }

        public ProfileViewManager(IPlayerRepository playerRepository, IProfileViewCallback callbackTest = null)
        {
            _playerRepository = playerRepository;
            _callback = callbackTest ?? OperationContext.Current?.GetCallbackChannel<IProfileViewCallback>();
        }

        public async void GetProfileData(string nickname)
        {
            string userNickname = nickname ?? "Unknown";
            ResponseInfo<ProfileData> responseInfo;

            try
            {
                responseInfo = await ExecuteGetProfileLogic(userNickname);
            }
            catch (TimeoutException timeoutEx)
            {
                responseInfo = new ResponseInfo<ProfileData>(MessageCode.Timeout, false,
                    $"[ERROR] Tiempo de espera agotado al obtener perfil para '{userNickname}'. Error: {timeoutEx.Message}"
                );
                Logger.Error(responseInfo.LogMessage, timeoutEx);
            }
            catch (CommunicationException communicationEx)
            {
                responseInfo = new ResponseInfo<ProfileData>(MessageCode.ConnectionFailed, false,
                    $"[ERROR] Comunicación al obtener perfil para '{userNickname}'. Error: {communicationEx.Message}"
                );
                Logger.Error(responseInfo.LogMessage, communicationEx);
            }
            catch (SqlException dbEx)
            {
                responseInfo = new ResponseInfo<ProfileData>(MessageCode.DatabaseError, false,
                    $"[FATAL] Error de base de datos al obtener perfil para '{userNickname}'. Error: {dbEx.Message}"
                );
                Logger.Error(responseInfo.LogMessage, dbEx);
            }
            catch (EntityException entityEx)
            {
                responseInfo = new ResponseInfo<ProfileData>(MessageCode.DatabaseError, false,
                    $"[FATAL] Error de Entity Framework: {entityEx.Message}");
                Logger.Error(responseInfo.LogMessage, entityEx);
            }
            catch (Exception ex)
            {
                responseInfo = new ResponseInfo<ProfileData>(
                    MessageCode.ProfileFetchFailed,
                    false,
                    $"[FATAL] Error inesperado al obtener perfil para '{userNickname}'. Error: {ex.Message}"
                );
                Logger.Error(responseInfo.LogMessage, ex);
            }

            if (_callback != null)
            {
                ResponseHelper.SendResponse(_callback.ProfileDataReceived, responseInfo);
            }
        }

        private async Task<ResponseInfo<ProfileData>> ExecuteGetProfileLogic(string userNickname)
        {
            Logger.Log($"[INFO] Iniciando obtención de perfil para '{userNickname}'.");

            var player = await _playerRepository.GetPlayerProfileByNicknameAsync(userNickname);

            if (player == null)
            {
                return new ResponseInfo<ProfileData>(MessageCode.PlayerNotFound, false,
                    $"[INFO] No se encontró el perfil para '{userNickname}'.", null
                );
            }

            var profileData = MapToProfileData(player);

            return new ResponseInfo<ProfileData>(MessageCode.ProfileDataRetrieved, true,
                $"[INFO] Perfil obtenido exitosamente para '{userNickname}'.", profileData
            );
        }

        private ProfileData MapToProfileData(Player player)
        {
            var account = player.Account.FirstOrDefault();
            var statistics = player.PlayerStatistics.FirstOrDefault();
            var socialNetworks = player.SocialNetwork;

            string facebookUrl = socialNetworks.FirstOrDefault(sn => sn.tipoRedSocial == "Facebook")?.linkRedSocial;
            string instagramUrl = socialNetworks.FirstOrDefault(sn => sn.tipoRedSocial == "Instagram")?.linkRedSocial;
            string tikTokUrl = socialNetworks.FirstOrDefault(sn => sn.tipoRedSocial == "TikTok")?.linkRedSocial;

            string selectedAvatarName = "LogoUNO";
            if (player.SelectedAvatar_Avatar_idAvatar != null)
            {
                selectedAvatarName = player.AvatarsUnlocked
                    .FirstOrDefault(au => au.Avatar_idAvatar == player.SelectedAvatar_Avatar_idAvatar)
                    ?.Avatar.avatarName ?? selectedAvatarName;
            }

            return new ProfileData
            {
                Nickname = player.nickname,
                FullName = player.fullName,
                Email = account?.email,
                SelectedAvatarName = selectedAvatarName,

                ExperiencePoints = statistics?.globalPoints ?? 0,
                MatchesPlayed = statistics?.matchesPlayed ?? 0,
                Wins = statistics?.wins ?? 0,
                Losses = statistics?.loses ?? 0,
                Streak = statistics?.streak ?? 0,
                MaxStreak = statistics?.maxStreak ?? 0,

                FacebookUrl = facebookUrl,
                InstagramUrl = instagramUrl,
                TikTokUrl = tikTokUrl
            };
        }
    }
}
