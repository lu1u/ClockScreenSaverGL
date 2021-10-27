using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Textes;
using SharpGL;
using SharpGL.SceneGraph.Assets;
using System.Threading.Tasks;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;

namespace ClockScreenSaverGL.DisplayedObjects.Meteo
{
    internal class PanneauInfos : DisplayedObject, IDisposable
    {
        #region PARAMETRES
        public const string CAT = "PanneauInfos";
        protected static CategorieConfiguration c;
        public int DELAI_DEEZER;
        public int TAILLE_TITRE_METEO;
        public int TAILLE_TEXTE_METEO;
        public int TAILLE_TEMPERATURE_METEO;
        public float RATIO_INTERLIGNE;
        public int MARGE_H;
        public int MARGE_V;
        public int LARGEUR_JAUGE;
        public int TAILLE_TEXTE_LEVER_COUCHER;
        private byte ALPHA;
        private int DIAMETRE_HORLOGE;
        private int MARGE_HORLOGE;
        private int TAILLE_TEXTE_HEURE;
        private int TAILLE_TEXTE_DATE;
        private int MARGE_TEXTE_HEURE;
        public float LIGHT_FACTOR;
        private int NB_MAX_LIGNES;


        private static string getDeezerInfoDirectory()
        {
            string myExeDir = (new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location)).FullName;
            FileInfo p = new FileInfo(myExeDir);
            return Path.Combine(p.DirectoryName, "GetDeezerInformation.exe");

        }

        #endregion PARAMETRES ;

        private MeteoInfo _meteo;
        private HorlogeRonde _horloge;
        private HeureTexte _heureTexte;
        private DateTexte _dateTexte;

        private Bitmap _bitmap;
        private Texture _texture = new Texture();

        /// <summary>
        /// Constructeur
        /// </summary>
        public PanneauInfos(OpenGL gl) : base(gl)
        {
            c = getConfiguration();
            _taille = new SizeF(DIAMETRE_HORLOGE + MARGE_HORLOGE * 2, SystemInformation.VirtualScreen.Height);
        }

        public override void Init(OpenGL gl)
        {
            c = getConfiguration();

            //CreerBitmap(gl);
        }

        public override CategorieConfiguration getConfiguration()
        {
            if (c == null)
            {
                c = Configuration.getCategorie(CAT);
                MeteoInfo.NB_LIGNES_INFO_MAX = c.getParametre("Nb lignes info max", 4);
                DELAI_DEEZER = c.getParametre("DeezerInfo Delai (secondes)", 5);
                TAILLE_TITRE_METEO = c.getParametre("Taille Titre Meteo", 24);
                LignePrevisionMeteo.TAILLE_ICONE_METEO = c.getParametre("Taille Icone Meteo", 56);
                TAILLE_TEXTE_METEO = c.getParametre("Taille Texte Meteo", 18);
                TAILLE_TEMPERATURE_METEO = c.getParametre("Taille Temperature Meteo", 18);
                RATIO_INTERLIGNE = c.getParametre("RatioInterligne", 0.99f);
                MARGE_H = c.getParametre("MargeH", 12);
                MARGE_V = c.getParametre("MargeV", 12);
                LARGEUR_JAUGE = c.getParametre("LargeurJauge", 8);
                TAILLE_TEXTE_LEVER_COUCHER = c.getParametre("Taille Texte Lever", 24);
                ALPHA = (byte)c.getParametre("Alpha", 200);
                DIAMETRE_HORLOGE = c.getParametre("Diametre Horloge", 360);
                MARGE_HORLOGE = 20;
                TAILLE_TEXTE_HEURE = c.getParametre("Taille texte heure", 32);
                TAILLE_TEXTE_DATE = c.getParametre("Taille texte date", 32);
                MARGE_TEXTE_HEURE = c.getParametre("Marge texte heure", 32); ;
                LIGHT_FACTOR = c.getParametre("Luminosité", 5.0f);
                NB_MAX_LIGNES = c.getParametre("Nb max lignes", 5);
            }
            return c;
        }
        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            if (_bitmap == null)
                CreerBitmap(gl);

            InitProgressif(gl, tailleEcran);

