using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Finance.Core.Data.Models
{
	public class FinanceImage
	{
		public long Id { get; set; }

		//Datei-Pfad wo Bild abgelegt wird
		public string Path { get; set; }

		//Url um auf Bild zugreifen zu können
		public string Url { get; set; }

		public DateTime UploadDate { get; set; }

		public string ImageName { get; set; }


		#region Navigation Properties
		public long? EinnahmeAusgabeId { get; set; }

		[ForeignKey("EinnahmeAusgabeId")]
		public EinnahmeAusgabe Finance { get; set; }

		public FinanceAppUser User { get; set; }
		#endregion
	}
}
