using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
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
    public class ProfileViewManager : IProfileViewManager
    {
        private readonly IProfileViewCallback _callback;
        private ResponseInfo<ProfileData> _responseInfo;

        public ProfileViewManager()
        {
            _callback = OperationContext.Current.GetCallbackChannel<IProfileViewCallback>();
        }

        public void GetProfileData(string nickname)
        {
            string userNickname = nickname ?? "Unknown";
            try
            {
                Logger.Log($"[INFO] Iniciando obtención de perfil para '{userNickname}'.");

                var player = ProfileViewValidator.ValidateProfileData(userNickname);
                var profileData = GetProfileData(player);

                _responseInfo = new ResponseInfo<ProfileData>(
                    MessageCode.ProfileDataRetrieved,
                    true,
                    $"[INFO] Perfil obtenido exitosamente para '{userNickname}'.",
                    profileData
                );
            }
            catch (ValidationException validationEx)
            {
                _responseInfo = new ResponseInfo<ProfileData>(
                    validationEx.ErrorCode,
                    false,
                    $"[WARNING] Validación fallida al obtener perfil para '{userNickname}'. Error: {validationEx.Message}"
                );
            }
            catch (CommunicationException communicationEx)
            {
                _responseInfo = new ResponseInfo<ProfileData>(
                    MessageCode.ConnectionFailed,
                    false,
                    $"[ERROR] Comunicación al obtener perfil para '{userNickname}'. Error: {communicationEx.Message}"
                );
            }
            catch (TimeoutException timeoutEx)
            {
                _responseInfo = new ResponseInfo<ProfileData>(
                    MessageCode.Timeout,
                    false,
                    $"[ERROR] Tiempo de espera agotado al obtener perfil para '{userNickname}'. Error: {timeoutEx.Message}"
                );
            }
            catch (SqlException dbEx)
            {
                _responseInfo = new ResponseInfo<ProfileData>(
                    MessageCode.DatabaseError,
                    false,
                    $"[FATAL] Error de base de datos al obtener perfil para '{userNickname}'. Error: {dbEx.Message}"
                );
            }
            catch (Exception ex)
            {
                _responseInfo = new ResponseInfo<ProfileData>(
                    MessageCode.ProfileFetchFailed,
                    false,
                    $"[FATAL] Error inesperado al obtener perfil para '{userNickname}'. Error: {ex.Message}"
                );
            }
            ResponseHelper.SendResponse(_callback.ProfileDataReceived, _responseInfo);
        }

        private ProfileData GetProfileData(Player player)
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

            var profileData = new ProfileData
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
            return profileData;
        }
    }
}
