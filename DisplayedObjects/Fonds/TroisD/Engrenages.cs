
using ClockScreenSaverGL.Config;
using SharpGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GLfloat = System.Single;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD
{
    internal class Engrenages : MateriauGlobal, IDisposable
    {
        private const String CAT = "Engrenages";
        private CategorieConfiguration c;
        private int NB_ENGRENAGES;
        private float LONGUEUR_AXE;
        private float VITESSE;
        private float RAYON_MIN;
        private float RAYON_MAX;
        private float RATIO_COULEUR_MIN;
        private float RATIO_COULEUR_MAX;
        private float VITESSE_GROSSISSEMENT;
        private int NB_DENTS;
        private int NB_FACES_PAR_DENT;
        private float EPAISSEUR;
        private float TAILLE_DENT;
        private float VITESSE_ROTATION;
        private float FOG_DENSITY;
        private static readonly GLfloat[] fogcolor = { 0, 0, 0, 1 };
        private const float VIEWPORT_Z = 2.0f;
        private TimerIsole timer = new TimerIsole(500);
        private class Engrenage
        {
            public float x, y, z, diametre1, diametre2, largeur1, largeur2, largeur3, angle, vitesse, epaisseur, R, G, B, echelle;
            public int nbdents;
            public uint listId;
        }

        private List<Engrenage> _engrenages = new List<Engrenage>();
        private uint _nbEngrenages;
        private float _angle;
        private float _angleVue = FloatRandom(0, DEUX_PI);
        private float xCible, yCible, zCible;
        private readonly uint _genLists;
        public Engrenages(OpenGL gl) : base(gl)
        {
            c = Configuration.GetCategorie(CAT);
            VITESSE_ROTATION = c.GetParametre("Vitesse Rotation", 10.0f, (a) => { VITESSE_ROTATION = (float)Convert.ToDouble(a); });
            FOG_DENSITY = c.GetParametre("Densite brouillard", 10.0f, (a) => { FOG_DENSITY = (float)Convert.ToDouble(a); });
            NB_ENGRENAGES = c.GetParametre("NbEngrenages", 500);
            LONGUEUR_AXE = c.GetParametre("Longueur Axe", 1.5f);
            VITESSE = c.GetParametre("Vitesse", 20.0f);
            RAYON_MIN = c.GetParametre("Rayon Min", 0.2f);
            RAYON_MAX = c.GetParametre("Rayon Max", 0.8f);
            RATIO_COULEUR_MIN = c.GetParametre("Ration Couleur Min", 0.8f);
            RATIO_COULEUR_MAX = c.GetParametre("Ration Couleur Max", 1.2f);
            VITESSE_GROSSISSEMENT = c.GetParametre("Vitesse grossissement", 2.0f);
            NB_DENTS = c.GetParametre("Nb Dents", 20);
            NB_FACES_PAR_DENT = c.GetParametre("Nb faces par dent", 6);
            EPAISSEUR = c.GetParametre("Epaisseur", 0.5f);
            TAILLE_DENT = c.GetParametre("Taille dent", 0.05f);

            _angleVue = 3.14f;// FloatRandom(0, DEUX_PI);
            _genLists = gl.GenLists(NB_ENGRENAGES);

            float x = 0;
            float y = 0;
            float z = 0;
            float rayon = FloatRandom(RAYON_MIN, RAYON_MAX);
            float vitesse = VITESSE;
            _angle = 0;
            int nbDents = (int)Math.Round(rayon * NB_DENTS);
            float epaisseur = EPAISSEUR * FloatRandom(0.2f, 1.5f);

            _nbEngrenages = 1;
            CreeEngrenage(gl, 0, x, y, z, rayon, vitesse, 0, nbDents, epaisseur);

            xCible = x;
            yCible = y;
            zCible = z;

            LIGHTPOS[0] = VIEWPORT_Z;
            LIGHTPOS[1] = VIEWPORT_Z;
            LIGHTPOS[2] = VIEWPORT_Z;
            LIGHTPOS[3] = 0;
        }



        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
                c = Configuration.GetCategorie(CAT);
            return c;
        }

        private void CreeEngrenage(OpenGL gl, uint i, float x, float y, float z, float rayon, float vitesse, float angle, int nbDents, float epaisseur)
        {
            Engrenage e = new Engrenage();
            e.x = x;
            e.y = y;
            e.z = z;
            e.angle = angle;
            e.vitesse = vitesse;
            e.nbdents = nbDents;
            e.diametre1 = rayon;
            e.diametre2 = e.diametre1 + TAILLE_DENT;
            e.epaisseur = epaisseur;
            e.echelle = 0.01f;
            e.R = FloatRandom(RATIO_COULEUR_MIN, RATIO_COULEUR_MAX);
            e.G = FloatRandom(RATIO_COULEUR_MIN, RATIO_COULEUR_MAX);
            e.B = FloatRandom(RATIO_COULEUR_MIN, RATIO_COULEUR_MAX);
            e.listId = i + _genLists;
            float pas = DEUX_PI / e.nbdents;
            e.largeur1 = pas * FloatRandom(0.2f, 0.4f);
            e.largeur2 = pas * FloatRandom(0.2f, 0.4f);
            e.largeur3 = (pas - (e.largeur1 + e.largeur2)) / 2.0f;
            _engrenages.Add(e);

            gl.NewList(e.listId, OpenGL.GL_COMPILE);
            DessineRoue(gl, e);
            gl.EndList();
        }


        public override void Dispose()
        {
            base.Dispose();
            _gl.DeleteLists(_genLists, NB_ENGRENAGES);
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

            // Aspect de la surface
            gl.ShadeModel(OpenGL.GL_SMOOTH);
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.LookAt(0, 0, -8, 0, 0, 0, 0, 1, 0);
            //gl.Translate(0, 0, -8);
            gl.Rotate(_angleVue / 2.0f, _angleVue, _angleVue / 3.0f);
            gl.Translate(-xCible, -yCible, -zCible);
            setGlobalMaterial(gl, couleur);
            gl.Enable(OpenGL.GL_FOG);
            fogcolor[0] = col[0] * 0.5f;
            fogcolor[1] = col[1] * 0.5f;
            fogcolor[2] = col[2] * 0.5f;
            gl.Fog(OpenGL.GL_FOG_MODE, OpenGL.GL_EXP);
            gl.Fog(OpenGL.GL_FOG_COLOR, fogcolor);
            gl.Fog(OpenGL.GL_FOG_DENSITY, FOG_DENSITY);
            gl.Fog(OpenGL.GL_FOG_START, 0);
            gl.Fog(OpenGL.GL_FOG_END, 2);

            changeZoom(gl, tailleEcran.Width, tailleEcran.Height, 0.001f, 20.0f);

            foreach (Engrenage e in _engrenages)
            {
                setGlobalMaterial(gl, couleur.R * e.R / 256.0f, couleur.G * e.G / 256.0f, couleur.B * e.B / 256.0f);
                gl.PushMatrix();
                gl.Translate(e.x, e.y, e.z);
                gl.Rotate(0, 0, e.angle);
                gl.Scale(e.echelle, e.echelle, e.echelle);
                gl.CallList(e.listId);
                gl.PopMatrix();
            }


#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

        public override bool ClearBackGround(OpenGL gl, Color couleur)
        {
            gl.ClearColor(0, 0, 0, 1.0f);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            return true;
        }

        private void DessineRoue(OpenGL gl, Engrenage e)
        {
            float ep = e.epaisseur / 2.0f;
            Vecteur3D na = NormaleTriangle(new Vecteur3D(0, 0, 0), new Vecteur3D(1, 0, 0), new Vecteur3D(0, 1, 0));
            Vecteur3D nb = NormaleTriangle(new Vecteur3D(0, 0, 0), new Vecteur3D(0, 1, 0), new Vecteur3D(1, 0, 0));

            int NB_STEPS = (int)Math.Round((e.nbdents * (float)NB_FACES_PAR_DENT));

            #region couronne
            gl.Begin(OpenGL.GL_QUAD_STRIP);

            for (int i = 0; i <= NB_STEPS; i++)
            {
                float angle = (DEUX_PI * i / NB_STEPS);
                float x, y, xm1, ym1, xp1, yp1;

                CalculeXY(angle, e.nbdents, e.diametre1, e.diametre2, out x, out y);
                CalculeXY((DEUX_PI * (i + 1) / e.nbdents), e.nbdents, e.diametre1, e.diametre2, out xp1, out yp1);
                CalculeXY((DEUX_PI * (i - 1) / e.nbdents), e.nbdents, e.diametre1, e.diametre2, out xm1, out ym1);

                NormaleTriangle(new Vecteur3D(x, y, -ep), new Vecteur3D(x, y, ep), new Vecteur3D(xp1, yp1, ep)).Normal(gl);
                gl.Vertex(x, y, ep); gl.Vertex(x, y, -ep);

            }
            gl.End();
            #endregion

            #region face avant
            gl.Begin(OpenGL.GL_TRIANGLE_FAN);
            na.Normal(gl);
            gl.Vertex(0, 0, ep);
            for (int i = 0; i <= NB_STEPS; i++)
            {
                float angle = (DEUX_PI * i / NB_STEPS);
                float x, y;

                CalculeXY(angle, e.nbdents, e.diametre1, e.diametre2, out x, out y);
                gl.Vertex(x, y, ep);
            }
            gl.End();
            #endregion

            #region face arriere
            gl.Begin(OpenGL.GL_TRIANGLE_FAN);
            nb.Normal(gl);
            gl.Vertex(0, 0, -ep);
            for (int i = 0; i <= NB_STEPS; i++)
            {
                float angle = -(DEUX_PI * i / NB_STEPS);
                float x, y;

                CalculeXY(angle, e.nbdents, e.diametre1, e.diametre2, out x, out y);
                gl.Vertex(x, y, -ep);
            }
            gl.End();

            #endregion

            //#region axe
            //gl.Begin(OpenGL.GL_QUAD_STRIP);
            //const int NB_FACES = 20;
            //float LARGEUR_FACE = (DEUX_PI / NB_FACES);
            //const float DIAMETRE_AXE = 0.1f;
            //ep = e.epaisseur + 0.1f;
            //for (int i = 0; i <= NB_FACES; i++)
            //{
            //	float angle = (DEUX_PI * i / NB_FACES);
            //
            //	//float x0 = (float)(DIAMETRE_AXE * Math.Cos(angle - LARGEUR_FACE));
            //	//float y0 = (float)(DIAMETRE_AXE * Math.Sin(angle - LARGEUR_FACE));
            //	float x1 = (float)(DIAMETRE_AXE * Math.Cos(angle));
            //	float y1 = (float)(DIAMETRE_AXE * Math.Sin(angle));
            //	float x2 = (float)(DIAMETRE_AXE * Math.Cos(angle + LARGEUR_FACE));
            //	float y2 = (float)(DIAMETRE_AXE * Math.Sin(angle + LARGEUR_FACE));
            //
            //	//Vecteur3D v0a = new Vecteur3D(x0, y0, ep);
            //	Vecteur3D v1a = new Vecteur3D(x1, y1, ep);
            //	//Vecteur3D v0b = new Vecteur3D(x0, y0, -ep);
            //	Vecteur3D v1b = new Vecteur3D(x1, y1, -ep);
            //	Vecteur3D v2a = new Vecteur3D(x2, y2, ep);
            //	//Vecteur3D v2b = new Vecteur3D(x2, y2, -ep);
            //
            //	NormaleTriangle( v1a, v1b, v2a).Normal(gl);
            //    v1a.Vertex(gl);
            //    v1b.Vertex(gl); 		  				
            //}
            //gl.End();
            //#endregion
            //
            //#region axe avant
            //gl.Begin(OpenGL.GL_TRIANGLE_FAN);
            //na.Normal(gl);
            //gl.Vertex(0, 0, ep);
            //for (int i = 0; i <= NB_FACES; i++)
            //{
            //	float angle = (DEUX_PI * i / NB_FACES);
            //	float x1 = (float)(DIAMETRE_AXE * Math.Cos(angle));
            //	float y1 = (float)(DIAMETRE_AXE * Math.Sin(angle));
            //	gl.Vertex(x1, y1, ep);
            //}
            //gl.End();
            //#endregion
            ////#region axe arriere
            //gl.Begin(OpenGL.GL_TRIANGLE_FAN);
            //
            //nb.Normal(gl);
            //gl.Vertex(0, 0, -ep);
            //for (int i = 0; i <= NB_FACES; i++)
            //{
            //	float angle = -(DEUX_PI * i / NB_FACES);
            //	float x1 = (float)(DIAMETRE_AXE * Math.Cos(angle));
            //	float y1 = (float)(DIAMETRE_AXE * Math.Sin(angle));
            //	gl.Vertex(x1, y1, -ep);
            //}
            //gl.End();
            //#endregion
        }

        private static void CalculeXY(float angle, int nbdents, float diametre1, float diametre2, out float x, out float y)
        {
            // Sinusoide correspondant a l'ecart de diametre entre le haut et le bas des dents
            double ecart = diametre1 - diametre2;
            double diametre = diametre1 + (ecart * Math.Cos(angle * nbdents));

            x = (float)(diametre * Math.Cos(angle));
            y = (float)(diametre * Math.Sin(angle));
        }

        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {

#if TRACER
            RenderStart(CHRONO_TYPE.DEPLACE);
#endif
            // Angle de vue de la camera
            _angleVue += VITESSE_ROTATION * maintenant.intervalleDepuisDerniereFrame;

            // Cible visee par la camera
            xCible += (_engrenages[(int)_nbEngrenages - 1].x - xCible) * 0.1f * maintenant.intervalleDepuisDerniereFrame;
            yCible += (_engrenages[(int)_nbEngrenages - 1].y - yCible) * 0.1f * maintenant.intervalleDepuisDerniereFrame;
            zCible += (_engrenages[(int)_nbEngrenages - 1].z - zCible) * 0.1f * maintenant.intervalleDepuisDerniereFrame;

            // Tourner et Grossir les engrenages qui n'ont pas encore atteint leur taille max
            foreach (Engrenage e in _engrenages)
            {
                e.angle += e.vitesse * maintenant.intervalleDepuisDerniereFrame;
                if (e.echelle < 1.0f)
                    e.echelle += maintenant.intervalleDepuisDerniereFrame * VITESSE_GROSSISSEMENT;
                else
                    e.echelle = 1.0f;
            }

            if (timer.Ecoule())
            {
                _angle += FloatRandom(1, 2.0f) * 0.5f * SigneRandom();
                Engrenage dernier = _engrenages.Last();

                if (_nbEngrenages < NB_ENGRENAGES)
                {
                    Engrenage premier = _engrenages.First();
                    float epaisseur = EPAISSEUR * FloatRandom(0.2f, 1.5f);
                    float rayon = FloatRandom(RAYON_MIN, RAYON_MAX);
                    float x = dernier.x + (rayon + dernier.diametre1) * (float)Math.Sin(_angle);
                    float y = dernier.y + (rayon + dernier.diametre1) * (float)Math.Cos(_angle);
                    float z = dernier.z + 0.25f * epaisseur;
                    float vitesse = -dernier.vitesse * dernier.diametre1 / rayon;
                    float angle = DEUX_PI - dernier.angle;
                    int nbDents = (int)Math.Round(rayon * NB_DENTS);

                    CreeEngrenage(_gl, _nbEngrenages, x, y, z, rayon, vitesse, angle, nbDents, epaisseur);
                    _nbEngrenages++;
                }
                else
                {
                    timer.Intervalle = 1000;
                    Engrenage premier = _engrenages.First();

                    premier.x = dernier.x + (_engrenages[0].diametre1 + dernier.diametre2) * (float)Math.Sin(_angle);
                    premier.y = dernier.y + (_engrenages[0].diametre1 + dernier.diametre2) * (float)Math.Cos(_angle);
                    premier.z = dernier.z + 0.25f * -_engrenages[0].epaisseur;
                    premier.vitesse = -dernier.vitesse * dernier.diametre1 / _engrenages[0].diametre1;
                    premier.angle = dernier.angle;
                    premier.echelle = 0.01f;

                    _engrenages.RemoveAt(0);
                    _engrenages.Add(premier);
                }
            }

#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif
        }
    }
}
