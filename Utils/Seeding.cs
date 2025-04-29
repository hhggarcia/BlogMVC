using BlogMVC.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlogMVC.Utils
{
    public static class Seeding
    {
        private static List<string> roles = new List<string>
        {
            Constantes.RolAdmin,
            Constantes.CRUDEntradas,
            Constantes.BorraComentarios
        };

        public static void Aplicar(DbContext context, bool _)
        {
            foreach (var role in roles) 
            {
                var rolDb = context.Set<IdentityRole>().FirstOrDefault(x => x.Name == role);

                if (rolDb is null) 
                {
                    context.Set<IdentityRole>().Add(new IdentityRole()
                    {
                        Name = role,
                        NormalizedName = role.ToUpper(),
                    });

                    context.SaveChanges();
                }
            }
        }

        public static async Task AplicarAsync(DbContext context, bool _, CancellationToken cancellationToken)
        {
            foreach (var role in roles)
            {
                var rolDb = await context.Set<IdentityRole>().FirstOrDefaultAsync(x => x.Name == role);

                if (rolDb is null)
                {
                    context.Set<IdentityRole>().Add(new IdentityRole()
                    {
                        Name = role,
                        NormalizedName = role.ToUpper(),
                    });

                   await context.SaveChangesAsync(cancellationToken);
                }
            }
        }
    }
}
