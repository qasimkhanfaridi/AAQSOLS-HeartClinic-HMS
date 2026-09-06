using HeartClinicHms.Domain.Entities;
using HeartClinicHms.Domain.Enums;
using HeartClinicHms.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HeartClinicHms.Api.Services;

public static class PanelBillingHelper
{
    public static async Task<decimal> ResolveChargeAsync(
        AppDbContext db,
        Guid catalogServiceId,
        decimal defaultCharge,
        Guid? corporatePanelId)
    {
        if (!corporatePanelId.HasValue)
            return defaultCharge;

        var rate = await db.PanelServiceRates
            .FirstOrDefaultAsync(r => r.CorporatePanelId == corporatePanelId && r.CatalogServiceId == catalogServiceId);

        return rate?.PanelRate ?? defaultCharge;
    }

    public static (decimal PatientPayable, decimal PanelPayable, PanelBillingMode? Mode) SplitTotal(
        decimal total,
        bool isPanelPatient,
        PanelBillingMode? requestedMode)
    {
        if (!isPanelPatient || total <= 0)
            return (total, 0, null);

        var mode = requestedMode ?? PanelBillingMode.FullPanel;
        return mode switch
        {
            PanelBillingMode.SplitHalf => (Math.Round(total / 2, 2), Math.Round(total / 2, 2), mode),
            PanelBillingMode.PanelClaim => (0, total, mode),
            _ => (0, total, PanelBillingMode.FullPanel)
        };
    }
}
