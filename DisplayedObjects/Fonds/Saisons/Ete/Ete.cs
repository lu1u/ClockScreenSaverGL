using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;
using ClockScreenSaverGL.DisplayedObjects.OpenGLUtils;
using SharpGL;
using SharpGL.SceneGraph.Assets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
namespace ClockScreenSaverGL.DisplayedObjects.Fonds.Saisons.Ete
{
    internal class Ete : Fond
    {
        #region PARAMETRES
        private const string CAT = "Ete";
        protected static CategorieConfiguration c;

        private string REPERTOIRE_ETE;
        //private int NB_HERBES;
        private int NB_FLARES;
        private int NB_REFLETS;
        private float VX_SOLEIL;
        private float VY_SOLEIL;
        private float TAILLE_SOLEIL;
        private float RATIO_FLARE_MIN;
        private float RATIO_FLARE_MAX;
        private byte ALPHA;
        private float ALPHA_FLARE_MIN;
        private float ALPHA_FLARE_MAX;
        private bool AFFICHE_FOND;
        private float RATIO_MER;
        private const int NB_IMAGES_FLARES = 4;
        private const float DECALAGE_TEXTURE = 1.0f / NB_IMAGES_FLARES;
        #endregion
        private static readonly DateTime debut = DateTime.Now;
        // Soleil
        private readonly Texture _textureSoleil;
        private readonly Texture _textureFond;
        private readonly Texture _textureFlares;

        private float _xSoleil, _ySoleil;
        // Herbes
        private float _vent = 0;

        //private List<Herbe> _herbes;

        private readonly TimerIsole timer = new TimerIsole(500);

        // Lens Flares (reflets du soleil sur l'objectif
        sealed private class Flare
        {
            public float _distance;
            public float _taille;
            public float _alpha;
            public int _texture;
            public float rR, rG, rB;
        };
        private Flare[] _flares;

        sealed private class Reflet
        {
            public Reflet(float TAILLE_SOLEIL)
            {
                _dx = FloatRandom(-TAILLE_SOLEIL, TAILLE_SOLEIL) * 0.5f;
                _y = FloatRandom(0.01f, -0.1f);
                _alpha = 1.0f;
                _sX = FloatRandom(0.1f, 0.2f) * -_y;
                _sY = FloatRandom(0.002f, 0.004f); 
                //_vx = FloatRandom(-0.01f, 0.01f);
                _vy = -0.005f;
                _dalpha = 0.99f;
                _dSX = 0.01f;
                _dSY = 0.0002f;
            }
            public float _dx;
            public float _y;
            public float _alpha;
            public float _sX, _sY;
            public float _vy;
            public float _dalpha;
            public float _dSX, _dSY;
        }

        private List<Reflet> _reflets;
        /**
         * Constructeur
         */
        public Ete(OpenGL gl, int LargeurEcran, int HauteurEcran) : base(gl)
        {
            GetConfiguration();
            _textureSoleil = new Texture();
            _textureSoleil.Create(gl, c.GetParametre("Soleil", Config.Configuration.GetImagePath(REPERTOIRE_ETE + @"\soleil.png")));
            _textureFond = new Texture();
            _textureFond.Create(gl, c.GetParametre("Fond", Config.Configuration.GetImagePath(REPERTOIRE_ETE + @"\fondEte.png")));
            _textureFlares = new Texture();
            _textureFlares.Create(gl, c.GetParametre("Flares", Config.Configuration.GetImagePath(REPERTOIRE_ETE + @"\flares.png")));
            float ratio = LargeurEcran / (float)HauteurEcran;
            _xSoleil = -ratio;
            _ySoleil = 0;
            Initialise();
        }

        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);

