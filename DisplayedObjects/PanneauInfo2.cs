﻿using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Ephemerides;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;
using ClockScreenSaverGL.DisplayedObjects.Meteo;
using ClockScreenSaverGL.DisplayedObjects.OpenGLUtils;
using SharpGL;
using SharpGL.SceneGraph.Assets;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ClockScreenSaverGL.DisplayedObjects
{
    internal class PanneauInfo2 : DisplayedObject
    {
        #region Configuration
        private const string CAT = "PanneauInfo2";
        private CategorieConfiguration c;
        private float X_DEBUT;
        private float X_FIN;
        private float ALPHA_PANNEAU;
        private float LATITUDE, LONGITUDE;
        private int TIMEZONE;
        private bool HEURE_ETE;
        private int TAILLE_TEXTE_EPHEMERIDE;
        private float MARGE_EPHEMERIDE;
        private int TAILLE_TITRE_METEO;
        private int TAILLE_TEXTE_METEO;
        private int NB_MAX_LIGNES;
        private float RATIO_INTERLIGNE;
        private int MARGE_H;
        private int MARGE_V;
        private float BAS_EPHEMERIDE, BAS_HEURE, BAS_METEO;

        #endregion

        Ephemeride _ephemeride;
        Bitmap _bitmapEphemeride;
        private Texture _textureEphemeride;
        private Font _fonte;
        private MeteoAsynchrone _textureMeteo;
        
        class MeteoAsynchrone : ObjetAsynchrone
        {
            OpenGL _gl;
            MeteoInfo _meteo;
            Bitmap _bitmap;
            Texture _texture;
            private int TAILLE_TITRE_METEO;
            private int TAILLE_TEXTE_METEO;
            private int MARGE_H;
            private float RATIO_INTERLIGNE;
            private int NB_MAX_LIGNE;

            public MeteoAsynchrone( OpenGL gl, MeteoInfo meteo, int tailleTitre, int tailleTexte, int margeH, float ratioInterligne, int nbMaxLigne)
            {
                _gl = gl;
                _meteo = meteo;
                TAILLE_TITRE_METEO = tailleTitre;
                TAILLE_TEXTE_METEO = tailleTexte;
                MARGE_H = margeH;
                RATIO_INTERLIGNE = ratioInterligne;
                NB_MAX_LIGNE = nbMaxLigne;
            }

            /// <summary>
            /// Creation de la bitmap meteo en arriere plan
            /// </summary>
            protected override void InitAsynchrone()
            {
                float Y = 0;
                _bitmap = new Bitmap(500, 500, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(_bitmap))
                {
                    if (TAILLE_TITRE_METEO <= 0) TAILLE_TITRE_METEO = 32;
                    if (TAILLE_TEXTE_METEO <= 0) TAILLE_TEXTE_METEO = 24;
                    using (Font fonteTitreMeteo = new Font(FontFamily.GenericSansSerif, TAILLE_TITRE_METEO, FontStyle.Bold, GraphicsUnit.Pixel))
                    using (Font fonteTexteMeteo = new Font(FontFamily.GenericSansSerif, TAILLE_TEXTE_METEO, FontStyle.Regular, GraphicsUnit.Pixel))
                    {
                        g.DrawString(_meteo._title, fonteTitreMeteo, Brushes.White, MARGE_H, Y);
                        int nbLignes = Math.Min(_meteo._lignes.Count, NB_MAX_LIGNE);
                        Y += TAILLE_TITRE_METEO * RATIO_INTERLIGNE;
                        // Lignes de previsions
                        for (int i = 0; i < nbLignes; i++)
                        {
                            Y = (500 / (nbLignes+1)) * (i+1);
                             _meteo._lignes[i].affiche(g, fonteTexteMeteo, fonteTexteMeteo, Y);
                        }
                    }
                }
            }            

            /// <summary>
            /// La texture OpenGL doit etre créée dans le thread principal
            /// </summary>
            protected override void InitSynchrone()
            {
                if (_bitmap != null)
                {
                    _texture = new Texture();
                    _texture.Create(_gl, _bitmap);
                    _bitmap = null;// Plus besoin de la bitmap
                }
            }

            public Texture texture
            {
                get
                {
                    return _texture;
                }
            }
        }

        public PanneauInfo2(OpenGL gl) : base(gl)
        {
        }

        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_DEPTH);
            gl.Disable(OpenGL.GL_TEXTURE_2D);
            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);

            using (Viewport2D v = new Viewport2D(gl, 0, 0, 1, 1))
            {
                // Dessine le fond assombri derriere les infos
                float[] col = { 0,0,0, ALPHA_PANNEAU };
                gl.Color(col);
                gl.Begin(OpenGL.GL_QUADS);
                gl.Vertex(X_DEBUT, 0, 0);
                gl.Vertex(X_DEBUT, 1, 0);
                gl.Vertex(X_FIN, 1, 0);
                gl.Vertex(X_FIN, 0, 0);
                gl.End();

                // Dessine l'ephemeride
                if (_textureEphemeride != null)
                {
                    float[] colE = { couleur.R / 256.0f, couleur.G / 256.0f, couleur.B / 256.0f, 1.0f };
                    gl.Color(colE);
                    gl.Enable(OpenGL.GL_TEXTURE_2D);
                    _textureEphemeride.Bind(gl);
                    gl.Begin(OpenGL.GL_QUADS);
                    gl.TexCoord(0, 0.0f); gl.Vertex(X_DEBUT+ MARGE_EPHEMERIDE, 1- MARGE_EPHEMERIDE);
                    gl.TexCoord(0, 1.0f); gl.Vertex(X_DEBUT+ MARGE_EPHEMERIDE, BAS_EPHEMERIDE);
                    gl.TexCoord(1, 1.0f); gl.Vertex(X_FIN- MARGE_EPHEMERIDE, BAS_EPHEMERIDE);
                    gl.TexCoord(1, 0.0f); gl.Vertex(X_FIN- MARGE_EPHEMERIDE, 1- MARGE_EPHEMERIDE);
                    gl.End();
                }

                // Heure
                string sMaintenant = getTextHeure(maintenant);
                Bitmap bHeure = dessineHeure(gl, sMaintenant);
                Texture t = new Texture();
                t.Create(gl, bHeure);
                gl.Enable(OpenGL.GL_TEXTURE_2D);
                t.Bind(gl);
                gl.Begin(OpenGL.GL_QUADS);
                gl.TexCoord(0, 0.0f); gl.Vertex(X_DEBUT + MARGE_EPHEMERIDE, BAS_EPHEMERIDE - MARGE_EPHEMERIDE);
                gl.TexCoord(0, 1.0f); gl.Vertex(X_DEBUT + MARGE_EPHEMERIDE, BAS_HEURE);
                gl.TexCoord(1, 1.0f); gl.Vertex(X_FIN - MARGE_EPHEMERIDE, BAS_HEURE);
                gl.TexCoord(1, 0.0f); gl.Vertex(X_FIN - MARGE_EPHEMERIDE, BAS_EPHEMERIDE - MARGE_EPHEMERIDE);
                gl.End();
                t.Destroy(gl);

                // Meteo
                if (_textureMeteo.Pret)
                {
                    gl.Enable(OpenGL.GL_TEXTURE_2D);
                    _textureMeteo.texture.Bind(gl);
                    gl.Begin(OpenGL.GL_QUADS);
                    gl.TexCoord(0, 0.0f); gl.Vertex(X_DEBUT + MARGE_EPHEMERIDE, BAS_HEURE - MARGE_EPHEMERIDE);
                    gl.TexCoord(0, 1.0f); gl.Vertex(X_DEBUT + MARGE_EPHEMERIDE, BAS_METEO);
                    gl.TexCoord(1, 1.0f); gl.Vertex(X_FIN - MARGE_EPHEMERIDE, BAS_METEO);
                    gl.TexCoord(1, 0.0f); gl.Vertex(X_FIN - MARGE_EPHEMERIDE, BAS_HEURE - MARGE_EPHEMERIDE);
                    gl.End();
                }
            }
