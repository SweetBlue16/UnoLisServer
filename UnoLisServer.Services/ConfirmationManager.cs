using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data;

namespace UnoLisServer.Services
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ConfirmationManager : IConfirmationManager
    {
        private readonly UNOContext _context;
        private readonly IConfirmationCallback _callback;

        public ConfirmationManager()
        {
            _context = new UNOContext();
            _callback = OperationContext.Current.GetCallbackChannel<IConfirmationCallback>();
        }

        public void ConfirmCode(string email, string code)
        {
            // De momento, simulamos validación
            Logger.Log($"Confirmando código para {email}: {code}");
            _callback.ConfirmationResponse(true);
        }
    }
}
