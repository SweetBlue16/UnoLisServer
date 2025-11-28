using System;
using System.Data.SqlClient;
using System.Linq;
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
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ReportManager : IReportManager
    {
        private readonly ReportValidator _reportValidator;
        private readonly SanctionEnforcer _sanctionEnforcer;
        private readonly IPlayerRepository _playerRepository;

        public ReportManager()
        {
            _reportValidator = new ReportValidator();
            _sanctionEnforcer = new SanctionEnforcer();
            _playerRepository = new PlayerRepository();
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
                using (var context = new UNOContext())
                {
                    var reporter = _playerRepository.GetPlayerProfileByNicknameAsync(reportData.ReporterNickname).Result;
                    var reported = _playerRepository.GetPlayerProfileByNicknameAsync(reportData.ReportedNickname).Result;

                    if (!_reportValidator.ValidateRequest(reporter, reported, callback))
                    {
                        return;
                    }

                    if (!_reportValidator.CheckReportFrequency(context, reported.idPlayer, reporter.idPlayer, callback))
                    {
                        return;
                    }

                    SaveReport(context, reporter.idPlayer, reported.idPlayer, reportData.Description);
                    NotifySuccess(callback, reported.nickname);
                    _sanctionEnforcer.TryApplySanction(context, reported);
                }
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

        private void SaveReport(UNOContext context, int reporterId, int reportedId, string description)
        {
            var report = new Report
            {
                ReporterPlayer_idPlayer = reportedId,
                ReportedPlayer_idPlayer = reportedId,
                reportDescription = description,
                reportDate = DateTime.UtcNow
            };

            context.Report.Add(report);
            context.SaveChanges();
        }

        private void NotifySuccess(IReportCallback callback, string reportedNickname)
        {
            var response = new ResponseInfo<object>(
                MessageCode.ReportSubmitted,
                true,
                $"[INFO] Reporte enviado contra {reportedNickname}."
            );
            ResponseHelper.SendResponse(callback.ReportPlayerResponse, response);
        }

        private void NotifyError(IReportCallback callback, MessageCode code, string logMessage)
        {
            var response = new ResponseInfo<object>(
                code,
                false,
                logMessage
            );
            ResponseHelper.SendResponse(callback.ReportPlayerResponse, response);
        }
    }
}
