using System;
using System.Collections.Generic;
using System.Text;

namespace FootballTickets.Domain.Entities
{
    public class Seat
    {
        public Guid Id { get; set; }
        public Guid SectorId { get; set; }
        public Sector Sector { get; set; } = null!;
        public int Row { get; set; }
        public int Number { get; set; }
    }

}
