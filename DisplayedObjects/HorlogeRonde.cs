﻿/*
 * Created by SharpDevelop.
 * User: lucien
 * Date: 22/06/2014
 * Time: 17:11
 * 
 * To change this template use Tools  Options  Coding  Edit Standard Headers.
 */
using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;
using SharpGL;
using SharpGL.SceneGraph.Assets;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace ClockScreenSaverGL.DisplayedObjects
{
    /// <summary>
    /// Description of HorlogeRonde.
    /// </summary>
    public class HorlogeRonde : DisplayedObject, IDisposable
    {
        #region Parametres
        public const string CAT = "HorlogeRonde";
        protected CategorieConfiguration c;
        private byte ALPHA;
        private int HAUTEUR_FONTE;
        private int HAUTEUR_FONTE2;
        public byte ALPHA_AIGUILLES;
        public float EPAISSEUR_TROTTEUSE;
        public float EPAISSEUR_MINUTES;
        public float EPAISSEUR_HEURES;
        public float EPAISSEUR_TROTTEUSE_CONTINUE;
        public float RATIO_TROTTEUSE_CONTINUE;
        public float _pX, _pY;
        private readonly int _diametre;
        private readonly float _rayon;
        private string REPERTOIRE_HORLOGE;

        private Texture _textureFondHorloge;
        private Bitmap _bmpFondHorloge;
        //private bool _aDroite;
        // Optimisation, pour eviter de les passer en parametre
        private float _CentreX, _CentreY;

        private Color COULEUR_GRADUATIONS;

        Texture _textureHeure, _textureMinute, _textureTrotteuse;

        #endregion
        public override CategorieConfiguration getConfiguration()
        {
            if (c == null)
            {
                c = Configuration.getCategorie(CAT);
                ALPHA = c.getParametre("Alpha", (byte)200);
                HAUTEUR_FONTE = c.getParametre("HauteurFonte1", (byte)38);
                HAUTEUR_FONTE2 = c.getParametre("HauteurFonte2", (byte)16);
                ALPHA_AIGUILLES = c.getParametre("AlphaAiguilles", (byte)250);
                EPAISSEUR_TROTTEUSE = c.getParametre("EpaisseurTrotteuse", 2.0f);
                EPAISSEUR_MINUTES = c.getParametre("EpaisseurMinutes", 15.0f);
                EPAISSEUR_HEURES = c.getParametre("EpaisseurHeure", 25.0f);
                EPAISSEUR_TROTTEUSE_CONTINUE = c.getParametre("EpaisseurTrotteuseContinue", 2.0f);
                RATIO_TROTTEUSE_CONTINUE = c.getParametre("RatioTrotteuseContinue", 0.15f);
                COULEUR_GRADUATIONS = Color.FromArgb(ALPHA_AIGUILLES, 0, 0, 0);
                REPERTOIRE_HORLOGE = c.getParametre("Repertorie horloge", "Horloge");
            }

            return c;
        }


        private Lune lune;

        public HorlogeRonde(OpenGL gl, int d, float Px, float Py) : base(gl)
        {
            getConfiguration();
            _pX = Px;
            _pY = Py;
            _diametre = d;
            _rayon = _diametre / 2.0f;
        }

        ~HorlogeRonde()
        {
            Dispose();
        }

        public override void Dispose()
        {
            try
            {
                _bmpFondHorloge?.Dispose();
            }
            finally
            {
            }
        }

        protected override void Init(OpenGL gl)
        {
            _taille = new SizeF(_diametre, _diametre);
            lune = new Lune(ALPHA_AIGUILLES);
            _textureHeure = new Texture();
            _textureMinute = new Texture();
            _textureTrotteuse = new Texture();

            _textureHeure.Create(gl, c.getParametre("Heure", Configuration.getImagePath(REPERTOIRE_HORLOGE + @"\Heure.png")));
            _textureMinute.Create(gl, c.getParametre("Minute", Configuration.getImagePath(REPERTOIRE_HORLOGE + @"\Minute.png")));
            _textureTrotteuse.Create(gl, c.getParametre("Trotteuse", Configuration.getImagePath(REPERTOIRE_HORLOGE + @"\Trotteuse.png")));
            _textureFondHorloge = CreerBitmapFond(gl);
            base.Init(gl);
        }
        /// <summary>
        /// Date changee: changer l'image du fond de l'horloge qui contient
        /// la date et la phase lunaire
        /// </summary>
        /// <param name="g"></param>
        /// <param name="maintenant"></param>
        public override void DateChangee(OpenGL gl, Temps maintenant)
        {
            CreerBitmapFond(gl);
        }


#if USE_GDI_PLUS_FOR_2D
		/// <summary>
		/// Dessine l'horloge une fois pour toutes et la garde en memoire
		/// </summary>
		private void CreerBitmapFond(Graphics gr)
		{
			if (_bmpFondHorloge != null)
			{
				_bmpFondHorloge.Dispose();
				_bmpFondHorloge = null;
			}

			_bmpFondHorloge = new Bitmap(_diametre, _diametre, gr);

			using (Graphics g = Graphics.FromImage(_bmpFondHorloge))
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.TextRenderingHint = TextRenderingHint.AntiAlias;
				float Centre = _diametre / 2.0f;
				DessineFondHorloge(g, Centre, Centre);
			}
		}
#endif

        /// <summary>
        /// Dessine l'horloge une fois pour toutes et la garde en memoire
        /// </summary>
        private Texture CreerBitmapFond(OpenGL gl)
        {
            if (_bmpFondHorloge != null)
            {
                _bmpFondHorloge.Dispose();
                _bmpFondHorloge = null;
            }
            //Bitmap bmp = new Bitmap(_diametre, _diametre, PixelFormat.Format32bppArgb);
            Bitmap bmp = (Bitmap)Bitmap.FromFile(c.getParametre("Cadran", Configuration.getImagePath(REPERTOIRE_HORLOGE + @"\Cadran.png")));

            //using (Graphics g = Graphics.FromImage(bmp))
            //{
            //    g.SmoothingMode = SmoothingMode.AntiAlias;
            //    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //    g.TextRenderingHint = TextRenderingHint.AntiAlias;
            //    DessineFondHorloge(g, bmp.Size);
            //}

            Texture texture = new Texture();
            texture.Create(gl, bmp);

            _bmpFondHorloge = bmp;
            return texture;
        }



        /// <summary>
        /// Dessine une des aiguilles de l'horloge
        /// </summary>
        /// <param name="g"></param>
        /// <param name="CentreX"></param>
        /// <param name="CentreY"></param>
        /// <param name="val"></param>
        /// <param name="max"></param>
        /// <param name="ratioRayon1"></param>
        /// <param name="ratioRayon2"></param>
        /// <param name="pen"></param>
        private void DessineAiguille(OpenGL gl, Texture texture, float val, float max, float ratioRayon1, float largeurSurDeux)
        {
            // Calcul de l'angle en radian
            float Longueur1 = _rayon * ratioRayon1;
            float Angle = -((val / max) * (360.0f));

            gl.PushMatrix();
            gl.Rotate(0, 0, Angle);
            gl.Color(0, 0, 0, 1.0f);
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            texture.Bind(gl);
            gl.Begin(OpenGL.GL_QUADS);
            gl.TexCoord(0, 1.0f); gl.Vertex(-largeurSurDeux, 0);
            gl.TexCoord(0, 0.0f); gl.Vertex(-largeurSurDeux, Longueur1);
            gl.TexCoord(1, 0.0f); gl.Vertex(+largeurSurDeux, Longueur1);
            gl.TexCoord(1, 1.0f); gl.Vertex(+largeurSurDeux, 0);
            gl.End();

            gl.PopMatrix();
        }

        /// <summary>
        /// Deplacer l'horloge
        /// </summary>
        /// <param name="maintenant"></param>
        /// <param name="tailleEcran"></param>
        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.DEPLACE);