            if (_bitmap != null)
            {
                couleur = CouleurGlobale.Light(couleur, LIGHT_FACTOR);
                gl.Clear(OpenGL.GL_DEPTH_BUFFER_BIT);
                gl.PushMatrix();
                gl.MatrixMode(OpenGL.GL_PROJECTION);
                using (Viewport2D v = new Viewport2D(gl, 0, 0, tailleEcran.Width, tailleEcran.Height))
                {
                    gl.Disable(OpenGL.GL_LIGHTING);
                    gl.Disable(OpenGL.GL_DEPTH);
                    gl.Enable(OpenGL.GL_TEXTURE_2D);
                    gl.Enable(OpenGL.GL_BLEND);
                    gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);

                    float[] col = { couleur.R / 256.0f, couleur.G / 256.0f, couleur.B / 256.0f, 1 };
                    gl.Color(col);

                    float Y = (tailleEcran.Height - _taille.Height) / 2.0f;
                    _texture.Bind(gl);
                    gl.Translate(tailleEcran.Width - _taille.Width, Y, 0);
                    gl.Begin(OpenGL.GL_QUADS);
                    gl.TexCoord(0.0f, 0.0f); gl.Vertex(0, _taille.Height);
                    gl.TexCoord(0.0f, 1.0f); gl.Vertex(0, 0);
                    gl.TexCoord(1.0f, 1.0f); gl.Vertex(_taille.Width, 0);
                    gl.TexCoord(1.0f, 0.0f); gl.Vertex(_taille.Width, _taille.Height);
                    gl.End();
                }

                _heureTexte?.AfficheOpenGL(gl, maintenant, tailleEcran, couleur);
                _dateTexte?.AfficheOpenGL(gl, maintenant, tailleEcran, couleur);
                _horloge?.AfficheOpenGL(gl, maintenant, tailleEcran, couleur);
            }
