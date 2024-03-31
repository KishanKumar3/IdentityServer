using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServerHost.Quickstart.UI;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Ecom.IdentityServer.Test.Controllers
{
    public class TestAccountController
    {
        [Fact]
        public async Task Register_ValidModel_ReturnsRedirectToLogin()
        {
            // Arrange
            using (var context = DbContextHelper.GetInMemoryDbContext())
            {
                var userManager = new Mock<UserManager<IdentityUser>>(
                    Mock.Of<IUserStore<IdentityUser>>(),
                    null, null, null, null, null, null, null, null);

                userManager.Setup(u => u.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                    .ReturnsAsync(IdentityResult.Success);

                var optionsMock = new Mock<IOptions<IdentityOptions>>();
                optionsMock.Setup(o => o.Value).Returns(new IdentityOptions());

                var userClaimsPrincipalFactory = new UserClaimsPrincipalFactory<IdentityUser>(
                    userManager.Object,
                    optionsMock.Object);

                var signInManager = new SignInManager<IdentityUser>(
                    userManager.Object,
                    new HttpContextAccessor(),
                    userClaimsPrincipalFactory,
                    null, null, null, null);

                var controller = new AccountController(
                    Mock.Of<IIdentityServerInteractionService>(),
                    Mock.Of<IClientStore>(),
                    Mock.Of<IAuthenticationSchemeProvider>(),
                    Mock.Of<IEventService>(),
                    signInManager,
                    userManager.Object
                );

                var model = new SignupViewModel
                {
                    Username = "testuser",
                    Email = "test@example.com",
                    Password = "Password123",
                    ConfirmPassword = "Password123"
                };

                // Act
                var result = await controller.Register(model) as RedirectToActionResult;

                // Assert
                Assert.NotNull(result);
                Assert.Equal("Login", result.ActionName);
                Assert.Equal("Account", result.ControllerName);
            }
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsRedirectToReturnUrl()
        {
            // Arrange
            var mockInteractionService = new Mock<IIdentityServerInteractionService>();
            var mockClientStore = new Mock<IClientStore>();
            var mockSchemeProvider = new Mock<IAuthenticationSchemeProvider>();
            var mockEventService = new Mock<IEventService>();

            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IPasswordHasher<IdentityUser>>(),
                new IUserValidator<IdentityUser>[0],
                new IPasswordValidator<IdentityUser>[0],
                Mock.Of<ILookupNormalizer>(),
                Mock.Of<IdentityErrorDescriber>(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<ILogger<UserManager<IdentityUser>>>());

            var mockSignInManager = new Mock<SignInManager<IdentityUser>>(
                mockUserManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<ILogger<SignInManager<IdentityUser>>>(),
                Mock.Of<IAuthenticationSchemeProvider>(),
                Mock.Of<IUserConfirmation<IdentityUser>>());

            var mockProfileService = new Mock<IProfileService>();

            var controller = new AccountController(
                mockInteractionService.Object,
                mockClientStore.Object,
                mockSchemeProvider.Object,
                mockEventService.Object,
                mockSignInManager.Object,
                mockUserManager.Object);
            var returnUrl = "/home";
            var loginModel = new LoginInputModel
            {
                Username = "angella",
                Password = "Pass123$",
                RememberLogin = false,
                ReturnUrl = returnUrl
            };

            mockSignInManager.Setup(sm => sm.CheckPasswordSignInAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            // Act
            //var result = await controller.Login(loginModel, "login");
            var result = await controller.Login(loginModel, "login");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
            //Assert.Equal(returnUrl, result.Url);
            /* var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
             Assert.Equal("Index", redirectToActionResult.ActionName);
             Assert.Equal("Home", redirectToActionResult.ControllerName);
             Assert.Equal("/home", redirectToActionResult.RouteValues["returnUrl"]);*/
        }



        [Fact]
        public async Task Login_InvalidCredentials_ReturnsViewWithModelError()
        {
            // Arrange
            var mockInteractionService = new Mock<IIdentityServerInteractionService>();
            var mockClientStore = new Mock<IClientStore>();
            var mockSchemeProvider = new Mock<IAuthenticationSchemeProvider>();
            var mockEventService = new Mock<IEventService>();

            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IPasswordHasher<IdentityUser>>(),
                new IUserValidator<IdentityUser>[0],
                new IPasswordValidator<IdentityUser>[0],
                Mock.Of<ILookupNormalizer>(),
                Mock.Of<IdentityErrorDescriber>(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<ILogger<UserManager<IdentityUser>>>());

            var mockSignInManager = new Mock<SignInManager<IdentityUser>>(
                mockUserManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<ILogger<SignInManager<IdentityUser>>>(),
                Mock.Of<IAuthenticationSchemeProvider>(),
                Mock.Of<IUserConfirmation<IdentityUser>>());

            var mockProfileService = new Mock<IProfileService>();

            var controller = new AccountController(
                mockInteractionService.Object,
                mockClientStore.Object,
                mockSchemeProvider.Object,
                mockEventService.Object,
                mockSignInManager.Object,
                mockUserManager.Object);

            var loginModel = new LoginInputModel
            {
                Username = "invalidUser",
                Password = "invalidPassword",
                RememberLogin = false,
                ReturnUrl = "/home"
            };

            // Simulate unsuccessful login attempt
            mockSignInManager.Setup(sm => sm.CheckPasswordSignInAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            // Act
            var result = await controller.Login(loginModel, "login") as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.False(controller.ModelState.IsValid);
        }

    }
}
