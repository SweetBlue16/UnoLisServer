using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Contracts;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data;
using UnoLisServer.Services;

namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ProfileManager : IProfileManager
    {
        private readonly UNOContext _context;
        private readonly IProfileCallback _callback;

        public ProfileManager()
        {
            _context = new UNOContext();
            _callback = OperationContext.Current.GetCallbackChannel<IProfileCallback>();
        }

        public void GetProfileData(string nickname)
        {
            try
            {
                var player = _context.Player.FirstOrDefault(p => p.nickname == nickname);
                if (player == null)
                {
                    _callback.ProfileUpdateResponse(false, "Jugador no encontrado.");
                    return;
                }

                var profile = new ProfileData
                {
                    Nickname = player.nickname,
                    FullName = player.fullName
                };

                _callback.ProfileDataReceived(profile);
            }
            catch (Exception ex)
            {
                _callback.ProfileUpdateResponse(false, $"Error interno: {ex.Message}");
            }
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
                _context.SaveChanges();

                _callback.ProfileUpdateResponse(true, "Perfil actualizado correctamente.");
            }
            catch (Exception ex)
            {
                _callback.ProfileUpdateResponse(false, $"Error al actualizar: {ex.Message}");
            }
        }
    }
}
