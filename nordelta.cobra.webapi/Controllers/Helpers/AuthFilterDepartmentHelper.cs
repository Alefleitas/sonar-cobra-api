using Microsoft.AspNetCore.Http;
using nordelta.cobra.webapi.Controllers.Contracts;
using nordelta.cobra.webapi.Models;
using Serilog;
using System.Collections.Generic;
using System.Linq;

namespace nordelta.cobra.webapi.Controllers.Helpers
{
    static public class AuthFilterDepartmentHelper<T> where T : IFilterableByDepartment
    {
        static public List<T> FilterByDepartment(IEnumerable<T> result, User user)
        {
            //Removes all accountbalances from BUs that user has no permission to see
            return result.Where(x => user.Roles.Any(r => r.Name.ToLower() == x.GetDepartment().ToString().ToLower())).ToList();
        }
        static public List<T> FilterOnlyByDepartmentExterno(IEnumerable<T> result, User user)
        {
            //Removes all accountbalances from BUs that user has no permission to see
            return result.Where(x => {
                if (user.Roles.Any(r => r.Name.ToLower() == AccountBalance.EDepartment.Externo.ToString().ToLower()))
                    return (x.GetDepartment().ToString().ToLower() == AccountBalance.EDepartment.Externo.ToString().ToLower()) && (x.GetPublishDebt() == "N");
                else
                    return true;
                })
                .ToList();
        }
        static public bool AuthByDepartment(T data, User user, HttpContext context) => AuthByDepartment(new[] { data }, user, context);
        static public bool AuthByDepartment(IEnumerable<T> data, User user, HttpContext context)
        {
            //// All data has to be from a Department (role) of the user
            bool authorized = data.All(x => user.Roles.Any(r => r.Name.ToLower() == x.GetDepartment().ToString().ToLower()
            || r.Name.ToLower() == "admin" || r.Name.ToLower() == "obrasparticulares")); //warning using string for admin validation
            authorized = authorized || (user.Roles.Any(r => r.Name.ToLower() == AccountBalance.EDepartment.Legales.ToString().ToLower())); //Legales must be able to modify publish debt 
            authorized = authorized || (user.Roles.Any(r => r.Name.ToLower() == AccountBalance.EDepartment.CuentasACobrar.ToString().ToLower())); 

            if (!authorized)
            {
                context.Response.StatusCode = 401;
                Log.Debug(
                    @"Usuario no tiene permisos ejecutar el endpoint con datos de otro Departamento. 
                        Usuario: {@user}, HttpRequest:{@request}", user, context.Request);
            }
            return authorized;
        }
    }
}
