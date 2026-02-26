using GestaoDespesas.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GestaoDespesas.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var dateTimeProperties = entityType
                    .GetProperties()
                    .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?));

                foreach (var property in dateTimeProperties)
                {
                    property.SetValueConverter(new ValueConverter<DateTime, DateTime>(
                        v => v.ToUniversalTime(),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                    ));
                }
            }

            builder.Entity<Despesa>()
                .HasOne(d => d.Categoria)
                .WithMany()
                .HasForeignKey(d => d.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Categoria>()
                .HasIndex(c => new { c.UserId, c.Nome })
                .IsUnique();

            builder.Entity<UserProfile>()
                .HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<UserProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserProfile>()
                .Property(p => p.SalarioMensal)
                .HasPrecision(18, 2);
        }

        public override int SaveChanges()
        {
            ConvertDatesToUtc();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ConvertDatesToUtc();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void ConvertDatesToUtc()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                foreach (var property in entry.Properties)
                {
                    if (property.Metadata.ClrType == typeof(DateTime))
                    {
                        var value = property.CurrentValue;

                        if (value != null)
                        {
                            var date = (DateTime)value;

                            if (date.Kind == DateTimeKind.Unspecified)
                            {
                                property.CurrentValue = DateTime.SpecifyKind(date, DateTimeKind.Utc);
                            }
                            else if (date.Kind == DateTimeKind.Local)
                            {
                                property.CurrentValue = date.ToUniversalTime();
                            }
                        }
                    }

                    if (property.Metadata.ClrType == typeof(DateTime?))
                    {
                        var value = property.CurrentValue;

                        if (value != null)
                        {
                            var date = (DateTime?)value;

                            if (date.Value.Kind == DateTimeKind.Unspecified)
                            {
                                property.CurrentValue = DateTime.SpecifyKind(date.Value, DateTimeKind.Utc);
                            }
                            else if (date.Value.Kind == DateTimeKind.Local)
                            {
                                property.CurrentValue = date.Value.ToUniversalTime();
                            }
                        }
                    }
                }
            }
        }

        public DbSet<Categoria> Categorias => Set<Categoria>();
        public DbSet<Despesa> Despesas => Set<Despesa>();
        public DbSet<Orcamento> Orcamentos => Set<Orcamento>();
        public DbSet<UserProfile> UserProfiles { get; set; }
    }
}