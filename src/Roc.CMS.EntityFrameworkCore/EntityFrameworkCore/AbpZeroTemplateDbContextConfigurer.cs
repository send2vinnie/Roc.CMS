using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace Roc.CMS.EntityFrameworkCore
{
    public static class AbpZeroTemplateDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<AbpZeroTemplateDbContext> builder, string connectionString)
        {
            builder.UseSqlServer(connectionString).EnableSensitiveDataLogging();
        }

        public static void Configure(DbContextOptionsBuilder<AbpZeroTemplateDbContext> builder, DbConnection connection)
        {
            builder.UseSqlServer(connection).EnableSensitiveDataLogging();
        }
    }
}