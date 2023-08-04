using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using nordelta.cobra.webapi.Helpers;
using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Serilog;

namespace nordelta.cobra.webapi.Controllers.ActionFilters
{
    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class AuthTokenAttribute : Attribute, IOrderedFilter, IAsyncActionFilter
    {
        private const string HEADER_NAME = "Authorization";
        private readonly EPermission[] requiredPermissions;

        public int Order => 1; // Auth must execute first because it sets the token and the user data that next filters might use

        /// <summary>
        /// This attribute validates that user has a valid token. If not, it will return Unauthorized
        /// </summary>
        public AuthTokenAttribute(params EPermission[] requiredPermissions)
        {
            if (requiredPermissions != null)
            {
                this.requiredPermissions = requiredPermissions;
            }
            else
            {
                this.requiredPermissions = new EPermission[0];
            }
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            string appToken = context.HttpContext.Request.Headers[HEADER_NAME];

            ClaimsPrincipal claims = JwtManager.GetVerifiedPayload(appToken);
            if (claims != null)
            {
                if (claims.FindFirstValue("sistema") == "externo")
                {
                    var roles = JsonConvert.DeserializeObject<List<Role>>(claims.FindFirstValue("userRoles"));
                    if (!roles.Any(x => x.Name == "extern" && HasPermissions(x.Permissions, requiredPermissions)))
                    {
                        context.Result = new EmptyResult();
                        context.HttpContext.Response.StatusCode = 401;
                        return;
                    }
                } else if (!string.IsNullOrEmpty(claims.FindFirstValue("externalSystem")))
                {
                    var roles = JsonConvert.DeserializeObject<List<Role>>(claims.FindFirstValue("userRoles"));
                    if (!roles.Any(x => HasPermissions(x.Permissions, requiredPermissions)))
                    {
                        context.Result = new EmptyResult();
                        context.HttpContext.Response.StatusCode = 401;
                        return;
                    }
                } else
                {
                    User user = new User
                    {
                        Id = claims.FindFirstValue("userId"),
                        FirstName = claims.FindFirstValue("firstname"),
                        LastName = claims.FindFirstValue("lastname"),
                        Cuit = Convert.ToInt64(claims.FindFirstValue("cuit")),
                        AdditionalCuits = JsonConvert.DeserializeObject<List<string>>(claims.FindFirstValue("aditionalCuits")),
                        BusinessUnits = JsonConvert.DeserializeObject<List<BusinessUnit>>(claims.FindFirstValue("businessUnits")),
                        Email = claims.FindFirstValue("userEmail"),
                        BirthDate = Convert.ToDateTime(claims.FindFirstValue("userBirthDate")),
                        Roles = JsonConvert.DeserializeObject<List<Role>>(claims.FindFirstValue("userRoles")),
                        SupportUserId = claims?.FindFirstValue("supportUserId"),
                        SupportUserEmail = claims?.FindFirstValue("supportUserEmail"),
                        SupportUserName = claims?.FindFirstValue("supportUserName"),
                        AccountNumber = claims.FindFirstValue("accountNumber"),
                        ClientReference = claims.FindFirstValue("clientReference"),
                        IsForeignCuit = Convert.ToBoolean(claims.FindFirstValue("isForeignCuit"))
                    };


                    string serializedClaims = JsonConvert.SerializeObject(user);
                    //Role tokenRole = (Role)JsonConvert.DeserializeObject(claims.FindFirstValue("userRole"), typeof(Role));

                    if (!HasPermissions(user, requiredPermissions))
                    {
                        context.Result = new EmptyResult();
                        context.HttpContext.Response.StatusCode = 401;
                        Log.Debug(
                            @"Usuario no tiene permisos para acceder al endpoint. 
                        Usuario: {@user}, HttpRequest:{@request}, 
                        Token: {token}, 
                        PermisosRequeridos:{@requiredPermissions}", user, context.HttpContext.Request, appToken, requiredPermissions);
                        return;
                    }

                    context.HttpContext.Request.Headers["user"] = serializedClaims;
                    bool isAdmin = user.Roles.FirstOrDefault(e => e.Name.Equals("admin")) != null;
                    Claim[] authClaims = {
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                        new Claim(ClaimTypes.Role, isAdmin ? "admin" : "client")
                    };
                    ClaimsIdentity identity = new ClaimsIdentity(authClaims, "basic");
                    context.HttpContext.User = new ClaimsPrincipal(identity);
                }

                await next();
            }
            else
            {
                context.Result = new EmptyResult();
                context.HttpContext.Response.StatusCode = 403;
                Log.Debug(
                        @"Se ha utilizado un token invalido para intentar acceder a un endpoint. 
                        HttpRequestHeaders:{@requestHeaders}, 
                        HttpRequestMethod:{method},
                        HttpRequestParameters:{parameters}
                        Token: {token}", context.HttpContext.Request.Headers.Select(x => x.Key + "=" + x.Value).ToList(), context.HttpContext.Request.Path, context.ActionArguments.Values, appToken);
                return;
            }

        }

        public static  bool HasPermissions(User user, EPermission[] requiredPermissions)
        {
            return !(user == null || !requiredPermissions.All(requiredPermission =>
                        user.Roles.Any(role =>
                            role.Permissions.Select(rolPermission => rolPermission.Code).ToList().Contains(requiredPermission)
                            )
                    ));
        }
        public static bool HasRole(User user, string userRoleName)
        {
            return user != null && user.Roles.Any(x => x.Name.ToLower().Equals(userRoleName.ToLower()));
        }

        private static bool HasPermissions(List<Permission> permissions, EPermission[] requiredPermissions)
        {
            return requiredPermissions.Any(rp => permissions.Select(rolPermission => rolPermission.Code).ToList().Contains(rp));
        }
    }
}