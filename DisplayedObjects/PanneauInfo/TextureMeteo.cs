using ClockScreenSaverGL.DisplayedObjects.Meteo;
using ClockScreenSaverGL.DisplayedObjects.OpenGLUtils;
using SharpGL;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ClockScreenSaverGL.DisplayedObjects
{
    public class TextureMeteo : TextureAsynchrone
    {
        private int TAILLE_TITRE_METEO;
        private int TAILLE_TEXTE_METEO;
        private int MARGE_H;
        private int NB_MAX_LIGNE;
        private int TAILLE_ICONE_METEO;
        public TextureMeteo(OpenGL gl, int tailleTitre, int tailleTexte, int margeH, int nbMaxLigne, int tailleIconeMeteo) : base(gl, null)
        {
            TAILLE_TITRE_METEO = tailleTitre;
            TAILLE_TEXTE_METEO = tailleTexte;
            MARGE_H = margeH;
            NB_MAX_LIGNE = nbMaxLigne;
            TAILLE_ICONE_METEO = tailleIconeMeteo;
        }

        /// <summary>
        /// Creation de la bitmap meteo en arriere plan
        /// </summary>
        protected override void InitAsynchrone()
        {
            MeteoInfo meteoInfo = new MeteoInfo();
            float Y = 0;
            _bitmap = new Bitmap(500, 500, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(_bitmap))
            {
                if (TAILLE_TITRE_METEO <= 0) TAILLE_TITRE_METEO = 48;
                if (TAILLE_TEXTE_METEO <= 0) TAILLE_TEXTE_METEO = 32;
                using (Font fonteTitreMeteo = new Font(FontFamily.GenericSansSerif, TAILLE_TITRE_METEO, FontStyle.Bold, GraphicsUnit.Pixel))
                using (Font fonteTexteMeteo = new Font(FontFamily.GenericSansSerif, TAILLE_TEXTE_METEO, FontStyle.Regular, GraphicsUnit.Pixel))
                {
                    g.DrawString(meteoInfo._title, fonteTitreMeteo, Brushes.White, MARGE_H, Y);
                    SizeF tailleTitre = g.MeasureString(meteoInfo._title, fonteTitreMeteo);
                    int nbLignes = Math.Min(meteoInfo._lignes.Count, NB_MAX_LIGNE);
                    Y += tailleTitre.Height;
                    // Lignes de previsions
                    for (int i = 0; i < nbLignes; i++)
                    {
                        Y += meteoInfo._lignes[i].Affiche(g, fonteTexteMeteo, fonteTexteMeteo, Y, TAILLE_ICONE_METEO);
                    }
                }
            }
        }
    }

}
