using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Utility
{
    public class StripeKeys
    {
        public const string Section = "Stripe";

        public string PublicKey { get; set; }

        public string SecretKey { get; set; }
    }
}
