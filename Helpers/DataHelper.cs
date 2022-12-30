using ContactPro.Data;
using Microsoft.EntityFrameworkCore;

namespace ContactPro.Helpers
{
    public static class DataHelper
    {
        public static async Task ManageDataAsync(IServiceProvider svcProvider)
        {
            var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>(); 
            //get an instance of the db application context.
            await dbContextSvc.Database.MigrateAsync(); 
            //migration: this is equivalent to update-database
        }
    }
}
