using WinSCP;

namespace nordelta.cobra.webapi.Configuration
{
    public class WinScpConfiguration
    {
        public const string GaliciaSftpConfig = "GaliciaSFTP";

        public string InputFolder { get; set; }
        public string OutputFolder { get; set; }
        public SessionOptions SessionOptions { get; set; }
    }
}
