
namespace nordelta.cobra.webapi.Connected_Services.Itau.ArchivosCmlServiceItau.Constants
{
    public class TipoArchivo
    {
        public static readonly string
            RendicionNoConfirmada = "R",
            RendicionDiaria = "F",
            ReporteValidacionArchivo = "R";
    }

    public class TipoRendicion
    {
        public static readonly string
            RendicionTipoListado = "L",
            RendicionTipoArchivoLimitado = "A",
            Fin = "A";
    }

    public class CodigoRetorno
    {
        public static readonly string
            OkResult = "00",
            ErrorValidacion = "01",
            ErrorProceso = "99";
    }
}
