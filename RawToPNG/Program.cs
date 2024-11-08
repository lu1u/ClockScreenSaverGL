using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace RawToPNG
{
    internal class Program
    {
        /// <summary>
		/// Transforme un tableau de bytes: 1 octet par pixel, niveaux de gris
		/// en bitmap
		/// </summary>
		/// <param name="Raw"></param>
		/// <param name="Largeur"></param>
		/// <param name="Hauteur"></param>
		/// <returns></returns>
		private static unsafe Bitmap RawToBitmap(byte[] Raw, int Largeur, int Hauteur, PixelFormat format)
        {
            Bitmap bm = new Bitmap(Largeur, Hauteur, format);
            unsafe
            {
                fixed (byte* debut = Raw)
                {
                    byte* pointeur = debut;
                    for (int y = 0; y < Hauteur; y++)
                    {
                        Console.WriteLine(y + "/" + Hauteur);
                        for (int x = 0; x < Largeur; x++)
                        {
                            bm.SetPixel(x, y, Color.FromArgb(*pointeur, *pointeur, *pointeur));
                            pointeur++;
                        }
                    }
                }
            }
            return bm;
        }

        /// <summary>
        /// Converti un fichier RAW en fichier PNG
        /// L'image DOIT etre carree
        /// </summary>
        /// <param name="fichierRaw"></param>
        /// <param name="fichierPNG"></param>
        private static void RawFileToPNG(string fichierRaw, string fichierPNG)
        {
            byte[] bytes = File.ReadAllBytes(fichierRaw);
            int Largeur, Hauteur;

            // L'image DOIT etre carree
            Largeur = Hauteur = (int)Math.Sqrt(bytes.Length);

            Bitmap bm = RawToBitmap(bytes, Largeur, Hauteur, PixelFormat.Format24bppRgb);
            bm.Save(fichierPNG, ImageFormat.Png);
        }

        private static void Main(string[] args)
        {
            string cheminRaw = args[0];
            string cheminPng = Path.ChangeExtension(cheminRaw, "png");
            RawFileToPNG(cheminRaw, cheminPng);
        }
    }
}