                REPERTOIRE_ETE = c.GetParametre("Repertoire", "ete");
                //NB_HERBES = c.getParametre("Nb Herbes", 80);
                NB_REFLETS = c.GetParametre("Nb Reflets", 20);
                NB_FLARES = c.GetParametre("Nb Flares", 6);
                VX_SOLEIL = c.GetParametre("VX Soleil", 0.01f, (a) => { VX_SOLEIL = (float)Convert.ToDouble(a); });
                VY_SOLEIL = c.GetParametre("VY Soleil", 0.01f, (a) => { VY_SOLEIL = (float)Convert.ToDouble(a); });
                TAILLE_SOLEIL = c.GetParametre("Taille Soleil", 0.4f, (a) => { TAILLE_SOLEIL = (float)Convert.ToDouble(a); });
                RATIO_FLARE_MIN = c.GetParametre("Ratio Flare Min", 0.7f);
                RATIO_FLARE_MAX = c.GetParametre("Ratio Flare Max", 1.3f);
                ALPHA = (byte)c.GetParametre("Alpha", 64);
                ALPHA_FLARE_MIN = (byte)c.GetParametre("Alpha Flare Min", 3);
                ALPHA_FLARE_MAX = (byte)c.GetParametre("Alpha Flare Max", 16);
                AFFICHE_FOND = c.GetParametre("Affiche Fond", true);
                RATIO_MER = c.GetParametre("Ratio mer", 0.5f, (a) => { RATIO_MER = (float)Convert.ToDouble(a); });
            }
            return c;
        }

        /**
         * Initialisation du soleil, des lens flares et de l'herbe
         * */
        private void Initialise()
        {

            _flares = new Flare[NB_FLARES];
            for (int i = 0; i < NB_FLARES; i++)
            {
                _flares[i] = new Flare();
                _flares[i]._distance = FloatRandom(1f, -1.5f);
                _flares[i]._taille = FloatRandom(TAILLE_SOLEIL * RATIO_FLARE_MIN, TAILLE_SOLEIL * RATIO_FLARE_MAX);
                _flares[i]._alpha = FloatRandom(ALPHA_FLARE_MIN, ALPHA_FLARE_MAX);
                _flares[i]._texture = random.Next(0, NB_IMAGES_FLARES);
                _flares[i].rR = FloatRandom(RATIO_FLARE_MIN, RATIO_FLARE_MAX);
                _flares[i].rG = FloatRandom(RATIO_FLARE_MIN, RATIO_FLARE_MAX);
                _flares[i].rB = FloatRandom(RATIO_FLARE_MIN, RATIO_FLARE_MAX);
            }
            //GenereBrinsHerbe();

            // Reflets
            _reflets = new List<Reflet>();
            //for (int i = 0; i < NB_REFLETS; i++)
            //{
            //    Reflet r = new Reflet(TAILLE_SOLEIL);
            //
            //    _reflets.Add(r);
            //}
            //        _herbes.Add(new Herbe(r.Next(touffe, touffe + LARGEUR_TOUFFE),
            //                                HAUTEUR,
            //                                r.Next(HAUTEUR_TOUFFE / 2, HAUTEUR_TOUFFE * 4 / 3),
            //                                FloatRandom(0.2f, 5.0f)));
        }

        /// <summary>
        /// Genere les brins d'herbe
        /// </summary>
        //private void GenereBrinsHerbe()
        //{
        //    _herbes = new List<Herbe>();
        //    int touffe = r.Next(0, LARGEUR - LARGEUR_TOUFFE);
        //    for (int i = 0; i < NB_HERBES; i++)
        //        _herbes.Add(new Herbe(r.Next(touffe, touffe + LARGEUR_TOUFFE),
        //                                HAUTEUR,
        //                                r.Next(HAUTEUR_TOUFFE / 2, HAUTEUR_TOUFFE * 4 / 3),
        //                                FloatRandom(0.2f, 5.0f)));
        //}

        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            float[] col = { couleur.R / 255.0f, couleur.G / 255.0f, couleur.B / 255.0f, 1 };
            float ratio = tailleEcran.Width / (float)tailleEcran.Height;
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_DEPTH);
            using (new Viewport2D(gl, -ratio, -1.0f, ratio, 1.0f))
            {
                // Soleil
                gl.Enable(OpenGL.GL_TEXTURE_2D);
                gl.Color(1.0f, 1.0f, 1.0f, 1.0f);
                gl.Enable(OpenGL.GL_BLEND);
                gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE);
                _textureSoleil.Bind(gl);
                gl.Begin(OpenGL.GL_QUADS);
                gl.TexCoord(0.0f, 1.0f); gl.Vertex(_xSoleil - TAILLE_SOLEIL / 2.0f, _ySoleil - TAILLE_SOLEIL / 2.0f);
                gl.TexCoord(0.0f, 0.0f); gl.Vertex(_xSoleil - TAILLE_SOLEIL / 2.0f, _ySoleil + TAILLE_SOLEIL / 2.0f);
                gl.TexCoord(1.0f, 0.0f); gl.Vertex(_xSoleil + TAILLE_SOLEIL / 2.0f, _ySoleil + TAILLE_SOLEIL / 2.0f);
                gl.TexCoord(1.0f, 1.0f); gl.Vertex(_xSoleil + TAILLE_SOLEIL / 2.0f, _ySoleil - TAILLE_SOLEIL / 2.0f);
                gl.End();

                // Mer
                gl.Disable(OpenGL.GL_TEXTURE_2D);
                gl.Disable(OpenGL.GL_BLEND);
                gl.Color(col[0] * RATIO_MER, col[1] * RATIO_MER, col[2] * RATIO_MER, 1.0f);
                gl.Begin(OpenGL.GL_QUADS);
                gl.TexCoord(0.0f, 1.0f); gl.Vertex(-1.0f * ratio, 0);
                gl.TexCoord(0.0f, 0.01f); gl.Vertex(-1.0f * ratio, -1);
                gl.TexCoord(1.0f, 0.01f); gl.Vertex(1.0f * ratio, -1);
                gl.TexCoord(1.0f, 1.0f); gl.Vertex(1.0f * ratio, 0);
                gl.End();

                // Reflets
                gl.Enable(OpenGL.GL_TEXTURE_2D);
                gl.Enable(OpenGL.GL_BLEND);
                gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE);
                _textureSoleil.Bind(gl);
                gl.Begin(OpenGL.GL_QUADS);
                foreach (Reflet r in _reflets)
                {
                    gl.Color(1.0f, 1.0f, 1.0f, r._alpha);
                    gl.TexCoord(0.0f, 1.0f); gl.Vertex(_xSoleil + r._dx - r._sX, r._y - r._sY);
                    gl.TexCoord(0.0f, 0.01f); gl.Vertex(_xSoleil + r._dx - r._sX, r._y + r._sY);
                    gl.TexCoord(1.0f, 0.01f); gl.Vertex(_xSoleil + r._dx + r._sX, r._y + r._sY);
                    gl.TexCoord(1.0f, 1.0f); gl.Vertex(_xSoleil + r._dx + r._sX, r._y - r._sY);
                }
                gl.End();

                // Colline herbeuse
                if (AFFICHE_FOND)
                {
                    gl.Enable(OpenGL.GL_BLEND);
                    gl.Enable(OpenGL.GL_TEXTURE_2D);
                    gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
                    _textureFond.Bind(gl);
                    gl.Begin(OpenGL.GL_QUADS);
                    gl.TexCoord(0.0f, 1.0f); gl.Vertex(-1.0f * ratio, -1);
                    gl.TexCoord(0.0f, 0.01f); gl.Vertex(-1.0f * ratio, 1);
                    gl.TexCoord(1.0f, 0.01f); gl.Vertex(1.0f * ratio, 1);
                    gl.TexCoord(1.0f, 1.0f); gl.Vertex(1.0f * ratio, -1);
                    gl.End();
                }

                // Flares
                gl.Enable(OpenGL.GL_BLEND);
                gl.Enable(OpenGL.GL_TEXTURE_2D);
                gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE);
                _textureFlares.Bind(gl);
                gl.Begin(OpenGL.GL_QUADS);
                foreach (Flare f in _flares)
                {
                    gl.Color(col[0] * f.rR, col[1] * f.rG, col[2] * f.rB, f._alpha);
                    gl.TexCoord(DECALAGE_TEXTURE * f._texture, 1.0f); gl.Vertex(_xSoleil * f._distance - f._taille / 2, _ySoleil * f._distance - f._taille / 2);
                    gl.TexCoord(DECALAGE_TEXTURE * f._texture, 0.0f); gl.Vertex(_xSoleil * f._distance - f._taille / 2, _ySoleil * f._distance + f._taille / 2);
                    gl.TexCoord(DECALAGE_TEXTURE * (f._texture + 1), 0.0f); gl.Vertex(_xSoleil * f._distance + f._taille / 2, _ySoleil * f._distance + f._taille / 2);
                    gl.TexCoord(DECALAGE_TEXTURE * (f._texture + 1), 1.0f); gl.Vertex(_xSoleil * f._distance + f._taille / 2, _ySoleil * f._distance - f._taille / 2);

                }
                gl.End();
            }

            /*gl.PushMatrix();
			gl.MatrixMode(OpenGL.GL_PROJECTION);
			gl.PushMatrix();
			gl.LoadIdentity();
			gl.Ortho2D(0.0, tailleEcran.Width, tailleEcran.Height, 0.0);
			gl.MatrixMode(OpenGL.GL_MODELVIEW);
			gl.Disable(OpenGL.GL_LIGHTING);
			gl.Disable(OpenGL.GL_DEPTH);
			gl.Enable(OpenGL.GL_TEXTURE_2D);
			gl.Disable(OpenGL.GL_BLEND);
			couleur = getCouleurOpaqueAvecAlpha(couleur, ALPHA);
			float[] col = { couleur.R / 255.0f, couleur.G / 255.0f, couleur.B / 255.0f, 1 };
			gl.Color(col);
			// Affichage du fond
			if (AFFICHE_FOND)
			{
				_textureFond.Bind(gl);
				gl.Begin(OpenGL.GL_QUADS);
				gl.TexCoord(0.0f, 1.0f); gl.Vertex(tailleEcran.Left, tailleEcran.Bottom);
				gl.TexCoord(0.0f, 0.01f); gl.Vertex(tailleEcran.Left, tailleEcran.Bottom - (HAUTEUR_TOUFFE * 4));
				gl.TexCoord(1.0f, 0.01f); gl.Vertex(tailleEcran.Right, tailleEcran.Bottom - (HAUTEUR_TOUFFE * 4));
				gl.TexCoord(1.0f, 1.0f); gl.Vertex(tailleEcran.Right, tailleEcran.Bottom);
				gl.End();
			}

			gl.Color(1.0f, 1.0f, 1.0f, 1.0f);
			gl.Enable(OpenGL.GL_BLEND);
			gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE);

			_textureSoleil.Bind(gl);
			gl.Begin(OpenGL.GL_QUADS);
			gl.TexCoord(0.0f, 1.0f); gl.Vertex(_xSoleil - TAILLE_SOLEIL / 2, _ySoleil - TAILLE_SOLEIL / 2);
			gl.TexCoord(0.0f, 0.0f); gl.Vertex(_xSoleil - TAILLE_SOLEIL / 2, _ySoleil + TAILLE_SOLEIL / 2);
			gl.TexCoord(1.0f, 0.0f); gl.Vertex(_xSoleil + TAILLE_SOLEIL / 2, _ySoleil + TAILLE_SOLEIL / 2);
			gl.TexCoord(1.0f, 1.0f); gl.Vertex(_xSoleil + TAILLE_SOLEIL / 2, _ySoleil - TAILLE_SOLEIL / 2);
			gl.End();
			float dx = CENTREX - _xSoleil;
			float dy = CENTREY - _ySoleil;


			gl.Disable(OpenGL.GL_TEXTURE_2D);
			gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
			//gl.Color(0, 0, 0, 0.5);
			//foreach (Herbe h in _herbes)
			//	h.Affiche(gl, _vent);

			//gl.Color(col);
			gl.Enable(OpenGL.GL_BLEND);
			gl.Enable(OpenGL.GL_TEXTURE_2D);
			gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE);
			_textureFlares.Bind(gl);
			gl.Begin(OpenGL.GL_QUADS);
			foreach (Flare f in _flares)
			{
				gl.Color(col[0] * f.rR, col[1] * f.rG, col[2] * f.rB, f._alpha);
				gl.TexCoord(DECALAGE_TEXTURE * f._texture, 1.0f); gl.Vertex(_xSoleil + dx * f._distance - f._taille / 2, _ySoleil + dy * f._distance - f._taille / 2);
				gl.TexCoord(DECALAGE_TEXTURE * f._texture, 0.0f); gl.Vertex(_xSoleil + dx * f._distance - f._taille / 2, _ySoleil + dy * f._distance + f._taille / 2);
				gl.TexCoord(DECALAGE_TEXTURE * (f._texture + 1), 0.0f); gl.Vertex(_xSoleil + dx * f._distance + f._taille / 2, _ySoleil + dy * f._distance + f._taille / 2);
				gl.TexCoord(DECALAGE_TEXTURE * (f._texture + 1), 1.0f); gl.Vertex(_xSoleil + dx * f._distance + f._taille / 2, _ySoleil + dy * f._distance - f._taille / 2);

			}
			gl.End();
			gl.MatrixMode(OpenGL.GL_PROJECTION);
			gl.PopMatrix();
			gl.MatrixMode(OpenGL.GL_MODELVIEW);
			gl.PopMatrix();
            */
