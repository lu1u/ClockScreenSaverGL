using ClockScreenSaverGL.Config;
using SharpGL;
using SharpGL.SceneGraph.Quadrics;
using System;
using System.Collections.Generic;
using System.Drawing;

/// <summary>
/// Animation 3D Pipes historique de windowsd
/// </summary>
namespace ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD
{
    class TroisDPipes : MateriauGlobal, IDisposable
    {
         private const string CAT = "TroisDPipes";
        private CategorieConfiguration c;

        const int HAUT = 1 << 1;
        const int BAS = 1 << 2;
        const int NORD = 1 << 3;
        const int SUD = 1 << 4;
        const int EST = 1 << 5;
        const int OUEST = 1 << 6;
        const int FIN = 1 << 7;

        int[] directions = { HAUT, BAS, NORD, SUD, EST, OUEST };
        #region Parametres
        int DELAI_TIMER;
        int DELAI_TIMER_REINIT;
        int TAILLE;
        float PROBA_CHANGE_ALEATOIRE;
        float DIAMETRE_TUYAU;
        float DIAMETRE_JOINT;
        float DIAMETRE_FIN;
        bool AFFICHER_NOUVEAUX_DEPARTS;
        float VITESSE_ROTATION;
        float DISTANCE;
        int DETAIL_CYLINDRES;
        int DETAIL_SPHERES;
        #endregion

        private int[,,] _grille;
        private Color[,,] _couleurs;
        private int _X, _Y, _Z, _dx, _dy, _dz;
        private int _direction;
        private TimerIsole timer;
        private TimerIsole _timerReinit;
        private Color _derniereCouleur;
        int _nbTuyaux;

        class Point3
        {
            public Point3(int x, int y, int z)
            {
                X = x;
                Y = y;
                Z = z;
            }
            public int X, Y, Z;
        }
        List<Point3> _nouveauxDeparts;
        float _angle = 0;
        private Sphere _sphere = new Sphere();
        public TroisDPipes(OpenGL gl) : base(gl)
        {
            c = Configuration.getCategorie(CAT);
        }

        public override CategorieConfiguration getConfiguration()
        {
            if (c == null)
            {
                c = Configuration.getCategorie(CAT);
                TAILLE = c.getParametre("Taille", 10);
                DELAI_TIMER = c.getParametre("Delai Trace", 200, (a) => { DELAI_TIMER = Convert.ToInt32(a); timer = new TimerIsole(DELAI_TIMER); });
                DELAI_TIMER_REINIT = c.getParametre("Delai Trace Reinit", 5000);
                PROBA_CHANGE_ALEATOIRE = c.getParametre("Probabilité changement aléatoire", 0.5f, (a) => { PROBA_CHANGE_ALEATOIRE = (float)Convert.ToDouble(a); });
                DIAMETRE_TUYAU = c.getParametre("Diametre tuyau", 0.4f, (a) => { DIAMETRE_TUYAU = (float)Convert.ToDouble(a); });
                DIAMETRE_JOINT = c.getParametre("Diametre joint", 0.4f, (a) => { DIAMETRE_JOINT = (float)Convert.ToDouble(a); });
                DIAMETRE_FIN = c.getParametre("Diametre fin", 0.6f, (a) => { DIAMETRE_FIN = (float)Convert.ToDouble(a); });
                VITESSE_ROTATION = c.getParametre("Vitesse rotation", 0.5f, (a) => { VITESSE_ROTATION = (float)Convert.ToDouble(a); });
                DISTANCE = c.getParametre("Distance", 0.9f, (a) => { DISTANCE = (float)Convert.ToDouble(a); });
                AFFICHER_NOUVEAUX_DEPARTS = c.getParametre("Afficher nouveaux departs", false, (a) => { AFFICHER_NOUVEAUX_DEPARTS = Convert.ToBoolean(a); });
                DETAIL_CYLINDRES = c.getParametre("Details cylindres", 5, (a) => { DETAIL_CYLINDRES = Convert.ToInt32(a); });
                DETAIL_SPHERES = c.getParametre("Details spheres", 5, (a) => { DETAIL_SPHERES = Convert.ToInt32(a); _sphere.Slices = DETAIL_CYLINDRES; _sphere.Stacks = DETAIL_CYLINDRES; });
            }
            return c;
        }

