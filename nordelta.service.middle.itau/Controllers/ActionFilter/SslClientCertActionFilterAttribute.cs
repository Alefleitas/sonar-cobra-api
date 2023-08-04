using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.service.middle.itau.Controllers.ActionFilter
{
    //https://stackoverflow.com/questions/27323880/disable-ssl-client-certificate-on-some-webapi-controllers
    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class SslClientCertActionFilterAttribute : Attribute, IAsyncActionFilter
    {
        public List<string> AllowedThumbprints = new List<string>()
        {
            "D73BB1D357D037B0476DA59A7F06FD931852C488", //Consultatio 2023
            "ACF780D92451321553ABF6A6909CCB001E85BEFF", //Nordelta 2023
            "E8405E7701BB62190E89E46FF951B925FE5DD216", //Fideicomiso Golf Club 2023
            "F23ECA020969445FE650B2F0516525C8E0B235B3", //UT Puerto Madero 2023
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
