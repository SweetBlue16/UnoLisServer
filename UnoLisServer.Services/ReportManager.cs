using System;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
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
    public class ReportManager : IReportManager
    {
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
                    var playersInfo = LoadPlayers(context, reportData);

                    if (!playersInfo.IsValid)
                    {
                        SendReportResponse(CreateErrorResponse(MessageCode.PlayerNotFound, playersInfo.ErrorMessage), callback);
                        return;
                    }

                    if (IsRecentReport(context, playersInfo.Reporter, playersInfo.Reported))
                    {
                        SendReportResponse(CreateErrorResponse(
                            MessageCode.AlreadyReportedRecently,
                            $"Ya reportaste a {playersInfo.Reported.nickname} en las últimas 24 horas."
                        ), callback);
                        return;
                    }

                    SaveReport(context, playersInfo.Reporter, playersInfo.Reported, reportData.Description);

                    SendReportResponse(CreateSuccessResponse(
                        MessageCode.ReportSubmitted,
                        $"Reporte enviado contra {playersInfo.Reported.nickname}."
                    ), callback);

                    CheckAndApplySanction(playersInfo.Reported);
                }
            }
            catch (SqlException sqlEx)
            {
                SendReportResponse(CreateErrorResponse(
                    MessageCode.DatabaseError,
                    $"[ERROR] Error de base de datos al procesar el reporte: {sqlEx.Message}"
                ), callback);
            }
            catch (CommunicationException commEx)
            {
                SendReportResponse(CreateErrorResponse(
                    MessageCode.ConnectionFailed,
                    $"[ERROR] Error de comunicación al procesar el reporte: {commEx.Message}"
                ), callback);
            }
            catch (TimeoutException timeoutEx)
            {
                SendReportResponse(CreateErrorResponse(
                    MessageCode.Timeout,
                    $"[ERROR] Tiempo de espera agotado al procesar el reporte: {timeoutEx.Message}"
                ), callback);
            }
            catch (Exception ex)
            {
                SendReportResponse(CreateErrorResponse(
                    MessageCode.ReportInternalError,
                    $"[FATAL] Error al procesar el reporte: {ex.Message}"
                ), callback);
            }
        }

        private ReportPlayersInfo LoadPlayers(UNOContext context, ReportData data)
        {
            var info = new ReportPlayersInfo
            {
                Reporter = context.Player.FirstOrDefault(p => p.nickname == data.ReporterNickname),
                Reported = context.Player.FirstOrDefault(p => p.nickname == data.ReportedNickname)
            };

            if (info.Reporter == null || info.Reported == null)
            {
                info.ErrorMessage = "Uno o ambos jugadores no existen.";
            }
            else if (info.Reporter.idPlayer == info.Reported.idPlayer)
            {
                info.Reporter = null;
                info.ErrorMessage = "No puedes reportarte a ti mismo.";
            }

            return info;
        }

        private bool IsRecentReport(UNOContext context, Player reporter, Player reported)
        {
            var cutoff = DateTime.UtcNow.AddDays(-1);

            return context.Report.Any(r =>
                r.ReporterPlayer_idPlayer == reporter.idPlayer &&
                r.ReportedPlayer_idPlayer == reported.idPlayer &&
                r.reportDate >= cutoff
            );
        }

        private void SendReportResponse(ResponseInfo<object> info, IReportCallback callback)
        {
            ResponseHelper.SendResponse(callback.ReportPlayerResponse, info);
        }

        private ResponseInfo<object> CreateSuccessResponse(MessageCode code, string msg)
        {
            return new ResponseInfo<object>(code, true, msg);
        }

        private ResponseInfo<object> CreateErrorResponse(MessageCode code, string msg)
        {
            return new ResponseInfo<object>(code, false, msg);
        }

        private void SaveReport(UNOContext context, Player reporter, Player reported, string desc)
        {
            var report = new Report
            {
                ReporterPlayer_idPlayer = reporter.idPlayer,
                ReportedPlayer_idPlayer = reported.idPlayer,
                reportDescription = desc,
                reportDate = DateTime.UtcNow
            };

            context.Report.Add(report);
            context.SaveChanges();
        }

        private int CountReports(UNOContext context, Player reported)
        {
            return context.Report.Count(r => r.ReportedPlayer_idPlayer == reported.idPlayer);
        }

        private void CheckAndApplySanction(Player reported)
        {
            using (var context = new UNOContext())
            {
                int count = CountReports(context, reported);

                int hours = ComputeSanctionHours(count);
                if (hours == 0)
                {
                    return;
                }

                ApplySanction(context, reported, hours);
                NotifyAndKick(reported.nickname, hours);
            }
        }

        private int ComputeSanctionHours(int totalReports)
        {
            if (totalReports % 5 == 0)
            {
                return 24;
            }
            if (totalReports % 3 == 0)
            {
                return 1;
            }
            return 0;
        }

        private void ApplySanction(UNOContext context, Player p, int hours)
        {
            var sanction = new Sanction
            {
                Player_idPlayer = p.idPlayer,
                sanctionType = $"{hours}-hour ban",
                sanctionDescription = $"Automatic sanction applied due to reports. Duration: {hours} hours.",
                sanctionStartDate = DateTime.UtcNow,
                sanctionEndDate = DateTime.UtcNow.AddHours(hours)
            };

            context.Sanction.Add(sanction);
            context.SaveChanges();
        }

        private void NotifyAndKick(string nickname, int hours)
        {
            if (!SessionManager.IsOnline(nickname))
            {
                return;
            }

            var callback = SessionManager.GetSession(nickname) as IReportCallback;
            if (callback == null)
            {
                return;
            }

            var response = new ResponseInfo<object>(
                MessageCode.PlayerKicked,
                false,
                $"[INFO] {nickname} ha sido baneado por {hours} horas."
            );

            ResponseHelper.SendResponse(callback.OnPlayerKicked, response);
            SessionManager.RemoveSession(nickname);
        }
    }

    internal class ReportPlayersInfo
    {
        public Player Reporter { get; set; }
        public Player Reported { get; set; }
        public bool IsValid => Reporter != null && Reported != null;
        public string ErrorMessage { get; set; }
    }
}
