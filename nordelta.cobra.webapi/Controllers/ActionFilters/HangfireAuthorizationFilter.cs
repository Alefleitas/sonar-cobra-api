using Hangfire.Dashboard;

namespace nordelta.cobra.webapi.Controllers.ActionFilters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            //var httpContext = context.GetHttpContext();
            return true;
        }
    }
}
