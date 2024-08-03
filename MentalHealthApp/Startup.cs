using MentalHealthApp.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MentalHealthApp.Startup))]
namespace MentalHealthApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app); // 调用配置身份验证的方法
            CreateRolesAndUsers(); // 调用创建角色和用户的方法
        }

        private void CreateRolesAndUsers()
        {
            using (var context = new ApplicationDbContext())
            {
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

                // 检查是否存在 Admin 角色，不存在则创建
                if (!roleManager.RoleExists("Admin"))
                {
                    var role = new IdentityRole { Name = "Admin" };
                    roleManager.Create(role);

                    // 创建默认的 Admin 用户
                    var user = new ApplicationUser { UserName = "Admin@monash.edu", Email = "Admin@monash.edu" };
                    var userPassword = "Admin123!";
                    var result = userManager.Create(user, userPassword);

                    if (result.Succeeded)
                    {
                        userManager.AddToRole(user.Id, "Admin");
                    }
                }

                // 检查是否存在 User 角色，不存在则创建
                if (!roleManager.RoleExists("User"))
                {
                    var role = new IdentityRole { Name = "User" };
                    roleManager.Create(role);
                }
            }
        }


    }
}