        protected override void Init(OpenGL gl)
        {
            c = getConfiguration();
            LIGHTPOS[0] = TAILLE * 2;
            LIGHTPOS[1] = TAILLE * 2;
            LIGHTPOS[2] = TAILLE * 2;
            LIGHTPOS[3] = 0;
            ///
            _couleurs = new Color[TAILLE, TAILLE, TAILLE];
            _grille = new int[TAILLE, TAILLE, TAILLE];

            // Timer pour ajouter un tuyau
            timer = new TimerIsole(DELAI_TIMER);

            // Position de depart
            _X = TAILLE / 2;
            _Y = TAILLE / 2;
            _Z = TAILLE / 2;
            _direction = GetDirectionLibre(_X, _Y, _Z);
            calculDecalage(_direction, out _dx, out _dy, out _dz);
            _nbTuyaux = 0;
            _nouveauxDeparts = new List<Point3>();

            _sphere.CreateInContext(gl);
            _sphere.NormalGeneration = Normals.Smooth;
            _sphere.NormalOrientation = Orientation.Outside;
            _sphere.QuadricDrawStyle = DrawStyle.Fill;
            _sphere.Slices = DETAIL_SPHERES;
            _sphere.Stacks = DETAIL_SPHERES;
            _sphere.Radius = 1.0f;
        }

