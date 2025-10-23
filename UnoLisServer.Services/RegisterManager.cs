using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Globalization;
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
    public class RegisterManager : IRegisterManager
    {
        private readonly UNOContext _context;
        private readonly IRegisterCallback _callback;
        private ServiceResponse<object> _response;

        public RegisterManager()
        {
            _context = new UNOContext();
            _callback = OperationContext.Current.GetCallbackChannel<IRegisterCallback>();
        }

        public void Register(RegistrationData data)
        {
            if (data == null)
            {
                _response = new ServiceResponse<object>(false, MessageCode.InvalidData);
                _callback.RegisterResponse(_response);
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
                        _response = new ServiceResponse<object>(false, MessageCode.NicknameAlreadyTaken);
                        _callback.RegisterResponse(_response);
                        Logger.Log($"El nickname '{data.Nickname}' ya está registrado.");
                        return;
                    }

                    bool existsAccount = _context.Account.Any(a => a.email == data.Email);
                    if (existsAccount)
                    {
                        _response = new ServiceResponse<object>(false, MessageCode.EmailAlreadyRegistered);
                        _callback.RegisterResponse(_response);
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

                    _response = new ServiceResponse<object>(true, MessageCode.RegistrationSuccessful);
                    _callback.RegisterResponse(_response);
                    Logger.Log($"Registro exitoso para {data.Nickname}.");
                }
                catch (CommunicationException communicationEx)
                {
                    transaction.Rollback();
                    Logger.Log($"Error de comunicación en Register({data.Email}): {communicationEx.Message}");
                }
                catch (TimeoutException timeoutEx)
                {
                    transaction.Rollback();
                    Logger.Log($"Timeout en Register({data.Email}): {timeoutEx.Message}");
                }
                catch (DbUpdateException dbUpdateEx)
                {
                    _response = new ServiceResponse<object>(false, MessageCode.DatabaseError);
                    transaction.Rollback();
                    Logger.Log($"Error de base de datos en Register({data.Email}): {dbUpdateEx.Message}");
                    _callback.RegisterResponse(_response);
                }
                catch (SqlException dbEx)
                {
                    transaction.Rollback();
                    _response = new ServiceResponse<object>(false, MessageCode.SqlError);
                    Logger.Log($"Error SQL en Register({data.Email}): {dbEx.Message}");
                    _callback.RegisterResponse(_response);
                }
                catch (Exception ex)
                {
                    _response = new ServiceResponse<object>(false, MessageCode.GeneralServerError);
                    transaction.Rollback();
                    Logger.Log($"Error en Register({data.Email}): {ex.Message}");
                    _callback.RegisterResponse(_response);
                }
            }
        }
    }
}
