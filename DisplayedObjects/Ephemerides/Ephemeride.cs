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

                int ecartLeverMinutes = (int)leverAuj.Subtract(leverHier).TotalMinutes - (24 * 60);
                int ecartCoucherMinutes = (int)coucherAuj.Subtract(coucherHier).TotalMinutes - (24 * 60);

                TimeSpan jourAuj = (coucherAuj - leverAuj);
                TimeSpan jourHier = (coucherHier - leverHier);

                int ecartJour = (int)(jourAuj - jourHier).TotalMinutes;

                string texte = $"Lune:\n\n{getDate()}\n\n{aujourdhui.ToLongDateString()}\nLever:\t\t{leverAuj.ToShortTimeString()} ({formatMinutes(ecartLeverMinutes)})\nCoucher:\t{coucherAuj.ToShortTimeString()} ({formatMinutes(ecartCoucherMinutes)})"
                    + $"\nAujourd'hui:\t{jourAuj.Hours}:{jourAuj.Minutes} ({formatMinutes(ecartJour)})";


                using (Font fonte = new Font(FontFamily.GenericSansSerif, tailleFonte))
                {
                    Graphics gNull = Graphics.FromHwnd(IntPtr.Zero);
                    SizeF size = gNull.MeasureString(texte, fonte);
                    Bitmap lune = _lune.getImageLune(null, aujourdhui);


                    Bitmap bitmap = new Bitmap((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height), PixelFormat.Format32bppArgb);

                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        int hauteurLigne = (int)(fonte.Height * g.DpiX / 72.0); ;
                        var format = new StringFormat() { Alignment = StringAlignment.Far };
                        Rectangle r = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

                        g.DrawString("Lune", fonte, Brushes.Black, r);
                        r.Offset(0, hauteurLigne);
                        r.Offset(0, hauteurLigne);

                        g.DrawString(Beautiful(aujourdhui.ToLongDateString()), fonte, Brushes.Black, r);
                        r.Offset(0, hauteurLigne);

                        g.DrawString("Lever", fonte, Brushes.Black, r);
                        g.DrawString($"{leverAuj.ToShortTimeString()} ({formatMinutes(ecartLeverMinutes)})", fonte, Brushes.Black, r, format);
                        r.Offset(0, hauteurLigne);

                        g.DrawString("Coucher", fonte, Brushes.Black, r);
                        g.DrawString($"{coucherAuj.ToShortTimeString()} ({formatMinutes(ecartCoucherMinutes)})", fonte, Brushes.Black, r, format);
                        r.Offset(0, hauteurLigne);

                        g.DrawString("Aujoud'hui", fonte, Brushes.Black, r);
                        g.DrawString($"{jourAuj.Hours}:{jourAuj.Minutes} ({formatMinutes(ecartJour)})", fonte, Brushes.Black, r, format);
                        r.Offset(0, hauteurLigne);

                        float ratio = (hauteurLigne * 2.0f) / lune.Width;
                        g.DrawImage(lune, r.Right - (int)(lune.Width * ratio), 0, (int)(lune.Width * ratio), (int)(lune.Height * ratio));
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
            for (int i = 0; i < v.Length; i++)
            {
                if (char.IsLetter(v[i]))
                {
                    if (debutDeMot)
                        res += char.ToUpper(v[i]);
                    else
                        res += char.ToLower(v[i]);
                    debutDeMot = false;
                }
                else
                {
                    res += v[i];
                    debutDeMot = true;
                }
            }

            return res;
        }
    }
}
