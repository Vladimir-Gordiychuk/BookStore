namespace BulkyBook.Utility
{
    public class SmtpConfig
    {
        public const string Section = "Smtp";

        public string Host { get; set; }

        public int Port { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }
    };
}