#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif

        }

        private void InitProgressif(OpenGL gl, Rectangle tailleEcran)
        {
            int _X = (int)(tailleEcran.Width - _taille.Width);
            if (_horloge == null)
            {
                _horloge = new HorlogeRonde(gl, DIAMETRE_HORLOGE, _X + (_taille.Width - DIAMETRE_HORLOGE) / 2, tailleEcran.Bottom - DIAMETRE_HORLOGE - MARGE_HORLOGE);
                _horloge.Init(gl);
                return;
            }

            if (_heureTexte == null)
            {
                _heureTexte = new HeureTexte(gl, _X + MARGE_TEXTE_HEURE, (int)(_horloge._pY - MARGE_HORLOGE - TAILLE_TEXTE_HEURE - MARGE_TEXTE_HEURE), TAILLE_TEXTE_HEURE);
                _heureTexte.Init(gl);
                return;

            }

            if (_dateTexte == null)
            {
                _dateTexte = new DateTexte(gl, _X + MARGE_TEXTE_HEURE, (int)( _heureTexte._trajectoire._Py - TAILLE_TEXTE_HEURE - MARGE_TEXTE_HEURE),TAILLE_TEXTE_DATE);
                _dateTexte.Init(gl);
                return;

            }

            if (_meteo == null)
            {
                _meteo = new MeteoInfo();
                _meteo.Init(gl);
                return;
            }
        }

        /// <summary>
        /// Creer une fois pour toutes la bitmap qui sera affichee
        /// </summary>
        /// <param name="gl"></param>
        private void CreerBitmap(OpenGL gl)
        {
            _bitmap?.Dispose();
            _bitmap = null;
            if (_meteo?._donneesPretes != true)
                // Données meteo pas encore pretes
                return;

            CalculeTaille();
            if (_taille.Width < 1)
                return; // Pas de bitmap

            float Y = MARGE_V + DIAMETRE_HORLOGE + MARGE_HORLOGE * 2 + TAILLE_TEXTE_HEURE + MARGE_TEXTE_HEURE + TAILLE_TEXTE_DATE;
            _bitmap = new Bitmap((int)Math.Ceiling(_taille.Width), (int)Math.Ceiling(_taille.Height), PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(_bitmap))
            {
                g.Clear(Color.FromArgb(164, 32, 32, 32));
                float LargeurMax = _taille.Width;
                afficheMeteo(g, ref Y);
            }

            _texture.Create(gl, _bitmap);
        }


        ///// <summary>
        ///// Affichage de la chanson en cours dans deezer
        ///// </summary>
        ///// <param name="g"></param>
        ///// <param name="Y"></param>
        //private void afficheDeezer(Graphics g, ref float Y)
        //{
        //	/*
        //	if (_deezer != null)
        //	{
        //		using (Font fonteTitreMeteo = new Font(FontFamily.GenericSansSerif, TAILLE_TITRE_METEO, FontStyle.Bold, GraphicsUnit.Pixel))
        //		using (Font fonteTexteMeteo = new Font(FontFamily.GenericSansSerif, TAILLE_TEXTE_METEO, FontStyle.Regular, GraphicsUnit.Pixel))
        //		using (Font fonteTexte = new Font(FontFamily.GenericSansSerif, TAILLE_TEXTE_METEO, FontStyle.Regular, GraphicsUnit.Pixel))
        //		{
        //
        //			using (Pen p = new Pen(Color.White, 4))
        //				g.DrawLine(p, MARGE_H, Y, _bitmap.Width - MARGE_H, Y);
        //
        //			Y += TAILLE_TITRE_METEO / 2;
        //			g.DrawImage(Image.FromFile(Configuration.Configuration.getImagePath("music-note.png")), MARGE_H, Y, TAILLE_ICONE_METEO, TAILLE_ICONE_METEO);
        //
        //			g.DrawString(_deezer.Infos, fonteTexteMeteo, Brushes.White, MARGE_H + TAILLE_ICONE_METEO, Y);
        //			Y += g.MeasureString(_deezer.Infos, fonteTexte).Height * RATIO_INTERLIGNE;
        //		}
        //	}*/
        //}

        private void afficheMeteo(Graphics g, ref float Y)
        {
            if (_meteo?._donneesPretes == true)
            {
                using (Font fonteTitreMeteo = new Font(FontFamily.GenericSansSerif, TAILLE_TITRE_METEO, FontStyle.Bold, GraphicsUnit.Pixel))
                using (Font fonteTexteMeteo = new Font(FontFamily.GenericSansSerif, TAILLE_TEXTE_METEO, FontStyle.Regular, GraphicsUnit.Pixel))
                {
                    g.DrawString(_meteo._title, fonteTitreMeteo, Brushes.White, MARGE_H, Y);
                    Y += TAILLE_TITRE_METEO * RATIO_INTERLIGNE;
                    // Lignes de previsions
                    for (int i = 0; (i < _meteo._lignes.Count) && (i < NB_MAX_LIGNES); i++)
                        Y += _meteo._lignes[i].affiche(g, fonteTexteMeteo, fonteTexteMeteo, Y) * RATIO_INTERLIGNE + MARGE_V;
                }
            }
        }


        /// <summary>
        /// Calcule la taille de l'image necessaire pour afficher les informations
        /// </summary>
        private void CalculeTaille()
        {
            float largeur = 0;
            float hauteur = 0;

            if (_meteo != null)
                if (_meteo._donneesPretes)
                {
                    hauteur = SystemInformation.VirtualScreen.Height;
                    largeur = 200;
                    /*hauteur = TAILLE_TITRE + TAILLE_TEXTE_TITRE * RATIO_INTERLIGNE + TAILLE_TEXTE_LEVER_COUCHER * RATIO_INTERLIGNE
						 + (TAILLE_ICONE * RATIO_INTERLIGNE) * _infos._lignes.Count
						 + MARGE_V * 2;
						 */
                    /*
			   using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
			   using (Font f = new Font(FontFamily.GenericSansSerif, TAILLE_TEXTE, FontStyle.Regular, GraphicsUnit.Pixel))
			   using (Font fLever = new Font(FontFamily.GenericSansSerif, TAILLE_TEXTE_LEVER_COUCHER, FontStyle.Regular, GraphicsUnit.Pixel))
			   {
				   // Taille des previsions meteo
				   foreach (LignePrevisionMeteo ligne in _infos._lignes)
				   {
					   string text = String.Format(Resources.Temperatures, ligne.day, ligne.TMin, ligne.TMax);
					   float tailleTexte = g.MeasureString(text, f).Width + TAILLE_ICONE;
					   if (tailleTexte > largeur)
						   largeur = tailleTexte;
				   }

			  } */
                    largeur += MARGE_H * 2;
                    largeur = Math.Max(largeur, DIAMETRE_HORLOGE + MARGE_HORLOGE * 2);
                }
            _taille = new SizeF(largeur, hauteur);
        }

        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.DEPLACE);
#endif
            if (_meteo != null)
            {
                if (_meteo.MustRefresh(maintenant))
                    _meteo = new MeteoInfo();

                /* if (_deezer.MustRefresh(maintenant))
                     _deezer.Refresh();
                     */
                if (_meteo.HasNewInfo())// || _deezer.HasNewInfo())
                    _bitmap = null;
            }



#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif
        }

        public override void Dispose()
        {
            base.Dispose();

            _bitmap?.Dispose();
            //_deezer?.Dispose();
            _meteo?.Dispose();
            _horloge?.Dispose();
            _heureTexte?.Dispose();
            _dateTexte?.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}

