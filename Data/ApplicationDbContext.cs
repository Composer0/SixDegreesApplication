using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ContactPro.Models;

namespace ContactPro.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser> //We added the type <AppUser> to the original so that it will know to use that model.
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Contact> Contacts { get; set; } = default!;
        public virtual DbSet<Category> Categories { get; set; } = default!;
    }
}