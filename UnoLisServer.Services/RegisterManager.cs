using System;
using System.Collections.Generic;
using System.Globalization;
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
    public class RegisterManager : IRegisterManager
    {
        private readonly UNOContext _context;
        private readonly IRegisterCallback _callback;

        public RegisterManager()
        {
            _context = new UNOContext();
            _callback = OperationContext.Current.GetCallbackChannel<IRegisterCallback>();
        }

        public void Register(RegistrationData data)
        {
            if (data == null)
            {
                _callback.RegisterResponse(false, "Datos de registro inválidos.");
                return;
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    Logger.Log($"Intentando registrar el usuario '{data.Nickname}'...");

                    bool existsPlayer = _context.Player.Any(p => p.nickname == data.Nickname);
                    if (existsPlayer)
                    {
                        _callback.RegisterResponse(false, "El nickname ya está en uso.");
                        Logger.Log($"El nickname '{data.Nickname}' ya está registrado.");
                        return;
                    }

                    bool existsAccount = _context.Account.Any(a => a.email == data.Email);
                    if (existsAccount)
                    {
                        _callback.RegisterResponse(false, "El correo electrónico ya está en uso.");
                        Logger.Log($"El email '{data.Email}' ya está registrado.");
                        return;
                    }

                    var newPlayer = new Player
                    {
                        nickname = data.Nickname,
                        fullName = data.FullName
                    };
                    _context.Player.Add(newPlayer);

                    var newAccount = new Account
                    {
                        email = data.Email,
                        password = PasswordHelper.HashPassword(data.Password),
                        Player = newPlayer
                    };
                    _context.Account.Add(newAccount);

                    _context.SaveChanges();
                    transaction.Commit();

                    _callback.RegisterResponse(true, "Registro completado exitosamente.");
                    Logger.Log($"Registro exitoso para {data.Nickname}.");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Logger.Log($"Error en Register({data.Email}): {ex.Message}");
                    _callback.RegisterResponse(false, "Error interno del servidor.");
                }
            }
        }
    }
}