#endif

#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif

        }

        /// <summary>
        /// Dessine le jour du mois une fois pour toute dans le fond de l'horloge
        /// </summary>
        /// <param name="g"></param>
        /// <param name="CentreX"></param>
        /// <param name="CentreY"></param>
        /// <param name="Rayon"></param>
        /// <param name="Jour"></param>
        /// <param name="b"></param>
        /// <param name="p"></param>
        public void DessineJourDuMois(Graphics g, float CentreX, float CentreY, float Rayon, int Jour, Brush b, Pen p)
        {
            using (Font fonte = new Font(FontFamily.GenericMonospace, (HAUTEUR_FONTE), FontStyle.Bold, GraphicsUnit.Pixel))
            {
                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.LineAlignment = StringAlignment.Near;

                string texte = Jour.ToString("00");
                SizeF s = g.MeasureString(texte, fonte);

                int X = (int)(CentreX + (Rayon / 4.0f));
                int Y = (int)(CentreY - s.Height / 2.0f);
                Rectangle rect = new Rectangle(X, Y, (int)s.Width, (int)s.Height);

                rect.Inflate(4, 4);
                g.DrawRectangle(p, rect);
                rect.Inflate(-2, -2);
                g.DrawRectangle(p, rect);
                rect.Offset(0, 3);

                g.DrawString(texte, fonte, b, rect, stringFormat);
            }
        }

        /// <summary>
        /// Dessine une icone representant la phase lunaire
        /// </summary>
        /// <param name="g"></param>
        /// <param name="CentreX"></param>
        /// <param name="CentreY"></param>
        /// <param name="Rayon"></param>
        /// <param name="maintenant"></param>
        /// <param name="brush"></param>
        public void DessinePhaseLunaire(Graphics g, float CentreX, float CentreY, float Rayon, DateTime maintenant)
        {
            using (Bitmap bmpLune = lune.getImageLune(g, maintenant))
                if (bmpLune != null)
                {
                    float X = CentreX - (Rayon / 4.0f) - bmpLune.Width;
                    float Y = CentreY - bmpLune.Height / 2.0f;
                    g.DrawImage(bmpLune, X, Y, Rayon / 3.0f, Rayon / 3.0f);
                }
        }

