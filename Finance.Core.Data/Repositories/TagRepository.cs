using Finance.Core.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Finance.Core.Data.Repositories
{
	public class TagsRepository : ITagsRepository
	{
		private readonly FinanceContext _Context;
		//private readonly ILogger _Logger;

		public TagsRepository(FinanceContext context)
		{
			_Context = context;
		}

		/// <summary>
		/// Tag an einen Einnahme/Ausgabe anhängen
		/// TODO den User mit übergeben
		/// </summary>
		public void AddTagToEinnahmeAusgabe(EinnahmeAusgabe einnahmeAusgabe, Tag tag, FinanceAppUser user)
		{
			try
			{
				if (einnahmeAusgabe == null || tag == null)
				{
					throw new NullReferenceException("Einnahme /Ausgabe oder Tag ist null");
				}

				//Verweis erzeugen EinnahmeAusgabe <> Tag
				_Context.EinnahmeAusgabeTags.Add(new EinnahmeAusgabeTag() { EinnahmeAusgabeId = einnahmeAusgabe.Id, TagId = tag.Id });

				_Context.SaveChanges();
			}
			catch (Exception ex)
			{
				//TODO Exception loggen
				throw ex;
			}

		}

		/// <summary>
		/// 
		/// </summary>
		public void AddTagToEinnahmeAusgabe(long einnahmeAusgabeId, long tagId, FinanceAppUser user)
		{
			var einnahmeAusgabe = _Context.EinnahmenAusgaben.Where(ea => ea.Id == einnahmeAusgabeId).FirstOrDefault();
			var tag = _Context.Tags.Where(t => t.Id == tagId).FirstOrDefault();

			if (einnahmeAusgabe == null || tag == null) return;
			this.AddTagToEinnahmeAusgabe(einnahmeAusgabe, tag, user);
		}

		public IList<EinnahmeAusgabe> GetEinnahmeAusgabe(IList<Tag> listTags)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// TODO Exception-Handling einbauen
		/// </summary>
		/// <param name="tagId"></param>
		/// <returns></returns>
		public IList<EinnahmeAusgabe> GetEinnahmeAusgabe(long tagId)
		{
			if (tagId <= 0) return null;

			return _Context.EinnahmenAusgaben
				.Include(ea => ea.EinnahmenAusgabenTags)
				.Where(ea => ea.EinnahmenAusgabenTags.Select(t => t.TagId).Contains(tagId)).ToList();
		}

		/// <summary>
		/// TODO Exception-Handling einbauen
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		public IList<EinnahmeAusgabe> GetEinnahmenAusgaben(Tag tag)
		{
			if (tag == null) return null;

			return GetEinnahmeAusgabe(tag.Id);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tagId"></param>
		/// <returns></returns>
		public Tag GetTag(long tagId)
		{
			try
			{
				return _Context.Tags.Where(t => t.Id == tagId).FirstOrDefault();
			}
			catch (Exception ex)
			{
				//Fehler loggen
				throw ex;
			}
		}

		/// <summary>
		/// Liest alle Tags zu einer Einnahme/Ausgabe aus
		/// </summary>
		public IEnumerable<Tag> GetTags(long einnahmeAusgabeId)
		{
			try
			{
				var einnahmeAusgabe = _Context.EinnahmenAusgaben.Where(ae => ae.Id == einnahmeAusgabeId).FirstOrDefault();

				if (einnahmeAusgabe == null)
					return new List<Tag>();

				return einnahmeAusgabe.EinnahmenAusgabenTags.Select(eat => eat.Tag);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>
		/// Alle Tags die einem User zugeordnet sind
		/// </summary>
		/// <returns></returns>
		public IEnumerable<Tag> GetUserTags(FinanceAppUser user)
		{
			try
			{
				return _Context.Tags
					.Where(t => t.User == user);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public Tag AddUserTag(FinanceAppUser user, Tag newTag)
		{
			try
			{
				newTag.User = user;

				//Falls Tag schon vorhanden dann wird er nicht noch einmal abgespeichert
				if (_Context.Tags.Any(t => t.TagBenennung.Equals(newTag.TagBenennung, StringComparison.OrdinalIgnoreCase)
				&& t.User == user))
					return null;

				_Context.Tags.Add(newTag);
				_Context.SaveChanges();

				return newTag;
			}
			catch (Exception ex)
			{
				//TODO Exception loggen
				throw ex;
			}
		}

		/// <summary>
		/// Suche nach User-Custom-Tags welche den übergebenen String im Namen enthalten
		/// </summary>
		/// <param name="user"></param>
		/// <param name="tagName"></param>
		/// <returns></returns>
		IEnumerable<Tag> ITagsRepository.SearchUserTags(FinanceAppUser user, string tagName)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Entfernt Tag aus der Datenbank...wenn erfolgreich wird true zurückgegeben
		/// </summary>
		/// <param name="tagId"></param>
		/// <returns></returns>
		public bool RemoveTag(FinanceAppUser user, long tagId)
		{
			try
			{
				var tag2Remove = _Context.Tags.Where(t => t.Id == tagId && t.User == user).FirstOrDefault();

				if (tag2Remove == null)
					return false;

				//Verweise Finanzeintrag und Tag müssen gelöscht werden
				var finTags2Remove = _Context.EinnahmeAusgabeTags.Where(eat => eat.TagId == tagId);

				_Context.Remove(tag2Remove);

				if (finTags2Remove != null && finTags2Remove.Count() > 0)
					_Context.EinnahmeAusgabeTags.RemoveRange(finTags2Remove.ToList());

				_Context.SaveChanges();
				return true;
			}
			catch (Exception ex)
			{
				//loggen der Exception
				throw ex;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tagListe"></param>
		public void UpdateTagListeEinnahmeAusgabe(EinnahmeAusgabe updateEinnahmeAusgabe, IList<Tag> tagListe)
		{
			try
			{
				var listEAT = _Context.EinnahmeAusgabeTags
					.Include(eat => eat.Tag)
					.Include(eat => eat.EinnahmeAusgabe)
					.Where(eat => eat.EinnahmeAusgabeId == updateEinnahmeAusgabe.Id).ToList();

				//Ist die Tag-Liste noch aktuell
				if (!_Context.Tags.Any(t => tagListe.Contains(t)))
				{
					//Einige Tageinträge anscheinend nicht mehr aktuelle -> löschen
					for (int i = (tagListe.Count - 1); i >= 0; i--)
					{
						if (!_Context.Tags.Select(t => t.Id).Any(t => t == tagListe[i].Id))
							tagListe.RemoveAt(i);
					}
				}

				//Vergleichen ob alte und neu Tagliste gleich ist
				var altTagListe = listEAT.Select(eat => eat.Tag).OrderBy(t => t.Id).ToList();
				var neuTagListe = tagListe.OrderBy(t => t.Id).ToList();

				//Tags die hinzugekommen sind
				foreach (var neuTag in neuTagListe)
				{
					if (!altTagListe.Any(t => t.Id == neuTag.Id))
					{
						//neuen Tag hinzufügen da noch nicht vorhanden
						_Context.EinnahmeAusgabeTags.Add(new EinnahmeAusgabeTag()
						{
							EinnahmeAusgabe = updateEinnahmeAusgabe,
							TagId = neuTag.Id
						});
					}
				}

				//Tags die entfernt wurden
				foreach (var altTag in altTagListe)
				{
					if (!neuTagListe.Any(t => t.Id == altTag.Id))
					{
						//alten Tag entfernen da nicht mehr vorhanden
						var deleteEAT = _Context.EinnahmeAusgabeTags
							.Where(eat => eat.EinnahmeAusgabeId == updateEinnahmeAusgabe.Id)
							.Where(eat => eat.TagId == altTag.Id).FirstOrDefault();

						if (deleteEAT != null)
							_Context.EinnahmeAusgabeTags.Remove(deleteEAT);
					}
				}

				_Context.SaveChanges();
			}
			catch (Exception ex)
			{
				//Exception loggen
				throw ex;
			}

		}
	}

	public interface ITagsRepository
	{
		/// <summary>
		/// Alle Tags die einem User zugeordnet sind
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		IEnumerable<Tag> GetUserTags(FinanceAppUser user);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="username"></param>
		/// <param name="tagName"></param>
		/// <returns></returns>
		IEnumerable<Tag> SearchUserTags(FinanceAppUser user, string tagName);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tagListe"></param>
		void UpdateTagListeEinnahmeAusgabe(EinnahmeAusgabe updateEinnahmeAusgabe, IList<Tag> tagListe);

		/// <summary>
		/// zu einer TagId werden alle Einnahmen/Ausgaben gesucht
		/// </summary>
		/// <param name="tagId"></param>
		/// <returns></returns>
		IList<EinnahmeAusgabe> GetEinnahmeAusgabe(long tagId);

		/// <summary>
		/// Alle EinnnahmenAusgaben mit den entsprechenden Tag
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		IList<EinnahmeAusgabe> GetEinnahmenAusgaben(Tag tag);

		/// <summary>
		/// Liest den Tag zu einer Tag-Id aus
		/// </summary>
		/// <param name=""></param>
		/// <returns></returns>
		Tag GetTag(long tagId);

		/// <summary>
		/// Liest alle Tags zu einer Einnahme oder Ausgabe aus
		/// </summary>
		IEnumerable<Tag> GetTags(long einnahmeAusgabeId);


		/// <summary>
		/// Alle EinnahmenAusgaben die die Liste an Tags enthalten
		/// </summary>
		/// <param name="listTags"></param>
		/// <returns></returns>
		IList<EinnahmeAusgabe> GetEinnahmeAusgabe(IList<Tag> listTags);

		/// <summary>
		/// Tag an eine Einnahme/Ausgabe anhängen
		/// </summary>
		void AddTagToEinnahmeAusgabe(EinnahmeAusgabe einnahmeAusgabe, Tag tag, FinanceAppUser user);

		/// <summary>
		/// Tag an einen Einnahme/Ausgabe anhängen
		/// </summary>
		void AddTagToEinnahmeAusgabe(long einnahmeAusgabeId, long tagID, FinanceAppUser user);

		Tag AddUserTag(FinanceAppUser user, Tag newTag);

		bool RemoveTag(FinanceAppUser user, long tagId);
	}
}
