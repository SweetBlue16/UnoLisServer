using System;
using System.Linq;
using System.ServiceModel;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Models;
using UnoLisServer.Data;
using UnoLisServer.Data.Repositories;
using UnoLisServer.Data.RepositoryInterfaces;

namespace UnoLisServer.Services.Helpers
{
    public class SanctionEnforcer
    {
        private const int ReportsForMinorBan = 3;
        private const int ReportsForMajorBan = 5;
        private const int MinorBanHours = 1;
        private const int MajorBanHours = 24;

        private readonly ISanctionRepository _sanctionRepository = new SanctionRepository();

        public void TryApplySanction(UNOContext context, Player reported)
        {
            int reportCount = CountReports(context, reported.idPlayer);
            int banDuration = CalculateBanDuration(reportCount);

            if (banDuration > 0)
            {
                ApplyBan(reported, banDuration);
                KickUser(reported.nickname, "Sanction applied due to accumulation of reports.", banDuration);
            }
        }

        private int CountReports(UNOContext context, int playerId)
        {
            return context.Report.Count(r => r.ReportedPlayer_idPlayer == playerId);
        }

        private int CalculateBanDuration(int count)
        {
            if (count <= 0)
            {
                return 0;
            }

            if (count % ReportsForMajorBan == 0)
            {
                return MajorBanHours;
            }

            if (count % ReportsForMinorBan == 0)
            {
                return MinorBanHours;
            }
            return 0;
        }

        private void ApplyBan(Player player, int hours)
        {
            var sanction = new Sanction
            {
                Player_idPlayer = player.idPlayer,
                sanctionType = $"{hours}-hrs ban",
                sanctionDescription = $"Sanction applied due to accumulation of reports. Duration: {hours} hours.",
                sanctionStartDate = DateTime.UtcNow,
                sanctionEndDate = DateTime.UtcNow.AddHours(hours)
            };
            _sanctionRepository.AddSanction(sanction);
        }

        private void KickUser(string nickname, string reason, int hours)
        {
            ResponseInfo<object> responseInfo;
            var callback = SessionManager.GetSession(nickname);

            if (SessionManager.IsOnline(nickname))
            {
                try
                {
                    var endDate = DateTime.Now.AddHours(hours);

                    callback?.PlayerBanned(reason, endDate);
                }
                catch (CommunicationException communicationEx)
                {
                    responseInfo = new ResponseInfo<object>(
                        MessageCode.ConnectionFailed,
                        false,
                        $"[ERROR] Comunicación al aplicar sanción a {nickname}. Error: {communicationEx.Message}"
                    );
                }
                catch (TimeoutException timeoutEx)
                {
                    responseInfo = new ResponseInfo<object>(
                        MessageCode.Timeout,
                        false,
                        $"[ERROR] Tiempo de espera agotado al aplicar sanción a {nickname}. Error: {timeoutEx.Message}"
                    );
                }
                catch (Exception ex)
                {
                    responseInfo = new ResponseInfo<object>(
                        MessageCode.LoginInternalError,
                        false,
                        $"[ERROR] Excepción no controlada al aplicar sanción a '{nickname}': {ex.Message}"
                    );
                }
                finally
                {
                    SessionManager.RemoveSession(nickname);
                }
            }
        }
    }
}
