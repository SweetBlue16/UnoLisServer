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
    public class ProfileEditManager : IProfileEditManager
    {
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
