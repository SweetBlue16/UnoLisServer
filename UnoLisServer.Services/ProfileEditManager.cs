using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
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
    public class ProfileEditManager : IProfileEditManager
    {
        private const string FacebookSocialNetworkType = "Facebook";
        private const string InstagramSocialNetworkType = "Instagram";
        private const string TikTokSocialNetworkType = "TikTok";

        private readonly UNOContext _context;
        private readonly IProfileEditCallback _callback;
        private ServiceResponse<ProfileData> _response;

        public ProfileEditManager()
        {
            _context = new UNOContext();
            _callback = OperationContext.Current.GetCallbackChannel<IProfileEditCallback>();
        }

        public void UpdateProfileData(ProfileData data)
        {
            if (data == null)
            {
                _response = new ServiceResponse<ProfileData>(false, MessageCode.InvalidData);
                _callback.ProfileUpdateResponse(_response);
                return;
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var player = _context.Player.FirstOrDefault(p => p.nickname == data.Nickname);
                    if (player == null)
                    {
                        _response = new ServiceResponse<ProfileData>(false, MessageCode.PlayerNotFound);
                        _callback.ProfileUpdateResponse(_response);
                        return;
                    }

                    var account = _context.Account.FirstOrDefault(a => a.Player_idPlayer == player.idPlayer);
                    if (account == null)
                    {
                        _response = new ServiceResponse<ProfileData>(false, MessageCode.PlayerNotFound);
                        _callback.ProfileUpdateResponse(_response);
                        return;
                    }

                    player.fullName = data.FullName;
                    account.email = data.Email;

                    if (!string.IsNullOrWhiteSpace(data.Password))
                    {
                        bool samePassword = PasswordHelper.VerifyPassword(data.Password, account.password);
                        if (samePassword)
                        {
                            _response = new ServiceResponse<ProfileData>(false, MessageCode.SamePassword);
                            _callback.ProfileUpdateResponse(_response);
                            return;
                        }
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

                    _response = new ServiceResponse<ProfileData>(true, MessageCode.ProfileUpdated);
                    _callback.ProfileUpdateResponse(_response);
                }
                catch (CommunicationException communicationEx)
                {
                    Logger.Log($"Error de comunicación durante la actualización de perfil para '{data.Nickname}'. Error: {communicationEx.Message}");
                    transaction.Rollback();
                }
                catch (TimeoutException timeoutEx)
                {
                    Logger.Log($"Timeout durante la actualización de perfil para '{data.Nickname}'. Error: {timeoutEx.Message}");
                    transaction.Rollback();
                }
                catch (DbUpdateException dbUpdateEx)
                {
                    _response = new ServiceResponse<ProfileData>(false, MessageCode.DatabaseError);
                    Logger.Log($"Error de base de datos durante la actualización de perfil para '{data.Nickname}'. Error: {dbUpdateEx.Message}");
                    transaction.Rollback();
                    _callback.ProfileUpdateResponse(_response);
                }
                catch (SqlException sqlEx)
                {
                    _response = new ServiceResponse<ProfileData>(false, MessageCode.SqlError);
                    Logger.Log($"Error SQL durante la actualización de perfil para '{data.Nickname}'. Error: {sqlEx.Message}");
                    transaction.Rollback();
                    _callback.ProfileUpdateResponse(_response);
                }
                catch (Exception ex)
                {
                    _response = new ServiceResponse<ProfileData>(false, MessageCode.ProfileUpdateFailed);
                    Logger.Log($"Error inesperado durante la actualización de perfil para '{data.Nickname}'. Error: {ex.Message}");
                    transaction.Rollback();
                    _callback.ProfileUpdateResponse(_response);
                }
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
