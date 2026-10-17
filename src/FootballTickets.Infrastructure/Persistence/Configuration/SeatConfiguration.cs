using FootballTickets.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace FootballTickets.Infrastructure.Persistence.Configuration
{
    public class SeatConfiguration : IEntityTypeConfiguration<Seat>
    {
        public void Configure(EntityTypeBuilder<Seat> b)
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.SectorId, x.Row, x.Number }).IsUnique();
            b.HasOne(x => x.Sector).WithMany(s => s.Seats)
                .HasForeignKey(x => x.SectorId).OnDelete(DeleteBehavior.Cascade);
        }
    }

}
