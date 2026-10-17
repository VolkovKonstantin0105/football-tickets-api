using System;
using System.Collections.Generic;
using System.Text;

namespace FootballTickets.Domain.Entities
{
    public class Stadium
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string City { get; set; } = null!;
        public List<Sector> Sectors { get; set; } = new();
    }

}
