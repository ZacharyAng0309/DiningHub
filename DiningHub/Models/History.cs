using System;
using System.ComponentModel.DataAnnotations;

namespace DiningHub.Models
{
    public class History
    {
        public int HistoryId { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public DateTime Date { get; set; }
    }
}
