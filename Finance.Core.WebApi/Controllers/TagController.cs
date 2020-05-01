using AutoMapper;
using Finance.Core.Data.Models;
using Finance.Core.Data.Repositories;
using Finance.Core.WebApi.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Finance.Core.WebApi.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class TagController : ControllerBase
	{
		private readonly ITagsRepository _TagsRepository;
		private readonly IMapper _Mapper;
		private readonly UserManager<FinanceAppUser> _UserManager;

		public TagController(ITagsRepository tagsRepository,
			IMapper mapper,
			UserManager<FinanceAppUser> userManager)
		{
			_TagsRepository = tagsRepository;
			_Mapper = mapper;
			_UserManager = userManager;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tagId"></param>
		/// <returns></returns>
		[HttpGet("{tagId:long}")]
		public IActionResult GetTag(long tagId)
		{
			try
			{
				var tag = _TagsRepository.GetTag(tagId);

				return Ok(_Mapper.Map<Tag, TagViewModel>(tag));
			}
			catch (Exception ex)
			{
				return BadRequest($"Tag mit der Id '{tagId}' konnte nicht gefunden werden, Fehlermeldung: {ex.Message}");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="einnahmeAusgabeId"></param>
		/// <returns></returns>
		public IActionResult GetEinnahmeAusgabeTag(long einnahmeAusgabeId)
		{
			try
			{
				var tags = _TagsRepository.GetTags(einnahmeAusgabeId);

				if (tags == null || tags.Count() == 0) return Ok();

				return Ok(_Mapper.Map<IEnumerable<Tag>, IEnumerable<TagViewModel>>(tags));
			}
			catch (Exception ex)
			{
				return BadRequest($"Es ist ein Fehler aufgetreten: {ex.Message}");
			}
		}

		/// <summary>
		/// Alle Tags die dem eingeloggten User zugeordnet sind
		/// </summary>
		/// <returns></returns>
		public async Task<IActionResult> GetTags()
		{
			try
			{
				var username = User.Identity.Name;

				if (username == null)
					return BadRequest($"Username konnte nicht ausgelesen werden!");

				var loggedInUser = await _UserManager.FindByNameAsync(User.Identity.Name);
				var userTags = _TagsRepository.GetUserTags(loggedInUser);

				if (userTags == null)
					return BadRequest($"Fehler beim Holen der zum User zugehörigen Tags");

				return Ok(_Mapper.Map<IEnumerable<Tag>, IEnumerable<TagViewModel>>(userTags));
			}
			catch (Exception ex)
			{
				return BadRequest($"Es ist ein Fehler aufgetreten: {ex.Message}");
			}
		}

		[HttpPost]
		public async Task<IActionResult> GetTags([FromBody] string tagName)
		{
			try
			{
				var username = User.Identity.Name;

				if (username == null)
					return BadRequest($"Username konnte nicht ausgelesen werden!");
				var loggedInUser = await _UserManager.FindByNameAsync(User.Identity.Name);

				var tags = _TagsRepository.SearchUserTags(loggedInUser, tagName);

				if (tags == null)
					return BadRequest($"Fehler beim Holen der zum User zugehörigen Tags");

				return Ok(_Mapper.Map<IEnumerable<Tag>, IEnumerable<TagViewModel>>(tags));
			}
			catch (Exception ex)
			{
				return BadRequest($"Es ist ein Fehler aufgetreten: {ex.Message}");
			}
		}

		[HttpPost]
		public async Task<IActionResult> AddNewTag([FromBody] TagViewModel newTagVM)
		{
			if (ModelState.IsValid)
			{
				try
				{
					var loggedInUser = await _UserManager.FindByNameAsync(User.Identity.Name);
					var newTag = _Mapper.Map<TagViewModel, Tag>(newTagVM);
					Tag savedTag = _TagsRepository.AddUserTag(loggedInUser, newTag);

					if (savedTag == null)
						return BadRequest($"Tag konnte nicht gespeichert werden da schon vorhanden!");

					return Created($"api/tag/GetTag({savedTag.Id})", newTagVM);
				}
				catch (Exception ex)
				{
					return BadRequest($"Neuer Tag konnten nicht gespeichert werden");
				}
			}

			return BadRequest();

		}

		[HttpPost]
		public async Task<IActionResult> RemoveTag([FromBody] long tagId)
		{
			try
			{
				var loggedInUser = await _UserManager.FindByNameAsync(User.Identity.Name);

				if (_TagsRepository.RemoveTag(loggedInUser, tagId))
					return Ok();

				return BadRequest($"Tag konnte nicht entfernt werden!");
			}
			catch (Exception ex)
			{
				return BadRequest($"Tag konnte nicht entfernt werden!");
			}
		}
	}
}
