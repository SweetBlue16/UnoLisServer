using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.ServiceModel;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data;
using UnoLisServer.Data.Repositories;
using UnoLisServer.Data.RepositoryInterfaces;
using UnoLisServer.Services.Helpers;
using UnoLisServer.Services.Validators;

namespace UnoLisServer.Services
{
    /// <summary>
    /// Manages logic for reporting a player
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ReportManager : IReportManager
    {
        private static readonly Dictionary<string, IReportCallback> _suscribers =
            new Dictionary<string, IReportCallback>();
        private static readonly object _lock = new object();

        private readonly ReportValidator _reportValidator;
        private readonly SanctionEnforcer _sanctionEnforcer;
        private readonly IPlayerRepository _playerRepository;
        private readonly IReportRepository _reportRepository;

        public ReportManager()
        {
            _reportValidator = new ReportValidator();
            _sanctionEnforcer = new SanctionEnforcer();
            _playerRepository = new PlayerRepository();
            _reportRepository = new ReportRepository();
        }

        public void ReportPlayer(ReportData reportData)
        {
            var callback = OperationContext.Current?.GetCallbackChannel<IReportCallback>();
            if (callback == null)
            {
                return;
            }

            try
            {
                var reporter = _playerRepository.GetPlayerProfileByNicknameAsync(reportData.ReporterNickname).Result;
                var reported = _playerRepository.GetPlayerProfileByNicknameAsync(reportData.ReportedNickname).Result;

                if (!_reportValidator.ValidateRequest(reporter, reported, callback))
                {
                    return;
                }

                if (!_reportValidator.CheckReportFrequency(reported.idPlayer, reporter.idPlayer, callback))
                {
                    return;
                }

                SaveReport(reporter.idPlayer, reported.idPlayer, reportData.Description);
                NotifySuccess(callback, reported.nickname);
                _sanctionEnforcer.TryApplySanction(reported, NotifyBannedPlayer);
            }
            catch (SqlException sqlEx)
            {
                NotifyError(callback,
                    MessageCode.DatabaseError,
                    $"[ERROR] Error de base de datos al procesar el reporte: {sqlEx.Message}"
                );
            }
            catch (CommunicationException commEx)
            {
                NotifyError(callback,
                    MessageCode.ConnectionFailed,
                    $"[ERROR] Error de comunicación al procesar el reporte: {commEx.Message}"
                );
            }
            catch (TimeoutException timeoutEx)
            {
                NotifyError(callback,
                    MessageCode.Timeout,
                    $"[ERROR] Tiempo de espera agotado al procesar el reporte: {timeoutEx.Message}"
                );
            }
            catch (Exception ex)
            {
                NotifyError(callback,
                    MessageCode.ReportInternalError,
                    $"[FATAL] Error al procesar el reporte: {ex.Message}"
                );
            }
        }

        public void SuscrbeToBanNotifications(string nickname)
        {
            var callback = OperationContext.Current?.GetCallbackChannel<IReportCallback>();
            lock (_lock)
            {
                if (_suscribers.ContainsKey(nickname))
                {
                    _suscribers[nickname] = callback;
                }
                else
                {
                    _suscribers.Add(nickname, callback);
                }
            }
            Logger.Log($"[INFO] {nickname} se ha suscrito a las notificaciones de baneos.");
        }

        public void UnsubscribeFromBanNotifications(string nickname)
        {
            if (string.IsNullOrEmpty(nickname))
            {
                return;
            }

            lock (_lock)
            {
                if (_suscribers.ContainsKey(nickname))
                {
                    _suscribers.Remove(nickname);
                    Logger.Log($"[INFO] {nickname} se ha desuscrito de las notificaciones de baneos.");
                }
            }
        }

        private void SaveReport(int reporterId, int reportedId, string description)
        {
            var report = new Report
            {
                ReporterPlayer_idPlayer = reporterId,
                ReportedPlayer_idPlayer = reportedId,
                reportDescription = description,
                reportDate = DateTime.UtcNow
            };
            _reportRepository.SaveReport(report);
        }

        private static void NotifySuccess(IReportCallback callback, string reportedNickname)
        {
            var response = new ResponseInfo<object>(
                MessageCode.ReportSubmitted,
                true,
                $"[INFO] Reporte enviado contra {reportedNickname}."
            );
            ResponseHelper.SendResponse(callback.ReportPlayerResponse, response);
        }

        private static void NotifyError(IReportCallback callback, MessageCode code, string logMessage)
        {
            var response = new ResponseInfo<object>(
                code,
                false,
                logMessage
            );
            ResponseHelper.SendResponse(callback.ReportPlayerResponse, response);
        }

        private static void NotifyBannedPlayer(string nickname, BanInfo banInfo)
        {
            lock (_lock)
            {
                if (_suscribers.ContainsKey(nickname))
                {
                    try
                    {
                        var callback = _suscribers[nickname];
                        var response = new ResponseInfo<BanInfo>(
                            MessageCode.PlayerBanned,
                            true,
                            $"[INFO] El jugador {nickname} ha sido baneado.",
                            banInfo
                        );
                        ResponseHelper.SendResponse(callback.OnPlayerBanned, response);
                    }
                    catch (CommunicationException)
                    {
                        Logger.Log($"[WARN] No se pudo notificar a {nickname} sobre su baneo. Eliminando de la lista de suscriptores.");
                        _suscribers.Remove(nickname);
                    }
                    catch (TimeoutException)
                    {
                        Logger.Log($"[WARN] Tiempo de espera agotado al notificar baneo. Eliminando de la lista de suscriptores.");
                        _suscribers.Remove(nickname);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"[ERROR] Error al notificar baneo: {ex.Message}. Eliminando de la lista de suscriptores.");
                        _suscribers.Remove(nickname);
                    }
                }
            }
        }
    }
}
