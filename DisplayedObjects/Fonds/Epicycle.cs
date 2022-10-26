using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;
using SharpGL;
using System;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    class Epicycle : Fond, IDisposable
    {
        public const float TOUR_COMPLET = (float)(Math.PI * 2.0);
        const double ANGLE_DROIT = Math.PI * 0.5;

        #region Parametres
        public const string CAT = "Epicycle";
        private CategorieConfiguration c;
        int NBMAX_SEGMENTS;
        int NBMIN_SEGMENTS;
        float MIN_LONGUEUR;
        float MAX_LONGUEUR;
        float MIN_VITESSE;
        float MAX_VITESSE;
        float TAILLE_LIGNE_SEGMENTS;
        float TAILLE_LIGNE_TRACES;
        float ALPHA_SEGMENT;
        float RAYON_TOTAL;
        int NB_MAX_TRACE;
        int DELAI_TRACE;
        #endregion

        int _nbSegments;
        float[] _longueurSegments;
        float[] _angleSegments;
        float[] _vitesseSegments;

        int _nbTraces;
        float[] _xTrace;
        float[] _yTrace;
        //Color[] _couleurs;
        //Color _derniereCouleur = Color.FromArgb(0,0,0,0) ;

        bool frameInitiale = true;  // Ne pas tracer la frame initiale, pb dus au temps d'initialisation du programme
        private TimerIsole _timerTrace;

        /// <summary>
        /// Lecture de la configuration
        /// </summary>
        /// <returns></returns>
        public override CategorieConfiguration getConfiguration()
        {
            if (c == null)
            {
                c = Configuration.getCategorie(CAT);
                NBMAX_SEGMENTS = c.getParametre("NB Max segments", 3);
                NBMIN_SEGMENTS = c.getParametre("NB Min segments", 2);
                RAYON_TOTAL = c.getParametre("Rayon Total", 0.95f);
                MAX_LONGUEUR = c.getParametre("Longueur max", 0.25f);
                MIN_LONGUEUR = c.getParametre("Longueur min", 0.05f);
                MAX_VITESSE = c.getParametre("Vitesse max", 3.0f);
                MIN_VITESSE = c.getParametre("Vitesse min", 1.5f);
                TAILLE_LIGNE_SEGMENTS = c.getParametre("Largeur ligne segment", 25.0f, (a) => { TAILLE_LIGNE_SEGMENTS = (float)Convert.ToDouble(a); });
                TAILLE_LIGNE_TRACES = c.getParametre("Largeur ligne trace", 4.0f, (a) => { TAILLE_LIGNE_TRACES = (float)Convert.ToDouble(a); });
                ALPHA_SEGMENT = c.getParametre("Alpha segment", 0.5f, (a) => { ALPHA_SEGMENT = (float)Convert.ToDouble(a); });
                NB_MAX_TRACE = c.getParametre("Nb Max Trace", 10000);
                DELAI_TRACE = c.getParametre("Delai Trace", 20, (a) => { DELAI_TRACE = Convert.ToInt32(a); _timerTrace = new TimerIsole(DELAI_TRACE); });
            }
            return c;
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="gl"></param>
        public Epicycle(OpenGL gl) : base(gl)
        {
            getConfiguration();
        }

        /// <summary>
        /// Initialisation
        /// </summary>
        /// <param name="gl"></param>
        /// <returns></returns>
        protected override void Init(OpenGL gl)
        {
            _timerTrace = new TimerIsole(DELAI_TRACE, true);
            _nbSegments = r.Next(NBMIN_SEGMENTS, NBMAX_SEGMENTS + 1);
            _longueurSegments = new float[_nbSegments];
            _angleSegments = new float[_nbSegments];
            _vitesseSegments = new float[_nbSegments];

            // Generer les segments traceurs, tailles et vitesses variables
            float longueurTotale = 0;
            for (int i = 0; i < _nbSegments; i++)
            {
                if (i == 0)
                {
                    _longueurSegments[i] = 0.2f + FloatRandom(MIN_LONGUEUR, MAX_LONGUEUR);
                    _vitesseSegments[i] = FloatRandom(MIN_VITESSE, MAX_VITESSE);
                }
                else
                {
                    _longueurSegments[i] = _longueurSegments[i - 1] * FloatRandom(0.3f, 1.2f);
                    _vitesseSegments[i] = _vitesseSegments[i - 1] * FloatRandom(0.9f, 6.0f);
                }
                _angleSegments[i] = 0;
                longueurTotale += _longueurSegments[i];
            }

            // Normaliser la longueur des segments pour que le total fasse RAYON_TOTAL
            for (int i = 0; i < _nbSegments; i++)
            {
                _longueurSegments[i] = _longueurSegments[i] * RAYON_TOTAL / longueurTotale;
            }

            // Traces
            _nbTraces = 0;
            _xTrace = new float[NB_MAX_TRACE];
            _yTrace = new float[NB_MAX_TRACE];
            //_couleurs = new Color[NB_MAX_TRACE];
        }

        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {

#if TRACER
            RenderStart(CHRONO_TYPE.DEPLACE);
#endif
            if (!frameInitiale) // La premiere frame dure qq fois longtemps, ce qui fait des truc bizarres dans le dessin
            {
                // Deplacer les segments traceurs
                for (int i = 0; i < _nbSegments; i++)
                    _angleSegments[i] += _vitesseSegments[i] * maintenant.intervalleDepuisDerniereFrame;

                if (_timerTrace.Ecoule())
                {
                    // Ajoute une nouvelle trace a la suite des existantes
                    float x = 0;
                    float y = 0;
                    for (int i = 0; i < _nbSegments; i++)
                    {
                        x += (float)Math.Sin(_angleSegments[i]) * _longueurSegments[i];
                        y += (float)Math.Cos(_angleSegments[i]) * _longueurSegments[i];
                    }

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

                    _xTrace[_nbTraces] = x;
                    _yTrace[_nbTraces] = y;
                    //_couleurs[_nbTraces] = _derniereCouleur ;
                    _nbTraces++;
                }
            }
            frameInitiale = false;
#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif
        }

        public override bool ClearBackGround(OpenGL gl, Color c)
        {
            gl.ClearColor(c.R / 512.0f, c.G / 512.0f, c.B / 512.0f, 1);
            //gl.ClearColor(0,0,0, 1);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            return true;
        }

        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            float ratio = (float)tailleEcran.Width / (float)tailleEcran.Height;

            using (new Viewport2D(gl, -ratio, -1.0f, ratio, 1.0f))
            {
                gl.Disable(OpenGL.GL_LIGHTING);
                gl.Disable(OpenGL.GL_DEPTH);
                gl.Disable(OpenGL.GL_TEXTURE_2D);
                gl.Enable(OpenGL.GL_LINE_SMOOTH);

                // Dessine les segments traceurs
                PointF[] p = getMultilinePolygon();
                gl.Color(1.0f * ALPHA_SEGMENT, 1.0f * ALPHA_SEGMENT, 1.0f * ALPHA_SEGMENT);
                gl.Begin(OpenGL.GL_TRIANGLES);
                for (int i = 0; i < _nbSegments; i++)
                {
                    int POINT = i * 4;

                    // P0 P1 P3
                    gl.Vertex(p[POINT].X, p[POINT].Y);
                    gl.Vertex(p[POINT + 1].X, p[POINT + 1].Y);
                    gl.Vertex(p[POINT + 3].X, p[POINT + 3].Y);

                    // P3 P1 P2
                    gl.Vertex(p[POINT + 3].X, p[POINT + 3].Y);
                    gl.Vertex(p[POINT + 1].X, p[POINT + 1].Y);
                    gl.Vertex(p[POINT + 2].X, p[POINT + 2].Y);

                    if (i < _nbSegments - 1)
                    {
                        // Tracer le joint entre les lignes
                        // P1 P4 P7
                        gl.Vertex(p[POINT + 1].X, p[POINT + 1].Y);
                        gl.Vertex(p[POINT + 4].X, p[POINT + 4].Y);
                        gl.Vertex(p[POINT + 7].X, p[POINT + 7].Y);

                        // P1 P2 P7
                        gl.Vertex(p[POINT + 1].X, p[POINT + 1].Y);
                        gl.Vertex(p[POINT + 2].X, p[POINT + 2].Y);
                        gl.Vertex(p[POINT + 7].X, p[POINT + 7].Y);

                        // P2 P4 P7
                        gl.Vertex(p[POINT + 2].X, p[POINT + 2].Y);
                        gl.Vertex(p[POINT + 4].X, p[POINT + 4].Y);
                        gl.Vertex(p[POINT + 7].X, p[POINT + 7].Y);
                    }
                }
                gl.End();

                // Les traces
                gl.Enable(OpenGL.GL_BLEND);
                gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
                //_derniereCouleur = couleur; // Ca sera la couleur du prochain segment de trace
                gl.LineWidth(TAILLE_LIGNE_TRACES);
                gl.Begin(OpenGL.GL_LINE_STRIP);
                {
                    for (int i = 0; i < _nbTraces; i++)
                    {
                        gl.Color(1.0f, 1.0f, 1.0f, (float)i / (float)_nbTraces);
                        //gl.Color(_couleurs[i].R/255.0f, _couleurs[i].G/255.0f, _couleurs[i].B/255.0f, (float)i / (float)_nbTraces);
                        gl.Vertex(_xTrace[i], _yTrace[i]);
                    }
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
            PointF[] p = new PointF[(_nbSegments) * 4];
            float x = 0;
            float y = 0;
            PointF p1 = new PointF();
            PointF p2 = new PointF();

            // Les perpendiculaires aux lignes
            for (int i = 0; i < _nbSegments; i++)
            {
                p1.X = x;
                p1.Y = y;
                x += (float)Math.Sin(_angleSegments[i]) * _longueurSegments[i];
                y += (float)Math.Cos(_angleSegments[i]) * _longueurSegments[i];
                p2.X = x;
                p2.Y = y;
                int POINT = i * 4;
                calculePerpendiculaires(p1, p2, TAILLE_LIGNE_SEGMENTS, out p[POINT + 0], out p[POINT + 1], out p[POINT + 2], out p[POINT + 3]);
            }

            return p;
        }

        /// <summary>
        /// Calcule les points perpendiculaires autour d'un segment pour tracer un segment epais sous forme d'un rectangle
        /// 
        ///     per1--------per2
        ///     |           | 
        ///     P1          P2 
        ///     |           | 
        ///     per4--------per3
        ///     
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="largeur"></param>
        /// <param name="per1"></param>
        /// <param name="per2"></param>
        /// <param name="per3"></param>
        /// <param name="per4"></param>
        public static void calculePerpendiculaires(PointF p1, PointF p2, float largeur, out PointF per1, out PointF per2, out PointF per3, out PointF per4)
        {
            double angleAlpha;
            if (p1.X != p2.X)
            {
                angleAlpha = Math.Atan((p2.Y - p1.Y) / (p2.X - p1.X));
            }
            else
            {
                if (p1.Y > p2.Y)
                    angleAlpha = -ANGLE_DROIT;
                else
                    if (p1.Y < p2.Y)
                    angleAlpha = ANGLE_DROIT;
                else
                {
                    // Les deux points sont confondus, on ne peut pas calculer de perpendiculaire
                    per1 = new PointF();
                    per2 = new PointF();
                    per3 = new PointF();
                    per4 = new PointF();
                    return;
                }
            }

            angleAlpha += ANGLE_DROIT;

            float fX = largeur * (float)Math.Cos(angleAlpha);
            float fY = largeur * (float)Math.Sin(angleAlpha);

            per1 = new PointF(p1.X - fX, p1.Y - fY);
            per2 = new PointF(p2.X - fX, p2.Y - fY);
            per3 = new PointF(p2.X + fX, p2.Y + fY);
            per4 = new PointF(p1.X + fX, p1.Y + fY);
        }

#if TRACER
        public override String DumpRender()
        {
            string res = base.DumpRender() + " Nb Traces:" + _nbTraces;

            for (int i = 0; i < _nbSegments; i++)
                res += ", " + _longueurSegments[i] + "/" + _vitesseSegments[i];
            return res;
        }

#endif
    }
}