#if USE_GDI_PLUS_FOR_2D
        /// <summary>
        /// Affiche l'objet
        /// </summary>
        /// <param name="g"></param>
        /// <param name="maintenant"></param>
        /// <param name="tailleEcran"></param>
        /// <param name="couleur"></param>
        public override void AfficheGDI(Graphics g, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
            if (_bmpFondHorloge == null)
                CreerBitmapFond(g);

#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            CentreX = _pX + _rayon;
            CentreY = _pY + _rayon;

            using (Brush b = new SolidBrush(getCouleurAvecAlpha(couleur, ALPHA)))
            {
                SmoothingMode q = g.SmoothingMode;
                CompositingQuality c = g.CompositingQuality;
                InterpolationMode m = g.InterpolationMode;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                g.CompositingQuality = CompositingQuality.HighSpeed;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                // NB: les infos journalieres sont dessinees dans CreateBitmapFond

                // Fond de l'horloge
                g.FillEllipse(b, CentreX - _rayon, CentreY - _rayon, _diametre, _diametre);
                g.SmoothingMode = q;
                g.CompositingQuality = c;
                g.InterpolationMode = m;

                // Graduations
                g.DrawImage(_bmpFondHorloge, CentreX - _rayon, CentreY - _rayon);

                // Trotteuse
                DessineAiguille(g, maintenant._Seconde, 60.0f, 0.8f, -0.2f, _penTrotteuse);
                //Minutes
                DessineAiguille(g, maintenant._Minute + maintenant._Seconde / 60.0f, 60.0f, 0.7f, 0, _penMinutes);
                // Heure
                DessineAiguille(g, maintenant._Heure + maintenant._Minute / 60.0f, 12.0f, 0.7f, 0, _penHeures);

                // Secondes continues
                CentreY += ((_diametre * RATIO_TROTTEUSE_CONTINUE) * 0.9f);
                DessineAiguille(g, maintenant._Millieme, 1000.0f, 0.12f, -0.02f, _penTrotteuseC);
            }
#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

#else
        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif

            if (_textureFondHorloge != null)
            {
                _CentreX = _pX + _rayon;
                _CentreY = _pY + _rayon;
                using (new Viewport2D(gl, 0.0f, 0.0f, tailleEcran.Width, tailleEcran.Height))
                {
                    gl.Disable(OpenGL.GL_LIGHTING);
                    gl.Disable(OpenGL.GL_DEPTH);
                    gl.Enable(OpenGL.GL_BLEND);
                    gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);

                    float[] col = { (float)couleur.R / 256.0f, (float)couleur.G / 256.0f, (float)couleur.B / 256.0f, (float)ALPHA / 256.0f };
                    // Fond de l'horloge (bitmap)
                    gl.Enable(OpenGL.GL_TEXTURE_2D);
                    gl.Color(col);
                    _textureFondHorloge.Bind(gl);
                    gl.Translate(_CentreX, _CentreY, 0);
                    gl.Begin(OpenGL.GL_QUADS);
                    gl.TexCoord(0.0f, 0.0f); gl.Vertex(-_rayon, _rayon);
                    gl.TexCoord(0.0f, 1.0f); gl.Vertex(-_rayon, -_rayon);
                    gl.TexCoord(1.0f, 1.0f); gl.Vertex(_rayon, -_rayon);
                    gl.TexCoord(1.0f, 0.0f); gl.Vertex(_rayon, _rayon);
                    gl.End();

                    gl.Disable(OpenGL.GL_TEXTURE_2D);
                    DessineAiguille(gl, _textureTrotteuse, maintenant.seconde, 60.0f, 0.8f, EPAISSEUR_TROTTEUSE / 2.0f);
                    //Minutes
                    DessineAiguille(gl, _textureMinute, maintenant.minute + maintenant.seconde / 60.0f, 60.0f, 0.7f, EPAISSEUR_MINUTES / 2.0f);
                    // Heure
                    DessineAiguille(gl, _textureHeure, maintenant.heure + maintenant.minute / 60.0f, 12.0f, 0.5f, EPAISSEUR_HEURES / 2.0f);

                    // Secondes continues
                    gl.Translate(0, -((_diametre * RATIO_TROTTEUSE_CONTINUE) * 0.9f), 0);
                    DessineAiguille(gl, _textureMinute, maintenant.milliemesDeSecondes, 1000.0f, 0.12f, EPAISSEUR_TROTTEUSE_CONTINUE / 1.5f);
                }
            }
