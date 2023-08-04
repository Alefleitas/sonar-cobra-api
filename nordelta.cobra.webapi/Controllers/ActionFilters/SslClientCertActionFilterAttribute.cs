using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Controllers.ActionFilters
{
    //https://stackoverflow.com/questions/27323880/disable-ssl-client-certificate-on-some-webapi-controllers
    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class SslClientCertActionFilterAttribute : Attribute, IAsyncActionFilter
    {
        public List<string> AllowedThumbprints = new List<string>()
        {
            // Replace with the thumbprints the 3rd party
            // server will be presenting. You can make checks
            // more elaborate but always have thumbprint checking ...
            //"0CA13289676AA118A7B0B732CAD7425F8AF96F5B", //Nordelta 2022
            //"82E219A7EFE27A1606F47BF53CBF43D558FB95F4", //Consultatio 2022
            //"EF11953B6BCD3787141DF5B011005D6FC23A9577", //Huergo 2022
            "D73BB1D357D037B0476DA59A7F06FD931852C488", //Consultatio 2023
            "ACF780D92451321553ABF6A6909CCB001E85BEFF", //Nordelta 2023
            "E8405E7701BB62190E89E46FF951B925FE5DD216", //Fideicomiso Golf Club 2023
            "48E1B2EAC869AB8F80230B6AD82E41F49BFC8C5C", //UT Puerto Madero 2023
            "500A01E3843B060E0FA9858DBAC83BE358E36039" //UT Huergo 2023
        };


        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var request = context.HttpContext.Request;

            if (!await AuthorizeRequestAsync(request))
            {
                context.Result = new EmptyResult();
                context.HttpContext.Response.StatusCode = 401;
                return;
            }
            await next();
        }

        private async Task<bool> AuthorizeRequestAsync(HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var clientCertificate = await request.HttpContext.Connection.GetClientCertificateAsync(); //.GetClientCertificate();

            if (clientCertificate == null || AllowedThumbprints == null || AllowedThumbprints.Count < 1)
            {
                return false;
            }

            return AllowedThumbprints.Any(thumbprint =>
                clientCertificate.Thumbprint != null &&
                clientCertificate.Thumbprint.Equals(thumbprint, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
