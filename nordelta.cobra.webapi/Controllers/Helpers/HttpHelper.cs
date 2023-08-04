using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Controllers.Helpers
{
    public static class HttpHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task DownloadFileAsync(string uri
            , string outputPath)
        {
            Uri uriResult;

            if (!Uri.TryCreate(uri, UriKind.Absolute, out uriResult))
                throw new InvalidOperationException("URI is invalid.");

            byte[] fileBytes = await _httpClient.GetByteArrayAsync(uri);
            await File.WriteAllBytesAsync(outputPath, fileBytes);
        }
    }
}
