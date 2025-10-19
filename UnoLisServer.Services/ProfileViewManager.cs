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

                var profile = new ProfileData
                {
                    Nickname = player.nickname,
                    FullName = player.fullName
                };

                _callback.ProfileDataReceived(true, profile);
            }
            catch (Exception)
            {
                _callback.ProfileDataReceived(false, null);
            }
        }
    }
}
