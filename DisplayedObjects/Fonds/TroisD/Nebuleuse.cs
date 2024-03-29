﻿using ClockScreenSaverGL.Config;
using SharpGL;
using SharpGL.SceneGraph.Assets;
using System;
using System.Drawing;
using GLfloat = System.Single;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD
{
    /// <summary>
    /// Description of Neige.
    /// </summary>
    public class Nebuleuse : TroisD, IDisposable
    {
        #region Parametres
        public const string CAT = "Nebuleuse";
        protected CategorieConfiguration c;
        private int NB_ETOILES;
        private int NB_NUAGES;
        private float RATIO_VITESSE_NUAGES;
        private float PERIODE_TRANSLATION;
        private float PERIODE_ROTATION;
        private float VITESSE_ROTATION;
        private float VITESSE_TRANSLATION;
        private float VITESSE;
        private float DELTA_COULEUR;
        private bool ADDITIVE;
        private byte ALPHA_ETOILE;
        private byte ALPHA_NUAGE;
        private float TAILLE_ETOILE;
        private float TAILLE_NUAGE;

        #endregion
        private const float VIEWPORT_X = 5f;
        private const float VIEWPORT_Y = 5f;
        private const float VIEWPORT_Z = 5f;
        private static readonly GLfloat[] fogcolor = { 0, 0, 0, 1 };
        private const int TYPE_ETOILE = 0;
        private const int TYPE_NUAGE = 1;
        private class Etoile
        {
            public float x, y, z;
            public float rR, rG, rB;
        }
        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                NB_ETOILES = c.GetParametre("NbEtoiles", 100);
                NB_NUAGES = c.GetParametre("NbNuages", 50);
                RATIO_VITESSE_NUAGES = c.GetParametre("Ratio Vitesse Nuages", 1.5f, (a) => { RATIO_VITESSE_NUAGES = (float)Convert.ToDouble(a); });
                PERIODE_TRANSLATION = c.GetParametre("PeriodeTranslation", 13.0f, (a) => { PERIODE_TRANSLATION = (float)Convert.ToDouble(a); });
                PERIODE_ROTATION = c.GetParametre("PeriodeRotation", 10.0f, (a) => { PERIODE_ROTATION = (float)Convert.ToDouble(a); });
                VITESSE_ROTATION = c.GetParametre("VitesseRotation", 10f, (a) => { VITESSE_ROTATION = (float)Convert.ToDouble(a); });
                VITESSE_TRANSLATION = c.GetParametre("VitesseTranslation", 0.1f, (a) => { VITESSE_TRANSLATION = (float)Convert.ToDouble(a); });
                VITESSE = c.GetParametre("Vitesse", 5.0f, (a) => { VITESSE = (float)Convert.ToDouble(a); });
                DELTA_COULEUR = c.GetParametre("Delta Couleur", 0.1f, (a) => { DELTA_COULEUR = (float)Convert.ToDouble(a); });
                ADDITIVE = c.GetParametre("Additive", false, (a) => { ADDITIVE = Convert.ToBoolean(a); });
                ALPHA_ETOILE = c.GetParametre("Alpha Etoile", (byte)255, (a) => { ALPHA_ETOILE = Convert.ToByte(a); });
                ALPHA_NUAGE = c.GetParametre("Alpha Nuage", (byte)64, (a) => { ALPHA_NUAGE = Convert.ToByte(a); });
                TAILLE_ETOILE = c.GetParametre("Taille Etoiles", 0.25f, (a) => { TAILLE_ETOILE = (float)Convert.ToDouble(a); });
                TAILLE_NUAGE = c.GetParametre("Taille Nuages", 10.0f, (a) => { TAILLE_NUAGE = (float)Convert.ToDouble(a); });
            }
            return c;
        }

        private const int NB_TYPES_NUAGES = 3;
        private class Nuage
        {
            public float x, y, z;
            public float tx, ty;
            public float rR, rG, rB;
            public float Alpha;
            public int Type;
        }
        private readonly Etoile[] _etoiles;
        private readonly Nuage[] _nuages;
        private DateTime _dernierDeplacement = DateTime.Now;
        private DateTime _debutAnimation = DateTime.Now;
        private Texture _textureNuage = new Texture();
        private Texture _textureEtoile = new Texture();

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="gl"></param>
        public Nebuleuse(OpenGL gl) : base(gl, VIEWPORT_X, VIEWPORT_Y, VIEWPORT_Z, 100)
        {
            GetConfiguration();
            _etoiles = new Etoile[NB_ETOILES];
            _nuages = new Nuage[NB_NUAGES];
            _textureNuage.Create(gl, c.GetParametre("Nuage nebuleuse", Config.Configuration.GetImagePath("nuages_nebuleuse.png")));
            _textureEtoile.Create(gl, c.GetParametre("Etoile", Config.Configuration.GetImagePath("etoile2.png")));

            // Initialiser les etoiles
            for (int i = 0; i < NB_ETOILES; i++)
            {
                NouvelleEtoile(ref _etoiles[i]);
                // Au debut, on varie la distance des etoiles
                _etoiles[i].z = FloatRandom(-VIEWPORT_Z, _zCamera);
            }

            // Initialiser les nuages
            for (int i = 0; i < NB_NUAGES; i++)
            {
                NouveauNuage(ref _nuages[i]);
                // Au debut, on varie la distance des etoiles
                _nuages[i].z = FloatRandom(-VIEWPORT_Z, _zCamera);
            }

        }

        public override void Dispose()
        {
            base.Dispose();
            _textureNuage?.Destroy(_gl);
            _textureEtoile?.Destroy(_gl);
        }



        private static void NouvelleEtoile(ref Etoile f)
        {
            if (f == null)
                f = new Etoile();
            f.x = FloatRandom(-VIEWPORT_X * 6, VIEWPORT_X * 6);
            f.z = -VIEWPORT_Z;
            f.y = FloatRandom(-VIEWPORT_Y * 6, VIEWPORT_Y * 6);

            f.rR = FloatRandom(0.7f, 1.3f);
            f.rG = FloatRandom(0.7f, 1.3f);
            f.rB = FloatRandom(0.7f, 1.3f);
        }

        private void NouveauNuage(ref Nuage f)
        {
            if (f == null)
                f = new Nuage();
            f.x = FloatRandom(-VIEWPORT_X * 6, VIEWPORT_X * 6);
            f.z = -VIEWPORT_Z;
            f.y = FloatRandom(-VIEWPORT_Y * 6, VIEWPORT_Y * 6);

            f.rR = FloatRandom(0.4f, 1.8f) / 256.0f;
            f.rG = FloatRandom(0.4f, 1.8f) / 256.0f;
            f.rB = FloatRandom(0.4f, 1.8f) / 256.0f;

            f.tx = TAILLE_NUAGE * FloatRandom(0.75f, 1.5f);
            f.ty = TAILLE_NUAGE * FloatRandom(0.75f, 1.5f);
            f.Alpha = (ALPHA_NUAGE / 256.0f) * FloatRandom(0.5f, 1.2f);
            f.Type = random.Next(0, NB_TYPES_NUAGES);
        }

        /// <summary>
        /// Affichage des flocons
        /// </summary>
        /// <param name="g"></param>
        /// <param name="maintenant"></param>
        /// <param name="tailleEcran"></param>
        /// <param name="couleur"></param>
        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            float depuisdebut = (float)(_debutAnimation.Subtract(_dernierDeplacement).TotalMilliseconds / 1000.0);
            float rotationCamera = (float)Math.Sin(depuisdebut / PERIODE_ROTATION) * VITESSE_ROTATION;

            float[] fcolor = { couleur.R / 1024.0f, couleur.G / 1024.0f, couleur.B / 1024.0f, 1 };

            gl.ClearColor(fcolor[0], fcolor[1], fcolor[2], fcolor[3]);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_DEPTH_TEST);
            gl.Disable(OpenGL.GL_DEPTH);

            gl.Enable(OpenGL.GL_FOG);
            gl.Fog(OpenGL.GL_FOG_MODE, OpenGL.GL_LINEAR);
            gl.Fog(OpenGL.GL_FOG_COLOR, fcolor);
            gl.Fog(OpenGL.GL_FOG_DENSITY, 0.1f);
            gl.Fog(OpenGL.GL_FOG_START, VIEWPORT_Z * 1.0f);
            gl.Fog(OpenGL.GL_FOG_END, _zCamera);

            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE);
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.Translate(0, 0, -_zCamera);
            gl.Rotate(0, 0, rotationCamera);

            _textureNuage.Bind(gl);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, ADDITIVE ? OpenGL.GL_ONE : OpenGL.GL_ONE_MINUS_SRC_ALPHA);
            gl.Begin(OpenGL.GL_QUADS);
            foreach (Nuage nuage in _nuages)
            {
                float decalTextureG = (1.0f / NB_TYPES_NUAGES) * nuage.Type;
                float decalTextureD = (1.0f / NB_TYPES_NUAGES) * (nuage.Type + 1);


                gl.Color(couleur.R * nuage.rR, couleur.G * nuage.rG, couleur.B * nuage.rB, nuage.Alpha);
                gl.TexCoord(decalTextureG, 0.0f); gl.Vertex(nuage.x - nuage.tx, nuage.y - nuage.ty, nuage.z);
                gl.TexCoord(decalTextureG, 1.0f); gl.Vertex(nuage.x - nuage.tx, nuage.y + nuage.ty, nuage.z);
                gl.TexCoord(decalTextureD, 1.0f); gl.Vertex(nuage.x + nuage.tx, nuage.y + nuage.ty, nuage.z);
                gl.TexCoord(decalTextureD, 0.0f); gl.Vertex(nuage.x + nuage.tx, nuage.y - nuage.ty, nuage.z);
            }
            gl.End();

            _textureEtoile.Bind(gl);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE);
            gl.Disable(OpenGL.GL_DEPTH);
            gl.Begin(OpenGL.GL_QUADS);
            gl.Color(1.0f, 1.0f, 1.0f, 1.0f);
            foreach (Etoile o in _etoiles)
            {
                gl.Color(couleur.R * o.rR / 128.0f, couleur.G * o.rG / 128.0f, couleur.B * o.rB / 128.0f, ALPHA_ETOILE / 256.0f);

                gl.TexCoord(0.0f, 0.0f); gl.Vertex(o.x - TAILLE_ETOILE, o.y - TAILLE_ETOILE, o.z);
                gl.TexCoord(0.0f, 1.0f); gl.Vertex(o.x - TAILLE_ETOILE, o.y + TAILLE_ETOILE, o.z);
                gl.TexCoord(1.0f, 1.0f); gl.Vertex(o.x + TAILLE_ETOILE, o.y + TAILLE_ETOILE, o.z);
                gl.TexCoord(1.0f, 0.0f); gl.Vertex(o.x + TAILLE_ETOILE, o.y - TAILLE_ETOILE, o.z);
            }
            gl.End();


