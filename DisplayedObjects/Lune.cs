/*
 * Crée par SharpDevelop.
 * Utilisateur: lucien
 * Date: 18/11/2014
 * Heure: 14:12
 * 
 * Pour changer ce modèle utiliser Outils  Options  Codage  Editer les en-têtes standards.
 */

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ClockScreenSaverGL
{
    /// <summary>
    /// Description of Lune.
    /// </summary>
    public class Lune
    {
        private int _ageLune = -1;
        DateTime _maintenant;
		byte ALPHA_AIGUILLES;

		public Lune(byte alpha)
		{
			ALPHA_AIGUILLES = alpha;
		}
        private static int JulianDate(int d, int m, int y)
        {
            int mm, yy;
            int k1, k2, k3;
            int j;

            yy = y - (int)((12 - m) / 10);
            mm = m + 9;

            if (mm >= 12)
            {
                mm = mm - 12;
            }
            k1 = (int)(365.25 * (yy + 4712));
            k2 = (int)(30.6001 * mm + 0.5);
            k3 = (int)((int)((yy / 100) + 49) * 0.75) - 38;
            // 'j' for dates in Julian calendar:
            j = k1 + k2 + d + 59;
            if (j > 2299160)
            {
                // For Gregorian calendar:
                j = j - k3; // 'j' is the Julian date at 12h UT (Universal Time)
            }
            return j;
        }

        private static double MoonAge(int d, int m, int y)
        {
            int j = JulianDate(d, m, y);
            //Calculate the approximate phase of the moon
            double ip = (j + 4.867) / 29.53059;
            ip = ip - Math.Floor(ip);
            return ip;
        }
        public static double CalcMoonAge(DateTime dDate)
        {
            double fJD, fIP, fAge;

            fJD = JulianDate(dDate.Day, dDate.Month, dDate.Year);
            fIP = Normalize((fJD - 2451550.1) / 29.530588853);
            fAge = fIP * 29.530588853;
            return fAge;
        }

        private static double Normalize(double fN)
        {
            fN = fN - Math.Floor(fN);
            if (fN < 0)
            {
                fN = fN + 1;
            }
            return fN;
        }

        public String Dump()
        {
            return "Lune " + _ageLune + "/" + CalcMoonAge(_maintenant);
        }

        public Bitmap getImageLune(Graphics g, DateTime maintenant)
        {
            _maintenant = maintenant;
            int lune = (int)Math.Round(CalcMoonAge(_maintenant) / 29.530588853 * 26);

            string nomFichier = Path.Combine(Config.Configuration.getImagesDirectory() + "\\Lunes");
			nomFichier += "\\Lune" + lune.ToString("D2") + ".png";
			Image bmp = Image.FromFile(nomFichier);
            //switch (lune)
            //{
            //    case 0: bmp = Ressources.Lune00; break;
            //    case 1: bmp = Ressources.Lune01; break;
            //    case 2: bmp = Ressources.Lune02; break;
            //    case 3: bmp = Ressources.Lune03; break;
            //    case 4: bmp = Ressources.Lune04; break;
            //    case 5: bmp = Ressources.Lune05; break;
            //    case 6: bmp = Ressources.Lune06; break;
            //    case 7: bmp = Ressources.Lune07; break;
            //    case 8: bmp = Ressources.Lune08; break;
            //    case 9: bmp = Ressources.Lune09; break;
            //    case 10: bmp = Ressources.Lune10; break;
            //    case 11: bmp = Ressources.Lune11; break;
            //    case 12: bmp = Ressources.Lune12; break;
            //    case 13: bmp = Ressources.Lune13; break;
            //    case 14: bmp = Ressources.Lune14; break;
            //    case 15: bmp = Ressources.Lune15; break;
            //    case 16: bmp = Ressources.Lune16; break;
            //    case 17: bmp = Ressources.Lune17; break;
            //    case 18: bmp = Ressources.Lune18; break;
            //    case 19: bmp = Ressources.Lune19; break;
            //    case 20: bmp = Ressources.Lune20; break;
            //    case 21: bmp = Ressources.Lune21; break;
            //    case 22: bmp = Ressources.Lune22; break;
            //    case 23: bmp = Ressources.Lune23; break;
            //    case 24: bmp = Ressources.Lune24; break;
            //    case 25: bmp = Ressources.Lune25; break;
            //    default: bmp = Ressources.Lune00; break;
            //}
            Bitmap bmpRes;

            // Rendre cette bitmap conforme à la transparence de l'horloge
            if (g != null)
                bmpRes = new Bitmap(bmp.Width, bmp.Height, g);
            else
                bmpRes = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format32bppRgb);
            using (Graphics gMem = Graphics.FromImage(bmpRes))
            {

                float[][] ptsArray =
                {
                new float[] {1, 0, 0, 0, 0},
                new float[] {0, 1, 0, 0, 0},
                new float[] {0, 0, 1, 0, 0},
                new float[] {0, 0, 0, ALPHA_AIGUILLES/255.0f, 0},
                new float[] {0, 0, 0, 0, 1}
            };

                ColorMatrix clrMatrix = new ColorMatrix(ptsArray);
                ImageAttributes imgAttribs = new ImageAttributes();
                imgAttribs.SetColorMatrix(clrMatrix, ColorMatrixFlag.Default, ColorAdjustType.Default);

                gMem.DrawImage(bmp,
                               new Rectangle(0, 0, (int)(bmp.Width * 0.8f), (int)(bmp.Height * 0.8f)),
                               0, 0, bmp.Width, bmp.Height,
                               GraphicsUnit.Pixel, imgAttribs);
            }
            return bmpRes;
        }
    }
}