#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

        private string getTextHeure(Temps temps)
        {
            return temps.heure + ":"
                + temps.minute.ToString("D2") + ":"
                + temps.seconde.ToString("D2") + ":"
                + temps.milliemesDeSecondes.ToString("D3");
        }

        private Bitmap dessineHeure(OpenGL gl, string texte)
        {
            Graphics gNull = Graphics.FromHwnd(IntPtr.Zero);
            SizeF size = gNull.MeasureString(texte, _fonte);


            Bitmap bitmap = new Bitmap((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height), PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.DrawString(texte, _fonte, Brushes.White, 0, 0);
            }

            return bitmap;
        }

        public override void DateChangee(OpenGL gl, Temps maintenant)
        {
            _bitmapEphemeride = _ephemeride.getBitmap(TAILLE_TEXTE_EPHEMERIDE);
            _bitmapEphemeride = BitmapInvert(_bitmapEphemeride);
            _textureEphemeride = new Texture();
            _textureEphemeride.Create(gl, _bitmapEphemeride);
        }

        public override CategorieConfiguration getConfiguration()
        {
            if (c == null)
            {
                c = Configuration.getCategorie(CAT);
                X_DEBUT = c.getParametre("X Debut", 0.75f);
                X_FIN = c.getParametre("X Fin", 1.0f);
                ALPHA_PANNEAU = c.getParametre("Alpha panneau", 0.5f);
                LATITUDE = c.getParametre("Latitude", 45.188529f);
                LONGITUDE = c.getParametre("Longitude", 5.724524f);
                TIMEZONE = c.getParametre("Timezone", 14);
                HEURE_ETE = c.getParametre("Heure été", false);
                TAILLE_TEXTE_EPHEMERIDE = c.getParametre("Taille texte ephemeride", 16);
                TAILLE_TEXTE_METEO = c.getParametre("Taille Texte Meteo", 18);
                TAILLE_TITRE_METEO = c.getParametre("Taille Titre Meteo", 24);
                MARGE_H = c.getParametre("MargeH", 12);
                MARGE_V = c.getParametre("MargeV", 12);
                MARGE_EPHEMERIDE = c.getParametre("Marge ephemeride", 0.01f);
                RATIO_INTERLIGNE = c.getParametre("RatioInterligne", 0.99f);
                BAS_EPHEMERIDE = c.getParametre("Bas ephemeride", 0.8f);
                BAS_HEURE = c.getParametre("Bas heure", 0.7f); 
                BAS_METEO = c.getParametre("Bas météo", 0.01f); 
                NB_MAX_LIGNES = c.getParametre("Nb max lignes", 5);
            }
            return c;
        }

        protected override void Init(OpenGL gl)
        {
            base.Init(gl);
            getConfiguration();
            _ephemeride = new Ephemeride(LATITUDE, LONGITUDE, TIMEZONE, HEURE_ETE);
            
            _fonte = new Font(FontFamily.GenericSansSerif, 128, FontStyle.Bold);
            _textureMeteo = new MeteoAsynchrone(gl, new MeteoInfo(), TAILLE_TITRE_METEO, TAILLE_TEXTE_METEO, MARGE_H, RATIO_INTERLIGNE, NB_MAX_LIGNES);
        }

        
    }
}