        private void calculDecalage(int direction, out int dx, out int dy, out int dz)
        {
            switch (direction)
            {
                case EST:
                    dx = 1;
                    dy = 0;
                    dz = 0;
                    break;

                case OUEST:
                    dx = -1;
                    dy = 0;
                    dz = 0;
                    break;
                case NORD:
                    dx = 0;
                    dy = -1;
                    dz = 0;
                    break;
                case SUD:
                    dx = 0;
                    dy = 1;
                    dz = 0;
                    break;
                case HAUT:
                    dx = 0;
                    dy = 0;
                    dz = -1;
                    break;
                default:
                    dx = 0;
                    dy = 0;
                    dz = 1;
                    break;
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
            _derniereCouleur = couleur;
            float distance = TAILLE * DISTANCE;
            gl.LoadIdentity();
            gl.LookAt(distance, distance, distance, 0, 0, 0, 0, 1.0f, 0);
            setGlobalMaterial(gl, couleur);
            gl.Rotate(_angle, _angle, _angle);

            gl.Enable(OpenGL.GL_DEPTH);
            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_CULL_FACE);
            gl.CullFace(OpenGL.GL_BACK);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Disable(OpenGL.GL_FOG);
            gl.Disable(OpenGL.GL_ALPHA_TEST);
            gl.Disable(OpenGL.GL_BLEND);
            gl.Disable(OpenGL.GL_TEXTURE_2D);

            for (int x = 0; x < TAILLE; x++)
            {
                float coordX = x - (TAILLE / 2);
                for (int y = 0; y < TAILLE; y++)
                {
                    float coordY = y - (TAILLE / 2);
                    for (int z = 0; z < TAILLE; z++)
                    {
                        if (_grille[x, y, z] != 0)
                        {
                            float coordZ = z - (TAILLE / 2);

                            gl.Color(_couleurs[x, y, z].R, _couleurs[x, y, z].G, _couleurs[x, y, z].B);

                            if (!unBit(_grille[x, y, z]) && morceauxNonAlignes(_grille[x, y, z]))
                                renderSphere(gl, coordX, coordY, coordZ, DIAMETRE_JOINT);

                            // Dans le cas ou on a 2 demi tuyaux alignes, on les regroupe en un seul cylindre
                            if (((_grille[x, y, z] & HAUT) != 0) && ((_grille[x, y, z] & BAS) != 0))
                                renderCylinderZ(gl, coordX, coordY, coordZ - 0.5f, coordZ + 0.5f, DIAMETRE_TUYAU, DETAIL_CYLINDRES);
                            else
                            {
                                if ((_grille[x, y, z] & HAUT) != 0) renderCylinderZ(gl, coordX, coordY, coordZ - 0.5f, coordZ, DIAMETRE_TUYAU, DETAIL_CYLINDRES);
                                if ((_grille[x, y, z] & BAS) != 0) renderCylinderZ(gl, coordX, coordY, coordZ, coordZ + 0.5f, DIAMETRE_TUYAU, DETAIL_CYLINDRES);
                            }

                            if (((_grille[x, y, z] & EST) != 0) && ((_grille[x, y, z] & OUEST) != 0))
                                renderCylinderX(gl, coordX - 0.5f, coordX + 0.5f, coordY, coordZ, DIAMETRE_TUYAU, DETAIL_CYLINDRES);
                            else
                            {
                                if ((_grille[x, y, z] & EST) != 0) renderCylinderX(gl, coordX, coordX + 0.5f, coordY, coordZ, DIAMETRE_TUYAU, DETAIL_CYLINDRES);
                                if ((_grille[x, y, z] & OUEST) != 0) renderCylinderX(gl, coordX - 0.5f, coordX, coordY, coordZ, DIAMETRE_TUYAU, DETAIL_CYLINDRES);
                            }

                            if (((_grille[x, y, z] & SUD) != 0) && ((_grille[x, y, z] & NORD) != 0))
                                renderCylinderY(gl, coordX, coordY - 0.5f, coordY + 0.5f, coordZ, DIAMETRE_TUYAU, DETAIL_CYLINDRES);
                            else
                            {
                                if ((_grille[x, y, z] & SUD) != 0) renderCylinderY(gl, coordX, coordY, coordY + 0.5f, coordZ, DIAMETRE_TUYAU, DETAIL_CYLINDRES);
                                if ((_grille[x, y, z] & NORD) != 0) renderCylinderY(gl, coordX, coordY - 0.5f, coordY, coordZ, DIAMETRE_TUYAU, DETAIL_CYLINDRES);
                            }
                            if ((_grille[x, y, z] & FIN) != 0) renderCube(gl, coordX, coordY, coordZ, DIAMETRE_FIN);
                        }
                    }
                }
            }

            if (AFFICHER_NOUVEAUX_DEPARTS)
            {

                gl.Enable(OpenGL.GL_BLEND);
                gl.Color(1.0f, 1.0f, 1.0f, 0.2f);

                foreach (Point3 p in _nouveauxDeparts)
                    renderSphere(gl, p.X - TAILLE / 2, p.Y - TAILLE / 2, p.Z - TAILLE / 2, DIAMETRE_JOINT * 1.5f);
            }
#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

        /// <summary>
        /// Dessiner un cube centré aux coordonnées indiquees
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="taille"></param>
        private void renderCube(OpenGL gl, float x, float y, float z, float taille)
        {
            gl.Begin(OpenGL.GL_QUADS);
            // Bas
            gl.Normal(0.0f, -1.0f, 0.0f);
            gl.Vertex(x - taille, y - taille, z + taille);
            gl.Vertex(x - taille, y - taille, z - taille);
            gl.Vertex(x + taille, y - taille, z - taille);
            gl.Vertex(x + taille, y - taille, z + taille);


            // Haut
            gl.Normal(0.0f, 1.0f, 0.0f);
            gl.Vertex(x + taille, y + taille, z + taille);
            gl.Vertex(x + taille, y + taille, z - taille);
            gl.Vertex(x - taille, y + taille, z - taille);
            gl.Vertex(x - taille, y + taille, z + taille);

            //// Droite
            gl.Normal(1.0f, 0.0f, 0.0f);
            gl.Vertex(x + taille, y - taille, z + taille);
            gl.Vertex(x + taille, y - taille, z - taille);
            gl.Vertex(x + taille, y + taille, z - taille);
            gl.Vertex(x + taille, y + taille, z + taille);

            //// Gauche
            gl.Normal(-1.0f, 0.0f, 0.0f);
            gl.Vertex(x - taille, y + taille, z + taille);
            gl.Vertex(x - taille, y + taille, z - taille);
            gl.Vertex(x - taille, y - taille, z - taille);
            gl.Vertex(x - taille, y - taille, z + taille);

            //// Devant
            gl.Normal(0.0f, 0.0f, -1.0f);
            gl.Vertex(x + taille, y + taille, z - taille);
            gl.Vertex(x + taille, y - taille, z - taille);
            gl.Vertex(x - taille, y - taille, z - taille);
            gl.Vertex(x - taille, y + taille, z - taille);

            // Derriere
            gl.Normal(0.0f, 0.0f, 1.0f);
            gl.Vertex(x + taille, y - taille, z + taille);
            gl.Vertex(x + taille, y + taille, z + taille);
            gl.Vertex(x - taille, y + taille, z + taille);
            gl.Vertex(x - taille, y - taille, z + taille);
            gl.End();
        }

        private static void renderCylinderZ(OpenGL gl, float X, float Y, float Z1, float Z2, float rayon, int nbFaces)
        {
            gl.Begin(OpenGL.GL_QUAD_STRIP);

            for (int i = 0; i <= nbFaces; i++)
            {
                double angle = (DEUX_PI * i / nbFaces);
                double sin = Math.Sin(angle);
                double cos = Math.Cos(angle);

                gl.Normal(sin, cos, 0);

                sin *= rayon;
                cos *= rayon;

                gl.Vertex(X + sin, Y + cos, Z1);
                gl.Vertex(X + sin, Y + cos, Z2);
            }
            gl.End();
        }

        private static void renderCylinderX(OpenGL gl, float X1, float X2, float Y, float Z, float rayon, int nbFaces)
        {
            gl.Begin(OpenGL.GL_QUAD_STRIP);

            for (int i = 0; i <= nbFaces; i++)
            {
                double angle = (DEUX_PI * i / nbFaces);
                double sin = Math.Sin(angle);
                double cos = Math.Cos(angle);

                gl.Normal(0, sin, cos);

                sin *= rayon;
                cos *= rayon;

                gl.Vertex(X1, Y + sin, Z + cos);
                gl.Vertex(X2, Y + sin, Z + cos);
            }
            gl.End();
        }

        private static void renderCylinderY(OpenGL gl, float X, float Y1, float Y2, float Z, float rayon, int nbFaces)
        {
            gl.Begin(OpenGL.GL_QUAD_STRIP);
            for (int i = 0; i <= nbFaces; i++)
            {
                double angle = (DEUX_PI * i / nbFaces);
                double sin = Math.Sin(angle);
                double cos = Math.Cos(angle);

                gl.Normal(cos, 0, sin);

                sin *= rayon;
                cos *= rayon;

                gl.Vertex(X + cos, Y1, Z + sin);
                gl.Vertex(X + cos, Y2, Z + sin);
            }
            gl.End();
        }

        /// <summary>
        /// Retourne VRAI si un morceau et un seul est allumé
        /// </summary>
        /// <param name="valeur"></param>
        /// <returns></returns>
        private bool unBit(int valeur)
        {
            return valeur == NORD || valeur == SUD || valeur == EST || valeur == OUEST || valeur == EST || valeur == HAUT || valeur == BAS;
        }

        /// <summary>
        /// Retourne TRUE si on a pas deux demi tuyaux alignes
        /// </summary>
        /// <param name="valeur"></param>
        /// <returns></returns>
        private bool morceauxNonAlignes(int valeur)
        {
            return valeur != (NORD | SUD) && valeur != (EST | OUEST) && valeur != (HAUT | BAS);
        }

        private void renderSphere(OpenGL gl, float X, float Y, float Z, float diametre)
        {
            gl.PushMatrix();

            gl.Translate(X, Y, Z);
            _sphere.Radius = diametre;
            _sphere.PushObjectSpace(gl);
            _sphere.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            _sphere.PopObjectSpace(gl);

            gl.PopMatrix();
            //float alpha, beta; // Storage for coordinates and angles        
            //float x, y, z;
            //
            //for (alpha = 0.0f; alpha < Math.PI; alpha += (float)Math.PI / DETAIL_SPHERES)
            //{
            //    gl.Begin(OpenGL.GL_TRIANGLE_STRIP);
            //    for (beta = 0.0f; beta < 2.01 * Math.PI; beta += (float)Math.PI / DETAIL_SPHERES)
            //    {
            //        x = (float)(radius * Math.Cos(beta) * Math.Sin(alpha));
            //        y = (float)(radius * Math.Sin(beta) * Math.Sin(alpha));
            //        z = (float)(radius * Math.Cos(alpha));
            //        Vecteur3D v = new Vecteur3D(x, y, z);
            //        v.Normalize();
            //        v.Normal(gl);
            //        gl.Vertex(X + x, Y + y, Z + z);
            //        x = (float)(radius * Math.Cos(beta) * Math.Sin(alpha + Math.PI / DETAIL_SPHERES));
            //        y = (float)(radius * Math.Sin(beta) * Math.Sin(alpha + Math.PI / DETAIL_SPHERES));
            //        z = (float)(radius * Math.Cos(alpha + Math.PI / DETAIL_SPHERES));
            //        v = new Vecteur3D(x, y, z);
            //        v.Normalize();
            //        v.Normal(gl);
            //        gl.Vertex(X + x, Y + y, Z + z);
            //    }
            //    gl.End();
            //}
        }


        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.DEPLACE);
#endif
            _angle += maintenant.intervalleDepuisDerniereFrame * VITESSE_ROTATION;

