﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models.ViewModels
{
    public class ShoppingCartVm
    {
        public List<ShoppingCart> Items { get; set; }

        public OrderHeader OrderHeader { get; set; }
    }
}
