using Finance.Core.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Finance.Core.Data.Repositories
{
	public class EinnahmenAusgabenRepository : IEinnahmenAusgabenRepository
	{
		private readonly FinanceContext _Context;
		//private readonly ILogger<IEinnahmenAusgabenRepository> _Logger;

		public EinnahmenAusgabenRepository(FinanceContext context)
		{
			_Context = context;
		}

		public EinnahmeAusgabe DeleteEinnahmeAusgabe(long einnahmeAusgabeId)
		{
			var delEinnahmeAusgabeBilder = _Context.EinnahmeAusgabeBilder.Where(b => b.EinnahmeAusgabeId == einnahmeAusgabeId);
			var delEinnahmeAusgabe = _Context.EinnahmenAusgaben
				.Where(f => f.Id == einnahmeAusgabeId)
				.FirstOrDefault();
			var delTags = _Context.EinnahmeAusgabeTags.Where(t => t.EinnahmeAusgabeId == einnahmeAusgabeId);

			if (delEinnahmeAusgabe == null)
				return null;

			//erst die Bilder (aus der Verweis-Tabelle) entfernen
			if (delEinnahmeAusgabeBilder != null)
				foreach (var delBild in delEinnahmeAusgabeBilder)
					_Context.EinnahmeAusgabeBilder.Remove(delBild);

			//anschließend die Tags entfernen
			if (delTags != null)
				foreach (var delTag in delTags)
					_Context.EinnahmeAusgabeTags.Remove(delTag);

			_Context.EinnahmenAusgaben.Remove(delEinnahmeAusgabe);
			_Context.SaveChanges();

			return delEinnahmeAusgabe;
		}

		public EinnahmeAusgabe AddNeueEinnahmeAusgabe(EinnahmeAusgabe neueEinnahmeAusgabe)
		{
			if (neueEinnahmeAusgabe == null) return null;
			if (neueEinnahmeAusgabe.Buchungsdatum == null || neueEinnahmeAusgabe.Buchungsdatum == DateTime.MinValue) return null;

			try
			{
				_Context.EinnahmenAusgaben.Add(neueEinnahmeAusgabe);
				_Context.SaveChanges();

				return neueEinnahmeAusgabe;
			}
			catch (Exception ex)
			{
			}
			return null;
		}

		public EinnahmeAusgabe GetEinnahmeAusgabeDetail(FinanceAppUser user, long Id)
		{
			if (Id <= 0) return null;

			//Wenn keine Tags vorhanden sind
			var finEntry = _Context.EinnahmenAusgaben
				.Include(f => f.EinnahmenAusgabenTags)
				.ThenInclude(ft => ft.Tag)
				.Where(f => f.User == user)
				.Where(f => f.Id == Id)
				.FirstOrDefault();

			//Tags anhängen
			if (finEntry.EinnahmenAusgabenTags != null && finEntry.EinnahmenAusgabenTags.Count > 0)
				finEntry.Tags = finEntry.EinnahmenAusgabenTags.Select(t => t.Tag).ToList();

			return finEntry;
		}

		/// <summary>
		/// Alle Einnahmen und Ausgaben vom Datum her absteiegend
		/// defaultmässig nur die ersten 200 Einträge
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="maxAnzahl"></param>
		/// <returns></returns>
		public IList<EinnahmeAusgabe> GetEinnahmenAusgaben(string userName, int maxAnzahl = 200)
		{
			var einnahmenAusgaben = _Context.EinnahmenAusgaben
				.Where(ea => ea.User.Email.Equals(userName, StringComparison.OrdinalIgnoreCase) || ea.User.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase))
				.Include(ea => ea.Anhaenge)
				.Include(ea => ea.EinnahmenAusgabenTags)
				.ThenInclude(eat => eat.Tag)
				.OrderByDescending(ea => ea.Buchungsdatum).ToList();

			if (einnahmenAusgaben.Count > maxAnzahl)
				einnahmenAusgaben = einnahmenAusgaben.Take(maxAnzahl).ToList();

			return einnahmenAusgaben;
		}

		public IList<EinnahmeAusgabe> GetEinnahmenAusgaben(string userName, DateTime vonDate, DateTime bisDate)
		{
			if (vonDate == null) vonDate = DateTime.MinValue;

			if (bisDate == null) bisDate = DateTime.MaxValue;

			return _Context.EinnahmenAusgaben
				.Where(ea => ea.User.Email.Equals(userName, StringComparison.OrdinalIgnoreCase) || ea.User.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase))
				.Include(ea => ea.Anhaenge)
				.Include(ea => ea.EinnahmenAusgabenTags)
				.Where(ea => ea.Buchungsdatum >= vonDate && ea.Buchungsdatum <= bisDate).ToList();
		}

		/// <summary>
		/// Alle Finanzeinträge die einen User zugeordnet sind und mindestens einen der Tags enthalten
		/// </summary>
		/// <param name="user"></param>
		/// <param name="listFilterTags"></param>
		/// <returns></returns>
		public IList<EinnahmeAusgabe> GetEinnahmenAusgaben(FinanceAppUser user, IEnumerable<Tag> listFilterTags)
		{
			try
			{
				var einnahmenAusgabenGefiltert = _Context.EinnahmeAusgabeTags
					.Include(eat => eat.EinnahmeAusgabe)
					.Include(eat => eat.Tag)
					.Where(eat => eat.EinnahmeAusgabe.User == user)
					.Where(eat => listFilterTags.Contains(eat.Tag))
					.Select(eat => eat.EinnahmeAusgabe)
					.Include(ea => ea.EinnahmenAusgabenTags)
					.ThenInclude(eat => eat.Tag)
					.OrderByDescending(ea => ea.Buchungsdatum)
					.Distinct()
					.ToList();

				foreach (var einAusgabe in einnahmenAusgabenGefiltert)
				{
					einAusgabe.Tags = einAusgabe.EinnahmenAusgabenTags.Select(eat => eat.Tag).ToList();
				}

				return einnahmenAusgabenGefiltert;
			}
			catch (Exception ex)
			{
				//TODO Exception loggen
				throw ex;
			}
		}

		public EinnahmeAusgabe UpdateEinnahmeAusgabe(EinnahmeAusgabe neuEinnahmeAusgabe)
		{
			try
			{
				if (neuEinnahmeAusgabe == null) return neuEinnahmeAusgabe;

				var altEinnahmeAusgabe = _Context.EinnahmenAusgaben.FirstOrDefault(ae => ae.Id == neuEinnahmeAusgabe.Id);

				altEinnahmeAusgabe.Anhaenge = neuEinnahmeAusgabe.Anhaenge;
				altEinnahmeAusgabe.Benennung = neuEinnahmeAusgabe.Benennung;
				altEinnahmeAusgabe.BenennungKurz = neuEinnahmeAusgabe.BenennungKurz;
				altEinnahmeAusgabe.BetragFremdwaehrung = neuEinnahmeAusgabe.BetragFremdwaehrung;
				altEinnahmeAusgabe.BetragHauptwaehrung = neuEinnahmeAusgabe.BetragHauptwaehrung;
				altEinnahmeAusgabe.Buchungsdatum = neuEinnahmeAusgabe.Buchungsdatum;
				altEinnahmeAusgabe.FremdWaehrung = neuEinnahmeAusgabe.FremdWaehrung;
				altEinnahmeAusgabe.HauptWaehrung = neuEinnahmeAusgabe.HauptWaehrung;
				altEinnahmeAusgabe.Wiederkehrend = neuEinnahmeAusgabe.Wiederkehrend;

				_Context.Update(altEinnahmeAusgabe);

				//aktualisierter Eintrag wird zurückgegeben
				return _Context.EinnahmenAusgaben.Where(ea => ea.Id == altEinnahmeAusgabe.Id).FirstOrDefault();
			}
			catch (Exception ex)
			{
				//Exception loggen
				throw ex;
			}

		}
	}

	public interface IEinnahmenAusgabenRepository
	{
		/// <summary>
		/// Löscht den Finanzeintrag mit der entsprechenden Id
		/// </summary>
		/// <param name="einnahmeAusgabeId"></param>
		/// <returns></returns>
		EinnahmeAusgabe DeleteEinnahmeAusgabe(long einnahmeAusgabeId);

		/// <summary>
		/// Alle Einnahmen und Ausgaben vom Datum her absteiegend
		/// defaultmässig nur die ersten 200 Einträge
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="maxAnzahl"></param>
		/// <returns></returns>
		IList<EinnahmeAusgabe> GetEinnahmenAusgaben(string userName, int maxAnzahl = 200);

		/// <summary>
		/// Alle Einnahmen und Ausgaben die zwischen einen bestimmten Datums-Spanne liegen
		/// die zu einen bestimmten User gehören
		/// </summary>
		IList<EinnahmeAusgabe> GetEinnahmenAusgaben(string userName, DateTime vonDate, DateTime bisDate);

		/// <summary>
		/// Einnahme/Ausgabe
		/// </summary>
		EinnahmeAusgabe GetEinnahmeAusgabeDetail(FinanceAppUser user, long Id);

		/// <summary>
		/// Einnahme/Ausgabe die einen der Tags enthalten
		/// </summary>
		/// <param name="user"></param>
		/// <param name="listFilterTags"></param>
		/// <returns></returns>
		IList<EinnahmeAusgabe> GetEinnahmenAusgaben(FinanceAppUser user, IEnumerable<Tag> listFilterTags);

		/// <summary>
		/// Neue Einnahme/Ausgabe in Datenbank speichern
		/// </summary>
		EinnahmeAusgabe AddNeueEinnahmeAusgabe(EinnahmeAusgabe neueEinnahmeAusgabe);

		/// <summary>
		/// Update einer Einnahme/Ausgabe
		/// </summary>
		EinnahmeAusgabe UpdateEinnahmeAusgabe(EinnahmeAusgabe neuEinnahmeAusgabe);
	}
}
