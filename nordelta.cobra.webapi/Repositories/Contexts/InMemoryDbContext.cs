using Microsoft.EntityFrameworkCore;
using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Repositories.Contexts
{
    public partial class InMemoryDbContext : DbContext
    {
        public InMemoryDbContext(DbContextOptions<InMemoryDbContext> options)
        : base(options)
        {

        }

        public DbSet<SsoUser> SsoUsers { get; set; }
        public DbSet<SsoUserCuit> SsoUserCuits { get; set; }
        public DbSet<SsoUserRole> SsoUserRoles { get; set; }
        public DbSet<SsoUserEmpresa> SsoUserEmpresas { get; set; }
        public DbSet<SsoEmpresa> SsoEmpresas { get; set; }
        public DbSet<HolidayDay> HolidayDays { get; set; }

    }
}