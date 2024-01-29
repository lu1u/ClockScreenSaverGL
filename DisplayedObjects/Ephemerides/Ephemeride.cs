using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace ClockScreenSaverGL.DisplayedObjects.Ephemerides
{
    public class Ephemeride
    {
        private readonly LeverCoucherSoleil _leverCoucher;
        private readonly Lune _lune;

        public Ephemeride(double latitude, double longitude, int timezone, bool heureEte)
        {
            _leverCoucher = new LeverCoucherSoleil(longitude, latitude, timezone, heureEte);
            _lune = new Lune();
        }

        public Bitmap GetBitmap(int tailleFonte)
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

                string texte = $"Lune:\n\n{GetDate()}\n\n{aujourdhui.ToLongDateString()}\nLever:\t\t{leverAuj.ToShortTimeString()} ({FormatMinutes(ecartLeverMinutes)})\nCoucher:\t{coucherAuj.ToShortTimeString()} ({FormatMinutes(ecartCoucherMinutes)})"
                    + $"\nAujourd'hui:\t{jourAuj.Hours}:{jourAuj.Minutes} ({FormatMinutes(ecartJour)})";


                using (Font fonte = new Font(FontFamily.GenericSansSerif, tailleFonte))
                {
                    Graphics gNull = Graphics.FromHwnd(IntPtr.Zero);
                    SizeF size = gNull.MeasureString(texte, fonte);
                    Bitmap lune = _lune.GetImageLune(aujourdhui);


                    Bitmap bitmap = new Bitmap((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height), PixelFormat.Format32bppArgb);

                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        int hauteurLigne = (int)(fonte.Height * g.DpiX / 72.0);
                        var format = new StringFormat() { Alignment = StringAlignment.Far };
                        Rectangle r = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

                        g.DrawString("Lune", fonte, Brushes.Black, r);
                        r.Offset(0, hauteurLigne);
                        r.Offset(0, hauteurLigne);

                        g.DrawString(Embellir(aujourdhui.ToLongDateString()), fonte, Brushes.Black, r);
                        r.Offset(0, hauteurLigne);

                        g.DrawString("Lever", fonte, Brushes.Black, r);
                        g.DrawString($"{leverAuj.ToShortTimeString()} ({FormatMinutes(ecartLeverMinutes)})", fonte, Brushes.Black, r, format);
                        r.Offset(0, hauteurLigne);

                        g.DrawString("Coucher", fonte, Brushes.Black, r);
                        g.DrawString($"{coucherAuj.ToShortTimeString()} ({FormatMinutes(ecartCoucherMinutes)})", fonte, Brushes.Black, r, format);
                        r.Offset(0, hauteurLigne);

                        g.DrawString("Aujoud'hui", fonte, Brushes.Black, r);
                        g.DrawString($"{jourAuj.Hours}:{jourAuj.Minutes} ({FormatMinutes(ecartJour)})", fonte, Brushes.Black, r, format);
                        r.Offset(0, hauteurLigne);

                        float ratio = (hauteurLigne * 2.0f) / lune.Width;
                        g.DrawImage(lune, r.Right - (int)(lune.Width * ratio), 0, (int)(lune.Width * ratio), (int)(lune.Height * ratio));
                    }
                    return bitmap;
                }
            }
        }

        private string FormatMinutes(int minutes)
        {
            if (minutes >= 0)
                return "+" + minutes;
            else
                return "" + minutes;
        }

        private static string GetDate()
        {
            return Embellir(DateTime.Now.ToLongDateString());
        }

        private static string Embellir(string v)
        {
            bool debutDeMot = true;
            StringBuilder res = new StringBuilder();

            for (int i = 0; i < v.Length; i++)
            {
                if (char.IsLetter(v[i]))
                {
                    if (debutDeMot)
                        res.Append(char.ToUpper(v[i]));
                    else
                        res.Append(char.ToLower(v[i]));
                    debutDeMot = false;
                }
                else
                {
                    res.Append(v[i]);
                    debutDeMot = true;
                }
            }

            return res.ToString();
        }
    }
}
