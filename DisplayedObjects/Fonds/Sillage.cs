using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;
using ClockScreenSaverGL.DisplayedObjects.OpenGLUtils;
using SharpGL;
using System;
using System.Collections.Generic;
using System.Drawing;

///
/// Affiche un sillage de cercle qui s'agrandissent en devenant de plus en plus transparents
namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    internal class Sillage : Fond
    {
        #region Parametres
        public const string CAT = "Sillage";
        protected CategorieConfiguration c;
        private float VITESSE_MIN, VITESSE_MAX;
        private float VITESSE_ALPHA;
        private float VITESSE_TAILLE;
        private bool ADDITIVE;
        private bool WIRE_FRAME;
        private int NB_SECTEURS;
        private float LARGEUR_CERCLE;
        private float DISTANCE_POINTS;
        private int MAX_POINTS;
        #endregion

        private readonly float RADIANS = (float)(Math.PI * 2.0);
        private readonly TrajectoireDiagonale _trajectoire;
        private sealed class PointSillage
        {
            public float x, y, taille, alpha;
            public Color couleur;
        }

        private readonly List<PointSillage> _points;
        private Vecteur2D _dernierPoint = Vecteur2D.ZERO;
        private Color _derniereCouleur = Color.Black;
        private readonly Rectangle _rectangleViewPort = new Rectangle(-1, -1, 2, 2);

        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                VITESSE_MAX = c.GetParametre("Vitesse Max", 0.25f, (a) => { VITESSE_MAX = (float)Convert.ToDouble(a); });
                VITESSE_MIN = c.GetParametre("Vitesse Min", 0.1f, (a) => { VITESSE_MIN = (float)Convert.ToDouble(a); });
                VITESSE_ALPHA = c.GetParametre("Vitesse Alpha", 0.41f, (a) => { VITESSE_ALPHA = (float)Convert.ToDouble(a); });
                VITESSE_TAILLE = c.GetParametre("Vitesse Taille", 0.001f, (a) => { VITESSE_TAILLE = (float)Convert.ToDouble(a); });
                DISTANCE_POINTS = c.GetParametre("Distance Point", 0.01f, (a) => { DISTANCE_POINTS = (float)Convert.ToDouble(a); });
                LARGEUR_CERCLE = c.GetParametre("LargeurCercle", 0.003f, (a) => { LARGEUR_CERCLE = (float)Convert.ToDouble(a); });
                ADDITIVE = c.GetParametre("Additif", false, (a) => { ADDITIVE = Convert.ToBoolean(a); });
                WIRE_FRAME = c.GetParametre("Fil de fer", false, (a) => { WIRE_FRAME = Convert.ToBoolean(a); });
                NB_SECTEURS = c.GetParametre("Details Secteurs", 20, (a) => { NB_SECTEURS = Convert.ToInt32(a); });
                MAX_POINTS = c.GetParametre("Max points", 100, (a) => { MAX_POINTS = Convert.ToInt32(a); });
            }
            return c;
        }

        public Sillage(OpenGL gl) : base(gl)
        {
            float vitesse = FloatRandom(VITESSE_MIN, VITESSE_MAX);
            float angle = FloatRandom(5, 85) * RADIANS / 360.0f; // Eviter les direction vertical ou horizontal

            float vx = vitesse * (float)Math.Cos(angle);
            float vy = vitesse * (float)Math.Sin(angle);

            _trajectoire = new TrajectoireDiagonale(FloatRandom(-1, 1), FloatRandom(-1, 1), vx, vy);
            _points = new List<PointSillage>();
        }


        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
            _trajectoire.Avance(_rectangleViewPort, new SizeF(0, 0), maintenant);
            Vecteur2D position = _trajectoire.vecteur();

            if (_dernierPoint.Distance(position) > DISTANCE_POINTS)
            {
                // Limiter le nombre de points
                if (_points.Count >= MAX_POINTS)
                    _points.RemoveRange(0, _points.Count - MAX_POINTS + 1);

                // Ajoute un point de sillage
                _points.Add(new PointSillage()
                {
                    x = _trajectoire._Px,
                    y = _trajectoire._Py,
                    taille = 0.01f,
                    alpha = 1.0f,
                    couleur = GetColorWithHueChange(_derniereCouleur, 0.5f)
                });

                _dernierPoint = position;
            }

            int i = 0;
            while (i < _points.Count)
            {
                PointSillage point = _points[i];
                point.alpha *= 1.0f - (VITESSE_ALPHA * maintenant.intervalleDepuisDerniereFrame);
                if (point.alpha > 0.01f)
                {
                    point.taille += VITESSE_TAILLE;
                    i++;
                }
                else
                {
                    _points.RemoveAt(i);
                }
            }
        }

        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            _derniereCouleur = couleur;

            gl.LoadIdentity();
            float ratio = tailleEcran.Width / (float)tailleEcran.Height;
            using (new Viewport2D(gl, _rectangleViewPort.Left, _rectangleViewPort.Top, _rectangleViewPort.Right, _rectangleViewPort.Bottom))
            {
                gl.Disable(OpenGL.GL_LIGHTING);
                gl.Disable(OpenGL.GL_TEXTURE_2D);
                gl.Disable(OpenGL.GL_FOG);
                gl.Disable(OpenGL.GL_DEPTH);
                gl.Enable(OpenGL.GL_BLEND);
                gl.BlendFunc(OpenGL.GL_SRC_ALPHA, ADDITIVE ? OpenGL.GL_ONE : OpenGL.GL_ONE_MINUS_SRC_ALPHA);

                if (WIRE_FRAME)
                    gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_LINE);

                gl.Color(couleur.R, couleur.G, couleur.B, (byte)255);
                Cercle(gl, _trajectoire._Px, _trajectoire._Py, 0.01f, NB_SECTEURS, ratio);

                // Tracer les anneaux
                foreach (PointSillage point in _points)
                {
                    gl.Color(point.couleur.R, point.couleur.G, point.couleur.B, (byte)(point.alpha * 255));
                    Cercle(gl, point.x, point.y, point.taille, NB_SECTEURS, ratio);
                }

                if (WIRE_FRAME)
                    gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);
            }
#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

        private void Cercle(OpenGL gl, float x, float y, float taille, int nbSegments, float ratioXY)
        {
            gl.PushMatrix();
            gl.Translate(x, y, 0);
            gl.PointSize(8);

            float l1 = taille - LARGEUR_CERCLE;
            float l2 = taille + LARGEUR_CERCLE;
            using (new GLBegin(gl, OpenGL.GL_QUAD_STRIP))

                for (int i = 0; i <= nbSegments; i++)
                {
                    float angle = (i / (float)nbSegments) * RADIANS;
                    float sin = (float)Math.Sin(angle);
                    float cos = (float)Math.Cos(angle);
                    gl.Vertex(l1 * cos, l1 * ratioXY * sin, 0);
                    gl.Vertex(l2 * cos, l2 * ratioXY * sin, 0);
                }
            gl.PopMatrix();
        }

        public override void FillConsole(OpenGL gl)
        {
            base.FillConsole(gl);
            Console.GetInstance(gl).AddLigne(Color.Red, "Nb Cercles " + _points.Count);
        }
    }
}
