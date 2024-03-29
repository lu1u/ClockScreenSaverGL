﻿using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;
using ClockScreenSaverGL.DisplayedObjects.OpenGLUtils;
using SharpGL;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    internal class SpaceInvaders : Fond
    {
        #region Configuration
        private const string CAT = "SpaceInvaders";
        private CategorieConfiguration c;
        private int DELAI_TIMER_JEUX, DELAI_TIMER_BASE, DELAI_TIMER_MISSILES_BASE, DELAI_EXPLOSION, DELAI_TIMER_MISSILES;
        private float DECALAGE_ALIENS, DECALAGE_HAUT_ALIENS;
        private int NB_MAX_MISSILES;
        #endregion

        private const float MIN_VIEWPORT_X = 0;
        private const float MIN_VIEWPORT_Y = 0;
        private const float MAX_VIEWPORT_X = 1.0f;
        private const float MAX_VIEWPORT_Y = 1.0f;
        private const float LARGEUR_VIEWPORT = MAX_VIEWPORT_X - MIN_VIEWPORT_X;
        private const float HAUTEUR_VIEWPORT = MAX_VIEWPORT_Y - MIN_VIEWPORT_Y;
        private const int NB_LIGNES_ALIENS = 5;
        private const int NB_COLONNES_ALIENS = 11;
        private const float TAILLE_LIGNE = 0.4f * (HAUTEUR_VIEWPORT / NB_LIGNES_ALIENS);
        private const float TAILLE_COLONNE = 0.6f * (LARGEUR_VIEWPORT / NB_COLONNES_ALIENS);

        // Images dans la texture
        private const int IMAGE_ALIEN_1 = 0;
        private const int IMAGE_ALIEN_2 = 1;
        private const int IMAGE_ALIEN_3 = 2;
        private const int IMAGE_ALIEN_4 = 3;
        private const int IMAGE_BASE = 4;
        private const int IMAGE_MISSILE = 5;
        private const int IMAGE_EXPLOSION = 6;
        private const int NB_IMAGES_LARGEUR = 7;
        private Sprite[,] _aliens;
        private Sprite _base;
        private readonly List<Missile> _missilesBase = new List<Missile>();
        private readonly List<Explosion> _explosions = new List<Explosion>();
        private float dxBase = 0;
        private int dernierAlienADroite;
        private int dernierAlienAGauche;
        private int dernierAlienEnBas;
        private int dernierAlienEnHaut;
        private TimerIsole _timerJeu, _timerBase, _timerMissilesBase, _timerMissile;
        private readonly TextureAsynchrone _textureAliens;
        private int _phase;
        private enum SensDeplacement { DROITE, GAUCHE, BAS_PUIS_GAUCHE, BAS_PUIS_DROITE }

        private SensDeplacement _sensDeplacementAliens;
        private int _nbAliens;

        public SpaceInvaders(OpenGL gl) : base(gl)
        {
            c = GetConfiguration();
            _textureAliens = new TextureAsynchrone(gl, Configuration.GetImagePath("SpaceInvaders.png"));
            _textureAliens.Init();
            _timerJeu = new TimerIsole(DELAI_TIMER_JEUX);
            _timerBase = new TimerIsole(DELAI_TIMER_BASE);
            _timerMissilesBase = new TimerIsole(DELAI_TIMER_MISSILES_BASE);
            _timerMissile = new TimerIsole(DELAI_TIMER_MISSILES);
            InitJeu();
        }

        private void InitJeu()
        {
            _aliens = new Sprite[NB_COLONNES_ALIENS, NB_LIGNES_ALIENS];

            // Initialisation des aliens
            InitAliens(_aliens, IMAGE_ALIEN_1, 0, 0.0f);
            InitAliens(_aliens, IMAGE_ALIEN_2, 1, -0.2f);
            InitAliens(_aliens, IMAGE_ALIEN_2, 2, -0.2f);
            InitAliens(_aliens, IMAGE_ALIEN_3, 3, -0.2f);
            InitAliens(_aliens, IMAGE_ALIEN_3, 4, -0.1f);

            _nbAliens = NB_LIGNES_ALIENS * NB_COLONNES_ALIENS;

            dernierAlienADroite = NB_COLONNES_ALIENS - 1;
            dernierAlienAGauche = 0;
            dernierAlienEnBas = NB_LIGNES_ALIENS - 1;
            dernierAlienEnHaut = 0;

            _sensDeplacementAliens = SensDeplacement.DROITE;

            _base = new Sprite(MIN_VIEWPORT_X + LARGEUR_VIEWPORT / 2.0f, MAX_VIEWPORT_X - TAILLE_LIGNE, IMAGE_BASE, 0.2f);
            dxBase = FloatRandom(0.05f, 0.1f) * SigneRandom();

            _explosions.Clear();
            _missilesBase.Clear();
        }

        /// <summary>
        /// Initialise une ligne d'aliens
        /// </summary>
        /// <param name="aliens">Tableau d'aliens</param>
        /// <param name="image">Numero de l'image</param>
        /// <param name="ligne"></param>
        /// <param name="luminance">Ecart de luminance</param>
        private void InitAliens(Sprite[,] aliens, int image, int ligne, float luminance)
        {
            float y = ligne * TAILLE_LIGNE + DECALAGE_HAUT_ALIENS;
            for (int colonne = 0; colonne < NB_COLONNES_ALIENS; colonne++)
            {
                float x = colonne * TAILLE_COLONNE;
                aliens[colonne, ligne] = new Sprite(x, y, image, luminance);
            }
        }

        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                DELAI_TIMER_JEUX = c.GetParametre("Timer jeux", 400, a => { DELAI_TIMER_JEUX = Convert.ToInt32(a); _timerJeu = new TimerIsole(DELAI_TIMER_JEUX); });
                DELAI_TIMER_BASE = c.GetParametre("Timer base", 150, a => { DELAI_TIMER_BASE = Convert.ToInt32(a); _timerBase = new TimerIsole(DELAI_TIMER_BASE); });
                DELAI_TIMER_MISSILES_BASE = c.GetParametre("Timer deplacement missiles", 50, a => { DELAI_TIMER_MISSILES_BASE = Convert.ToInt32(a); _timerMissilesBase = new TimerIsole(DELAI_TIMER_MISSILES_BASE); });
                DELAI_TIMER_MISSILES = c.GetParametre("Timer lance missiles", 800, a => { DELAI_TIMER_MISSILES = Convert.ToInt32(a); _timerMissile = new TimerIsole(DELAI_TIMER_MISSILES); });
                DELAI_EXPLOSION = c.GetParametre("Delai explosions", 600, a => DELAI_EXPLOSION = Convert.ToInt32(a));
                NB_MAX_MISSILES = c.GetParametre("Nb max missiles", 2, a => NB_MAX_MISSILES = Convert.ToInt32(a));
                DECALAGE_ALIENS = c.GetParametre("Decalage aliens", 0.01f, a => DECALAGE_ALIENS = (float)Convert.ToDouble(a));
                DECALAGE_HAUT_ALIENS = c.GetParametre("Decalage haut aliens", 0.1f);
            }

            return c;
        }

        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            float[] col = { couleur.R / 256.0f, couleur.G / 256.0f, couleur.B / 256.0f, 1f };
            gl.Color(col);

            gl.LoadIdentity();
            using (new Viewport2D(gl, MIN_VIEWPORT_X, MAX_VIEWPORT_Y, MAX_VIEWPORT_X, MIN_VIEWPORT_Y))
            {
                if (_textureAliens.Pret)
                {
                    gl.Disable(OpenGL.GL_LIGHTING);
                    gl.Disable(OpenGL.GL_DEPTH);
                    gl.Enable(OpenGL.GL_TEXTURE_2D);
                    gl.Enable(OpenGL.GL_BLEND);
                    gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);

                    _textureAliens.Texture.Bind(gl);
                    using (new GLBegin(gl, OpenGL.GL_QUADS))
                    {
                        for (int ligne = dernierAlienEnHaut; ligne <= dernierAlienEnBas; ligne++)
                            for (int colonne = dernierAlienAGauche; colonne <= dernierAlienADroite; colonne++)
                            {
                                _aliens[colonne, ligne]?.Affiche(gl, NB_IMAGES_LARGEUR, 2, _phase, TAILLE_COLONNE, TAILLE_LIGNE, couleur);
                            }


                        foreach (Explosion explosion in _explosions)
                            explosion.Affiche(gl, NB_IMAGES_LARGEUR, 2, _phase, TAILLE_COLONNE, TAILLE_LIGNE, couleur);

                        foreach (Missile missile in _missilesBase)
                            missile.Affiche(gl, NB_IMAGES_LARGEUR, 2, _phase, TAILLE_COLONNE, TAILLE_LIGNE, couleur);
                        _base.Affiche(gl, NB_IMAGES_LARGEUR, 2, 0, TAILLE_COLONNE, TAILLE_LIGNE, couleur);
                    }
                }
            }

            LookArcade(gl, couleur);
