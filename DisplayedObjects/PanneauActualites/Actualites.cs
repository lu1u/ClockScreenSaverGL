﻿using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;
using SharpGL;
using System;
using System.Drawing;
using System.Windows.Forms;

/// <summary>
/// Affichage des actualites, extraites de flux RSS
/// </summary>
namespace ClockScreenSaverGL.DisplayedObjects.PanneauActualites
{
    internal class Actualites : DisplayedObject
    {
        #region Parametres
        public const string CAT = "Actualites";
        protected static CategorieConfiguration c;
        public int NB_JOURS_MAX_INFO;
        public int HAUTEUR_BANDEAU;
        public float VITESSE;
        public int MIN_LIGNES;
        public int MAX_LIGNES;
        public int MAX_LIGNES_PAR_SOURCE;
        public int TAILLE_SOURCE;
        public int TAILLE_TITRE;
        public int TAILLE_DESCRIPTION;
        public bool AFFICHE_DESCRIPTION;
        public bool AFFICHE_IMAGES;
        public float SATURATION_IMAGES;
        public float LIGHT_FACTOR;

        #endregion

        private float _decalageX = SystemInformation.VirtualScreen.Width;

        private ActuFactory _actuFactory;

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="gl"></param>
        public Actualites(OpenGL gl) : base(gl)
        {
            GetConfiguration();
            LigneActu.TAILLE_TITRE = TAILLE_TITRE;
            LigneActu.TAILLE_DESCRIPTION = TAILLE_DESCRIPTION;
            LigneActu.TAILLE_SOURCE = TAILLE_SOURCE;
            LigneActu.HAUTEUR_BANDEAU = HAUTEUR_BANDEAU;
            LigneActu.SATURATION_IMAGES = SATURATION_IMAGES;

        }

        protected override void Init(OpenGL gl)
        {
            base.Init(gl);
            _actuFactory = new ActuFactory(gl);
            _actuFactory.Init();
        }



        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                NB_JOURS_MAX_INFO = c.GetParametre("Nb jours info max", 4);
                HAUTEUR_BANDEAU = c.GetParametre("Hauteur bandeau", 150);
                VITESSE = c.GetParametre("Vitesse", 75.0f);
                MIN_LIGNES = c.GetParametre("Nb lignes min", 50);
                MAX_LIGNES = c.GetParametre("Nb lignes max", 100);
                MAX_LIGNES_PAR_SOURCE = c.GetParametre("Nb lignes max par source", 10);
                TAILLE_SOURCE = c.GetParametre("Taille fonte source", 16);
                TAILLE_TITRE = c.GetParametre("Taille fonte titre", 30);
                TAILLE_DESCRIPTION = c.GetParametre("Taille fonte description", 14);
                AFFICHE_DESCRIPTION = c.GetParametre("Affiche Description", true);
                AFFICHE_IMAGES = c.GetParametre("Affiche Images", true);
                SATURATION_IMAGES = c.GetParametre("Saturation images", 0.5f);
                LIGHT_FACTOR = c.GetParametre("Luminosité", 5.0f);
            }
            return c;
        }
        /// <summary>
        /// Affichage deroulant des actualites
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="maintenant"></param>
        /// <param name="tailleEcran"></param>
        /// <param name="couleur"></param>
        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            couleur = CouleurGlobale.Light(couleur, LIGHT_FACTOR);
            using (var v = new Viewport2D(gl, 0, 0, tailleEcran.Width, tailleEcran.Bottom))
            {
                gl.Disable(OpenGL.GL_LIGHTING);
                gl.Disable(OpenGL.GL_DEPTH);
                gl.Enable(OpenGL.GL_BLEND);
                gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);

                gl.Disable(OpenGL.GL_TEXTURE_2D);
                gl.Color(0.0f, 0.0f, 0.0f, 0.5f); // Fond sombre
                gl.Rect(tailleEcran.Left, tailleEcran.Top + HAUTEUR_BANDEAU, tailleEcran.Right, tailleEcran.Top);

                float x = tailleEcran.Left + _decalageX;

                gl.Enable(OpenGL.GL_TEXTURE_2D);
                gl.Color(couleur.R / 256.0f, couleur.G / 256.0f, couleur.B / 256.0f, 1.0f);

                if (_actuFactory?.Pret == true)
                    try
                    {
                        foreach (LigneActu l in _actuFactory._lignes)
                        {
                            l.Affiche(gl, x, tailleEcran.Top + HAUTEUR_BANDEAU);
                            x += l.Largeur;
                            if (x > tailleEcran.Right)
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore
                    }
            }
#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
            _decalageX -= VITESSE * maintenant.intervalleDepuisDerniereFrame;

            if (_actuFactory._lignes != null)
            {
                if (_actuFactory._lignes.Count > 1)
                    if (_decalageX + _actuFactory._lignes[0].Largeur < 0)
                    {
                        // Deplacer la ligne vers la fin du tableau
                        _decalageX += _actuFactory._lignes[0].Largeur;
                        LigneActu premiereLigne = _actuFactory._lignes[0];
                        premiereLigne.Clear();

                        _actuFactory._lignes.RemoveAt(0);
                        _actuFactory._lignes.Add(premiereLigne);
                    }
            }
        }

        public override bool KeyDown(Form f, Keys k)
        {
            if (Keys.J.Equals(k))
            {
                if (_actuFactory._lignes?.Count >= 1)
                    lock (_actuFactory._lignes)
                        _actuFactory._lignes.RemoveAt(0);
                return true;
            }
            if (Keys.E.Equals(k))
            {
                lock (_actuFactory._lignes)
                    _actuFactory._lignes?.Clear();
                AFFICHE_DESCRIPTION = !AFFICHE_DESCRIPTION;
                c.SetParametre("Affiche Description", AFFICHE_DESCRIPTION);
                c.Flush();
                return true;
            }
            if (Keys.I.Equals(k))
            {
                lock (_actuFactory._lignes)
                    _actuFactory._lignes?.Clear();
                AFFICHE_IMAGES = !AFFICHE_IMAGES;
                c.SetParametre("Affiche Images", AFFICHE_DESCRIPTION);
                c.Flush();
                return true;
            }

            return base.KeyDown(f, k);
        }
    }
}