#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }



        /// <summary>
        /// Deplacement de tous les objets: flocons, camera...
        /// </summary>
        /// <param name="maintenant"></param>
        /// <param name="tailleEcran"></param>
        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.DEPLACE);
#endif

            float depuisdebut = (float)(_debutAnimation.Subtract(_dernierDeplacement).TotalMilliseconds / 1000.0);
            float vitesseCamera = (float)Math.Sin(depuisdebut / PERIODE_ROTATION) * VITESSE_ROTATION;
            float deltaZ = VITESSE * maintenant.intervalleDepuisDerniereFrame;
            float deltaWind = (float)Math.Sin(depuisdebut / PERIODE_TRANSLATION) * VITESSE_TRANSLATION * maintenant.intervalleDepuisDerniereFrame;

            // Deplace les etoiles
            bool trier = false;
            for (int i = 0; i < NB_ETOILES; i++)
            {
                if (_etoiles[i].z > _zCamera)
                {
                    NouvelleEtoile(ref _etoiles[i]);
                    trier = true;
                }
                else
                {
                    _etoiles[i].z += deltaZ;
                    _etoiles[i].x += deltaWind;
                }
            }

            if (trier)
                Array.Sort(_etoiles, delegate (Etoile O1, Etoile O2)
                {
                    if (O1.z > O2.z) return 1;
                    if (O1.z < O2.z) return -1;
                    return 0;
                });

            // Deplace les nuages
            trier = false;
            for (int i = 0; i < NB_NUAGES; i++)
            {
                if (_nuages[i].z > _zCamera)
                {
                    NouveauNuage(ref _nuages[i]);
                    trier = true;
                }
                else
                {
                    _nuages[i].z += deltaZ * RATIO_VITESSE_NUAGES;
                    _nuages[i].x += deltaWind;
                }
            }

            if (trier)
                Array.Sort(_nuages, delegate (Nuage O1, Nuage O2)
                {
                    if (O1.z > O2.z) return 1;
                    if (O1.z < O2.z) return -1;
                    return 0;
                });
            _dernierDeplacement = maintenant.temps;

#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif

        }

        /// <summary>
        /// Reception d'une touche
        /// </summary>
        /// <param name="f"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        /*
        public override bool KeyDown(Form f, Keys k)
        {
            switch (k)
            {
                case TOUCHE_ADDITIVE:
                    ADDITIVE = !ADDITIVE;
                    break;

                default:
                    return base.KeyDown(f, k);
            }

            return base.KeyDown(f, k); ;
        }
        */
#if TRACER
        public override String DumpRender()
        {
            return base.DumpRender() + " NbParticules:" + NB_ETOILES;
        }

#endif
    }
}