            if (_timerReinit != null)
            {
                if (_timerReinit.Ecoule())
                {
                    _timerReinit = null;
                    Init(_gl);
                }
            }
            else
            if (timer.Ecoule())
            {
                if (_direction == -1)
                {
                    // On est bloqué, essayer un des nouveaux points de depart
                    if (_nouveauxDeparts.Count > 0)
                    {
                        int indice = _nouveauxDeparts.Count - 1;
                        _X = _nouveauxDeparts[indice].X;
                        _Y = _nouveauxDeparts[indice].Y;
                        _Z = _nouveauxDeparts[indice].Z;
                        _nouveauxDeparts.RemoveAt(indice);
                        _direction = GetDirectionLibre(_X, _Y, _Z);
                    }
                    else
                    {
                        // Plus de possibilites, enclencher le timer pour reinitialiser
                        if (_timerReinit == null)
                            _timerReinit = new TimerIsole(DELAI_TIMER_REINIT);
                    }
                }

                if (_direction != -1)
                    // OK, on a une direction a suivre
                    if (_grille[_X, _Y, _Z] == 0)
                    {
                        _nbTuyaux++;
                        // Premiere moitie du tuyau
                        _grille[_X, _Y, _Z] = oppose(_direction);
                        _couleurs[_X, _Y, _Z] = _derniereCouleur;
                    }
                    else
                    {
                        int nX = _X + _dx;
                        int nY = _Y + _dy;
                        int nZ = _Z + _dz;
                        if (!libre(nX, nY, nZ) || Probabilite(PROBA_CHANGE_ALEATOIRE))
                        {
                            _nouveauxDeparts.Add(new Point3(_X, _Y, _Z));
                            _direction = GetDirectionLibre(_X, _Y, _Z);
                            if (_direction != -1)
                                calculDecalage(_direction, out _dx, out _dy, out _dz);
                            else
                                _grille[_X, _Y, _Z] |= FIN;
                        }
                        else
                        {
                            // Avancer dans la direction choisie
                            _grille[_X, _Y, _Z] |= _direction;
                            _X += _dx;
                            _Y += _dy;
                            _Z += _dz;
                        }

                    }
            }
#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif
        }


        private int oppose(int direction)
        {
            switch (direction)
            {
                case NORD: return SUD;
                case SUD: return NORD;
                case HAUT: return BAS;
                case BAS: return HAUT;
                case EST: return OUEST;
                default: return EST;
            }
        }

        private int GetDirectionLibre(int x, int y, int z)
        {
            int indice = r.Next(directions.Length);
            int dx, dy, dz;
            for (int i = 0; i < directions.Length; i++)
            {
                int d = directions[indice];
                calculDecalage(d, out dx, out dy, out dz);
                if (libre(x + dx, y + dy, z + dz))
                    return d;
                indice = (indice + 1) % directions.Length;
            }

            return -1;
        }

        private bool libre(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0)
                return false;

            if (x >= TAILLE || y >= TAILLE || z >= TAILLE)
                return false;

            if (_grille[x, y, z] != 0)
                return false;

            return true;
        }
#if TRACER
        public override string DumpRender()
        {
            return base.DumpRender() + $"Nb tuyaux: {_nbTuyaux}/{TAILLE * TAILLE * TAILLE}, Nouveaux départs:{ _nouveauxDeparts.Count}";
        }

#endif
    }
}
