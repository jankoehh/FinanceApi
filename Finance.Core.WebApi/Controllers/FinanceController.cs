using AutoMapper;
using Finance.Core.Data.Models;
using Finance.Core.Data.Repositories;
using Finance.Core.Services.FileServices;
using Finance.Core.WebApi.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace Finance.Core.WebApi.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class FinanceController : ControllerBase
	{
		private readonly IMapper _Mapper;
		private readonly IEinnahmenAusgabenRepository _EinnahmeAusgabenRepository;
		private readonly ITagsRepository _TagsRepository;
		private readonly IUserRepository _UserRepository;
		private readonly IImageRepository _ImageRepository;
		private readonly UserManager<FinanceAppUser> _UserManager;
		private readonly IHostingEnvironment _HostingEnvironment;
		private readonly ICreateZufallsDateinamenService _CreateZufallsDateinamenService;
		private readonly ICheckDateiGleichBildService _CheckDateiGleichBildService;

		public FinanceController(IMapper mapper,
			IEinnahmenAusgabenRepository einnahmeAusgabenRepository,
			ITagsRepository tagsRepository,
			IUserRepository userRepository,
			IImageRepository imageRepository,
			UserManager<FinanceAppUser> userManager,
			IHostingEnvironment hostingEnvironment,
			ICreateZufallsDateinamenService createZufallsDateinamenService,
			ICheckDateiGleichBildService checkDateiGleichBildService)
		{
			_Mapper = mapper;
			_EinnahmeAusgabenRepository = einnahmeAusgabenRepository;
			_TagsRepository = tagsRepository;
			_UserManager = userManager;
			_ImageRepository = imageRepository;
			_HostingEnvironment = hostingEnvironment;
			_CreateZufallsDateinamenService = createZufallsDateinamenService;
			_CheckDateiGleichBildService = checkDateiGleichBildService;
		}

		/// <summary>
		/// Einnahmen/Ausgabe auslesen
		/// </summary>
		/// <returns></returns>
		[HttpGet()]
		public IActionResult GetEinnahmenAusgaben()
		{
			try
			{
				IList<EinnahmeAusgabe> listEinnahmenAusgaben = _EinnahmeAusgabenRepository.GetEinnahmenAusgaben(User.Identity.Name);

				if (listEinnahmenAusgaben == null)
					return BadRequest("Probleme beim Holen der Daten aus den Repository");

				return Ok(_Mapper.Map<IList<EinnahmeAusgabe>, IList<EinnahmeAusgabeViewModel>>(listEinnahmenAusgaben));
			}
			catch (Exception ex)
			{
				return BadRequest($"Irgendein Problem welches nicht behandet wurde ist aufgetreten, Fehler: {ex.Message}; {ex.InnerException}");
			}
		}

		[HttpPost()]
		public async Task<IActionResult> GetEinnahmenAusgaben([FromBody] IEnumerable<TagViewModel> listTagsVM)
		{
			try
			{
				var listTags = _Mapper.Map<IEnumerable<TagViewModel>, IEnumerable<Tag>>(listTagsVM);
				var loggedInUser = await _UserManager.FindByNameAsync(User.Identity.Name);
				IList<EinnahmeAusgabe> listEinnahmenAusgaben = _EinnahmeAusgabenRepository.GetEinnahmenAusgaben(loggedInUser, listTags);

				//Was wird hier gemappt?
				var listEinAusgabeVM = _Mapper.Map<IList<EinnahmeAusgabe>, IList<EinnahmeAusgabeViewModel>>(listEinnahmenAusgaben);

				return Ok(_Mapper.Map<IList<EinnahmeAusgabe>, IList<EinnahmeAusgabeViewModel>>(listEinnahmenAusgaben));
			}
			catch (Exception ex)
			{
				return BadRequest($"Irgendein Problem welches nicht behandet wurde ist aufgetreten, Fehler: {ex.Message}; {ex.InnerException}");
			}

		}

		[HttpGet("{einnahmeAusgabeId:long}")]
		public async Task<IActionResult> GetEinnahmeAusgabe(long einnahmeAusgabeId)
		{
			try
			{
				FinanceAppUser loggedInUser = await _UserManager.FindByNameAsync(User.Identity.Name);
				EinnahmeAusgabe einnahmeAusgabe = _EinnahmeAusgabenRepository.GetEinnahmeAusgabeDetail(loggedInUser, einnahmeAusgabeId);

				if (einnahmeAusgabe == null)
					return BadRequest("Probleme beim Holen der Daten aus dem Repository");

				EinnahmeAusgabeViewModel eaVM = _Mapper.Map<EinnahmeAusgabe, EinnahmeAusgabeViewModel>(einnahmeAusgabe);

				//Alle zum Finanzeintrag zugehörigen Bilder anhängen
				IList<FinanceImage> listFinImage = _ImageRepository.GetEinnahmeAusgabeBilder(einnahmeAusgabe, loggedInUser);

				eaVM.ListImages = _Mapper.Map<IList<FinanceImage>, IList<ImageMetaViewModel>>(listFinImage);

				return Ok(eaVM);
			}
			catch (Exception ex)
			{
				return BadRequest($"Irgendein Problem welches nicht behandet wurde ist aufgetreten, Fehler: {ex.Message}; {ex.InnerException}");
			}
		}

		[HttpPost]
		public async Task<IActionResult> UploadImage([FromForm(Name = "image")] IFormFile uploadImg)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(User.Identity.Name))
					return BadRequest("User nicht eingeloggt, hochgeladene Datei kann User nicht zugeordnet werden!");

				if (uploadImg == null)
					return BadRequest("Datei konnt nicht entgegengenommen werde.");

				if (uploadImg.Length == 0)
					return BadRequest("Es wurde keine Datei übergeben (Dateigröße ist 0 Byte)");

				//Checken ob hochgeladene Datei ein Bild ist
				if (!_CheckDateiGleichBildService.CheckenDateiIstBild(uploadImg))
					return BadRequest("Hochgeladene Datei ist kein Bild, momentan können nur Bilder hochgeladen werden!");

				//Filename randommässig erzeugen (Achtung Original-Dateiname nie abspeichern!!! Kann von bösatigen Usern zu Hackzwecken verwendet werden)
				string newUploadFileName = _CreateZufallsDateinamenService.CreateZufallsDateinamen(_CheckDateiGleichBildService.GetFileExtension(uploadImg));

				string speicherOrdner = $"{_HostingEnvironment.WebRootPath}\\{User.Identity.Name}\\UploadFiles\\";
				string speicherOrdnerDatei = $"{_HostingEnvironment.WebRootPath}\\{User.Identity.Name}\\UploadFiles\\{newUploadFileName}";
				string webUrlBild = $"\\{User.Identity.Name}\\UploadFiles\\{newUploadFileName}";

				//Existiert der Ordner? Wenn nein neu anlegen
				if (!Directory.Exists(speicherOrdner))
					Directory.CreateDirectory(speicherOrdner);

				//Statt dem original Dateinamen wird eine Random-Name erzeugt der dann abgespeichert wird
				using (FileStream stream = System.IO.File.Create(speicherOrdnerDatei))
				{
					uploadImg.CopyTo(stream);
					stream.Flush();
				}

				//Speichern des Verweises (wo Datei abgelegt wurde) in der Datenbank
				var loggedInUser = await _UserManager.FindByNameAsync(User.Identity.Name);

				FinanceImage neuImage = new FinanceImage()
				{
					Path = speicherOrdnerDatei,
					UploadDate = DateTime.Now,
					ImageName = $"Am {DateTime.Now.ToShortDateString()} hochgeladenes Bild",
					Url = webUrlBild
				};

				var storedImage = _ImageRepository.AddNeuImage(neuImage, loggedInUser);

				if (storedImage.Id <= 0)
					return BadRequest("Das hochgeladene Bild konnte nicht gespeichert werden.");

				//Metadaten des Bildes die an den Client zurückgesandt werden sobald Bild hochgeladen wurde
				ImageMetaViewModel uploadImageMeta = _Mapper.Map<FinanceImage, ImageMetaViewModel>(storedImage);
				//uploadImageMeta.ImageSource = webUrlBild;

				return CreatedAtAction("UploadImage", uploadImageMeta);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[HttpPost]
		public async Task<IActionResult> DeleteBilder([FromBody] IList<ImageMetaViewModel> listBilder4Delete)
		{
			try
			{
				if (ModelState.IsValid)
				{
					//Entfernen des Bildverweises in der Datenbank
					var deletedImages = _ImageRepository.DeleteImages(listBilder4Delete.Select(b => b.ImageId).ToList());

					if (deletedImages == null || deletedImages.Count == 0)
						return BadRequest("Bilder wurden nicht in Datenbank gelöscht");

					foreach (var image in deletedImages)
					{
						//Entfernen des Bildes aus dem Ordner
						System.IO.File.Delete($"{image.Path}");
					}

					return CreatedAtAction("DeleteBilder", deletedImages);
				}
				else
				{
					return BadRequest(ModelState);
				}
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[HttpPost]
		public async Task<IActionResult> DeleteEinnahmeAusgabe([FromBody] long einnhameAusgabeId)
		{
			try
			{
				if (einnhameAusgabeId <= 0)
					return BadRequest("Zu löschende Finanzeintrag hat keine Id, er kann nicht in der Datenbank gefunden werden (und gelöscht werden).");

				var deletedEinnahmeAusgabe = _EinnahmeAusgabenRepository.DeleteEinnahmeAusgabe(einnhameAusgabeId);

				if (deletedEinnahmeAusgabe == null)
					return BadRequest("Es wurde kein Finanzeintrag gelöscht");

				//Löschen der physikalischen Bilder auf dem Server
				if (deletedEinnahmeAusgabe.Bilder != null)
				{
					foreach (var delImage in deletedEinnahmeAusgabe.Bilder)
					{
						try
						{
							//Entfernen des Bildes aus dem Ordner
							System.IO.File.Delete($"{delImage.Path}");
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.Message);
						}
					}
				}

				return CreatedAtAction("DeleteEinnahmeAusgabe", deletedEinnahmeAusgabe);
			}
			catch (Exception ex)
			{
				string errorMessage = ex.Message;

				if (ex.InnerException != null)
					errorMessage += $"{Environment.NewLine}Inner Exception: {ex.InnerException.Message}";

				return BadRequest(errorMessage);
			}
		}

		/// <summary>
		/// Neuerzeugte Einnahme-Ausgabe in Datenbank ablegen
		/// </summary>
		/// <param name="neuEinnahmeAusgabeViewModel"></param>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> AddEinnahmeAusgabe([FromBody] EinnahmeAusgabeViewModel neuEinnahmeAusgabeViewModel)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var currentUser = await _UserManager.FindByNameAsync(User.Identity.Name);

					EinnahmeAusgabe neuEinahmeAusgabe = new EinnahmeAusgabe();
					EinnahmeAusgabe returnEinnahmeAusgabe = new EinnahmeAusgabe();

					//Falls Eintrag wiederkehrend ist muss er mehrmals angelegt werden
					if (neuEinnahmeAusgabeViewModel.Wiederkehrend)
					{
						var buchungsDatum = DateTime.Parse(neuEinnahmeAusgabeViewModel.BuchungsdatumJson);
						var endDatum = DateTime.Parse(neuEinnahmeAusgabeViewModel.EnddatumWiederkehrendJson);

						//Wiederholungsrhythmus: wöchentlich, monatlich, jährlich
						var wiederholungsRhythmus = neuEinnahmeAusgabeViewModel.RhythmusWiederkehrend;

						if (buchungsDatum == null || endDatum == null)
						{
							returnEinnahmeAusgabe = SaveEinnahmeAusgabeInDB(neuEinnahmeAusgabeViewModel, currentUser);
							return Created($"api/finance/GetEinnahmeAusgabe({returnEinnahmeAusgabe.Id})", neuEinnahmeAusgabeViewModel);
						}

						if (endDatum.CompareTo(buchungsDatum) >= 0)
						{
							switch (wiederholungsRhythmus)
							{
								case 0:
									//wöchentlicher Wiederholungsrhythmus
									for (DateTime datum = buchungsDatum; datum.CompareTo(endDatum.AddDays(-7)) <= 0; datum = datum.AddDays(7))
										returnEinnahmeAusgabe = SaveEinnahmeAusgabeInDB(neuEinnahmeAusgabeViewModel, currentUser, datum);

									return Created($"api/finance/GetEinnahmeAusgabe({returnEinnahmeAusgabe.Id})", neuEinnahmeAusgabeViewModel);
								case 1:
									//monatlicher Wiederholungsrhythmus
									for (DateTime datum = buchungsDatum; datum.CompareTo(endDatum.AddMonths(-1)) <= 0; datum = datum.AddMonths(1))
										returnEinnahmeAusgabe = SaveEinnahmeAusgabeInDB(neuEinnahmeAusgabeViewModel, currentUser, datum);

									return Created($"api/finance/GetEinnahmeAusgabe({returnEinnahmeAusgabe.Id})", neuEinnahmeAusgabeViewModel);
								default:
									//jährlicher Wiederholungsrhythmus
									for (DateTime datum = buchungsDatum; datum.CompareTo(endDatum.AddYears(-1)) <= 0; datum = datum.AddYears(1))
										returnEinnahmeAusgabe = SaveEinnahmeAusgabeInDB(neuEinnahmeAusgabeViewModel, currentUser, datum);

									return Created($"api/finance/GetEinnahmeAusgabe({returnEinnahmeAusgabe.Id})", neuEinnahmeAusgabeViewModel);
							}
						}
					}

					returnEinnahmeAusgabe = SaveEinnahmeAusgabeInDB(neuEinnahmeAusgabeViewModel, currentUser);
					return Created($"api/finance/GetEinnahmeAusgabe({returnEinnahmeAusgabe.Id})", neuEinnahmeAusgabeViewModel);
				}
				else
				{
					return BadRequest(ModelState);
				}
			}
			catch (Exception ex)
			{
				return BadRequest($"Neuer Eintrag konnte nicht gespeichert werden! Fehler: {ex.Message}, Innerer Fehler: {ex.InnerException}");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="neuEinahmeAusgabe"></param>
		/// <param name="currentUser"></param>
		/// <param name="imageViewModel"></param>
		/// <param name="tagViewModels"></param>
		private EinnahmeAusgabe SaveEinnahmeAusgabeInDB(
			EinnahmeAusgabeViewModel neuEinnahmeAusgabeViewModel,
			FinanceAppUser currentUser,
			DateTime? buchungsDatum = null)
		{
			EinnahmeAusgabe neuEinahmeAusgabe = _Mapper.Map<EinnahmeAusgabeViewModel, EinnahmeAusgabe>(neuEinnahmeAusgabeViewModel);
			neuEinahmeAusgabe.User = currentUser;

			if (buchungsDatum.HasValue)
			{
				if (neuEinahmeAusgabe.Wiederkehrend)
					neuEinahmeAusgabe.Benennung += $"{Environment.NewLine}{Environment.NewLine}Dies ist ein wiederkehrender Eintrag vom {neuEinahmeAusgabe.Buchungsdatum.ToString("dddd, dd MMMM yyyy")}" +
						$"{Environment.NewLine}aktuelles Buchungsdatum: {buchungsDatum.Value.ToString("dddd, dd MMMM yyyy")}";

				neuEinahmeAusgabe.Buchungsdatum = buchungsDatum.Value;
			}


			EinnahmeAusgabe einnahmeAusgabe = _EinnahmeAusgabenRepository.AddNeueEinnahmeAusgabe(neuEinahmeAusgabe);

			//Verweis EinnahmeAusgabe <> hochgeladenes Bild
			foreach (var imgId in neuEinnahmeAusgabeViewModel.ListImages.Select(i => i.ImageId))
			{
				_ImageRepository.AssignImage2EinnahmeAusgabe(imgId, einnahmeAusgabe);
			}

			//Verweis EinnahmeAusgabe <> Tag
			var tagListe = _Mapper.Map<IList<TagViewModel>, IList<Tag>>(neuEinnahmeAusgabeViewModel.ListTags);
			foreach (var tag in tagListe)
			{
				_TagsRepository.AddTagToEinnahmeAusgabe(einnahmeAusgabe, tag, currentUser);
			}

			return einnahmeAusgabe;
		}

		[HttpPost]
		public IActionResult UpdateEinnahmeAusgabe([FromBody] EinnahmeAusgabeViewModel existingEinnahmeAusgabeViewModel)
		{
			try
			{
				if (ModelState.IsValid)
				{
					EinnahmeAusgabe updateEinnahmeAusgabe = _Mapper.Map<EinnahmeAusgabeViewModel, EinnahmeAusgabe>(existingEinnahmeAusgabeViewModel);

					updateEinnahmeAusgabe = _EinnahmeAusgabenRepository.UpdateEinnahmeAusgabe(updateEinnahmeAusgabe);

					//Tagliste aktualisieren, falls sie sich geändert hat
					var tagListe = _Mapper.Map<IList<TagViewModel>, IList<Tag>>(existingEinnahmeAusgabeViewModel.ListTags);
					_TagsRepository.UpdateTagListeEinnahmeAusgabe(updateEinnahmeAusgabe, tagListe);

					//neu hochgeladene Bilder anhängen
					foreach (var imgId in existingEinnahmeAusgabeViewModel.ListImages.Select(i => i.ImageId))
					{
						_ImageRepository.AssignImage2EinnahmeAusgabe(imgId, updateEinnahmeAusgabe);
					}

					return Ok();
				}
				else
				{
					return BadRequest(ModelState);
				}
			}
			catch (Exception ex)
			{
				return BadRequest($"Eintrag konnte nicht aktualisiert werden! Fehler: {ex.Message}");
			}
		}
	}
}
