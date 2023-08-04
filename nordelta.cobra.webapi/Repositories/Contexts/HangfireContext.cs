using Microsoft.EntityFrameworkCore;

namespace nordelta.cobra.webapi.Repositories.Contexts
{
    public class HangfireContext : DbContext
    {
        public HangfireContext()
        {

        }

        public HangfireContext(DbContextOptions<HangfireContext> options) : base(options)
        {

        }
    }
}
