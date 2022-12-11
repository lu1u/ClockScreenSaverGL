/*
 * Gestion de la couleur globale de l'économiseur qui est presque monochrome
 */

using System;
using System.Drawing;

namespace ClockScreenSaverGL
{
    /// <summary>
    /// Description of CouleurGlobale.
    /// </summary>
    public class CouleurGlobale
    {
        public const double VITESSE_CHANGEMENT_COULEUR = 0.05;
        private const string HUE = "hue";
        private const string VALUE = "value";
        private const string SATURATION = "saturation";

        public double alpha;
        public double hue;
        public double saturation;
        public double luminance;
        private DateTime dernier = DateTime.Now;


        public CouleurGlobale(double h, double s, double l)
        {
            //c = Configuration.getCategorie(CAT);

            alpha = 1.0;
            hue = h;
            saturation = s;
            luminance = l;
            //hue = c.getParametre("Teinte", 0.5f);
            //saturation = c.getParametre("Saturation", 0.9f);
            //luminance = c.getParametre("Valeur", 0.75f);
        }

        public CouleurGlobale(Color color)
        {
            alpha = color.A / 255.0f;
            hue = color.GetHue() / 360.0;
            saturation = color.GetSaturation();
            luminance = color.GetBrightness();
        }

        /// <summary>
        /// Change la teinte de la couleur a vitesse constante
        /// </summary>
        public void AvanceCouleur()
        {
            DateTime maintenant = DateTime.Now;

            hue += VITESSE_CHANGEMENT_COULEUR * (maintenant.Subtract(dernier).TotalMilliseconds / 1000.0f);
            while (hue > 1.0f)
                hue -= 1.0f;

            dernier = maintenant;
        }

        public void ChangeHue(int Sens)
        {
            Change(ref hue, Sens, HUE);
        }

        public void SetHue(double hueValue)
        {
            while (hueValue < 0)
                hueValue += 1.0;
            while (hueValue > 1.0)
                hueValue -= 1.0;

            hue = hueValue;
        }

        public void SetLuminance(double luminanceValue)
        {
            luminance = luminanceValue;
            if (luminance < 0)
                luminance = 0;
            if (luminance > 1.0)
                luminance = 1.0;

        }

        public Color getColorWithHueChange(double hueChange)
        {
            CouleurGlobale c = new CouleurGlobale(hue, saturation, luminance);
            c.alpha = this.alpha;
            c.hue = this.hue;
            c.luminance = this.luminance;
            c.saturation = this.saturation;

            c.SetHue(c.hue + hueChange);
            return c.GetARGB();
        }
        public Color getColorWithValueChange(double luminance)
        {
            CouleurGlobale c = new CouleurGlobale(hue, saturation, this.luminance);
            c.alpha = this.alpha;
            c.hue = this.hue;
            c.luminance = this.luminance;
            c.saturation = this.saturation;

            c.SetLuminance(c.luminance + luminance);
            return c.GetARGB();
        }

        public void ChangeSaturation(int Sens)
        {
            Change(ref saturation, Sens, SATURATION);
        }

        public void ChangeValue(int Sens)
        {
            Change(ref luminance, Sens, VALUE);
        }

        /// <summary>
        /// Change une des valeurs H, S ou V, dans un intervalle de 0 à 1
        /// </summary>
        /// <param name="V"></param>
        /// <param name="Sens"></param>
        private void Change(ref double V, int Sens, string parameterName)
        {
            if (Sens > 0)
            {
                if (V < 0.99)
                    V += 0.01f;
            }
            else
            {
                if (V > 0.01) V -= 0.01f;
            }

            //c.setParametre(parameterName, (float)V);
        }


        public override string ToString()
        {
            return "H:" + hue.ToString("0.00") + ",S:" + saturation.ToString("0.00") + ",V:" + luminance.ToString("0.00");
        }

        /// <summary>
        /// Conversion HSV -> RGB
        /// </summary>
        /// <returns></returns>
        public Color GetARGB()
        {
            double v, r, g, b;
            double Luminance = luminance;
            double Hue = hue;
            double Saturation = saturation;

            r = Luminance;   // default to gray
            g = Luminance;
            b = Luminance;
            v = (Luminance <= 0.5) ? (Luminance * (1.0f + Saturation)) : (Luminance + Saturation - Luminance * Saturation);
            if (v > 0)
            {
                double m;
                double sv;
                int sextant;
                double fract, vsf, mid1, mid2;

                m = Luminance + Luminance - v;
                sv = (v - m) / v;
                Hue *= 6.0f;
                sextant = (int)Hue;
                fract = Hue - sextant;
                vsf = v * sv * fract;
                mid1 = m + vsf;
                mid2 = v - vsf;
                switch (sextant)
                {
                    case 0:
                        r = v;
                        g = mid1;
                        b = m;
                        break;
                    case 1:
                        r = mid2;
                        g = v;
                        b = m;
                        break;
                    case 2:
                        r = m;
                        g = v;
                        b = mid1;
                        break;
                    case 3:
                        r = m;
                        g = mid2;
                        b = v;
                        break;
                    case 4:
                        r = mid1;
                        g = m;
                        b = v;
                        break;
                    case 5:
                        r = v;
                        g = m;
                        b = mid2;
                        break;
                }
            }

            if (r > 1.0f) r = 1.0f; else if (r < 0) r = 0;
            if (g > 1.0f) g = 1.0f; else if (g < 0) g = 0;
            if (b > 1.0f) b = 1.0f; else if (b < 0) b = 0;

            return Color.FromArgb(Convert.ToByte(alpha * 255.0f), Convert.ToByte(r * 255.0f), Convert.ToByte(g * 255.0f), Convert.ToByte(b * 255.0f));
        }


        /// <summary>
        /// Retourne une couleur en appliquant un ratio de luminosité à celle qui est donnée en parametre
        /// </summary>
        /// <param name="couleur"></param>
        /// <param name="lIGHT_FACTOR"></param>
        /// <returns></returns>
        public static Color Light(Color couleur, float lightFactor)
        {
            float r = ((couleur.R * lightFactor) > 255) ? 255 : (couleur.R * lightFactor);
            float g = ((couleur.G * lightFactor) > 255) ? 255 : (couleur.G * lightFactor);
            float b = ((couleur.B * lightFactor) > 255) ? 255 : (couleur.B * lightFactor);

            return Color.FromArgb(couleur.A, (int)r, (int)g, (int)b);
        }
    }
}
