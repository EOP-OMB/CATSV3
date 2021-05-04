using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Mod.Framework.Runtime.Session;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.EfCore
{
    public class CatsV3AppContextFactory : IDesignTimeDbContextFactory<CatsV3AppContext>
    {
        public CatsV3AppContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("MOD.CatsV3.ConnectionString");

            var optionsBuilder = new DbContextOptionsBuilder<CatsV3AppContext>();
            optionsBuilder.UseSqlServer(connectionString);

            var context = new CatsV3AppContext(optionsBuilder.Options, NullModSession.Instance);

            return context;
        }
    }
}
