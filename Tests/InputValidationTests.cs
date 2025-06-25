using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
//using MyWebApp.Controllers;  // Adjust namespace if needed
using MyWebApp.Helpers;      // Assuming InputSanitizer is defined here
using System.ComponentModel.DataAnnotations; // For the data annotations

namespace MyWebApp.Tests
{
    [TestFixture]
    public class AccountControllerTests
    {
        private Mock<UserManager<IdentityUser>> _userManagerMock;
        private Mock<SignInManager<IdentityUser>> _signInManagerMock;
        private AccountController _controller;

        [SetUp]
        public void SetUp()
        {
            _userManagerMock = CreateMockUserManager();
            _signInManagerMock = CreateMockSignInManager(_userManagerMock.Object);

            // Instantiate the AccountController with the mocks.
            _controller = new AccountController(_userManagerMock.Object, _signInManagerMock.Object)
            {
                // Set up a dummy ControllerContext to support HttpContext usage.
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        // You can add dummy claims here if needed.
                        User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity())
                    }
                }
            };
        }

        
        [Test]
        public async Task Register_WithSqlInjection_ShouldReturnViewWithSanitizedError()
        {
            // Arrange: Use a malicious SQL injection payload in the username.
            var maliciousUsername = "test'; DROP TABLE Users; --";
            var model = new RegisterViewModel
            {
                Username = maliciousUsername,
                Email = "inject@example.com",
                Password = "Test#123"
            };

            // Act: Call the Register action.
            var result = await _controller.Register(model);

            // Assert: Check that we get a ViewResult with a ModelState error indicating sanitization occurred.
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null, "Expected a ViewResult from Register.");
            Assert.That(_controller.ModelState.ContainsKey(string.Empty), Is.True, "Expected a global ModelState error key.");
            
            var errorMsg = _controller.ModelState[string.Empty].Errors.First().ErrorMessage;
            Assert.That(errorMsg, Does.Contain("sanitized"), "Expected an error message indicating the input was sanitized.");
        }

        [Test]
        public async Task Register_WithXssInput_ShouldReturnViewWithSanitizedError()
        {
            // Arrange: Use an XSS payload in the username.
            var maliciousUsername = "<script>alert('xss')</script>";
            var model = new RegisterViewModel
            {
                Username = maliciousUsername,
                Email = "xss@example.com",
                Password = "Test#123"
            };

            // Act: Call the Register action.
            var result = await _controller.Register(model);

            // Assert: Verify the ViewResult indicates a sanitization error.
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null, "Expected a ViewResult from Register.");
            Assert.That(_controller.ModelState.ContainsKey(string.Empty), Is.True, "Expected a global ModelState error key.");
            
            var errorMsg = _controller.ModelState[string.Empty].Errors.First().ErrorMessage;
            Assert.That(errorMsg, Does.Contain("sanitized"), "Expected an error message indicating input was sanitized.");
        }

        [Test]
        public async Task Login_WithXssInput_ShouldReturnViewWithSanitizedError()
        {
            // Arrange: For login, assume the username is sanitized.
            var maliciousUsername = "<script>alert('xss')</script>";
            var model = new LoginViewModel
            {
                Username = maliciousUsername,
                Password = "Test#123"
            };

            // Act: Call the Login action.
            var result = await _controller.Login(model);

            // Assert: Verify that the returned ViewResult contains a ModelState error.
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null, "Expected a ViewResult from Login.");
            Assert.That(_controller.ModelState.ContainsKey(string.Empty), Is.True, "Expected a global ModelState error key.");
            
            var errorMsg = _controller.ModelState[string.Empty].Errors.First().ErrorMessage;
            Assert.That(errorMsg, Does.Contain("sanitized"), "Expected an error message indicating input was sanitized.");
        }

        [Test]
        public async Task Profile_Display_ShouldNotRenderStoredXss()
        {
            var maliciousUsername = "<script>alert('xss')</script>";
            var user = new IdentityUser { UserName = maliciousUsername };

            _userManagerMock.Setup(m => m.FindByIdAsync("someUserId"))
                .ReturnsAsync(user);

            var result = await _controller.Profile("someUserId");

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null, "Expected a ViewResult from Profile.");

            var model = viewResult.Model as ProfileViewModel;
            Assert.That(model, Is.Not.Null, "Expected ProfileViewModel in the ViewResult.");
            Assert.That(model.Username, Does.Not.Contain("<script>"), "Username should be sanitized for XSS.");
        }



        // Helper method to set up a mock UserManager.
        private Mock<UserManager<IdentityUser>> CreateMockUserManager()
        {
            var store = new Mock<IUserStore<IdentityUser>>();
            var mgr = new Mock<UserManager<IdentityUser>>(
                store.Object, null, null, null, null, null, null, null, null);
            mgr.Object.UserValidators.Add(new UserValidator<IdentityUser>());
            mgr.Object.PasswordValidators.Add(new PasswordValidator<IdentityUser>());
            return mgr;
        }

        // Helper method to set up a mock SignInManager.
        private Mock<SignInManager<IdentityUser>> CreateMockSignInManager(UserManager<IdentityUser> userManager)
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var userClaimsPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
            return new Mock<SignInManager<IdentityUser>>(
                userManager,
                contextAccessor.Object,
                userClaimsPrincipalFactory.Object,
                null, null, null, null);
        }
    }
}
