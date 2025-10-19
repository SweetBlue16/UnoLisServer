using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
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
                    _callback.ProfileDataReceived(false, null);
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
                    Password = account?.password,

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
                _callback.ProfileDataReceived(true, profileData);
            }
            catch (Exception)
            {
                _callback.ProfileDataReceived(false, null);
            }
        }
    }
}
