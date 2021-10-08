using System;
using System.Threading.Tasks;
using Amazon.AspNetCore.Identity.Cognito;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAdvert.Web.Models.Account;

namespace WebAdvert.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<CognitoUser> _signInManager;
        private readonly UserManager<CognitoUser> _userManager;
        private readonly CognitoUserPool _cognitoUserPool;
        public AccountController(SignInManager<CognitoUser> signInManager, 
            UserManager<CognitoUser> userManager, CognitoUserPool cognitoUserPool)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _cognitoUserPool = cognitoUserPool;
        }
        public async Task<IActionResult> Signup()
        {
            var model = new SignupModel();
            return View(model);
        }

        [HttpPost]
        [ActionName("Signup")]
        public async Task<IActionResult> Signup(SignupModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _cognitoUserPool.GetUser(model.Email);
                if (user.Status != null)
                {
                    ModelState.AddModelError("UserExist", "User with this email already exists");
                    return View(model);
                }
                user.Attributes.Add(CognitoAttribute.Email.AttributeName, model.Email);
                var createdUser = await _userManager.CreateAsync(user, model.Password);
                if (createdUser.Succeeded)
                {
                    return RedirectToAction("Confirm", "Account");
                }
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Confirm(ConfirmModel model)
        {
            return View(model);
        }

        [HttpPost]
        [ActionName("Confirm")]
        public async Task<IActionResult> ConfirmPost(ConfirmModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user.Status == null)
                {
                    ModelState.AddModelError("NotFound", 
                        "User with the given email was not found");
                    return View(model);
                }

                var result = await ((CognitoUserManager<CognitoUser>)_userManager)
                    .ConfirmSignUpAsync(user, model.Code, true);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError(item.Code, item.Description);
                }
                return View(model);
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Login(LoginModel model)
        {
            return View(model);
        }

        [HttpPost]
        [ActionName("Login")]
        public async Task<IActionResult> LoginPost(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("Login Error", "Email or password is not correct");
                    return View(model);
                }
                
            }
            return View("Login", model);
        }

        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            return View(model);
        }

        public async Task<IActionResult> GenerateCodeToResetPassword(GenerateCodeToResetPasswordModel model)
        {
            return View(model);
        }

        [HttpPost]
        [ActionName("GenerateCodeToResetPassword")]
        public async Task<IActionResult> GenerateCodeToResetPasswordPost(GenerateCodeToResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("NotFound",
                        "User with the given email was not found");
                    return View(model);
                }
                
                try
                {
                    await user.ForgotPasswordAsync();
                    var resetModel = new ResetPasswordModel()
                    {
                        Email = model.Email
                    };
                    return RedirectToAction("ResetPassword", "Account", resetModel);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("ResetPasswordError", ex.Message);
                }

            }
            return View(model);
        }

        [HttpPost]
        [ActionName("ResetPassword")]
        public async Task<IActionResult> ResetPasswordPost(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("NotFound",
                        "User with the given email was not found");
                    return View(model);
                }

                try
                {
                    await user.ConfirmForgotPasswordAsync(model.Code, model.NewPassword);
                    return RedirectToAction("Login", "Account");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("ResetPasswordError", ex.Message);
                }
                
            }
            return View(model);
        }
    }
}