using System;
using System.Collections.Generic;
using System.Text;

namespace FootballTickets.Domain.Entities
{
    public class Ticket
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public Guid MatchId { get; set; }
        public Match Match { get; set; } = null!;
        public Guid SeatId { get; set; }
        public Seat Seat { get; set; } = null!;
        public decimal Price { get; set; }
        public bool IsCancelled { get; set; }
    }

}
