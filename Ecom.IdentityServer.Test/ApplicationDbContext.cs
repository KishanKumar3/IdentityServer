using Microsoft.EntityFrameworkCore;

namespace Ecom.IdentityServer.Test
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }
    }
}
