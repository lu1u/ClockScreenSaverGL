using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.Fourmilliere;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;
using SharpGL;
using SharpGL.SceneGraph.Assets;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    internal class Fourmis : Fond
    {
        #region Parametres
        private const string CAT = "Fourmis";
        protected CategorieConfiguration c;
        private int NB_FOURMIS, NB_OBSTACLES;
        private float TAILLE_FOURMI, TAILLE_OBSTACLES;
        private float VITESSE;
        public double DELAI_TIMER;
        private int DETAIL_CERCLES;
        private int TAILLE_GRILLE;
        private bool AFFICHER_PERCEPTION;
        private bool AFFICHER_RECHERCHE_NOURRITURE, AFFICHER_RAPPORTE_NOURRITURE;
        #endregion


        private List<Fourmi> _fourmis;
        private Monde _monde;

        private Texture _texture = new Texture();
        private TimerIsole _timer;
        private float _maxPheromoneCherche, _maxPheromoneRapporte;
        public Fourmis(OpenGL gl) : base(gl)
        {
        }

        /// <summary>
        /// Initialisation
        /// </summary>
        /// <param name="gl"></param>
        protected override void Init(OpenGL gl)
        {

            string nomImage = c.getParametre("Etoile", Configuration.getImagePath("fourmi.png"));
            _texture.Create(gl, nomImage);
            _fourmis = new List<Fourmi>();
            _timer = new TimerIsole(DELAI_TIMER, false);

            // Construction des obstacles
            for (int i = 0; i < NB_OBSTACLES; i++)
            {
                float xy = FloatRandom(0.0f, 1.0f);
                _monde._listeObstacles.Add(new Obstacle(xy, FloatRandom(0.8f, 1.2f) - xy, FloatRandom(0.25f, 2.0f) * TAILLE_OBSTACLES, FloatRandom(-0.1f, 0.1f)));
            }
        }

        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            CouleurGlobale c = new CouleurGlobale(couleur);

            gl.LoadIdentity();

            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_FOG);
            gl.Disable(OpenGL.GL_DEPTH);

            using (new Viewport2D(gl, left: 0.0f, top: 0.0f, right: 1.0f, bottom: 1.0f))
            {
                gl.Disable(OpenGL.GL_TEXTURE_2D);
                gl.Disable(OpenGL.GL_BLEND);

                if (AFFICHER_RECHERCHE_NOURRITURE)
                    _monde._pheromonesRechercheNourriture.Affiche(gl, _maxPheromoneCherche, new CouleurGlobale(couleur).getColorWithHueChange(-0.25f));

                if (AFFICHER_RAPPORTE_NOURRITURE)
                    _monde._grilleRapporteNourriture.Affiche(gl, _maxPheromoneRapporte, new CouleurGlobale(couleur).getColorWithHueChange(0.25f));

                // Obstacles
                gl.Enable(OpenGL.GL_BLEND);
                gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
                foreach (Obstacle o in _monde._listeObstacles)
                    o.Affiche(gl, c, DETAIL_CERCLES);
                gl.Disable(OpenGL.GL_BLEND);

                // Nid
                gl.Color(0.5f, 0.5f, 0.5f);
                gl.Disable(OpenGL.GL_TEXTURE_2D);
                DessineCercle(gl, _monde.X_NID, _monde.Y_NID, _monde.TAILLE_NID, DETAIL_CERCLES);

                // Source de nourriture
                gl.Color(0.8f, 0.8f, 0.8f);
                gl.Disable(OpenGL.GL_TEXTURE_2D);
                DessineCercle(gl, _monde.X_NOURRITURE, _monde.Y_NOURRITURE, _monde.TAILLE_NOURRITURE, DETAIL_CERCLES);


                // Fourmis
                gl.Enable(OpenGL.GL_TEXTURE_2D);
                gl.Enable(OpenGL.GL_BLEND);
                gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
                _texture.Bind(gl);


                foreach (Fourmi f in _fourmis)
                    f.Affiche(gl, TAILLE_FOURMI, AFFICHER_PERCEPTION, c);
            }

