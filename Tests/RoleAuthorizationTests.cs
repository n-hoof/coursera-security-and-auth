using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Security.Claims;


namespace MyWebApp.Tests
{
    [TestFixture]
    public class RoleAuthorizationTests
    {
        private DefaultHttpContext CreateContext(string username, string role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            return new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        }

        [Test]
        public void Admin_WithAdminRole_CanAccessDashboard()
        {
            var context = CreateContext("AdminUser", "admin");
            var controller = new AdminController(CreateFakeUserManager())
            {
                ControllerContext = new ControllerContext { HttpContext = context }
            };

            var result = controller.Dashboard();

            Assert.That(result, Is.TypeOf<ViewResult>());
        }

        [Test]
        public void Admin_WithUserRole_IsForbidden()
        {
            var context = CreateContext("RegularUser", "user");
            var controller = new AdminController(CreateFakeUserManager())
            {
                ControllerContext = new ControllerContext { HttpContext = context }
            };

            var result = controller.Dashboard();

            Assert.That(result, Is.TypeOf<ForbidResult>());
        }

        [Test]
        public void User_WithUserRole_CanAccessDashboard()
        {
            var context = CreateContext("RegularUser", "user");
            var controller = new UserController
            {
                ControllerContext = new ControllerContext { HttpContext = context }
            };

            var result = controller.Dashboard() as ContentResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Content, Does.Contain("RegularUser"));
        }

        [Test]
        public void User_WithoutUserRole_IsForbidden()
        {
            var context = CreateContext("Imposter", "admin"); // Not a "user"
            var controller = new UserController
            {
                ControllerContext = new ControllerContext { HttpContext = context }
            };

            var result = controller.Dashboard();

            Assert.That(result, Is.TypeOf<ForbidResult>());
        }

        private UserManager<IdentityUser> CreateFakeUserManager()
        {
            var store = new Mock<IUserStore<IdentityUser>>();
            store.As<IQueryableUserStore<IdentityUser>>()
                .Setup(x => x.Users)
                .Returns(new List<IdentityUser>
                {
                    new IdentityUser { UserName = "admin", Email = "admin@example.com" }
                }.AsQueryable());

            return new UserManager<IdentityUser>(
                store.Object, null, null, null, null, null, null, null, null);
        }
    }
}