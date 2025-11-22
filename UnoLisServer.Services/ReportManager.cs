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

namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ReportManager : IReportManager
    {
        private readonly UNOContext _context;
        private ResponseInfo<object> _responseInfo;
        private readonly IReportCallback _callback;

        public ReportManager()
        {
            _callback = OperationContext.Current.GetCallbackChannel<IReportCallback>();
            _context = new UNOContext();
        }

        public void ReportPlayer(ReportData reportData)
        {
            try
            {
                var reporterPlayer = _context.Player.FirstOrDefault(p => p.nickname == reportData.ReporterNickname);
                var reportedPlayer = _context.Player.FirstOrDefault(p => p.nickname == reportData.ReportedNickname);

                if (reporterPlayer == null || reportedPlayer == null)
                {
                    _responseInfo = new ResponseInfo<object>(
                        MessageCode.PlayerNotFound,
                        false,
                        "[ERROR] Uno o ambos jugadores no fueron encontrados."
                    );
                    return;
                }
                var oneDayAgo = DateTime.UtcNow.AddDays(-1);
                bool alreadyReportedRecently = _context.Report.Any(r =>
                    r.ReporterPlayer_idPlayer == reporterPlayer.idPlayer &&
                    r.ReportedPlayer_idPlayer == reportedPlayer.idPlayer &&
                    r.reportDate >= oneDayAgo);

                if (alreadyReportedRecently)
                {
                    _responseInfo = new ResponseInfo<object>(
                        MessageCode.AlreadyReportedRecently,
                        false,
                        $"[WARNING] {reporterPlayer.nickname} ya ha reportado a {reportedPlayer.nickname} en las últimas 24 horas."
                    );
                    return;
                }

                var report = new Report
                {
                    ReporterPlayer_idPlayer = reporterPlayer.idPlayer,
                    ReportedPlayer_idPlayer = reportedPlayer.idPlayer,
                    reportDescription = reportData.Description,
                    reportDate = DateTime.UtcNow
                };
                _context.Report.Add(report);
                _context.SaveChanges();

                _responseInfo = new ResponseInfo<object>(
                    MessageCode.ReportSubmitted,
                    true,
                    $"[INFO] Reporte a {reportedPlayer.nickname} enviado con éxito."
                );
                CheckAndApplySanction(reportedPlayer);
            }
            catch (SqlException dbEx)
            {
                _responseInfo = new ResponseInfo<object>(
                    MessageCode.DatabaseError,
                    false,
                    $"[ERROR] Error de base de datos al reportar a '{reportData.ReportedNickname}': {dbEx.Message}"
                );
            }
            catch (CommunicationException communicationEx)
            {
                _responseInfo = new ResponseInfo<object>(
                    MessageCode.ConnectionFailed,
                    false,
                    $"[ERROR] Comunicación al reportar a '{reportData.ReportedNickname}'. Error: {communicationEx.Message}"
                );
            }
            catch (TimeoutException timeoutEx)
            {
                _responseInfo = new ResponseInfo<object>(
                    MessageCode.Timeout,
                    false,
                    $"[ERROR] Tiempo de espera agotado al reportar a '{reportData.ReportedNickname}'. Error: {timeoutEx.Message}"
                );
            }
            catch (Exception ex)
            {
                _responseInfo = new ResponseInfo<object>(
                    MessageCode.RegistrationInternalError,
                    false,
                    $"[ERROR] Error general al reportar a '{reportData.ReportedNickname}': {ex.Message}"
                );
            }
            ResponseHelper.SendResponse(_callback.ReportPlayerResponse, _responseInfo);
        }

        private void CheckAndApplySanction(Player reportedPlayer)
        {
            try
            {
                int totalReports = _context.Report.Count(r => r.ReportedPlayer_idPlayer == reportedPlayer.idPlayer);
                bool isMultipleOfThree = totalReports % 3 == 0;
                bool isMultipleOfFive = totalReports % 5 == 0;

                if (!isMultipleOfThree && !isMultipleOfFive)
                {
                    return;
                }

                int sanctionHours = 0;
                string sanctionType = "";

                if (isMultipleOfFive)
                {
                    sanctionHours = 24;
                    sanctionType = "24-hour ban";
                }
                else if (isMultipleOfThree)
                {
                    sanctionHours = 1;
                    sanctionType = "1-hour ban";
                }
                _responseInfo = ApplySanction(reportedPlayer, sanctionType, sanctionHours);
            }
            catch (SqlException dbEx)
            {
                _responseInfo = new ResponseInfo<object>(
                    MessageCode.DatabaseError,
                    false,
                    $"[ERROR] Error de base de datos al procesar el reporte: {dbEx.Message}"
                );
            }
            catch (CommunicationException communicationEx)
            {
                _responseInfo = new ResponseInfo<object>(
                    MessageCode.ConnectionFailed,
                    false,
                    $"[ERROR] Comunicación al procesar el reporte. Error: {communicationEx.Message}"
                );
            }
            catch (TimeoutException timeoutEx)
            {
                _responseInfo = new ResponseInfo<object>(
                    MessageCode.Timeout,
                    false,
                    $"[ERROR] Tiempo de espera agotado al procesar el reporte. Error: {timeoutEx.Message}"
                );
            }
            catch (Exception ex)
            {
                _responseInfo = new ResponseInfo<object>(
                    MessageCode.RegistrationInternalError,
                    false,
                    $"[ERROR] Error general al procesar el reporte: {ex.Message}"
                );
            }
            ResponseHelper.SendResponse(_callback.ReportPlayerResponse, _responseInfo);
        }

        private ResponseInfo<object> ApplySanction(Player player, string type, int hours)
        {
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddHours(hours);

            var sanction = new Sanction
            {
                Player_idPlayer = player.idPlayer,
                sanctionType = type,
                sanctionDescription = $"Automatic sanction applied due to reports. Duration: {hours} hours.",
                sanctionDate = startDate,
                sanctionStartDate = startDate,
            };

            _context.Sanction.Add(sanction);
            _context.SaveChanges();

            return new ResponseInfo<object>(
                MessageCode.SanctionApplied,
                true,
                $"[INFO] Sanción aplicada a {player.nickname}. Fin: {endDate}"
            );
        }
    }
}
