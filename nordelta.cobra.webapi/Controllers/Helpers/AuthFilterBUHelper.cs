using Microsoft.AspNetCore.Http;
using nordelta.cobra.webapi.Controllers.Contracts;
using nordelta.cobra.webapi.Models;
using Serilog;
using System.Collections.Generic;
using System.Linq;

namespace nordelta.cobra.webapi.Controllers.Helpers
{
    static public class AuthFilterBUHelper<T> where T : IFilterableByBU
    {

        static public List<T> FilterByBU(IEnumerable<T> result, User user)
        {
            //Removes all accountbalances from BUs that user has no permission to see
            return result.Where(x => user.BusinessUnits.Any(b => b.Name.ToLower() == x.GetBU().ToLower())).ToList();
        }


        static public bool AuthByBU(T data, User user, HttpContext context) => AuthByBU(new[] { data }, user, context);

        static public bool AuthByBU(IEnumerable<T> data, User user, HttpContext context)
        {
            // All data has to be from a BU of the user
            bool authorized = data.All(x => user.BusinessUnits.Any(b => b.Name.ToLower() == x.GetBU().ToLower()));

            if (!authorized)
            {
                context.Response.StatusCode = 401;
                Log.Debug(
                    @"Usuario no tiene permisos ejecutar el endpoint con datos de otra BU. 
                        Usuario: {@user}, HttpRequest:{@request}", user, context.Request);
            }
            return authorized;
        }


    }
}
