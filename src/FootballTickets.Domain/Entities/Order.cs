using FootballTickets.Domain.Enums;
namespace FootballTickets.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Paid;
        public decimal TotalAmount { get; set; }
        public List<Ticket> Tickets { get; set; } = new();
    }

}
