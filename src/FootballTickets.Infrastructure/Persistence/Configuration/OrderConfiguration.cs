using FootballTickets.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace FootballTickets.Infrastructure.Persistence.Configuration
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> b)
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.TotalAmount).HasPrecision(10, 2);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            b.HasOne(x => x.User).WithMany(u => u.Orders)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        }
    }

}
