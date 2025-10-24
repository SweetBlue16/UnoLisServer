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

namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ProfileViewManager : IProfileViewManager
    {
        private readonly UNOContext _context;
        private readonly IProfileViewCallback _callback;
        private ServiceResponse<ProfileData> _response;

        public ProfileViewManager()
        {
            _context = new UNOContext();
            _callback = OperationContext.Current.GetCallbackChannel<IProfileViewCallback>();
        }

        public void GetProfileData(string nickname)
        {
            try
            {
                var player = _context.Player.FirstOrDefault(p => p.nickname == nickname);
                if (player == null)
                {
                    _response = new ServiceResponse<ProfileData>(false, MessageCode.PlayerNotFound);
                    _callback.ProfileDataReceived(_response);
                    Logger.Log($"No se encontró el perfil para '{nickname}'.");
                    return;
                }

                var account = _context.Account.FirstOrDefault(a => a.Player_idPlayer == player.idPlayer);
                var statistics = _context.PlayerStatistics.FirstOrDefault(s => s.Player_idPlayer == player.idPlayer);
                var socialNetworks = _context.SocialNetwork
                    .Where(sn => sn.Player_idPlayer == player.idPlayer)
                    .ToList();

                string facebookUrl = socialNetworks.FirstOrDefault(sn => sn.tipoRedSocial == "Facebook")?.linkRedSocial;
                string instagramUrl = socialNetworks.FirstOrDefault(sn => sn.tipoRedSocial == "Instagram")?.linkRedSocial;
                string tikTokUrl = socialNetworks.FirstOrDefault(sn => sn.tipoRedSocial == "TikTok")?.linkRedSocial;

                var profileData = new ProfileData
                {
                    Nickname = player.nickname,
                    FullName = player.fullName,

                    Email = account?.email,
                    Password = PasswordHelper.HashPassword(account?.password),

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
                _response = new ServiceResponse<ProfileData>(true, MessageCode.ProfileDataRetrieved, profileData);
                _callback.ProfileDataReceived(_response);
            }
            catch (CommunicationException communicationEx)
            {
                Logger.Log($"Error de comunicación al obtener el perfil para '{nickname}': {communicationEx.Message}");
                _response = new ServiceResponse<ProfileData>(false, MessageCode.ProfileFetchFailed);
                _callback.ProfileDataReceived(_response);
            }
            catch (TimeoutException timeoutEx)
            {
                Logger.Log($"Tiempo de espera agotado al obtener el perfil para '{nickname}': {timeoutEx.Message}");
                _response = new ServiceResponse<ProfileData>(false, MessageCode.Timeout);
                _callback.ProfileDataReceived(_response);
            }
            catch (SqlException dbEx)
            {
                Logger.Log($"Error de base de datos al obtener el perfil para '{nickname}': {dbEx.Message}");
                _response = new ServiceResponse<ProfileData>(false, MessageCode.DatabaseError);
                _callback.ProfileDataReceived(_response);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error inesperado al obtener el perfil para '{nickname}': {ex.Message}");
                _response = new ServiceResponse<ProfileData>(false, MessageCode.GeneralServerError);
                _callback.ProfileDataReceived(_response);
            }
        }
    }
}
