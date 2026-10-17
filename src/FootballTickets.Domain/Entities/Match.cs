using FootballTickets.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FootballTickets.Domain.Entities
{
    public class Match
    {
        public Guid Id { get; set; }
        public Guid HomeTeamId { get; set; }
        public Team HomeTeam { get; set; } = null!;
        public Guid AwayTeamId { get; set; }
        public Team AwayTeam { get; set; } = null!;
        public Guid StadiumId { get; set; }
        public Stadium Stadium { get; set; } = null!;
        public DateTime StartsAt { get; set; }
        public decimal BasePrice { get; set; }
        public MatchStatus Status { get; set; } = MatchStatus.Scheduled;
    }

}