#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }
#endif
        /// <summary>
        /// Dessine le fond de l'horloge (partie fixe)
        /// </summary>
        /// <param name="g"></param>
        /// <param name="centreX"></param>
        /// <param name="centreY"></param>
        private void DessineFondHorloge(Graphics g, Size taille)
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.CompositingQuality = CompositingQuality.HighQuality;

            DateTime maintenant = DateTime.Now;
            float diametre = (taille.Width + taille.Height) / 2.0f;
            float centreX = taille.Width / 2.0f;
            float centreY = taille.Height / 1.9f;
            float rayon = diametre / 2.0f;
            DessineJourDuMois(g, centreX, centreY, rayon, maintenant.Day, Brushes.Black, new Pen(Color.Black, EPAISSEUR_TROTTEUSE_CONTINUE));
            DessinePhaseLunaire(g, centreX, centreY, rayon, maintenant);

            using (Brush bTexte = new SolidBrush(COULEUR_GRADUATIONS))

            using (Font fonte = new Font(FontFamily.GenericSansSerif, HAUTEUR_FONTE, FontStyle.Bold, GraphicsUnit.Pixel),
                   fonte2 = new Font(FontFamily.GenericSansSerif, HAUTEUR_FONTE2, FontStyle.Regular, GraphicsUnit.Pixel))
            {

                // Inscription publicitaire
                //using (Font f = new Font(FontFamily.GenericSansSerif, (int)HAUTEUR_FONTE * 0.4f, FontStyle.Bold, GraphicsUnit.Pixel))
                //{
                //    StringFormat stringFormat = new StringFormat();
                //    stringFormat.Alignment = StringAlignment.Center;
                //    stringFormat.LineAlignment = StringAlignment.Center;
                //
                //    int y = (int)(centreY - (_rayon * 0.5f));
                //    g.DrawString("Lucien Pilloni\nKinésithérapeute\n04 56 00 29 78\nlpilloni.kine@gmail.com", f, bTexte, centreX, y, stringFormat);
                //}

                // Cadran de la troteuse seconde
                rayon = diametre * 0.9f;// RATIO_TROTTEUSE_CONTINUE;
                float Longueur1 = _diametre * 0.49f;
                float Longueur2 = _diametre * 0.46f;
                float Longueur3 = _diametre * 0.42f;
                float Longueur4 = _diametre * 0.30f;
                float Longueur5 = _diametre * 0.38f;
                centreY = centreY + (rayon * 0.9f);

                using (Pen pinceauNoir = new Pen(COULEUR_GRADUATIONS, 8), pinceauNoir2 = new Pen(COULEUR_GRADUATIONS, 6),
                       p = new Pen(COULEUR_GRADUATIONS))
                {
                    pinceauNoir.EndCap = LineCap.Triangle;
                    pinceauNoir2.EndCap = LineCap.Triangle;
                    for (int i = 1; i <= 100; i++)
                    {
                        double Angle = ((i / 100.0f) * (Math.PI * 2.0)) - (Math.PI / 2.0);
                        float s = (float)Math.Sin(Angle);
                        float c = (float)Math.Cos(Angle);

                        // Traits de graduation
                        if (i % 10 == 0)
                            g.DrawLine(p, centreX + (Longueur1 * c), centreY + (Longueur1 * s), centreX + (Longueur3 * c), centreY + (Longueur3 * s));
                        else
                            g.DrawLine(p, centreX + (Longueur1 * c), centreY + (Longueur1 * s), centreX + (Longueur2 * c), centreY + (Longueur2 * s));
                    }
                }
                g.FillEllipse(bTexte, centreX - rayon * 0.05f, centreY - rayon * 0.05f, rayon * 0.1f, rayon * 0.1f);
            }
        }

    }
}
