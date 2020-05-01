using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Finance.Core.WebApi.ViewModels
{
	public class EinnahmeAusgabeViewModel
	{
		public int Id { get; set; }

		public string Benennung { get; set; }

		[Required]
		[MinLength(10)]
		public string BenennungKurz { get; set; }

		[Required]
		public DateTime Buchungsdatum { get; set; }

		/// <summary>
		/// Buchungsdatum in string-Format um das Senden zu vereinfachen
		/// </summary>
		public string BuchungsdatumJson { get; set; }

		/// <summary>
		/// Betrag immer in Hauptwährung, beim einigen Einnahmen/Ausgaben muss dieser errechnet werden
		/// </summary>
		[Required]
		public decimal BetragHauptwaehrung { get; set; }

		public decimal BetragFremdwaehrung { get; set; }

		/// <summary>
		/// Gibt an ob die Einnahme Ausgabe wiederkehrend ist
		/// </summary>
		public bool Wiederkehrend { get; set; }

		/// <summary>
		/// In welchen Zeitrhythmus der Finanzeintrag sich wiederholt
		/// </summary>
		public int RhythmusWiederkehrend { get; set; }

		/// <summary>
		/// Wenn der Finanzeintrag wiederkehrend ist dann hier das Datum bis zu welchen Zeitpunkt
		/// </summary>
		public DateTime EnddatumWiederkehrend { get; set; }

		/// <summary>
		/// Wenn der Finanzeintrag wiederkehrend ist dann hier das Datum bis zu welchen Zeitpunkt
		/// (als string-Format um das Senden zu vereinfachen)
		/// </summary>
		public string EnddatumWiederkehrendJson { get; set; }

		/// <summary>
		/// Flag gibt an ob Eintrag steuerlich absetzbar ist
		/// </summary>
		public bool SteuerlichAbsetzbar { get; set; }

		/// <summary>
		/// Betrag der steuerlich absetzbar ist (sollte nicht größer als Betrag sein)
		/// </summary>
		public decimal BetragSteuerlichAbsetzbar { get; set; }

		/// <summary>
		/// Zusätzliche Bemerkungen bezüglich der steuerlich Absetzbarkeit
		/// </summary>
		public string BemerkungSteuerlichAbsetzbar { get; set; }

		public IList<TagViewModel> ListTags { get; set; } = new List<TagViewModel>();

		//Verweis auf die hochgeladenen Bilder
		public IList<ImageMetaViewModel> ListImages { get; set; } = new List<ImageMetaViewModel>();
	}
}
