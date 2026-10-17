using FootballTickets.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace FootballTickets.Infrastructure.Persistence.Configuration
{
    public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
    {
        public void Configure(EntityTypeBuilder<Ticket> b)
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Price).HasPrecision(10, 2);

            b.HasOne(x => x.Order).WithMany(o => o.Tickets)
                .HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Match).WithMany()
                .HasForeignKey(x => x.MatchId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Seat).WithMany()
                .HasForeignKey(x => x.SeatId).OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.MatchId, x.SeatId })
                .IsUnique()
                .HasFilter("\"IsCancelled\" = false");
        }
    }

}
