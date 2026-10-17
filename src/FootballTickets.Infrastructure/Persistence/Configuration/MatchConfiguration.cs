using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FootballTickets.Domain.Entities;

namespace FootballTickets.Infrastructure.Persistence.Configuration
{
    public class MatchConfiguration : IEntityTypeConfiguration<Match>
    {
        public void Configure(EntityTypeBuilder<Match> b)
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.BasePrice).HasPrecision(10, 2);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            b.HasIndex(x => x.StartsAt);

            b.HasOne(x => x.HomeTeam).WithMany()
                .HasForeignKey(x => x.HomeTeamId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.AwayTeam).WithMany()
                .HasForeignKey(x => x.AwayTeamId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Stadium).WithMany()
                .HasForeignKey(x => x.StadiumId).OnDelete(DeleteBehavior.Restrict);

            b.ToTable(t => t.HasCheckConstraint(
                "CK_Matches_DifferentTeams", "\"HomeTeamId\" <> \"AwayTeamId\""));
        }
    }

}