#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }



        public override bool ClearBackGround(OpenGL gl, Color c)
        {
            gl.ClearColor(c.R / 2048.0f, c.G / 2048.0f, c.B / 2048.0f, 1);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            return true;
        }

        /// <summary>
        /// Deplacer les fourmis
        /// </summary>
        /// <param name="maintenant"></param>
        /// <param name="tailleEcran"></param>
        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
            _maxPheromoneCherche = _monde._pheromonesRechercheNourriture.Evapore(maintenant.intervalleDepuisDerniereFrame * 0.02f);
            _maxPheromoneRapporte = _monde._grilleRapporteNourriture.Evapore(maintenant.intervalleDepuisDerniereFrame * 0.02f);

            if (_timer.Ecoule())
                if (_fourmis.Count < NB_FOURMIS)
                {
                    Fourmi f = new Fourmi(_monde.DISTANCE_PERCEPTION);
                    f.X = _monde.X_NID;
                    f.Y = _monde.Y_NID;
                    f.Cap = FloatRandom(0, DEUX_PI);
                    f.Vitesse = VITESSE * FloatRandom(0.9f, 1.1f);
                    _fourmis.Add(f);
                }

            foreach (Fourmi f in _fourmis)
                f.Deplace(_monde, maintenant.intervalleDepuisDerniereFrame);
        }

        public override CategorieConfiguration getConfiguration()
        {
            if (c == null)
            {
                c = Configuration.getCategorie(CAT);
                TAILLE_GRILLE = c.getParametre("Taille grille marqueurs", 1000);
                _monde = new Monde(TAILLE_GRILLE);
                _monde.X_NID = c.getParametre("X Nid", 0.1f, a => { _monde.X_NID = (float)Convert.ToDouble(a); });
                _monde.Y_NID = c.getParametre("Y Nid", 0.1f, a => { _monde.Y_NID = (float)Convert.ToDouble(a); });
                _monde.X_NOURRITURE = c.getParametre("X Nourriture", 0.75f, a => { _monde.X_NOURRITURE = (float)Convert.ToDouble(a); });
                _monde.Y_NOURRITURE = c.getParametre("Y Nourriture", 0.9f, a => { _monde.Y_NOURRITURE = (float)Convert.ToDouble(a); });
                _monde.TAILLE_NOURRITURE = c.getParametre("Taille nourriture", 0.02f, a => { _monde.TAILLE_NOURRITURE = (float)Convert.ToDouble(a); });
                _monde.DISTANCE_PERCEPTION = c.getParametre("Distance perception", 0.03f, a => { _monde.DISTANCE_PERCEPTION = (float)Convert.ToDouble(a); });
                _monde.VALEUR_RENFORCE = c.getParametre("Valeur renforcement", 0.00001f, a => { _monde.VALEUR_RENFORCE = (float)Convert.ToDouble(a); });

                _monde.DELAI_MARQUEUR = c.getParametre("Delai marqueur", 10, a => { _monde.DELAI_MARQUEUR = Convert.ToInt16(a); });
                _monde.TAILLE_NID = c.getParametre("Taille Nid", 0.02f, a => { _monde.TAILLE_NID = (float)Convert.ToDouble(a); });

                NB_FOURMIS = c.getParametre("Nb max fourmis", 100, a => { NB_FOURMIS = Convert.ToInt32(a); });
                NB_OBSTACLES = c.getParametre("Nb obstacles", 2);
                TAILLE_OBSTACLES = c.getParametre("Taille obstacles", 0.02f);
                AFFICHER_PERCEPTION = c.getParametre("Afficher perception", true, a => { AFFICHER_PERCEPTION = Convert.ToBoolean(a); });
                AFFICHER_RECHERCHE_NOURRITURE = c.getParametre("Afficher recherche nourriture", true, a => { AFFICHER_RECHERCHE_NOURRITURE = Convert.ToBoolean(a); });
                AFFICHER_RAPPORTE_NOURRITURE = c.getParametre("Afficher rapporte nourriture", true, a => { AFFICHER_RAPPORTE_NOURRITURE = Convert.ToBoolean(a); });
                TAILLE_FOURMI = c.getParametre("Taille fourmi", 0.01f, a => { TAILLE_FOURMI = (float)Convert.ToDouble(a); });
                VITESSE = c.getParametre("Vitesse fourmi", 0.5f, a => { VITESSE = (float)Convert.ToDouble(a); });
                _monde.VARIATION_CAP = c.getParametre("Variation cap", 2.0f, a => { _monde.VARIATION_CAP = (float)Convert.ToDouble(a); });
                _monde.VARIATION_CAP_OBSTACLE = c.getParametre("Variation cap", 4.0f, a => { _monde.VARIATION_CAP_OBSTACLE = (float)Convert.ToDouble(a); });
                DETAIL_CERCLES = c.getParametre("Détail cercles", 16, a => { DETAIL_CERCLES = Convert.ToInt16(a); });
                DELAI_TIMER = c.getParametre("Delai création fourmi", 300, a => { DELAI_TIMER = Convert.ToInt16(a); });
            }
            return c;
        }




        public override void fillConsole(OpenGL gl)
        {
            base.fillConsole(gl);
            Console c = Console.getInstance(gl);
            c.AddLigne(Color.IndianRed, "Nb fourmis " + _fourmis.Count);
        }
    }
}
