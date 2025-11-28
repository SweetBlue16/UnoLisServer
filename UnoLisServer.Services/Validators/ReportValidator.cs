using System;
using System.Linq;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data;

namespace UnoLisServer.Services.Validators
{
    public class ReportValidator
    {
        public bool ValidateRequest(Player reporter, Player reported, IReportCallback callback)
        {
            if (reported == null || reporter == null)
            {
                SendError(callback, MessageCode.PlayerNotFound, $"[WARNING] Jugador {reported.nickname} o {reporter.nickname} no encontrado.");
                return false;
            }

            if (reported.idPlayer == reporter.idPlayer)
            {
                SendError(callback, MessageCode.ReportInternalError, "[WARNING] Un jugador no puede reportarse a sí mismo.");
                return false;
            }
            return true;
        }

        public bool CheckReportFrequency(UNOContext context, int reportedId, int reporterId, IReportCallback callback)
        {
            var cutoff = DateTime.UtcNow.AddHours(-24);

            bool exists = context.Report.Any(r =>
                r.ReporterPlayer_idPlayer == reporterId &&
                r.ReportedPlayer_idPlayer == reportedId &&
                r.reportDate >= cutoff
            );

            if (exists)
            {
                SendError(callback, MessageCode.AlreadyReportedRecently, "[WARNING] Ya has reportado a este jugador en las últimas 24 horas.");
                return false;
            }
            return true;
        }

        private void SendError(IReportCallback callback, MessageCode code, string message)
        {
            var response = new ResponseInfo<object>(
                code,
                false,
                message
            );
            ResponseHelper.SendResponse(callback.ReportPlayerResponse, response);
        }
    }
}
