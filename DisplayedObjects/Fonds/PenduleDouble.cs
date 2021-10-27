using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;
using ClockScreenSaverGL.Config;
using SharpGL;
using SharpGL.SceneGraph.Assets;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    class PenduleDouble : Fond, IDisposable
    {
        public const float TOUR_COMPLET = (float)(Math.PI * 2.0);
        #region Parametres
        public const string CAT = "Double pendule";
        private CategorieConfiguration c;
        float TAILLE_LIGNE_SEGMENTS;
        float ALPHA_SEGMENT;
        float MIN_LONGUEUR;
        float MAX_LONGUEUR;
        float RAYON_TOTAL;
        float MIN_VITESSE;
        float MAX_VITESSE;
        float INTENSITE_FORCE;
        int NB_MAX_TRACE;
        int DELAI_TRACE;
        float TAILLE_LIGNE_TRACES;
        float TAILLE_PENDULE;
        #endregion
        private const int NB_PENDULES = 2; // La methode de calcule fonctionne uniquement avec 2 pendules
        private float[] _longueur;
        private float[] _angle;
        private float[] _vitesse;
        private float[] _masse;
        int _nbTraces;
        float[] _xTrace;
        float[] _yTrace;
        private TimerIsole _timerTrace;
        private Texture _texture = new Texture();
        /// <summary>
        /// Lecture de la configuration
        /// </summary>
        /// <returns></returns>
        public override CategorieConfiguration getConfiguration()
        {
            if (c == null)
            {
                c = Configuration.getCategorie(CAT);
                TAILLE_LIGNE_SEGMENTS = c.getParametre("Largeur pendules", 0.2f, (a) => { TAILLE_LIGNE_SEGMENTS = (float)Convert.ToDouble(a); });
                ALPHA_SEGMENT = c.getParametre("Alpha segment", 0.5f, (a) => { ALPHA_SEGMENT = (float)Convert.ToDouble(a); });
                INTENSITE_FORCE = c.getParametre("Intensite force", 2.0f, (a) => { INTENSITE_FORCE = (float)Convert.ToDouble(a); });
                MAX_LONGUEUR = c.getParametre("Longueur max", 0.25f);
                MIN_LONGUEUR = c.getParametre("Longueur min", 0.05f);
                RAYON_TOTAL = c.getParametre("Rayon Total", 0.95f);
                MAX_VITESSE = c.getParametre("Vitesse max", 0.3f);
                MIN_VITESSE = c.getParametre("Vitesse min", 0.1f);
                NB_MAX_TRACE = c.getParametre("Nb Max Trace", 10000);
                DELAI_TRACE = c.getParametre("Delai Trace", 20, (a) => { DELAI_TRACE = Convert.ToInt32(a); _timerTrace = new TimerIsole(DELAI_TRACE); });
                TAILLE_PENDULE = c.getParametre("Taille pendule", 0.05f, (a) => { TAILLE_PENDULE = (float)Convert.ToDouble(a); });
                TAILLE_LIGNE_TRACES = c.getParametre("Largeur ligne trace", 4.0f, (a) => { TAILLE_LIGNE_TRACES = (float)Convert.ToDouble(a); });
            }
            return c;
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="gl"></param>
        public PenduleDouble(OpenGL gl) : base(gl)
        {
            getConfiguration();
            _texture.Create(gl, c.getParametre("Pendule", Config.Configuration.getImagePath("balle.png")));
        }
        public override void Dispose()
        {
            base.Dispose();
            _texture.Destroy(_gl);
        }

        /// <summary>
        /// Initialisation
        /// </summary>
        /// <param name="gl"></param>
        /// <returns></returns>
        public override void Init(OpenGL gl)
        {
            _timerTrace = new TimerIsole(DELAI_TRACE, true);
            _longueur = new float[NB_PENDULES];
            _angle = new float[NB_PENDULES];
            _vitesse = new float[NB_PENDULES];
            _masse = new float[NB_PENDULES];

            // Generer les segments traceurs, tailles et vitesses variables
            float longueurTotale = 0;
            for (int i = 0; i < NB_PENDULES; i++)
            {
                _longueur[i] = 0.1f + FloatRandom(MIN_LONGUEUR, MAX_LONGUEUR);
                _vitesse[i] = FloatRandom(MIN_VITESSE, MAX_VITESSE);
                _angle[i] = (float)Math.PI * FloatRandom(0.1f, 0.5f);
                _masse[i] = FloatRandom(0.5f, 1.5f);
                longueurTotale += _longueur[i];
            }

            // Normaliser la longueur des segments pour que le total fasse RAYON_TOTAL
            for (int i = 0; i < NB_PENDULES; i++)
            {
                _longueur[i] = _longueur[i] * RAYON_TOTAL / longueurTotale;
            }

            // Traces
            _nbTraces = 0;
            _xTrace = new float[NB_MAX_TRACE];
            _yTrace = new float[NB_MAX_TRACE];
        }

        public override bool ClearBackGround(OpenGL gl, Color c)
        {
            gl.ClearColor(c.R / 512.0f, c.G / 512.0f, c.B / 512.0f, 1);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            return true;
        }

        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {

#if TRACER
            RenderStart(CHRONO_TYPE.DEPLACE);
#endif
            float intervalle = maintenant.intervalleDepuisDerniereFrame;

            // Un intervalle trop long pourrait faire foirer la simulation
            if (intervalle > 0.05f)
                intervalle = 0.05f;

            double g = -INTENSITE_FORCE;

            double ddth1 = -g * (2 * _masse[0] + _masse[1]) * Math.Sin(_angle[0]);
            ddth1 -= g * _masse[1] * Math.Sin(_angle[0] - 2 * _angle[1]);
            ddth1 -= 2 * _masse[1] * _vitesse[1] * _vitesse[1] * _longueur[1] * Math.Sin(_angle[0] - _angle[1]);
            ddth1 -= _masse[1] * _vitesse[0] * _vitesse[0] * _longueur[0] * Math.Sin(2 * (_angle[0] - _angle[1]));
            ddth1 /= _longueur[0] * (2 * _masse[0] + _masse[1] - _masse[1] * Math.Cos(2 * (_angle[0] - _angle[1])));

            double ddth2 = (_masse[0] + _masse[1]) * _vitesse[0] * _vitesse[0] * _longueur[0];
            ddth2 += g * (_masse[0] + _masse[1]) * Math.Cos(_angle[0]);
            ddth2 += _masse[1] * _vitesse[1] * _vitesse[1] * _longueur[1] * Math.Cos(_angle[0] - _angle[1]);
            ddth2 = ddth2 * 2 * Math.Sin(_angle[0] - _angle[1]);
            ddth2 /= (_longueur[1] * (2 * _masse[0] + _masse[1] - _masse[1] * Math.Cos(2 * (_angle[0] - _angle[1]))));

            _vitesse[0] += (float)ddth1 * intervalle;
            _angle[0] += _vitesse[0] * intervalle;

            _vitesse[1] += (float)ddth2 * intervalle;
            _angle[1] += _vitesse[1] * intervalle;


            if (_timerTrace.Ecoule())
            {
                // Tableau plein?
                if (_nbTraces >= NB_MAX_TRACE)
                {
                    // Supprimer la plus ancienne trace
                    for (int i = 0; i < NB_MAX_TRACE - 1; i++)
                    {
                        _xTrace[i] = _xTrace[i + 1];
                        _yTrace[i] = _yTrace[i + 1];
                    }
                    _nbTraces--;
                }

                // Ajoute une nouvelle trace a la suite des existantes
                float x = 0;
                float y = 0;
                for (int i = 0; i < NB_PENDULES; i++)
                {
                    x += (float)Math.Sin(_angle[i]) * _longueur[i];
                    y += (float)Math.Cos(_angle[i]) * _longueur[i];
                }

                _xTrace[_nbTraces] = x;
                _yTrace[_nbTraces] = y;
                _nbTraces++;
            }

#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif
        }

        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            float ratio = (float)tailleEcran.Width / (float)tailleEcran.Height;

            using (new Viewport2D(gl, -ratio, -1.1f, ratio, 0.9f))
            {
                gl.Disable(OpenGL.GL_LIGHTING);
                gl.Disable(OpenGL.GL_DEPTH);
                gl.Disable(OpenGL.GL_TEXTURE_2D);
                gl.Enable(OpenGL.GL_LINE_SMOOTH);

                // Les traces
                gl.Enable(OpenGL.GL_BLEND);
                gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
                gl.LineWidth(TAILLE_LIGNE_TRACES);
                gl.Begin(OpenGL.GL_LINE_STRIP);
                {
                    for (int i = 0; i < _nbTraces; i++)
                    {
                        gl.Color(1.0f, 1.0f, 1.0f, (float)i / (float)_nbTraces);
                        gl.Vertex(_xTrace[i], _yTrace[i]);
                    }
                }
                gl.End();

                // Tiges des pendules
                PointF[] p = getMultilinePolygon();
                gl.Color(0, 0, 0, ALPHA_SEGMENT);
                gl.Begin(OpenGL.GL_QUAD_STRIP);
                for (int i = 0; i < NB_PENDULES; i++)
                {
                    int POINT = i * 4;
                    gl.Vertex(p[POINT + 3].X, p[POINT + 3].Y);
                    gl.Vertex(p[POINT + 0].X, p[POINT + 0].Y);
                    gl.Vertex(p[POINT + 2].X, p[POINT + 2].Y);
                    gl.Vertex(p[POINT + 1].X, p[POINT + 1].Y);
                }
                gl.End();

                gl.Enable(OpenGL.GL_BLEND);
                gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
                gl.Enable(OpenGL.GL_TEXTURE_2D);
                _texture.Bind(gl);
                float[] col = { couleur.R / 256.0f, couleur.G / 256.0f, couleur.B / 256.0f };
                gl.Color(col);
                float x = 0.0f;
                float y = 0.0f;

                gl.Begin(OpenGL.GL_QUADS);
                gl.TexCoord(0.0f, 0.0f); gl.Vertex(x - TAILLE_PENDULE, y + TAILLE_PENDULE);
                gl.TexCoord(0.0f, 1.0f); gl.Vertex(x - TAILLE_PENDULE, y - TAILLE_PENDULE);
                gl.TexCoord(1.0f, 1.0f); gl.Vertex(x + TAILLE_PENDULE, y - TAILLE_PENDULE);
                gl.TexCoord(1.0f, 0.0f); gl.Vertex(x + TAILLE_PENDULE, y + TAILLE_PENDULE);


                // Boules
                for (int i = 0; i < NB_PENDULES; i++)
                {
                    float taille = (float)Math.Sqrt(_masse[i]);
                    x += (float)Math.Sin(_angle[i]) * _longueur[i];
                    y += (float)Math.Cos(_angle[i]) * _longueur[i];
                    gl.TexCoord(0.0f, 0.0f); gl.Vertex(x - TAILLE_PENDULE*taille, y + TAILLE_PENDULE*taille);
                    gl.TexCoord(0.0f, 1.0f); gl.Vertex(x - TAILLE_PENDULE*taille, y - TAILLE_PENDULE*taille);
                    gl.TexCoord(1.0f, 1.0f); gl.Vertex(x + TAILLE_PENDULE*taille, y - TAILLE_PENDULE*taille);
                    gl.TexCoord(1.0f, 0.0f); gl.Vertex(x + TAILLE_PENDULE*taille, y + TAILLE_PENDULE*taille);
                }
                gl.End();
            }

#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

        /// <summary>
        /// Calcule un ensemble de points pour tracer une multiline (de plusieurs segments) epaisse
        /// Si la ligne contient N points, la "ligne epaisse" contient (N-1) * 4 points
        /// 
        /// (0), (1), (2)... les points de la ligne
        /// 
        ///     p0------------p1     p4------------p5     p8------------p9
        ///     |             |      |             |      |             |
        ///    (0) segment 1 (1)    (1) segment 2 (2)    (2) segment 3 (3)
        ///     |             |      |             |      |             |
        ///     p3------------p2     p7------------p6     p11-----------p10
        ///     
        /// </summary>
        /// <param name="multiline"></param>
        /// <param name="largeurLigne"></param>
        /// <returns></returns>
        public PointF[] getMultilinePolygon()
        {
            PointF[] p = new PointF[(NB_PENDULES) * 4];
            float x = 0;
            float y = 0;
            PointF p1 = new PointF();
            PointF p2 = new PointF();

            // Les perpendiculaires aux lignes
            for (int i = 0; i < NB_PENDULES; i++)
            {
                p1.X = x;
                p1.Y = y;
                x += (float)Math.Sin(_angle[i]) * _longueur[i];
                y += (float)Math.Cos(_angle[i]) * _longueur[i];
                p2.X = x;
                p2.Y = y;
                int POINT = i * 4;
                Epicycle.calculePerpendiculaires(p1, p2, TAILLE_LIGNE_SEGMENTS, out p[POINT + 0], out p[POINT + 1], out p[POINT + 2], out p[POINT + 3]);
            }

            return p;
        }
    }
}
