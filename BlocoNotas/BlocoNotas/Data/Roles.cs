using Microsoft.AspNetCore.Identity;

namespace BlocoNotas.Data
{
    /// <summary>
    /// Classe estática responsável por inicializar os papéis (roles) de usuário no sistema.
    /// </summary>
    public static class SeedRoles
    {
        /// <summary>
        /// Cria os papéis padrão ("Admin" e "Utilizador") no sistema se ainda não existirem.
        /// </summary>
        /// <param name="serviceProvider">Provedor de serviços para obter o RoleManager</param>
        /// <returns>Tarefa assíncrona</returns>
        public static async Task CreateRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roleNames = { "Admin", "Utilizador" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}