#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

        public override bool ClearBackGround(OpenGL gl, Color c)
        {
            c = OpenGLColor.GetCouleurOpaqueAvecAlpha(c, ALPHA);

            gl.ClearColor(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f, 1.0f);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT);
            return true;
        }
        /// <summary>
        /// Pression sur une touche, retourner true si l'objet a traite, false = fin de l'economiseur
        /// </summary>
        /// <param name="f"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        /*public override bool KeyDown(Form f, Keys k)
        {
            switch (k)
            {
                case TOUCHE_INVERSER:
                    AFFICHE_FOND = !AFFICHE_FOND;
                    c.setParametre( "Affiche Fond", AFFICHE_FOND);
                    return true;
            }
            return base.KeyDown(f, k); ;
        }
*/
        public override void AppendHelpText(StringBuilder s)
        {
            //s.Append(Resources.AideEte);
        }
        /// <summary>
        /// Deplacement
        /// </summary>
        /// <param name="maintenant"></param>
        /// <param name="tailleEcran"></param>
        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.DEPLACE);
#endif
            Varie(ref _vent, -100f, 300f, 0.4f, maintenant.intervalleDepuisDerniereFrame); //_vent = (float)Math.Cos((double)maintenant._temps.Ticks * 0.00000005) * 0.5f + 0.4f;
            _xSoleil += VX_SOLEIL * maintenant.intervalleDepuisDerniereFrame;
            _ySoleil += VY_SOLEIL * maintenant.intervalleDepuisDerniereFrame;
            /*if (_xSoleil > LARGEUR + TAILLE_SOLEIL || _ySoleil < -TAILLE_SOLEIL)
                Init();*/

            if (timer.Ecoule())
                if (_reflets.Count < NB_REFLETS)
                {
                    Reflet r = new Reflet(TAILLE_SOLEIL);
                    _reflets.Add(r);
                }

            for (int i = 0; i < _reflets.Count; i++)
            {
                _reflets[i]._y += maintenant.intervalleDepuisDerniereFrame * _reflets[i]._vy;
                if (_reflets[i]._y < -1)
                    _reflets[i] = new Reflet(TAILLE_SOLEIL);
                _reflets[i]._vy *= 1.0025f;
                //_reflets[i]._dx += maintenant.intervalleDepuisDerniereFrame * _reflets[i]._vx;
                _reflets[i]._sX += maintenant.intervalleDepuisDerniereFrame * _reflets[i]._dSX;
                _reflets[i]._sY += maintenant.intervalleDepuisDerniereFrame * _reflets[i]._dSY;
            }
#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif
        }

#if TRACER
        public override string DumpRender()
        {
            return base.DumpRender() + _vent.ToString("Vent:  000.000");
        }
#endif
    }
}


