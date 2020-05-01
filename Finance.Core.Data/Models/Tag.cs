using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Finance.Core.Data.Models
{
	public class Tag
	{
		[Key]
		public long Id { get; set; }

		[Required]
		[MinLength(3)]
		public string TagBenennung { get; set; }

		#region navigation properties
		/// <summary>
		/// Alle Einnahmen/Ausgaben den der Tag zugeordnet ist
		/// </summary>
		public virtual ICollection<EinnahmeAusgabeTag> EinnahmenAusgabenTags { get; set; }

		/// <summary>
		/// Tags sind personalisiert
		/// </summary>
		public FinanceAppUser User { get; set; }
		#endregion

	}
}
