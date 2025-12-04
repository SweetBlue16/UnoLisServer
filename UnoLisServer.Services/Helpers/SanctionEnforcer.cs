using System;
using System.Linq;
using System.ServiceModel;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.DTOs;
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
        private readonly IReportRepository _reportRepository = new ReportRepository();

        public void TryApplySanction(Player reported, Action<string, BanInfo> onBanAction)
        {
            int reportCount = _reportRepository.CountReports(reported.idPlayer);
            int banDuration = CalculateBanDuration(reportCount);

            if (banDuration > 0)
            {
                ApplyBan(reported, banDuration);
                var banInfo = CreateBanInfo(banDuration);
                onBanAction?.Invoke(reported.nickname, banInfo);
            }
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

        private BanInfo CreateBanInfo(int hours)
        {
            var endDate = DateTime.UtcNow.AddHours(hours);
            return new BanInfo
            {
                Reason = "Sanction applied due to accumulation of reports",
                EndDate = endDate,
                RemainingHours = hours,
                FormattedTimeRemaining = $"{hours} h"
            };
        }
    }
}
