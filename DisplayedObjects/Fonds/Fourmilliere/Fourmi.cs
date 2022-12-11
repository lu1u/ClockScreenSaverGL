using SharpGL;
using System;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.Fourmilliere
{
    internal class Fourmi
    {
        private const float RADIAN_TO_DEG = 180.0f / (float)Math.PI;
        private const float PHEROMONES_MAX = 1.0f;
        private float _x, _y, _cap, _vitesse, _dx, _dy;
        private float _dpx, _dpy;
        private float _pheromonesRestantes;
        private float _distancePerception;
        private int _noImage = 0;

        private enum MODE
        {
            RECHERCHE_NOURRITURE, RETOUR_NID
        }

        private MODE _mode;

        public Fourmi(float distancePerception)
        {
            _mode = MODE.RECHERCHE_NOURRITURE;
            _pheromonesRestantes = PHEROMONES_MAX;
            _distancePerception = distancePerception;
        }

        public float X
        {
            get => _x;
            set => _x = value;
        }

        public float Y
        {
            get => _y;
            set => _y = value;
        }

        public float dX => _dx;
        public float dY => _dy;

        public float Cap
        {
            get => _cap;
            set
            {
                _cap = value;
                _dx = (float)Math.Cos(_cap) * _vitesse;
                _dy = (float)Math.Sin(_cap) * _vitesse;
            }


        }

        public float Vitesse
        {
            get => _vitesse;
            set
            {
                _vitesse = value;
                _dx = (float)Math.Cos(_cap) * _vitesse;
                _dy = (float)Math.Sin(_cap) * _vitesse;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Deplace(Monde monde, float intervalleDepuisDerniereFrame)
        {
            // Centre de perception
            _dpx = _x;//+ _dx * monde.DISTANCE_PERCEPTION / _vitesse;
            _dpy = _y; //+_dy * monde.DISTANCE_PERCEPTION / _vitesse;

            switch (_mode)
            {
                case MODE.RECHERCHE_NOURRITURE:
                    {
                        if (trouveNourriture(monde, _dpx, _dpy))
                        {
                            // Passer en mode "Retour vers le nid"
                            _mode = MODE.RETOUR_NID;
                            _cap += DisplayedObject.PI; // Demi tour
                            _pheromonesRestantes = PHEROMONES_MAX;
                        }
                        else
                        // Sinon: si nourriture a proximite: se tourner vers la nourriture
                        if (nourritureAProximite(monde, _dpx, _dpy))
                        {
                            tourneVers(monde.X_NOURRITURE, monde.Y_NOURRITURE);
                        }
                        else
                        {
                            // Chercher une pheromone de trace de retour de nourriture    
                            float xMarqueur, yMarqueur;

                            // Chercher une piste vers la nourriture
                            if (monde._grilleRapporteNourriture.chercheMarqueurMax(_x + _dx, _y + _dy, monde.DISTANCE_PERCEPTION, out xMarqueur, out yMarqueur))
                            {
                                tourneVers(xMarqueur, yMarqueur);
                                monde._grilleRapporteNourriture.Renforce(xMarqueur, yMarqueur, monde.VALEUR_RENFORCE);
                            }
                            else if (nidTrouvé(monde, _dpx, _dpy))
                            {
                                // Refait le plein de phéromones
                                _pheromonesRestantes = PHEROMONES_MAX;
                            }
                        }

                        if (_pheromonesRestantes > 0)
                        {
                            monde._pheromonesRechercheNourriture.PoseMarqueur(_x, _y, _pheromonesRestantes);
                            _pheromonesRestantes -= 0.001f;
                        }
                        _noImage = 0;   // Premiere image dans la texture
                    }
                    break;

                case MODE.RETOUR_NID:
                    {
                        if (nidTrouvé(monde, _dpx, _dpy))
                        {
                            // Passer en mode "Recherche nourriture"
                            _mode = MODE.RECHERCHE_NOURRITURE;
                            _cap += DisplayedObject.PI; // Demi tour
                            _pheromonesRestantes = PHEROMONES_MAX;
                        }
                        else
                        // Sinon: si nourriture a proximite: se tourner vers la nourriture
                        if (nidAProximite(monde, _dpx, _dpy))
                        {
                            tourneVers(monde.X_NID, monde.Y_NID);
                        }
                        else
                        {
                            float xMarqueur, yMarqueur;
                            // Retrouver la pheromone de retour vers le nid la plus forte
                            if (monde._pheromonesRechercheNourriture.chercheMarqueurMax(_x + _dx, _y + _dy, monde.DISTANCE_PERCEPTION, out xMarqueur, out yMarqueur))
                            {
                                tourneVers(xMarqueur, yMarqueur);
                                monde._pheromonesRechercheNourriture.Renforce(xMarqueur, yMarqueur, monde.VALEUR_RENFORCE);
                            }
                        }

                        if (_pheromonesRestantes > 0)
                        {
                            monde._grilleRapporteNourriture.PoseMarqueur(_x, _y, _pheromonesRestantes);
                            _pheromonesRestantes -= 0.001f;
                        }

                        _noImage = 1; // Deuxieme image dans la texture
                    }
                    break;
            }

            // Variations aleatoires du cap
            _cap += DisplayedObject.FloatRandom(-monde.VARIATION_CAP, monde.VARIATION_CAP) * intervalleDepuisDerniereFrame;

            // Collision avec les bords
            if (_x >= 1.0f)
            {
                _cap = DisplayedObject.PI_SUR_DEUX + _cap;
                _x = 1.0f;
            }
            else if (_x <= 0.0f)
            {
                _cap = DisplayedObject.PI_SUR_DEUX + _cap;
                _x = 0.0f;
            }

            if (_y >= 1.0f)
            {
                _cap = -_cap;
                _y = 1.0f;
            }
            else if (_y <= 0.0f)
            {
                _cap = -_cap;
                _y = 0.0f;
            }

            EviterObstacles(monde, intervalleDepuisDerniereFrame);

            // Normaliser le cap entre 0 et DEUX_PI
            while (_cap < 0)
                _cap += DisplayedObject.DEUX_PI;

            while (_cap > DisplayedObject.DEUX_PI)
                _cap -= DisplayedObject.DEUX_PI;

            _dx = (float)Math.Cos(_cap) * _vitesse;
            _dy = (float)Math.Sin(_cap) * _vitesse;

            // Avancer
            _x += intervalleDepuisDerniereFrame * dX;
            _y += intervalleDepuisDerniereFrame * dY;
        }

        /// <summary>
        /// Parcourir la liste des obstacles pour les eviter
        /// </summary>
        /// <param name="monde"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void EviterObstacles(Monde monde, float intervalle)
        {
            float dx = (float)Math.Cos(_cap) * _vitesse;
            float dy = (float)Math.Sin(_cap) * _vitesse;
            float cumulVirage = 0;
            foreach (Obstacle o in monde._listeObstacles)
                if (o.ModifieCap(_x, _y, dx, dy, _cap, ref cumulVirage, intervalle * _vitesse, intervalle * monde.VARIATION_CAP_OBSTACLE))
                    // L'obstacle a demandé un changement de cap urgent
                    break;

            _cap += cumulVirage;
        }

        static private bool nourritureAProximite(Monde monde, float x, float y)
        {
            float dx = (x - monde.X_NOURRITURE);
            float dy = (y - monde.Y_NOURRITURE);
            return monde.DISTANCE_PERCEPTION * 1.5f > Math.Sqrt(dx * dx + dy * dy);
        }

        static private bool nidAProximite(Monde monde, float x, float y)
        {
            float dx = (x - monde.X_NID);
            float dy = (y - monde.Y_NID);
            return monde.DISTANCE_PERCEPTION * 1.5f > Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Oriente la fourmi vers le point fourni
        /// </summary>
        /// <param name="xMarqueur"></param>
        /// <param name="yMarqueur"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void tourneVers(float xMarqueur, float yMarqueur)
        {
            float dx = xMarqueur - _x;
            float dy = yMarqueur - _y;
            float distanceCarre = (dx * dx) + (dy * dy);
            if (distanceCarre > 0.00001f) // Eviter les divisions par zero
                Cap = (float)Math.Atan2(dy, dx);
        }



        /// <summary>
        /// Determiner si la fourmi est suffisament proche de la source de nourriture
        /// </summary>
        /// <param name="monde"></param>
        /// <returns></returns>
        static private bool trouveNourriture(Monde monde, float x, float y)
        {
            float dx = x - monde.X_NOURRITURE;
            float dy = y - monde.Y_NOURRITURE;

            return Math.Sqrt((dx * dx) + (dy * dy)) - Math.Sqrt(monde.TAILLE_NOURRITURE * monde.TAILLE_NOURRITURE) < monde.TAILLE_NOURRITURE;
        }

        /// <summary>
        /// Determiner si la fourmi est suffisament proche du nid
        /// </summary>
        /// <param name="monde"></param>
        /// <returns></returns>
        static private bool nidTrouvé(Monde monde, float x, float y)
        {
            float dx = x - monde.X_NID;
            float dy = y - monde.Y_NID;

            return Math.Sqrt((dx * dx) + (dy * dy)) - Math.Sqrt(monde.TAILLE_NID * monde.TAILLE_NID) < monde.TAILLE_NID;
        }

        /// <summary>
        /// Affiche la fourmi
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="taille"></param>
        /// <param name="afficherPerception"></param>
        /// <param name="couleur"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Affiche(OpenGL gl, float taille, bool afficherPerception, CouleurGlobale couleur)
        {
            if (afficherPerception)
            {
                // Cercle de perception
                gl.Disable(OpenGL.GL_TEXTURE_2D);
                gl.Color(1.0f, 1.0f, 1.0f, 0.2f);
                Fourmis.DessineCercle(gl, _dpx, _dpy, _distancePerception, 16);
                gl.Enable(OpenGL.GL_TEXTURE_2D);
            }

            Color couleurFourmi;
            switch (_mode)
            {
                case MODE.RETOUR_NID: couleurFourmi = couleur.getColorWithHueChange(0.25f); break;
                default: couleurFourmi = couleur.getColorWithHueChange(-0.25f); break;
            }
            gl.Color(couleurFourmi.R, couleurFourmi.G, couleurFourmi.B);

            gl.PushMatrix();
            gl.Translate(_x, _y, 0);
            gl.Rotate(0, 0, _cap * RADIAN_TO_DEG);
            gl.Begin(OpenGL.GL_QUADS);

            gl.TexCoord(0.0f + (_noImage * 0.5f), 1.0f); gl.Vertex(-taille, -taille);
            gl.TexCoord(0.5f + (_noImage * 0.5f), 1.0f); gl.Vertex(-taille, +taille);
            gl.TexCoord(0.5f + (_noImage * 0.5f), 0.0f); gl.Vertex(+taille, +taille);
            gl.TexCoord(0.0f + (_noImage * 0.5f), 0.0f); gl.Vertex(+taille, -taille);

            gl.End();

            gl.PopMatrix();
        }
    }
}
