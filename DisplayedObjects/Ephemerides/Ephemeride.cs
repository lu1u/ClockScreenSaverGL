using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ClockScreenSaverGL.DisplayedObjects.Ephemerides
{
    public class Ephemeride
    {
        private readonly LeverCoucherSoleil _leverCoucher;
        private readonly Lune _lune;

        public Ephemeride(double latitude, double longitude, int timezone, bool heureEte)
        {
            _leverCoucher = new LeverCoucherSoleil(longitude, latitude, timezone, heureEte);
            _lune = new Lune(255);
        }

        public Bitmap getBitmap(int tailleFonte)
        {
            using (var c = new Chronometre("Texte CreateBitmap"))
            {
                DateTime aujourdhui = DateTime.Now;
                DateTime leverAuj = _leverCoucher.CalculateSunRise(aujourdhui);
                DateTime coucherAuj = _leverCoucher.CalculateSunSet(aujourdhui);

                DateTime hier = aujourdhui.AddDays(-1);
                DateTime leverHier = _leverCoucher.CalculateSunRise(hier);
                DateTime coucherHier = _leverCoucher.CalculateSunSet(hier);

                int ecartLeverMinutes = (int)leverAuj.Subtract(leverHier).TotalMinutes - (24*60);
                int ecartCoucherMinutes = (int)coucherAuj.Subtract(coucherHier).TotalMinutes - (24*60);

                TimeSpan jourAuj = (coucherAuj - leverAuj);
                TimeSpan jourHier = (coucherHier - leverHier);

                int ecartJour = (int)(jourAuj - jourHier).TotalMinutes;

                string texte = $"Lune:\n\n{getDate()}\n\nLever:\t\t{leverAuj.ToShortTimeString()} ({formatMinutes(ecartLeverMinutes)})\nCoucher:\t{coucherAuj.ToShortTimeString()} ({formatMinutes(ecartCoucherMinutes)})"
                    + $"\nAujourd'hui:\t{jourAuj.Hours}:{jourAuj.Minutes} ({formatMinutes(ecartJour)})";


                using (Font fonte = new Font(FontFamily.GenericSansSerif, tailleFonte))
                {
                    Graphics gNull = Graphics.FromHwnd(IntPtr.Zero);
                    SizeF size = gNull.MeasureString(texte, fonte);
                    Bitmap lune = _lune.getImageLune(null, aujourdhui);


                    Bitmap bitmap = new Bitmap((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height) , PixelFormat.Format32bppArgb);

                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.DrawImageUnscaled(lune, bitmap.Width - lune.Width, 0);
                        g.DrawString(texte, fonte, Brushes.Black, 0, 0);
                    }
                    return bitmap;
                }
            }
        }

        private string formatMinutes(int minutes)
        {
            if (minutes >= 0)
                return "+" + minutes;
            else
                return "" + minutes;
        }

        private static string getDate()
        {
            return Beautiful(DateTime.Now.ToLongDateString());
        }

        private static string Beautiful(string v)
        {
            bool debutDeMot = true;
            string res = "";
            for (int i = 0; i < v.Length;i++)
            {
                if ( Char.IsLetter(v[i]) )
                {
                    if (debutDeMot)
                        res += Char.ToUpper(v[i]);
                    else
                        res += Char.ToLower(v[i]);
                    debutDeMot = false;
                }
                else
                {
                    res += v[i];
                    debutDeMot = false;
                }                
            }

            return res;
        }
    }
}
