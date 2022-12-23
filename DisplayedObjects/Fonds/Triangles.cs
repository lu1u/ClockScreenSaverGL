using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;
using SharpGL;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    internal class Triangles : Fond
    {
        public const string CAT = "Triangles";
        protected CategorieConfiguration c;

        private int NB_TRIANGLES;
        private float MAX_DISTANCE;
        private float MIN_DISTANCE;
        private float DISTANCE_CIBLE;
        private float LARGEUR_LIGNE;
        private float TAILLE_X;
        private float TAILLE_Y;
        private float FORCE;
        private float AMORTISSEMENT;
        private float CHANGE_COULEUR;
        private readonly Triangle[] _triangles;
        private readonly float MIN_X = 0.0f;
        private readonly float MAX_X = 1.0f;
        private readonly float MIN_Y = 0.0f;
        private readonly float MAX_Y = 0.7f;

        private int NB_PLUS_PROCHES = 3;
        private int indicePointEnCours = 0;
        sealed private class Triangle
        {
            public float _x;
            public float _y;
            public float _vx;
            public float _vy;
            public List<Distance> _plusProches;
            public float changeCouleur;
            public Triangle(float x, float y)
            {
                _x = x;
                _y = y;
                _plusProches = new List<Distance>();
                changeCouleur = FloatRandom(-1.0f, 1.0f);
            }
        }
        sealed private class Distance
        {
            public float _distance;
            public int _indice;
            public Distance(float distance, int i)
            {
                _distance = distance;
                _indice = i;
            }
        }

        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                NB_TRIANGLES = c.GetParametre("Nb triangles", 100);
                NB_PLUS_PROCHES = c.GetParametre("Nb plus proches", 3);
                FORCE = c.GetParametre("Force", 0.001f, a => { FORCE = (float)Convert.ToDouble(a); });
                AMORTISSEMENT = c.GetParametre("Amortissement", 0.99f, a => { AMORTISSEMENT = (float)Convert.ToDouble(a); });
                MAX_DISTANCE = c.GetParametre("Max distance", 0.0254f, a => { MAX_DISTANCE = (float)Convert.ToDouble(a); });
                MIN_DISTANCE = c.GetParametre("Min distance", 0.0154f, a => { MIN_DISTANCE = (float)Convert.ToDouble(a); });
                DISTANCE_CIBLE = c.GetParametre("Distance cible", 0.02f, a => { DISTANCE_CIBLE = (float)Convert.ToDouble(a); if (DISTANCE_CIBLE < MIN_DISTANCE) DISTANCE_CIBLE = MIN_DISTANCE; else if (DISTANCE_CIBLE > MAX_DISTANCE) DISTANCE_CIBLE = MAX_DISTANCE; });
                LARGEUR_LIGNE = c.GetParametre("Largeur lignes", 2.0f, a => { LARGEUR_LIGNE = (float)Convert.ToDouble(a); });
                TAILLE_X = c.GetParametre("Taille X", 0.0005f, a => { TAILLE_X = (float)Convert.ToDouble(a); });
                TAILLE_Y = c.GetParametre("Taille Y", 0.0005f, a => { TAILLE_Y = (float)Convert.ToDouble(a); });
                CHANGE_COULEUR = c.GetParametre("Change couleur", 0.2f, a => { CHANGE_COULEUR = (float)Convert.ToDouble(a); });
            }
            return c;
        }

        public Triangles(OpenGL gl) : base(gl)
        {
            GetConfiguration();
            _triangles = new Triangle[NB_TRIANGLES];

            for (int i = 0; i < NB_TRIANGLES; i++)
            {
                _triangles[i] = new Triangle(FloatRandom(MIN_X, MAX_X), FloatRandom(MIN_Y, MAX_Y));
            }

        }

        public override bool ClearBackGround(OpenGL gl, Color c)
        {
            gl.ClearColor(0, 0, 0, 1);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            return true;
        }

        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_DEPTH);
            gl.Disable(OpenGL.GL_DEPTH_TEST);
            gl.Disable(OpenGL.GL_ALPHA_TEST);
            gl.Disable(OpenGL.GL_BLEND);
            gl.Disable(OpenGL.GL_TEXTURE_2D);

            using (new Viewport2D(gl, MIN_X, MIN_Y, MAX_X, MAX_Y))
            {
                // Points de reference
                gl.Color(couleur.R, couleur.G, couleur.B, (byte)255);
                gl.Begin(OpenGL.GL_QUADS);
                foreach (Triangle p in _triangles)
                {
                    gl.Vertex(p._x - TAILLE_X, p._y + TAILLE_Y);
                    gl.Vertex(p._x - TAILLE_X, p._y - TAILLE_Y);
                    gl.Vertex(p._x + TAILLE_X, p._y - TAILLE_Y);
                    gl.Vertex(p._x + TAILLE_X, p._y + TAILLE_Y);
                }
                gl.End();

                gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
                gl.Enable(OpenGL.GL_BLEND);

                // Polygones
                Color cG = Color.FromArgb(64, couleur.R, couleur.G, couleur.B);
                foreach (Triangle o in _triangles)
                {
                    SetColorWithHueChange(gl, cG, o.changeCouleur * CHANGE_COULEUR);
                    gl.Begin(OpenGL.GL_TRIANGLE_FAN);
                    gl.Vertex(o._x, o._y);
                    for (int i = 0; i < o._plusProches.Count; i++)
                        gl.Vertex(_triangles[o._plusProches[i]._indice]._x, _triangles[o._plusProches[i]._indice]._y);
                    gl.End();
                }

                // Bords des polygones
                gl.LineWidth(LARGEUR_LIGNE);
                gl.Color(couleur.R, couleur.G, couleur.B, (byte)255);
                using (new PolygonMode(gl, OpenGL.GL_LINE, LARGEUR_LIGNE))
                {
                    foreach (Triangle p in _triangles)
                    {
                        gl.Begin(OpenGL.GL_LINE_LOOP);

                        gl.Vertex(p._x, p._y);
                        for (int i = 0; i < p._plusProches.Count; i++)
                            gl.Vertex(_triangles[p._plusProches[i]._indice]._x, _triangles[p._plusProches[i]._indice]._y);
                        gl.End();
                    }
                }
            }
