namespace nordelta.cobra.webapi.Models.ValueObject.Certificate
{
    public class CheckSetting
    {
        public bool Enabled { get; set; }
        public int Amount { get; set; }
        public EmailSetting EmailSettings { get; set; }
    }
}
