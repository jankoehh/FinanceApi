using Finance.Core.Services.Exceptions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Finance.Core.Services.FileServices
{
	public class CheckDateiGleichBildService : ICheckDateiGleichBildService
	{
		public string GetFileExtension(IFormFile datei)
		{
			try
			{
				byte[] fileBytes;
				using (var ms = new MemoryStream())
				{
					datei.CopyTo(ms);
					fileBytes = ms.ToArray();
				}

				if (GetImageFormat(fileBytes) == ImageFormat.unknown)
					throw new NotImageException("Übergebene Datei ist kein Bild, es können nur Bilder abgelegt werden!");

				//TEST TEST TEST TEST
				var test = GetImageFormat(fileBytes).ToString();

				return GetImageFormat(fileBytes).ToString();
			}
			catch (NotImageException notImgEx)
			{
				throw notImgEx;
			}
			catch (Exception ex)
			{
				//TODO Fehler loggen
				throw ex;
			}

		}

		public bool CheckenDateiIstBild(IFormFile datei)
		{
			try
			{
				byte[] fileBytes;
				using (var ms = new MemoryStream())
				{
					datei.CopyTo(ms);
					fileBytes = ms.ToArray();
				}

				return GetImageFormat(fileBytes) != ImageFormat.unknown;
			}
			catch (Exception ex)
			{
				//TODO Fehler loggen
				throw ex;
			}
		}

		private enum ImageFormat
		{
			bmp,
			jpeg,
			gif,
			tiff,
			png,
			unknown
		}

		private ImageFormat GetImageFormat(byte[] bytes)
		{
			var bmp = Encoding.ASCII.GetBytes("BM");     // BMP
			var gif = Encoding.ASCII.GetBytes("GIF");    // GIF
			var png = new byte[] { 137, 80, 78, 71 };              // PNG
			var tiff = new byte[] { 73, 73, 42 };                  // TIFF
			var tiff2 = new byte[] { 77, 77, 42 };                 // TIFF
			var jpeg = new byte[] { 255, 216, 255, 224 };          // jpeg
			var jpeg2 = new byte[] { 255, 216, 255, 225 };         // jpeg canon

			if (bmp.SequenceEqual(bytes.Take(bmp.Length)))
				return ImageFormat.bmp;

			if (gif.SequenceEqual(bytes.Take(gif.Length)))
				return ImageFormat.gif;

			if (png.SequenceEqual(bytes.Take(png.Length)))
				return ImageFormat.png;

			if (tiff.SequenceEqual(bytes.Take(tiff.Length)))
				return ImageFormat.tiff;

			if (tiff2.SequenceEqual(bytes.Take(tiff2.Length)))
				return ImageFormat.tiff;

			if (jpeg.SequenceEqual(bytes.Take(jpeg.Length)))
				return ImageFormat.jpeg;

			if (jpeg2.SequenceEqual(bytes.Take(jpeg2.Length)))
				return ImageFormat.jpeg;

			return ImageFormat.unknown;
		}
	}

	public interface ICheckDateiGleichBildService
	{
		string GetFileExtension(IFormFile datei);

		bool CheckenDateiIstBild(IFormFile datei);
	}
}
