using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.OpenGLUtils;
using SharpGL;
using System;
using System.Drawing;
using GLfloat = System.Single;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD
{
    internal class Moto : MateriauGlobal
    {
        public const string CAT = "Moto";
        public const float ANGLE_DROIT = (float)(Math.PI / 2.0);
        private const float COORD_TEXTURE_DECORS_G = 0.01f;
        private const float COORD_TEXTURE_DECORS_D = 0.249f;
        private const float COORD_TEXTURE_TALUSG_G = 0.25f;
        private const float COORD_TEXTURE_TALUSG_D = 0.5f;
        private const float COORD_TEXTURE_ROUTE_G = 0.5f;
        private const float COORD_TEXTURE_ROUTE_D = 0.75f;
        private const float COORD_TEXTURE_TALUSD_G = 0.75f;
        private const float COORD_TEXTURE_TALUSD_D = 1.0f;
        private const int NB_TYPES = 5;
        public const float TAILLE_DECORS = 0.5f;

        private CategorieConfiguration c;
        private int NB_TRONCONS;
        private float LONGUEUR_TRONCON;
        private float LARGEUR_TRONCON;
        private float ANGLE_TRONCON;
        private float RATIO_FOG;
        private float FOG_DENSITY;
        private float ANGLE_VIRAGE;
        private int NB_PAS_MIN;
        private int NB_PAS_MAX;
        private float HORIZON_L;
        private float HORIZON_H;
        private float COULEUR;
        private float VITESSE;
        private float ALTITUDE;
        private float HAUTEUR_TALUS;
        private float LARGEUR_TALUS;
        private float MAX_PENTE;
        private float VITESSE_PENTE;
        private static readonly GLfloat[] fogcolor = { 0.11f, 0.11f, 0.11f, 1 };
        private const int INDICE_CAMERA = 2;
        private const int INDICE_REGARD = 5;
        private const float SEUIL = 0.55f;
        private TextureAsynchrone _texture;
        private TextureAsynchrone _texturePaysage;

        // Virage en cours
        private int _nombrePasVirageEnCours;
        private float _anglePasVirageEnCours;

        // Angle de la moto
        private float _angleMoto = 0;
        private float _angleMotoCible = 0;

        // Pente
        private float _pente = 0;
        private float _altitude = 0;

        // Camera
        private Vecteur3D _camera, _regard;
        private readonly Troncon[] _troncons;

        // Hauteur variable des talus
        private float _hauteurTalusDroit = 0;
        private float _hauteurTalusGauche = 0;

        // Pour les billboards
        private Vecteur3D vRight;
        private Vecteur3D vUp;


        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                NB_TRONCONS = c.GetParametre("Nb troncons", 50);
                LONGUEUR_TRONCON = c.GetParametre("Longueur Troncon", 0.5f, (a) => { LONGUEUR_TRONCON = (float)Convert.ToDouble(a); });
                LARGEUR_TRONCON = c.GetParametre("Largeur Troncon", 0.25f, (a) => { LARGEUR_TRONCON = (float)Convert.ToDouble(a); });
                ANGLE_TRONCON = c.GetParametre("Angle Troncon", 0.5f, (a) => { ANGLE_TRONCON = (float)Convert.ToDouble(a); });
                RATIO_FOG = c.GetParametre("Ratio fog", 0.5f, (a) => { RATIO_FOG = (float)Convert.ToDouble(a); });
                FOG_DENSITY = c.GetParametre("Fog Density", 0.5f, (a) => { FOG_DENSITY = (float)Convert.ToDouble(a); });
                ANGLE_VIRAGE = c.GetParametre("Angle virage", 2.0f, (a) => { ANGLE_VIRAGE = (float)Convert.ToDouble(a); });
                NB_PAS_MIN = c.GetParametre("Pas virage min", 5, (a) => { NB_PAS_MIN = Convert.ToInt32(a); });
                NB_PAS_MAX = c.GetParametre("Pas virage max", 20, (a) => { NB_PAS_MAX = Convert.ToInt32(a); });
                HORIZON_L = c.GetParametre("Horizon Largeur", 20.0f, (a) => { HORIZON_L = (float)Convert.ToDouble(a); });
                HORIZON_H = c.GetParametre("Horizon Hauteur", 20.0f, (a) => { HORIZON_H = (float)Convert.ToDouble(a); });
                COULEUR = c.GetParametre("Couleur", 20.0f, (a) => { COULEUR = (float)Convert.ToDouble(a); });
                VITESSE = c.GetParametre("Vitesse", 2.0f, (a) => { VITESSE = (float)Convert.ToDouble(a); });
                ALTITUDE = c.GetParametre("Altitude", 0.25f, (a) => { ALTITUDE = (float)Convert.ToDouble(a); });
                HAUTEUR_TALUS = c.GetParametre("Hauteur Talus", 0.25f, (a) => { HAUTEUR_TALUS = (float)Convert.ToDouble(a); });
                LARGEUR_TALUS = c.GetParametre("Largeur Talus", 0.25f, (a) => { LARGEUR_TALUS = (float)Convert.ToDouble(a); });
                MAX_PENTE = c.GetParametre("Max pente", 1.0f, (a) => { MAX_PENTE = (float)Convert.ToDouble(a); });
                VITESSE_PENTE = c.GetParametre("Vitesse pente", 0.25f, (a) => { VITESSE_PENTE = (float)Convert.ToDouble(a); });
            }
            return c;
        }

        private class Troncon
        {
            public class Decors
            {
                public Vecteur3D position = new Vecteur3D();
                public int type;
                public float taille;

                internal Decors Clone()
                {
                    Decors d = new Decors();
                    d.position = new Vecteur3D(position);
                    d.type = type;
                    d.taille = taille;
                    return d;
                }
            }
            public class Cote
            {
                public Vecteur3D bord = new Vecteur3D();
                public Vecteur3D talus = new Vecteur3D();
                public Decors decors = new Decors();
            };

            public float angle;
            public Vecteur3D centre = new Vecteur3D();
            public Cote gauche = new Cote();
            public Cote droite = new Cote();
            //
            //  <talus gauche> <decors gauche(type gauche), _taille gauche)> <gauche> <centre> <droite> <decors droite> <talus droite>
            //
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="gl"></param>
        public Moto(OpenGL gl) : base(gl)
        {
            GetConfiguration();
            _texture = new TextureAsynchrone(gl, Configuration.GetImagePath("texture_route.png"));
            _texture.Init();
            _texturePaysage = new TextureAsynchrone(gl, c.GetParametre("Texture Paysage", Configuration.GetImagePath("paysage_route.png")));
            _texturePaysage.Init();

            _anglePasVirageEnCours = 0.1f * FloatRandom(-ANGLE_TRONCON, ANGLE_TRONCON);
            _nombrePasVirageEnCours = random.Next(NB_PAS_MIN, NB_PAS_MAX);
            _troncons = new Troncon[NB_TRONCONS];

            for (int i = 0; i < NB_TRONCONS; i++)
                nouveauTroncon(i);

            _camera = new Vecteur3D(_troncons[INDICE_CAMERA].centre.x, ALTITUDE, _troncons[INDICE_CAMERA].centre.z);
            _regard = new Vecteur3D(_troncons[INDICE_REGARD].centre.x, ALTITUDE, _troncons[INDICE_REGARD].centre.z);

            LIGHTPOS[0] = 20;
            LIGHTPOS[1] = 1;
            LIGHTPOS[2] = 1;

            COL_COLOR[0] = 1.0f;
            COL_COLOR[1] = 1.0f;
            COL_COLOR[2] = 1.0f;

        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Creer un nouveau troncon
        /// </summary>
        /// <param name="i"></param>
        ///////////////////////////////////////////////////////////////////////////////////////////
        private void nouveauTroncon(int i)
        {
            _troncons[i] = new Troncon();

            _altitude += _pente;

            if (i > 0)
            {
                _troncons[i].angle = _troncons[i - 1].angle + _anglePasVirageEnCours;
                _nombrePasVirageEnCours--;
                if (_nombrePasVirageEnCours <= 0)
                {
                    _anglePasVirageEnCours = 0.1f * FloatRandom(-ANGLE_TRONCON, ANGLE_TRONCON);
                    _nombrePasVirageEnCours = random.Next(NB_PAS_MIN, NB_PAS_MAX);
                }

                float cosAngle = (float)Math.Cos(_troncons[i].angle);
                float sinAngle = (float)Math.Sin(_troncons[i].angle);

                _troncons[i].centre.x = _troncons[i - 1].centre.x + sinAngle * LONGUEUR_TRONCON;
                _troncons[i].centre.y = _altitude;
                _troncons[i].centre.z = _troncons[i - 1].centre.z + cosAngle * LONGUEUR_TRONCON;
            }
            else
            {
                _troncons[i].angle = 0;
                _troncons[i].centre.x = 0;
                _troncons[i].centre.y = _altitude;
                _troncons[i].centre.z = 0;
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Gauche
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Bord de la route
            _troncons[i].gauche.bord.x = _troncons[i].centre.x - (float)Math.Sin(_troncons[i].angle - ANGLE_DROIT) * LARGEUR_TRONCON * 2.0f;
            _troncons[i].gauche.bord.y = _troncons[i].centre.y;
            _troncons[i].gauche.bord.z = _troncons[i].centre.z - (float)Math.Cos(_troncons[i].angle - ANGLE_DROIT) * LARGEUR_TRONCON * 2.0f;

            // Bord exterieur du talus
            Varie(ref _hauteurTalusGauche, HAUTEUR_TALUS * -1.5f, HAUTEUR_TALUS * 1.5f, 0.1f, 1f);
            _troncons[i].gauche.talus.x = _troncons[i].centre.x + (float)Math.Sin(_troncons[i].angle + ANGLE_DROIT) * LARGEUR_TALUS;
            _troncons[i].gauche.talus.y = _troncons[i].centre.y + _hauteurTalusGauche;
            _troncons[i].gauche.talus.z = _troncons[i].centre.z + (float)Math.Cos(_troncons[i].angle + ANGLE_DROIT) * LARGEUR_TALUS;

            // Decors
            _troncons[i].gauche.decors.type = random.Next(2) > 0 ? 0 : random.Next(1, NB_TYPES);
            _troncons[i].gauche.decors.taille = TAILLE_DECORS;
            if (i > 0)
            {
                _troncons[i].gauche.decors.position = (_troncons[i - 1].gauche.talus + _troncons[i].gauche.bord) / 2.0f;
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Droite
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Bord de la route
            _troncons[i].droite.bord.x = _troncons[i].centre.x - (float)Math.Sin(_troncons[i].angle + ANGLE_DROIT) * LARGEUR_TRONCON * 0.5f;
            _troncons[i].droite.bord.y = _troncons[i].centre.y;
            _troncons[i].droite.bord.z = _troncons[i].centre.z - (float)Math.Cos(_troncons[i].angle + ANGLE_DROIT) * LARGEUR_TRONCON * 0.5f;

            Varie(ref _hauteurTalusDroit, HAUTEUR_TALUS * -1.5f, HAUTEUR_TALUS * 1.5f, 0.2f, 1f);
            _troncons[i].droite.talus.x = _troncons[i].centre.x - (float)Math.Sin(_troncons[i].angle + ANGLE_DROIT) * LARGEUR_TALUS;
            _troncons[i].droite.talus.y = _troncons[i].droite.bord.y - _hauteurTalusDroit;
            _troncons[i].droite.talus.z = _troncons[i].centre.z - (float)Math.Cos(_troncons[i].angle + ANGLE_DROIT) * LARGEUR_TALUS;


            _troncons[i].droite.decors.type = random.Next(2) > 0 ? 0 : random.Next(1, NB_TYPES);
            _troncons[i].droite.decors.taille = TAILLE_DECORS;//* FloatRandom(0.9f, 1.1f);

            if (i > 0)
            {
                _troncons[i].droite.decors.position = (_troncons[i].droite.bord + _troncons[i - 1].droite.talus) / 2.0f;
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Effacer l'ecran
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="couleur"></param>
        ///////////////////////////////////////////////////////////////////////////////////////////
        public override bool ClearBackGround(OpenGL gl, Color couleur)
        {
            gl.ClearColor(couleur.R / COULEUR, couleur.G / COULEUR, couleur.B / COULEUR, 1);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            return true;
        }


        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Affichage
        /// </summary>
        /// <param name="g"></param>
        /// <param name="maintenant"></param>
        /// <param name="tailleEcran"></param>
        /// <param name="couleur"></param>
        ///////////////////////////////////////////////////////////////////////////////////////////
        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif

            gl.LoadIdentity();
            gl.Disable(OpenGL.GL_DEPTH);
            gl.ShadeModel(OpenGL.GL_SMOOTH);
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.CullFace(OpenGL.GL_BACK);
            gl.Enable(OpenGL.GLU_CULLING);
            bilboardParams(gl);
            gl.LoadIdentity();
            gl.Rotate(0, 0, _angleMoto * -180.0f / (float)Math.PI);
            gl.LookAt(_camera.x, _camera.y, _camera.z, _regard.x, _regard.y, _regard.z, 0, 1, 0);
            changeZoom(gl, tailleEcran.Width, tailleEcran.Height, 0.001f, HORIZON_L * 1.5f);
            setGlobalMaterial(gl, couleur);

            float[] col = { couleur.R / COULEUR * RATIO_FOG, couleur.G / COULEUR * RATIO_FOG, couleur.B / COULEUR * RATIO_FOG, 1 };
            gl.Color(col);
            brouillard(gl, col);

            if (_texturePaysage?.Pret == true)
                dessinePaysage(gl);

            if (_texture?.Pret == true)
                dessineRoute(gl);

#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

        private void dessineRoute(OpenGL gl)
        {
            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_BLEND);
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            _texture.Texture.Bind(gl);

            for (int i = NB_TRONCONS - 1; i > 0; i--)
            {
                float haut = (1.0f / NB_TYPES) * _troncons[i].gauche.decors.type;
                float bas = (1.0f / NB_TYPES) * (_troncons[i].gauche.decors.type + 1);

                gl.Begin(OpenGL.GL_QUAD_STRIP);

                // Talus gauche
                gl.TexCoord(COORD_TEXTURE_TALUSG_G, haut); NormalQuad(gl, _troncons[i].gauche.talus, _troncons[i - 1].gauche.talus, _troncons[i - 1].gauche.bord); _troncons[i - 1].gauche.talus.Vertex(gl);
                gl.TexCoord(COORD_TEXTURE_TALUSG_G, bas); NormalQuad(gl, _troncons[i].gauche.bord, _troncons[i].gauche.talus, _troncons[i - 1].gauche.talus); _troncons[i].gauche.talus.Vertex(gl);
                gl.TexCoord(COORD_TEXTURE_TALUSG_D, haut); NormalQuad(gl, _troncons[i - 1].gauche.talus, _troncons[i - 1].gauche.bord, _troncons[i].gauche.bord); _troncons[i - 1].gauche.bord.Vertex(gl);
                gl.TexCoord(COORD_TEXTURE_TALUSG_D, bas); NormalQuad(gl, _troncons[i - 1].gauche.bord, _troncons[i].gauche.bord, _troncons[i].gauche.talus); _troncons[i].gauche.bord.Vertex(gl);

                // Route
                Vecteur3D.Y.Normal(gl);
                gl.TexCoord(COORD_TEXTURE_ROUTE_G, haut); _troncons[i - 1].gauche.bord.Vertex(gl);
                gl.TexCoord(COORD_TEXTURE_ROUTE_G, bas); _troncons[i].gauche.bord.Vertex(gl);
                gl.TexCoord(COORD_TEXTURE_ROUTE_D, haut); _troncons[i - 1].droite.bord.Vertex(gl);
                gl.TexCoord(COORD_TEXTURE_ROUTE_D, bas); _troncons[i].droite.bord.Vertex(gl);

                // Talus droite
                gl.TexCoord(COORD_TEXTURE_TALUSD_G, bas); NormalQuad(gl, _troncons[i].droite.bord, _troncons[i - 1].droite.bord, _troncons[i - 1].droite.talus); _troncons[i - 1].droite.bord.Vertex(gl);
                gl.TexCoord(COORD_TEXTURE_TALUSD_G, haut); NormalQuad(gl, _troncons[i].droite.talus, _troncons[i].droite.bord, _troncons[i - 1].droite.bord); _troncons[i].droite.bord.Vertex(gl);
                gl.TexCoord(COORD_TEXTURE_TALUSD_D, bas); NormalQuad(gl, _troncons[i - 1].droite.bord, _troncons[i - 1].droite.talus, _troncons[i].droite.talus); _troncons[i - 1].droite.talus.Vertex(gl);
                gl.TexCoord(COORD_TEXTURE_TALUSD_D, haut); NormalQuad(gl, _troncons[i - 1].droite.talus, _troncons[i].droite.talus, _troncons[i].droite.bord); _troncons[i].droite.talus.Vertex(gl);

                gl.End();
            }

            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_COLOR_MATERIAL);
            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);

            Troncon.Decors[] decors = new Troncon.Decors[NB_TRONCONS * 2];
            // Decors de gauche
            for (int i = 0; i < NB_TRONCONS; i++)
            {
                decors[(i * 2)] = _troncons[i].droite.decors;
                decors[(i * 2) + 1] = _troncons[i].gauche.decors;
            }

            Array.Sort(decors, delegate (Troncon.Decors O1, Troncon.Decors O2)
            {
                float camDistance1 = _camera.Distance(O1.position);
                float camDistance2 = _camera.Distance(O2.position);

                if (camDistance1 > camDistance2) return -1;
                if (camDistance1 < camDistance2) return 1;
                return 0;
            });

            for (int i = 0; i < NB_TRONCONS * 2; i++)
                dessineDecors(gl, decors[i]);
        }


        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Parametrer le brouillard
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="col"></param>
        ///////////////////////////////////////////////////////////////////////////////////////////
        private void brouillard(OpenGL gl, float[] col)
        {
            // Brouillard
            gl.Enable(OpenGL.GL_FOG);
            fogcolor[0] = col[0] * RATIO_FOG;
            fogcolor[1] = col[1] * RATIO_FOG;
            fogcolor[2] = col[2] * RATIO_FOG;

            gl.Fog(OpenGL.GL_FOG_MODE, OpenGL.GL_EXP);
            gl.Fog(OpenGL.GL_FOG_COLOR, fogcolor);
            gl.Fog(OpenGL.GL_FOG_DENSITY, FOG_DENSITY);
            gl.Fog(OpenGL.GL_FOG_START, 0);
            gl.Fog(OpenGL.GL_FOG_END, _camera.Distance(_troncons[NB_TRONCONS - 1].centre) * 2.0f);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Calcul les parametres pour afficher les Billboards
        /// </summary>
        /// <param name="gl"></param>
        ///////////////////////////////////////////////////////////////////////////////////////////
        private void bilboardParams(OpenGL gl)
        {
            gl.LookAt(_camera.x, _camera.y, _camera.z, _regard.x, _regard.y, _regard.z, 0, 1, 0);
            float[] mat = new float[16];
            gl.GetFloat(OpenGL.GL_MODELVIEW_MATRIX, mat);

            vRight = new Vecteur3D(mat[0], mat[4], mat[8]);
            vUp = new Vecteur3D(mat[1], mat[5], mat[9]);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Dessine un decors sur le bord de la route
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="cote"></param>
        private void dessineDecors(OpenGL gl, Troncon.Decors decors)
        {
            if (decors.type >= NB_TYPES)
                return;

            // S'assurer que le billboard est bien paralelle a l'ecran
            Vecteur3D p1 = decors.position + ((-vRight - vUp) * decors.taille);
            Vecteur3D p2 = decors.position + ((vRight - vUp) * decors.taille);
            Vecteur3D p3 = decors.position + ((vRight + vUp) * decors.taille);
            Vecteur3D p4 = decors.position + ((-vRight + vUp) * decors.taille);
            p1.y += decors.taille * 0.8f;
            p2.y += decors.taille * 0.8f;
            p3.y += decors.taille * 0.8f;
            p4.y += decors.taille * 0.8f;

            float haut = (1.0f / NB_TYPES) * decors.type;
            float bas = (1.0f / NB_TYPES) * (decors.type + 1);

            gl.Begin(OpenGL.GL_QUADS);
            gl.TexCoord(COORD_TEXTURE_DECORS_G, haut); p4.Vertex(gl);
            gl.TexCoord(COORD_TEXTURE_DECORS_D, haut); p3.Vertex(gl);
            gl.TexCoord(COORD_TEXTURE_DECORS_D, bas); p2.Vertex(gl);
            gl.TexCoord(COORD_TEXTURE_DECORS_G, bas); p1.Vertex(gl);
            gl.End();
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Dessine le paysage lointain
        /// </summary>
        /// <param name="gl"></param>
        ///////////////////////////////////////////////////////////////////////////////////////////
        private void dessinePaysage(OpenGL gl)
        {
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            _texturePaysage.Texture.Bind(gl);
            gl.Translate(_camera.x, _camera.y, _camera.z);
            gl.Begin(OpenGL.GL_QUADS);

            // Z
            gl.TexCoord(1.0f, 0.01f); gl.Vertex(-HORIZON_L, HORIZON_H, HORIZON_L);
            gl.TexCoord(0.0f, 0.01f); gl.Vertex(HORIZON_L, HORIZON_H, HORIZON_L);
            gl.TexCoord(0.0f, 0.99f); gl.Vertex(HORIZON_L, -0.5f * HORIZON_H, HORIZON_L);
            gl.TexCoord(1.0f, 0.99f); gl.Vertex(-HORIZON_L, -0.5f * HORIZON_H, HORIZON_L);

            // -Z
            gl.TexCoord(0.0f, 0.01f); gl.Vertex(HORIZON_L, HORIZON_H, -HORIZON_L);
            gl.TexCoord(1.0f, 0.01f); gl.Vertex(-HORIZON_L, HORIZON_H, -HORIZON_L);
            gl.TexCoord(1.0f, 0.99); gl.Vertex(-HORIZON_L, -0.5f * HORIZON_H, -HORIZON_L);
            gl.TexCoord(0.0f, 0.99); gl.Vertex(HORIZON_L, -0.5f * HORIZON_H, -HORIZON_L);

            // X
            gl.TexCoord(1.0f, 0.01f); gl.Vertex(HORIZON_L, HORIZON_H, HORIZON_L);
            gl.TexCoord(0.0f, 0.01f); gl.Vertex(HORIZON_L, HORIZON_H, -HORIZON_L);
            gl.TexCoord(0.0f, 0.99); gl.Vertex(HORIZON_L, -0.5f * HORIZON_H, -HORIZON_L);
            gl.TexCoord(1.0f, 0.99); gl.Vertex(HORIZON_L, -0.5f * HORIZON_H, HORIZON_L);

            // -X
            gl.TexCoord(0.0f, 0.01f); gl.Vertex(-HORIZON_L, HORIZON_H, -HORIZON_L);
            gl.TexCoord(1.0f, 0.01f); gl.Vertex(-HORIZON_L, HORIZON_H, HORIZON_L);
            gl.TexCoord(1.0f, 0.99); gl.Vertex(-HORIZON_L, -0.5f * HORIZON_H, HORIZON_L);
            gl.TexCoord(0.0f, 0.99); gl.Vertex(-HORIZON_L, -0.5f * HORIZON_H, -HORIZON_L);

            // Y
            gl.TexCoord(0.0f, 0.01f); gl.Vertex(-HORIZON_L, HORIZON_H, -HORIZON_L);
            gl.TexCoord(1.0f, 0.01f); gl.Vertex(-HORIZON_L, HORIZON_H, HORIZON_L);
            gl.TexCoord(1.0f, 0.051f); gl.Vertex(HORIZON_L, HORIZON_H, HORIZON_L);
            gl.TexCoord(0.0f, 0.051f); gl.Vertex(HORIZON_L, HORIZON_H, -HORIZON_L);

            // -Y
            gl.TexCoord(0.0f, 0.9f); gl.Vertex(HORIZON_L, -0.5f * HORIZON_H, -HORIZON_L);
            gl.TexCoord(1.0f, 0.9f); gl.Vertex(HORIZON_L, -0.5f * HORIZON_H, HORIZON_L);
            gl.TexCoord(1.0f, 0.99f); gl.Vertex(-HORIZON_L, -0.5f * HORIZON_H, HORIZON_L);
            gl.TexCoord(0.0f, 0.99f); gl.Vertex(-HORIZON_L, -0.5f * HORIZON_H, -HORIZON_L);

            gl.End();
            gl.Translate(-_camera.x, -_camera.y, -_camera.z);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Deplacement de tous les objets
        /// </summary>
        /// <param name="maintenant"></param>
        /// <param name="tailleEcran"></param>
        ///////////////////////////////////////////////////////////////////////////////////////////
        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.DEPLACE);
#endif
            if (_camera.Distance(_troncons[INDICE_CAMERA].centre) < SEUIL)
            {
                // Supprimer le troncon le plus proche
                for (int i = 1; i < NB_TRONCONS; i++)
                {
                    _troncons[i - 1] = _troncons[i];
                }

                nouveauTroncon(NB_TRONCONS - 1);
            }

            // Deplacer la camera
            Vecteur3D decalage = (_troncons[INDICE_CAMERA].centre - _camera) + (Vecteur3D.Y * ALTITUDE);
            decalage.Normalize();
            _camera += decalage * (maintenant.intervalleDepuisDerniereFrame * VITESSE);
            //_camera.y = _troncons[INDICE_CAMERA].centre.y + ALTITUDE;

            // Deplacer le point vise par le regard
            decalage = (_troncons[INDICE_REGARD].centre - _regard) + (Vecteur3D.Y * ALTITUDE);
            decalage.Normalize();
            _regard += decalage * (maintenant.intervalleDepuisDerniereFrame * VITESSE);
            //_regard.y = _troncons[INDICE_REGARD].centre.y + ALTITUDE;

            _angleMotoCible = (_troncons[INDICE_CAMERA + 1].angle - _troncons[INDICE_CAMERA].angle) * ANGLE_VIRAGE;
            _angleMoto += (_angleMotoCible - _angleMoto) * maintenant.intervalleDepuisDerniereFrame * 1.5f;


            Varie(ref _pente, -MAX_PENTE, MAX_PENTE, VITESSE_PENTE, maintenant.intervalleDepuisDerniereFrame);
#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif
        }
    }
}