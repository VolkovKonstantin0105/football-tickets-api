using System;
using System.Collections.Generic;
using System.Text;

namespace FootballTickets.Domain.Entities
{
    public class Sector
    {
        public Guid Id { get; set; }
        public Guid StadiumId { get; set; }
        public Stadium Stadium { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal PriceMultiplier { get; set; } = 1.0m;
        public List<Seat> Seats { get; set; } = new();
    }

}
