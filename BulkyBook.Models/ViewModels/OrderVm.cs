using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models.ViewModels
{
    public class OrderVm
    {
        public OrderHeader Header { get; set; }

        public List<OrderDetail> Details { get; set; }
    }
}
