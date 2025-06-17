using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bezorgapp.Models
{
    public class DeliveryState
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int State { get; set; } // Bijvoorbeeld: 1 = In afwachting, 2 = Onderweg, 3 = Afgeleverd
        public DateTime DateTime { get; set; }

        public DeliveryService? DeliveryService { get; set; }
        public Order? Order { get; set; }
    }

}