#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
            if (_timerJeu.Ecoule())
            {
                AnimerAliens();
                AnimerExplosions();
            }
            if (_timerMissilesBase.Ecoule())
                AnimerMissilesBase();

            if (_timerBase.Ecoule())
                AnimerBase();

            if (_timerMissile.Ecoule())
                LancerMissile();
        }

        private void AnimerExplosions()
        {
            int i = 0;
            while (i < _explosions.Count)
                if (_explosions[i].Ecoule())
                    _explosions.RemoveAt(i);
                else
                    i++;
        }

        private void AnimerMissilesBase()
        {
            // Deplacer les particules
            int i = 0;
            while (i < _missilesBase.Count)
            {
                Missile p = _missilesBase[i];
                int ligneAlien, colonneAlien;
                if (collisionMissileAlien(p, out colonneAlien, out ligneAlien))
                {
                    _missilesBase.RemoveAt(i);
                    _explosions.Add(new Explosion(_aliens[colonneAlien, ligneAlien].X, _aliens[colonneAlien, ligneAlien].Y, IMAGE_EXPLOSION, DELAI_EXPLOSION));

                    // Alien touché! Supprimer du tableau
                    SupprimeAlien(colonneAlien, ligneAlien);
                }
                else
                if (p.Y > MIN_VIEWPORT_Y)
                {
                    p.Y -= DECALAGE_ALIENS;
                    i++;
                }
                else
                    _missilesBase.RemoveAt(i);
            }
        }

        private void SupprimeAlien(int colonne, int ligne)
        {

            // Chercher la premiere colonne non vide a gauche
            dernierAlienAGauche = 0;
            while (dernierAlienAGauche < NB_COLONNES_ALIENS - 1 && ColonneVide(dernierAlienAGauche))
                dernierAlienAGauche++;

            // Chercher la premiere colonne non vide a droite
            dernierAlienADroite = NB_COLONNES_ALIENS - 1;
            while (dernierAlienADroite > 0 && ColonneVide(dernierAlienADroite))
                dernierAlienADroite--;

            // Chercher la premier ligne non vide en bas
            dernierAlienEnBas = NB_LIGNES_ALIENS - 1;
            while (dernierAlienEnBas >= 0 && LigneVide(dernierAlienEnBas))
                dernierAlienEnBas--;

            // Chercher la premier ligne non vide en haut
            dernierAlienEnHaut = 0;
            while (dernierAlienEnBas < NB_LIGNES_ALIENS && LigneVide(dernierAlienEnHaut))
                dernierAlienEnHaut++;

            _aliens[colonne, ligne] = null;
            _nbAliens--;
            if (_nbAliens <= 0)
                Gagne();
        }

        /// <summary>
        /// Verifie si la ligne donnee est vide
        /// </summary>
        /// <param name="ligne"></param>
        /// <returns></returns>
        private bool LigneVide(int ligne)
        {
            for (int colonne = 0; colonne < NB_COLONNES_ALIENS; colonne++)
                if (_aliens[colonne, ligne] != null)
                    return false;

            return true;
        }

        private bool ColonneVide(int colonne)
        {
            for (int l = 0; l < NB_LIGNES_ALIENS; l++)
                if (_aliens[colonne, l] != null)
                    return false;

            return true;
        }

        /// <summary>
        /// Determine si un missile rencontre un alien
        /// </summary>
        /// <param name="missile"></param>
        /// <param name="colonneAlien"></param>
        /// <param name="ligneAlien"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private bool collisionMissileAlien(Sprite missile, out int colonneAlien, out int ligneAlien)
        {
            RectangleF rMissile = new RectangleF(missile.X, missile.Y, TAILLE_COLONNE, TAILLE_LIGNE);
            for (int ligne = 0; ligne < NB_LIGNES_ALIENS; ligne++)
                for (int colonne = 0; colonne < NB_COLONNES_ALIENS; colonne++)
                    if (_aliens[colonne, ligne] != null)
                    {
                        RectangleF rAlien = new RectangleF(_aliens[colonne, ligne].X, _aliens[colonne, ligne].Y, TAILLE_COLONNE, TAILLE_LIGNE);

                        if (rMissile.IntersectsWith(rAlien))
                        {
                            colonneAlien = colonne;
                            ligneAlien = ligne;
                            return true;
                        }
                    }

            colonneAlien = 0;
            ligneAlien = 0;
            return false;
        }

        private void AnimerAliens()
        {
            // Animer les differentes images des aliens
            _phase++;
            if (_phase > 1)
                _phase = 0;

            // Deplacer les aliens
            float dx = 0;
            float dy = 0;
            float xDroite = GetX(dernierAlienADroite);
            float xGauche = GetX(dernierAlienAGauche);

            switch (_sensDeplacementAliens)
            {
                case SensDeplacement.DROITE:
                    if (xDroite + TAILLE_COLONNE + DECALAGE_ALIENS < MAX_VIEWPORT_X)
                    {
                        dx = DECALAGE_ALIENS;
                        dy = 0;
                    }
                    else
                    {
                        dx = 0;
                        dy = DECALAGE_ALIENS;
                        _sensDeplacementAliens = SensDeplacement.BAS_PUIS_GAUCHE;
                    }
                    break;

                case SensDeplacement.GAUCHE:
                    if (xGauche + DECALAGE_ALIENS > MIN_VIEWPORT_X)
                    {
                        dx = -DECALAGE_ALIENS;
                        dy = 0;
                    }
                    else
                    {
                        dx = 0;
                        dy = DECALAGE_ALIENS;
                        _sensDeplacementAliens = SensDeplacement.BAS_PUIS_DROITE;
                    }
                    break;
                case SensDeplacement.BAS_PUIS_GAUCHE:
                case SensDeplacement.BAS_PUIS_DROITE:
                    if ((dernierAlienEnBas * TAILLE_LIGNE) + DECALAGE_ALIENS < MAX_VIEWPORT_Y)
                    {
                        if (_sensDeplacementAliens == SensDeplacement.BAS_PUIS_GAUCHE)
                        {
                            dx = -DECALAGE_ALIENS;
                            dy = 0;
                            _sensDeplacementAliens = SensDeplacement.GAUCHE;
                        }
                        else
                        {
                            dx = DECALAGE_ALIENS;
                            dy = 0;
                            _sensDeplacementAliens = SensDeplacement.DROITE;
                        }
                    }
                    else
                    {
                        // Perdu!
                        Perdu();
                    }

                    break;
            }


            // Maintenant qu'on a le sens de deplacement: decaler tous les aliens
            for (int ligne = 0; ligne < NB_LIGNES_ALIENS; ligne++)
                for (int colonne = 0; colonne < NB_COLONNES_ALIENS; colonne++)
                    if (_aliens[colonne, ligne] != null)
                    {
                        _aliens[colonne, ligne].X += dx;
                        _aliens[colonne, ligne].Y += dy;
                    }

            foreach (Explosion e in _explosions)
            {
                e.X += dx;
                e.Y += dy;
            }
        }

        /// <summary>
        /// Retourne la coordonnee X d'un alien dans la collone donnee
        /// </summary>
        /// <param name="colonne"></param>
        /// <returns></returns>
        private float GetX(int colonne)
        {
            for (int l = 0; l < NB_LIGNES_ALIENS; l++)
                if (_aliens[colonne, l] != null)
                    return _aliens[colonne, l].X;

            return 0;
        }

        private void AnimerBase()
        {
            dxBase += FloatRandom(0, 0.025f) * SigneRandom();
            if (Math.Abs(dxBase) > TAILLE_COLONNE)
                dxBase = TAILLE_COLONNE * Signe(dxBase);

            if (_base.X + dxBase < MIN_VIEWPORT_X)
                dxBase = SignePlus(dxBase);
            else
                if (_base.X + dxBase + TAILLE_COLONNE > MAX_VIEWPORT_X)
                dxBase = SigneMoins(dxBase);

            _base.X += dxBase;
        }

        private void LancerMissile()
        {
            if (_missilesBase.Count < NB_MAX_MISSILES)
                // Ajouter un missile
                _missilesBase.Add(new Missile(_base.X, _base.Y, IMAGE_MISSILE));
        }

        private void Perdu()
        {
            InitJeu();
        }

        private void Gagne()
        {
            InitJeu();
        }

        public override void FillConsole(OpenGL gl)
        {
            base.FillConsole(gl);
            Console console = Console.GetInstance(gl);
            console.AddLigne(Color.IndianRed, $"Dernier a droite: {dernierAlienADroite}");
            console.AddLigne(Color.IndianRed, $"Dernier a gauche: {dernierAlienAGauche}");
            console.AddLigne(Color.IndianRed, $"Dernier en bas: {dernierAlienEnBas}");
            console.AddLigne(Color.IndianRed, $"Dernier en haut: {dernierAlienEnHaut}");
            console.AddLigne(Color.IndianRed, $"Nb Aliens: {_nbAliens}");
        }
    }
}
