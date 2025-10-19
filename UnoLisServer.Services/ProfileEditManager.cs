using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data;

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

        public ProfileEditManager()
        {
            _context = new UNOContext();
            _callback = OperationContext.Current.GetCallbackChannel<IProfileEditCallback>();
        }

        public void UpdateProfileData(ProfileData data)
        {
            try
            {
                var player = _context.Player.FirstOrDefault(p => p.nickname == data.Nickname);
                if (player == null)
                {
                    _callback.ProfileUpdateResponse(false, "Jugador no encontrado.");
                    return;
                }
                player.fullName = data.FullName;

                var account = _context.Account.FirstOrDefault(a => a.Player_idPlayer == player.idPlayer);
                if (account != null)
                {
                    account.email = data.Email;
                    if (!string.IsNullOrWhiteSpace(data.Password))
                    {
                        account.password = PasswordHelper.HashPassword(data.Password);
                    }
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
                _callback.ProfileUpdateResponse(true, "Perfil actualizado correctamente.");
            }
            catch (Exception ex)
            {
                _callback.ProfileUpdateResponse(false, $"Error al actualizar: {ex.Message}");
            }
        }

        private void UpdateOrAddNetwork(int playerId, List<SocialNetwork> existingNetworks, NetworkUpdateData data)
        {
            if (data == null)
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
