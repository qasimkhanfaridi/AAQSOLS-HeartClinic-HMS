using HeartClinicHms.Domain.Entities;
using HeartClinicHms.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeartClinicHms.Infrastructure.Data.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");
        builder.HasIndex(p => p.MrNumber).IsUnique();
        builder.HasIndex(p => p.CnicOrPassport).IsUnique();
        builder.HasIndex(p => new { p.LegacySourceSystem, p.LegacyExternalId });
        builder.Property(p => p.FirstName).HasMaxLength(120).IsRequired();
        builder.Property(p => p.GuardianName).HasMaxLength(120).IsRequired();
        builder.Property(p => p.CnicOrPassport).HasMaxLength(20).IsRequired();
        builder.HasOne(p => p.PatientType).WithMany(t => t.Patients).HasForeignKey(p => p.PatientTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.CorporatePanel).WithMany(cp => cp.Patients).HasForeignKey(p => p.CorporatePanelId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasIndex(u => u.UserName).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.UserName).HasMaxLength(64).IsRequired();
        builder.Property(u => u.DisplayName).HasMaxLength(120).IsRequired();
    }
}

public class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
{
    public void Configure(EntityTypeBuilder<Medicine> builder)
    {
        builder.ToTable("Medicines");
        builder.HasIndex(m => new { m.LegacySourceSystem, m.LegacyExternalId });
        builder.Property(m => m.Name).HasMaxLength(256).IsRequired();
    }
}

public class CheckInConfiguration : IEntityTypeConfiguration<CheckIn>
{
    public void Configure(EntityTypeBuilder<CheckIn> builder)
    {
        builder.ToTable("CheckIns");
        builder.Property(c => c.SubTotal).HasPrecision(18, 2);
        builder.HasOne(c => c.Challan).WithOne(c => c.CheckIn).HasForeignKey<Challan>(c => c.CheckInId);
        builder.HasOne(c => c.Consultation).WithOne(c => c.CheckIn).HasForeignKey<Consultation>(c => c.CheckInId);
        builder.HasOne(c => c.Patient).WithMany(p => p.CheckIns).HasForeignKey(c => c.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.CheckedInByUser).WithMany().HasForeignKey(c => c.CheckedInByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.Branch).WithMany().HasForeignKey(c => c.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.Doctor).WithMany(d => d.CheckIns).HasForeignKey(c => c.DoctorId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class CatalogServiceConfiguration : IEntityTypeConfiguration<CatalogService>
{
    public void Configure(EntityTypeBuilder<CatalogService> builder)
    {
        builder.ToTable("CatalogServices");
        builder.Property(s => s.DefaultCharges).HasPrecision(18, 2);
    }
}

public class CheckInServiceLineConfiguration : IEntityTypeConfiguration<CheckInServiceLine>
{
    public void Configure(EntityTypeBuilder<CheckInServiceLine> builder)
    {
        builder.Property(l => l.Charges).HasPrecision(18, 2);
    }
}

public class ChallanConfiguration : IEntityTypeConfiguration<Challan>
{
    public void Configure(EntityTypeBuilder<Challan> builder)
    {
        builder.Property(c => c.SubTotal).HasPrecision(18, 2);
        builder.Property(c => c.PatientPayable).HasPrecision(18, 2);
        builder.Property(c => c.PanelPayable).HasPrecision(18, 2);
        builder.HasIndex(c => c.ChallanNumber).IsUnique();
        builder.HasOne(c => c.CorporatePanel).WithMany().HasForeignKey(c => c.CorporatePanelId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class CorporatePanelConfiguration : IEntityTypeConfiguration<CorporatePanel>
{
    public void Configure(EntityTypeBuilder<CorporatePanel> builder)
    {
        builder.ToTable("CorporatePanels");
        builder.HasIndex(p => p.Code).IsUnique();
        builder.Property(p => p.Code).HasMaxLength(32).IsRequired();
        builder.Property(p => p.Name).HasMaxLength(120).IsRequired();
        builder.HasOne(p => p.Branch).WithMany().HasForeignKey(p => p.BranchId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PanelServiceRateConfiguration : IEntityTypeConfiguration<PanelServiceRate>
{
    public void Configure(EntityTypeBuilder<PanelServiceRate> builder)
    {
        builder.ToTable("PanelServiceRates");
        builder.Property(r => r.PanelRate).HasPrecision(18, 2);
        builder.HasIndex(r => new { r.CorporatePanelId, r.CatalogServiceId }).IsUnique();
        builder.HasOne(r => r.CorporatePanel).WithMany(p => p.ServiceRates).HasForeignKey(r => r.CorporatePanelId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.CatalogService).WithMany().HasForeignKey(r => r.CatalogServiceId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class LabWorkItemConfiguration : IEntityTypeConfiguration<LabWorkItem>
{
    public void Configure(EntityTypeBuilder<LabWorkItem> builder)
    {
        builder.ToTable("LabWorkItems");
        builder.HasIndex(l => l.CheckInServiceLineId).IsUnique();
        builder.HasOne(l => l.CheckInServiceLine).WithOne(s => s.LabWorkItem).HasForeignKey<LabWorkItem>(l => l.CheckInServiceLineId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(l => l.PaymentVerifiedByUser).WithMany().HasForeignKey(l => l.PaymentVerifiedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(l => l.SampleCollectedByUser).WithMany().HasForeignKey(l => l.SampleCollectedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
