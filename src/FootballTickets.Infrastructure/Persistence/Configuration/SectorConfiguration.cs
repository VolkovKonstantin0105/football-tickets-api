using FootballTickets.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace FootballTickets.Infrastructure.Persistence.Configuration
{
    public class SectorConfiguration : IEntityTypeConfiguration<Sector>
    {
        public void Configure(EntityTypeBuilder<Sector> b)
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(50).IsRequired();
            b.Property(x => x.PriceMultiplier).HasPrecision(5, 2);
            b.HasOne(x => x.Stadium).WithMany(s => s.Sectors)
                .HasForeignKey(x => x.StadiumId).OnDelete(DeleteBehavior.Cascade);
        }
    }

}
