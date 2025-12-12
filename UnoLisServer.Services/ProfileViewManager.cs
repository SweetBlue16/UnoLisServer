using System;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data;
using UnoLisServer.Data.Repositories;
using UnoLisServer.Data.RepositoryInterfaces;

namespace UnoLisServer.Services
{
    /// <summary>
    /// Class to manage logic for viewing profile
    /// </summary>
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
            ResponseInfo<ProfileData> responseInfo = null;

            try
            {
                if (UserHelper.IsGuest(userNickname))
                {
                    responseInfo = CreateGuestProfileResponse(userNickname);
                }
                else
                {
                    responseInfo = await ExecuteGetProfileLogic(userNickname);
                }
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[WCF] Communication error with requesting profile. '{commEx}'.");
                responseInfo = new ResponseInfo<ProfileData>(MessageCode.ConnectionFailed, false, "Connection error.");
            }
            catch (TimeoutException timeoutEx)
            {
                Logger.Warn($"[WCF] WCF Timeout. '{timeoutEx}'.");
                responseInfo = new ResponseInfo<ProfileData>(MessageCode.Timeout, false, "Request timed out.");
            }
            catch (Exception ex) when (ex.Message == "DataStore_Unavailable")
            {
                Logger.Error($"[CRITICAL] Failed fetching profile for '{userNickname}'. Data Store unavailable.", ex);
                responseInfo = new ResponseInfo<ProfileData>(
                    MessageCode.DatabaseError,
                    false,
                    "Service unavailable. Please try again later."
                );
            }
            catch (Exception ex) when (ex.Message == "Server_Busy")
            {
                Logger.Warn($"[WARN] Timeout fetching profile for '{userNickname}'.");
                responseInfo = new ResponseInfo<ProfileData>(
                    MessageCode.Timeout,
                    false,
                    "Server request timed out."
                );
            }
            catch (Exception ex)
            {
                Logger.Error($"[CRITICAL] Unexpected error fetching profile for '{userNickname}'.", ex);
                responseInfo = new ResponseInfo<ProfileData>(
                    MessageCode.ProfileFetchFailed,
                    false,
                    "An unexpected error occurred."
                );
            }

            try
            {
                if (_callback != null && responseInfo != null)
                {
                    ResponseHelper.SendResponse(_callback.ProfileDataReceived, responseInfo);
                }
            }
            catch (Exception sendEx)
            {
                Logger.Warn($"[WCF] Failed to send profile data response. Client might be disconnected. '{sendEx}'");
            }
        }

        private async Task<ResponseInfo<ProfileData>> ExecuteGetProfileLogic(string userNickname)
        {
            var player = await _playerRepository.GetPlayerProfileByNicknameAsync(userNickname);

            if (player.idPlayer == 0)
            {
                return new ResponseInfo<ProfileData>(MessageCode.PlayerNotFound, false,
                    "Player profile not found.", null
                );
            }

            var profileData = MapToProfileData(player);

            return new ResponseInfo<ProfileData>(MessageCode.ProfileDataRetrieved, true,
                "Profile data retrieved successfully.", profileData
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

        private ResponseInfo<ProfileData> CreateGuestProfileResponse(string nickname)
        {
            var guestData = new ProfileData
            {
                Nickname = nickname,
                FullName = "Guest Player",
                Email = " ",
                SelectedAvatarName = "LogoUNO",

                ExperiencePoints = 0,
                MatchesPlayed = 0,
                Wins = 0,
                Losses = 0,
                Streak = 0,
                MaxStreak = 0,

                FacebookUrl = null,
                InstagramUrl = null,
                TikTokUrl = null
            };

            Logger.Log($"[PROFILE] Generated dummy profile for guest.");

            return new ResponseInfo<ProfileData>(
                MessageCode.ProfileDataRetrieved,
                true,
                "Guest profile generated.",
                guestData
            );
        }
    }
}
