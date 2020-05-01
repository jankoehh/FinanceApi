using Finance.Core.Data.Models;
using Finance.Core.WebApi.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Finance.Core.WebApi.Controllers
{
	public class AccountController : Controller
	{
		private readonly ILogger<AccountController> _Logger;
		private readonly SignInManager<FinanceAppUser> _SignInManager;
		private readonly UserManager<FinanceAppUser> _UserManager;
		private readonly IConfiguration _Config;

		public AccountController(ILogger<AccountController> logger,
			SignInManager<FinanceAppUser> signInManager,
			UserManager<FinanceAppUser> userManager,
			IConfiguration config)
		{
			_Logger = logger;
			_SignInManager = signInManager;
			_UserManager = userManager;
			_Config = config;

		}

		public IActionResult Login()
		{
			if (this.User.Identity.IsAuthenticated)
			{
				//TODO momentan noch HomeController Index-Action-Methode...noch ändern
				return RedirectToAction("Finance", "Home");
			}

			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Login(LoginViewModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var result = await _SignInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, false);

					if (result.Succeeded)
					{
						if (Request.Query.Keys.Contains("ReturnUrl"))
							return Redirect(Request.Query["ReturnUrl"].First());

						return RedirectToAction("Finance", "Home");
					}
					else
					{
						ModelState.AddModelError("", $"Failed to Login! ({result.ToString()})");
					}
				}
				else
				{
					ModelState.AddModelError("", "ModelState is invalid");
				}
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", $"Fehler beim Login: {ex.Message}");
			}

			return View();
		}

		[HttpGet]
		public async Task<IActionResult> Logout()
		{
			await _SignInManager.SignOutAsync();
			return RedirectToAction("Login", "Account");
		}

		/// <summary>
		/// Erzeugen einen neuen Token
		/// (Beispielsweise nach Registrierung oder nach Login)
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		private object CreateToken(FinanceAppUser user)
		{
			try
			{
				//Create the Token
				var claims = new[]
				{
							new Claim(JwtRegisteredClaimNames.Sub, user.Email),
							new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
							//TODO Wird dies benötigt???
							new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
						};

				//Token-Key in der config-Datei abgelegt
				var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_Config["Tokens:Key"]));
				var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

				var token = new JwtSecurityToken(
					_Config["Tokens:Issuer"],
					_Config["Tokens:Audience"],
					claims,
					expires: DateTime.UtcNow.AddHours(4),
					signingCredentials: credentials);

				var results = new
				{
					token = new JwtSecurityTokenHandler().WriteToken(token),
					expiration = token.ValidTo
				};

				return results;
			}
			catch (Exception ex)
			{
				//Falls es beim CreateToken zum Fehler kommt
				return ex.Message;
			}
		}

		/// <summary>
		/// Registriert einen neuen User und erzeugt nach erfolgreicher Registrierung einen Token und gibt diesen zurück
		/// </summary>
		/// <param name="regModel"></param>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> Register([FromBody] RegisterViewModel regModel)
		{
			if (ModelState.IsValid)
			{
				var neuUser = new FinanceAppUser { UserName = regModel.Username, Email = regModel.Email };

				var result = await _UserManager.CreateAsync(neuUser, regModel.Password);
				if (result.Succeeded)
				{
					await _SignInManager.SignInAsync(neuUser, false);

					var results = CreateToken(neuUser);
					return Created("", results);
				}
				else
				{
					foreach (var error in result.Errors)
					{
						ModelState.AddModelError("", error.Description);
					}
					return BadRequest(ModelState);
				}
			}

			return BadRequest();
		}

		/// <summary>
		/// Wenn User korrekt eingelogt ist wird Token erzeugt und zurückgegeben?
		/// </summary>
		/// <param name="model">Daten des eingeloggten Users</param>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> CreateToken([FromBody] LoginViewModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var user = await _UserManager.FindByNameAsync(model.Username);

					if (user != null)
					{
						var result = await _SignInManager.CheckPasswordSignInAsync(user, model.Password, false);

						if (result.Succeeded)
						{

							var results = CreateToken(user);

							//erzeugte Token wird zurückgegeben
							return Created("", results);
						}
					}
				}

				return BadRequest("Fehler beim Erzeugen des Tokens");
			}
			catch (Exception ex)
			{
				string fehlerMeldung = ex.Message;

				if (ex.InnerException != null)
					fehlerMeldung += $"{Environment.NewLine}{ex.InnerException.Message}";

				//Fehlermeldung aus der Exception wird überschrieben
				fehlerMeldung = "Wahrscheinlich kein Zugriff auf Datenbank möglich, siehe Firewall. Bitte an Admin oder Entwickler wenden.";

				return BadRequest(fehlerMeldung);
			}
		}
	}
}
