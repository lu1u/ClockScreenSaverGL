using ClockScreenSaverGL.Config;
using SharpGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD
{
    internal class Escaliers : MateriauGlobal, IDisposable
    {

        #region Parametres
        private const String CAT = "Escaliers";
        private CategorieConfiguration c;
        private int NB_ESCALIERS;
        private float RATIO_COULEUR_MIN;
        private float RATIO_COULEUR_MAX;
        private float MIN_TAILLE_X;
        private float MAX_TAILLE_X;
        private float MIN_TAILLE_Y;
        private float MAX_TAILLE_Y;
        private float MIN_TAILLE_Z;
        private float MAX_TAILLE_Z;
        private float VITESSE_ROTATION;
        private float SEUIL_DECALAGE;
        private float ACCELERATION_DECALAGE;
        #endregion

        // Palier en cours
        private float _sensEscalierZ;
        private float _xEscalier;
        private float _largeurMarche;
        private float _profondeurMarche;
        private int _nbMarchesPalier;
        private float _prochaineLargeur;
        private float _prochainDecalage;
        private TimerIsole _timer = new TimerIsole(500);
        private List<Marche> _escaliers = new List<Marche>();
        private float _angleVue = FloatRandom(0, DEUX_PI);
        private float xCible, yCible, zCible;
        private uint _genLists;


        // Largeur des marches orientees sur l'axe des X, l'escalier monte sur l'axe des Y en progressant horizontalement sur l'axe des Z
        private class Marche
        {
            public float x, y, z, tX, tY, tZ, R, G, B, decalage;
        }


        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="gl"></param>
        public Escaliers(OpenGL gl) : base(gl)
        {

            getConfiguration();
            _angleVue = 3.14f;
            _genLists = gl.GenLists(1);

            float x = 0;
            float y = 0;
            float z = 0;
            float tY = FloatRandom(MIN_TAILLE_Y, MAX_TAILLE_Y);
            //_nbEscaliers = 1;
            CreerObjetMarche(gl);

            _xEscalier = 0;
            _sensEscalierZ = SigneRandom();
            _nbMarchesPalier = r.Next(5, 10);
            _largeurMarche = FloatRandom(MIN_TAILLE_X, MAX_TAILLE_X);
            _profondeurMarche = FloatRandom(MIN_TAILLE_Z, MAX_TAILLE_Z);
            Marche e = new Marche();
            e.x = _xEscalier + x;
            e.y = y;
            e.z = z;
            e.decalage = 10.0f;

            e.R = FloatRandom(RATIO_COULEUR_MIN, RATIO_COULEUR_MAX);
            e.G = FloatRandom(RATIO_COULEUR_MIN, RATIO_COULEUR_MAX);
            e.B = FloatRandom(RATIO_COULEUR_MIN, RATIO_COULEUR_MAX);
            e.tX = _largeurMarche;
            e.tY = tY;
            e.tZ = _profondeurMarche;
            _escaliers.Add(e);

            xCible = x;
            yCible = y;
            zCible = z;
        }

        /// <summary>
        /// Creation de la genlist OpenGL pour representer une marche
        /// </summary>
        /// <param name="gl"></param>
        private void CreerObjetMarche(OpenGL gl)
        {
            gl.NewList(_genLists, OpenGL.GL_COMPILE);
            gl.Begin(OpenGL.GL_QUADS);
            gl.Normal(0.0f, -1.0f, 0.0f);
            gl.Vertex(-1, -1, +1);
            gl.Vertex(-1, -1, -1);
            gl.Vertex(+1, -1, -1);
            gl.Vertex(+1, -1, +1);


            // Haut
            gl.Normal(0.0f, 1.0f, 0.0f);
            gl.Vertex(+1, +1, +1);
            gl.Vertex(+1, +1, -1);
            gl.Vertex(-1, +1, -1);
            gl.Vertex(-1, +1, +1);

            // Droite
            gl.Normal(1.0f, 0.0f, 0.0f);
            gl.Vertex(+1, -1, +1);
            gl.Vertex(+1, -1, -1);
            gl.Vertex(+1, +1, -1);
            gl.Vertex(+1, +1, +1);

            // Gauche
            gl.Normal(-1.0f, 0.0f, 0.0f);
            gl.Vertex(-1, +1, +1);
            gl.Vertex(-1, +1, -1);
            gl.Vertex(-1, -1, -1);
            gl.Vertex(-1, -1, +1);

            // Devant
            gl.Normal(0.0f, 0.0f, -1.0f);
            gl.Vertex(+1, +1, -1);
            gl.Vertex(+1, -1, -1);
            gl.Vertex(-1, -1, -1);
            gl.Vertex(-1, +1, -1);

            // Derriere
            gl.Normal(0.0f, 0.0f, 1.0f);
            gl.Vertex(+1, -1, +1);
            gl.Vertex(+1, +1, +1);
            gl.Vertex(-1, +1, +1);
            gl.Vertex(-1, -1, +1);
            gl.End();
            gl.EndList();
        }

        public override CategorieConfiguration getConfiguration()
        {
            if (c == null)
            {
                c = Configuration.getCategorie(CAT);
                NB_ESCALIERS = c.getParametre("NbEscaliers", 500);
                RATIO_COULEUR_MIN = c.getParametre("Ratio Couleur Min", 0.95f, (a) => { RATIO_COULEUR_MIN = (float)Convert.ToDouble(a); });
                RATIO_COULEUR_MAX = c.getParametre("Ratio Couleur Max", 1.05f, (a) => { RATIO_COULEUR_MAX = (float)Convert.ToDouble(a); });
                MIN_TAILLE_X = c.getParametre("Min tailleX", 0.3f);
                MAX_TAILLE_X = c.getParametre("Max tailleX", 0.8f);
                MIN_TAILLE_Y = c.getParametre("Min tailleY", 0.1f);
                MAX_TAILLE_Y = c.getParametre("Max tailleY", 0.2f);
                MIN_TAILLE_Z = c.getParametre("Min tailleZ", 0.2f);
                MAX_TAILLE_Z = c.getParametre("Max tailleZ", 0.5f);
                VITESSE_ROTATION = c.getParametre("Vitesse Rotation", 10.0f, (a) => { VITESSE_ROTATION = (float)Convert.ToDouble(a); });
                SEUIL_DECALAGE = c.getParametre("Seuil Decalage", 0.01f, (a) => { SEUIL_DECALAGE = (float)Convert.ToDouble(a); });
                ACCELERATION_DECALAGE = c.getParametre("Acceleration Decalage", 0.9f, (a) => { ACCELERATION_DECALAGE = (float)Convert.ToDouble(a); });
            }
            return c;
        }

        public override void Dispose()
        {
            base.Dispose();
            _gl.DeleteLists(_genLists, 1);
        }


        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            float[] col = { couleur.R / 256.0f, couleur.G / 256.0f, couleur.B / 256.0f, 1f };
            gl.LoadIdentity();
            gl.Disable(OpenGL.GL_ALPHA_TEST);
            gl.Disable(OpenGL.GL_BLEND);
            gl.Disable(OpenGL.GL_FOG);
            gl.Enable(OpenGL.GL_DEPTH);
            gl.Enable(OpenGL.GL_CULL_FACE);
            gl.CullFace(OpenGL.GL_BACK);
            gl.Disable(OpenGL.GL_TEXTURE_2D);
            LIGHTPOS[1] = yCible + 10;

            setGlobalMaterial(gl, couleur);
            // Aspect de la surface
            gl.ShadeModel(OpenGL.GL_SMOOTH);
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.LookAt(0, 2, -8, 0, 0, 0, 0, 1, 0);

            gl.Rotate(0, _angleVue, 0);
            gl.Translate(-xCible, -yCible, -zCible);
            changeZoom(gl, tailleEcran.Width, tailleEcran.Height, 0.001f, 20.0f);

            foreach (Marche e in _escaliers)
            {
                setGlobalMaterial(gl, couleur.R * e.R / 256.0f, couleur.G * e.G / 256.0f, couleur.B * e.B / 256.0f);
                gl.PushMatrix();
                gl.Translate(e.x, e.y + e.decalage, e.z);
                gl.Scale(e.tX, e.tY, e.tZ);
                gl.CallList(_genLists);
                gl.PopMatrix();
            }

#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

        public override bool ClearBackGround(OpenGL gl, Color c)
        {
            gl.ClearColor(0, 0, 0, 1);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT);

            gl.Disable(OpenGL.GL_TEXTURE_2D);
            gl.Disable(OpenGL.GL_LIGHTING);

            using (new Viewport2D(gl, -1.0f, -1.0f, 1.0f, 1.0f))
            {
                gl.Begin(OpenGL.GL_QUAD_STRIP);
                {
                    gl.Color(0f, 0f, 0f); gl.Vertex(-1f, -1f); gl.Vertex(1f, -1f);
                    gl.Color(c.R / 256.0f, c.G / 256.0f, c.B / 256.0f, 1); gl.Vertex(-1f, 1f); gl.Vertex(1f, 1f);
                }
                gl.End();
            }

            gl.Clear(OpenGL.GL_DEPTH_BUFFER_BIT);
            return true;
        }

        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {

#if TRACER
            RenderStart(CHRONO_TYPE.DEPLACE);
#endif
            _angleVue += VITESSE_ROTATION * maintenant.intervalleDepuisDerniereFrame;

            Marche derniere = _escaliers[_escaliers.Count - 1];
            xCible += (derniere.x - xCible) * 0.2f * maintenant.intervalleDepuisDerniereFrame;
            yCible += (derniere.y - yCible + derniere.tY) * 0.5f * maintenant.intervalleDepuisDerniereFrame;
            zCible += (derniere.z - zCible) * 0.2f * maintenant.intervalleDepuisDerniereFrame;

            foreach (Marche e in _escaliers)
            {
                if (e.decalage > SEUIL_DECALAGE)
                    e.decalage *= ACCELERATION_DECALAGE;
                else
                    e.decalage = 0f;
            }

            if (_timer.Ecoule())
            {
                _timer.Intervalle = 1000;

                switch (_nbMarchesPalier)
                {
                    case 0:
                        // Faire un palier
                        _prochainDecalage = SigneRandom();
                        _prochaineLargeur = FloatRandom(MIN_TAILLE_X, MAX_TAILLE_X);
                        _largeurMarche = (_largeurMarche + _prochaineLargeur);
                        _xEscalier += _largeurMarche / 2.0f * _prochainDecalage;
                        _nbMarchesPalier--;
                        NouvelleMarche();
                        break;

                    case -1:
                        // Repartir sur un escalier dans l'autre sens
                        _sensEscalierZ = -_sensEscalierZ;
                        _xEscalier += (_largeurMarche / 2.0f) * _prochainDecalage;
                        _largeurMarche = _prochaineLargeur;
                        _profondeurMarche = FloatRandom(MIN_TAILLE_Z, MAX_TAILLE_Z);
                        _nbMarchesPalier = r.Next(5, 10);
                        NouvelleMarche();
                        break;


                    default:
                        // Continuer l'escalier
                        NouvelleMarche();
                        _nbMarchesPalier--;
                        break;
                }
            }

#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif
        }

        private void NouvelleMarche()
        {
            Marche dernier = _escaliers.Last();

            float tX = dernier.tX;
            float tY = FloatRandom(MIN_TAILLE_Y, MAX_TAILLE_Y);
            float x = 0;
            float y = dernier.y + (tY + dernier.tY);
            float z = dernier.z + (_profondeurMarche + dernier.tZ) * _sensEscalierZ;
            Marche marche = new Marche();
            marche.x = _xEscalier + x;
            marche.y = y;
            marche.z = z;

            marche.R = FloatRandom(RATIO_COULEUR_MIN, RATIO_COULEUR_MAX);
            marche.G = FloatRandom(RATIO_COULEUR_MIN, RATIO_COULEUR_MAX);
            marche.B = FloatRandom(RATIO_COULEUR_MIN, RATIO_COULEUR_MAX);
            marche.tX = _largeurMarche;
            marche.tY = tY;
            marche.tZ = _profondeurMarche;
            marche.decalage = 10.0f;
            if (_escaliers.Count >= NB_ESCALIERS)
                _escaliers.RemoveAt(0);

            _escaliers.Add(marche);
        }
    }


}
