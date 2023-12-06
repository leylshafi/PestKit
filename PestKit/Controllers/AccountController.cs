using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging.Signing;
using PestKit.Enumerations;
using PestKit.Models;
using PestKit.Utilities.Extetions;
using PestKit.ViewModels;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace PestKit.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;
        public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
        }

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM userVM)
        {
            if(!ModelState.IsValid) return View();
            AppUser user = new AppUser()
            {
                Name = userVM.Name,
                Surname = userVM.Surname,
                Email = userVM.Email,
                UserName = userVM.Username,
                Gender = userVM.Gender.ToString(),
            };

            if(userVM.ProfilePhoto is not null)
            {
                if (!userVM.ProfilePhoto.ValidateType())
                {
                    ModelState.AddModelError("Photo", "Wrong file type");
                    return View();
                }
                if (!userVM.ProfilePhoto.ValidateSize(2 * 1024))
                {
                    ModelState.AddModelError("Photo", "It shouldn't exceed 2 mb");
                    return View();
                }
                user.ImageUrl = await userVM.ProfilePhoto.CreateFile(_env.WebRootPath, "assets", "img");
            }
            
            
            IdentityResult result = await _userManager.CreateAsync(user, userVM.Password);
            if(!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(String.Empty, error.Description);
                }
                return View();
            }
            await _userManager.AddToRoleAsync(user,Roles.Member.ToString());
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index","Home");
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

		public IActionResult Login()
		{
			return View();
		}
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM userVM,string? returnUrl)
        {
			if (!ModelState.IsValid) return View();
            AppUser user = await _userManager.FindByNameAsync(userVM.UsernameOrEmail);
            if(user is null)
            {
                user = await _userManager.FindByEmailAsync(userVM.UsernameOrEmail);
                if(user is null)
                {
                    ModelState.AddModelError(String.Empty, "Username (Email) or Password is incorrect");
                    return View();
                }
            }

            SignInResult result = await _signInManager.PasswordSignInAsync(user, userVM.Password, userVM.IsRemembered, true);
            if(!result.Succeeded)
            {
				ModelState.AddModelError(String.Empty, "Username (Email) or Password is incorrect");
				return View();
			}
            if (returnUrl is null)
            {
                return RedirectToAction("Index", "Home");
            }
            return Redirect(returnUrl);
           
		}

        public async Task<IActionResult> CreateRoles()
        {
            foreach (var item in Enum.GetValues(typeof(Roles)))
            {
                if(!await _roleManager.RoleExistsAsync(item.ToString()))
                {
					await _roleManager.CreateAsync(new IdentityRole()
					{
						Name = item.ToString()
					});
				}
            }
            return RedirectToAction("Index","Home");
        }

        public IActionResult Profile()
        {
            var user = _userManager.Users.FirstOrDefault(u => User.Identity.Name == u.UserName);
            var vm = new ProfileVM()
            {
                ProfileImage = user.ImageUrl,
                Name = user.Name,
                Surname = user.Surname,
                Bio = user.Bio,
                Birthday = user.Birthday
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(ProfileVM profileVM)
        {
            var user = _userManager.Users.FirstOrDefault(u => User.Identity.Name == u.UserName);
            user.Name = profileVM.Name;
            user.Bio = profileVM.Bio;
            if(profileVM.ProfilePhoto != null)
            {
                user.ImageUrl = await profileVM.ProfilePhoto.CreateFile(_env.WebRootPath,"assets","img");
            }
            user.Birthday = profileVM.Birthday;
            user.Surname = profileVM.Surname;
           
            await _userManager.UpdateAsync(user);
            return View(profileVM);
        }
    }
}
