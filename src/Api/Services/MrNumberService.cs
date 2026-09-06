using HeartClinicHms.Domain.Entities;
using HeartClinicHms.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HeartClinicHms.Api.Services;

public class MrNumberService(AppDbContext db)
{
    public async Task<string> GenerateNextAsync(Guid branchId, CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year % 100;
        var seq = await db.MrNumberSequences.FirstOrDefaultAsync(s => s.BranchId == branchId && s.Year == year, ct);
        if (seq is null)
        {
            seq = new MrNumberSequence { BranchId = branchId, Year = year, Prefix = 5601, LastSequence = 0 };
            db.MrNumberSequences.Add(seq);
        }

        seq.LastSequence++;
        await db.SaveChangesAsync(ct);
        return $"{seq.Prefix:D4}-{seq.Year:D2}-{seq.LastSequence:D6}";
    }
}
