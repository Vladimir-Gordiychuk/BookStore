namespace BulkyBook.Config
{
    public class StripeKeys
    {
        public const string Section = "Stripe";

        public string PublicKey { get; set; }

        public string SecretKey { get; set; }
    }
}