#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.DEPLACE);
#endif

            // Deplace les points
            foreach (Triangle p in _triangles)
            {
                Vecteur2D v = new Vecteur2D(p._x, p._y);
                // Essayer de garder la bonne distance de chaque point connecté
                foreach (Distance d in p._plusProches)
                {
                    Vecteur2D v2 = new Vecteur2D(_triangles[d._indice]._x, _triangles[d._indice]._y);
                    Vecteur2D diff = v - v2;
                    float distance = DISTANCE_CIBLE - diff.Longueur();
                    diff.Normalize();
                    diff.multiplier_par(distance * FORCE);
                    p._vx += diff.x;
                    p._vy += diff.y;
                }

                p._x += (p._vx * maintenant.intervalleDepuisDerniereFrame);
                p._y += (p._vy * maintenant.intervalleDepuisDerniereFrame);

                Limite(ref p._x, ref p._vx, MIN_X, MAX_X);
                Limite(ref p._y, ref p._vy, MIN_Y, MAX_Y);

                p._vx *= AMORTISSEMENT;
                p._vy *= AMORTISSEMENT;
            }

            indicePointEnCours++;
            if (indicePointEnCours >= NB_TRIANGLES)
                indicePointEnCours = 0;

            // Supprimer les liaisons trop loin
            for (int i = _triangles[indicePointEnCours]._plusProches.Count - 1; i >= 0; i--)
            {
                int indice = _triangles[indicePointEnCours]._plusProches[i]._indice;
                float distance = CalculeDistance(_triangles[indicePointEnCours], _triangles[indice]);
                if (distance > MAX_DISTANCE)
                    _triangles[indicePointEnCours]._plusProches.RemoveAt(i);
            }

            // Ajouter tous les points suffisament proches
            for (int i = 0; i < _triangles.Length; i++)
                if (i != indicePointEnCours && NotIn(_triangles[indicePointEnCours], i))
                {
                    float distance = CalculeDistance(_triangles[indicePointEnCours], _triangles[i]);
                    if (distance < MIN_DISTANCE)
                        _triangles[indicePointEnCours]._plusProches.Add(new Distance(distance, i));
                }

            _triangles[indicePointEnCours]._plusProches.Sort(delegate (Distance D1, Distance D2)
          {
              if (D1._distance > D2._distance) return 1;
              if (D1._distance < D2._distance) return -1;
              return 0;
          });

            if (_triangles[indicePointEnCours]._plusProches.Count > NB_PLUS_PROCHES)
                _triangles[indicePointEnCours]._plusProches.RemoveRange(NB_PLUS_PROCHES, _triangles[indicePointEnCours]._plusProches.Count - NB_PLUS_PROCHES);

#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif
        }


        private bool NotIn(Triangle triangle, int val)
        {
            for (int i = 0; i < triangle._plusProches.Count; i++)
                if (triangle._plusProches[i]._indice == val)
                    return false;

            return true;
        }

        private float CalculeDistance(Triangle triangle1, Triangle triangle2)
        {
            float dx = triangle1._x - triangle2._x;
            float dy = triangle1._y - triangle2._y;
            return (dx * dx) + (dy * dy);
        }

        private void Limite(ref float x, ref float vx, float min, float max)
        {
            if (x <= min)
            {
                x = min;
                vx = Math.Abs(vx);
            }
            else
                if (x >= max)
            {
                x = max;
                vx = -Math.Abs(vx);
            }
        }
    }
}
