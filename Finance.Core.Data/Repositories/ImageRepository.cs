using Finance.Core.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Finance.Core.Data.Repositories
{
	public class ImageRepository : IImageRepository
	{
		private readonly FinanceContext _Context;

		public ImageRepository(FinanceContext context)
		{
			_Context = context;
		}

		public FinanceImage AddNeuImage(FinanceImage neuImage, FinanceAppUser user)
		{
			try
			{
				neuImage.User = user;
				_Context.EinnahmeAusgabeBilder.Add(neuImage);
				_Context.SaveChanges();

				return neuImage;
			}
			catch (Exception ex)
			{
				//TODO Fehler loggen
				throw ex;
			}
		}

		public void AssignImage2EinnahmeAusgabe(long imageId, EinnahmeAusgabe einnahmeAusgabe)
		{
			try
			{
				var image = _Context.EinnahmeAusgabeBilder.Where(b => b.Id == imageId).FirstOrDefault();

				if (image == null)
					new Exception("Bild wurde nicht in der Datenbank gefunden!");

				//Falls Bild schon an FinanceEIntrag angehangen wurde brauch es nicht noch einmal angehangen werden
				if (image.Finance != null && image.Finance.Id == einnahmeAusgabe.Id)
					return;

				image.Finance = einnahmeAusgabe;

				_Context.EinnahmeAusgabeBilder.Update(image);
				_Context.SaveChanges();

			}
			catch (Exception ex)
			{
				//TODO Fehler loggen
				throw ex;
			}
		}

		public IList<FinanceImage> GetEinnahmeAusgabeBilder(EinnahmeAusgabe einnahmeAusgabe, FinanceAppUser user)
		{
			try
			{
				return _Context.EinnahmeAusgabeBilder.Where(b => b.Finance == einnahmeAusgabe && b.User == user).ToList();
			}
			catch (Exception ex)
			{
				//TODO Fehler loggen
				throw ex;
			}
		}

		public string GetImageSource(long imageId, FinanceAppUser user)
		{
			try
			{
				var finImage = _Context.EinnahmeAusgabeBilder.Where(eab => eab.User == user && eab.Id == imageId).FirstOrDefault();

				if (finImage == null)
					return string.Empty;

				return finImage.Path;
			}
			catch (Exception ex)
			{
				//TODO Fehler loggen
				throw ex;
			}
		}

		public IList<FinanceImage> DeleteImages(IList<long> imageIds4Delete)
		{
			try
			{
				IList<FinanceImage> deletedImages = new List<FinanceImage>();

				foreach (var imageId in imageIds4Delete)
				{
					var deletedImage = _Context.EinnahmeAusgabeBilder.Where(b => b.Id == imageId).FirstOrDefault();

					if (deletedImage == null)
						continue;

					_Context.EinnahmeAusgabeBilder.Remove(deletedImage);

					deletedImages.Add(deletedImage);
				}

				_Context.SaveChanges();

				return deletedImages;
			}
			catch (Exception ex)
			{
				//TODO Fehler loggen
				throw ex;
			}
		}

		public IList<FinanceImage> DeleteImages(EinnahmeAusgabe einnahmeAusgabe)
		{
			try
			{
				return this.DeleteImages(einnahmeAusgabe.Id);
			}
			catch (Exception ex)
			{
				//Fehler brauch hier nicht geloggt werden, wenn Fehler auftritt wird er in der Methode zuvor geloggt
				throw ex;
			}
		}

		public IList<FinanceImage> DeleteImages(long einnahmeAusgabeId)
		{
			try
			{
				IList<FinanceImage> deletedImages = new List<FinanceImage>();

				deletedImages = _Context.EinnahmeAusgabeBilder.Where(b => b.EinnahmeAusgabeId == einnahmeAusgabeId).ToList();

				if (deletedImages == null)
					return new List<FinanceImage>();

				foreach (var delBild in deletedImages)
				{
					_Context.EinnahmeAusgabeBilder.Remove(delBild);
				}
				_Context.SaveChanges();

				return deletedImages;
			}
			catch (Exception ex)
			{
				//TODO Fehler loggen (direkt in der Datenbank)
				throw ex;
			}
		}
	}

	public interface IImageRepository
	{
		FinanceImage AddNeuImage(FinanceImage neuImage, FinanceAppUser user);

		void AssignImage2EinnahmeAusgabe(long imageId, EinnahmeAusgabe einnahmeAusgabe);

		string GetImageSource(long imageId, FinanceAppUser user);

		IList<FinanceImage> DeleteImages(IList<long> imageIds4Delete);

		IList<FinanceImage> DeleteImages(EinnahmeAusgabe einnahmeAusgabe);

		IList<FinanceImage> DeleteImages(long einnahmeAusgabeId);

		IList<FinanceImage> GetEinnahmeAusgabeBilder(EinnahmeAusgabe einnahmeAusgabe, FinanceAppUser user);
	}
}
