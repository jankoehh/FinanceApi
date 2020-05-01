using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Finance.Core.Data.Models
{
	public class EinnahmeAusgabe
	{
		#region data properties
		[Key]
		public long Id { get; set; }

		public string Benennung { get; set; }

		[MaxLength(80)]
		public string BenennungKurz { get; set; }

		public DateTime Buchungsdatum { get; set; }

		/// <summary>
		/// Betrag immer in Hauptwährung, beim einigen Einnahmen/Ausgaben muss dieser errechnet werden
		/// </summary>
		[Column(TypeName = "decimal(16,2)")]
		public decimal BetragHauptwaehrung { get; set; }

		[Column(TypeName = "decimal(16,2)")]
		public decimal BetragFremdwaehrung { get; set; }

		/// <summary>
		/// Gibt an ob die Einnahme Ausgabe wiederkehrend ist
		/// </summary>
		public bool Wiederkehrend { get; set; }

		/// <summary>
		/// Flag gibt an ob Eintrag steuerlich absetzbar ist
		/// </summary>
		public bool SteuerlichAbsetzbar { get; set; }

		/// <summary>
		/// Betrag der steuerlich absetzbar ist (sollte nicht größer als Betrag sein)
		/// </summary>
		[Column(TypeName = "decimal(16,2)")]
		public decimal BetragSteuerlichAbsetzbar { get; set; }

		/// <summary>
		/// Zusätzliche Bemerkungen bezüglich der steuerlich Absetzbarkeit
		/// </summary>
		public string BemerkungSteuerlichAbsetzbar { get; set; }
		#endregion


		#region navigation properties
		//Zum Finanzeintrag hinterlegte Bilder (eingescannte Rechnungen)
		public IList<FinanceImage> Bilder { get; set; }

		public IList<Anhang> Anhaenge { get; set; }

		//Liste an Tags die einer EinnahmeAusgabe zugeordnet sind
		public ICollection<EinnahmeAusgabeTag> EinnahmenAusgabenTags { get; set; }

		public long? HauptWaehrungId { get; set; }
		[ForeignKey("HauptWaehrungId")]
		public Waehrung HauptWaehrung { get; set; }

		public long? FremdWaehrungId { get; set; }
		[ForeignKey("FremdWaehrungId")]
		public Waehrung FremdWaehrung { get; set; }

		/// <summary>
		/// EinnahmeAusgabe ist dem User zugeordnet
		/// </summary>
		public FinanceAppUser User { get; set; }
		#endregion


		#region Hilfsproperties die nicht gemappt werden
		[NotMapped]
		public IList<Tag> Tags { get; set; } = new List<Tag>();
		#endregion
	}
}
