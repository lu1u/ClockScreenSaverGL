﻿using ClockScreenSaverGL.Config;
using SharpGL;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD.Grilles
{
    internal class Grille : GrilleBase
    {
        #region Parametres
        public const string CAT = "Grille";
        private CategorieConfiguration c;
        private int NB_BARRES_X;
        private int NB_BARRES_Y;
        private int NB_BARRES_Z;

        private float EPAISSEUR_GRILLE;
        private float ECART_GRILLE;
        #endregion

        private const int NB_FIGURES = 1;
        private const uint FIGURE_X = 0;
        private const uint FIGURE_Y = 1;
        private const uint FIGURE_Z = 2;


        public Grille(OpenGL gl) : base(gl)
        {
            GetConfiguration();
            VITESSE_ROTATION = c.GetParametre("Vitesse Rotation", 0.5f);
            TRANSLATE_Z = ECART_GRILLE * NB_BARRES_X * -0.25f;
            LIGHTPOS[0] = ECART_GRILLE * NB_BARRES_X;
            LIGHTPOS[1] = ECART_GRILLE * NB_BARRES_Y;
            LIGHTPOS[2] = ECART_GRILLE * NB_BARRES_Z;

            fogEnd = ECART_GRILLE * NB_BARRES_X * 0.75f;
        }

        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                NB_BARRES_X = c.GetParametre("Nb Cubes X", 40);
                NB_BARRES_Y = c.GetParametre("Nb Cubes Y", 40);
                NB_BARRES_Z = c.GetParametre("Nb Cubes Z", 40);

                EPAISSEUR_GRILLE = c.GetParametre("Epaisseur grille", 0.025f);
                ECART_GRILLE = c.GetParametre("Ecart grille", 0.5f);
            }
            return c;
        }

        protected override void GenererListe(OpenGL gl)
        {
            float ORIGINE_X = -(NB_BARRES_X * 0.5f) * ECART_GRILLE;
            float ORIGINE_Y = -(NB_BARRES_Y * 0.5f) * ECART_GRILLE;
            float ORIGINE_Z = -(NB_BARRES_Z * 0.5f) * ECART_GRILLE;
            gl.Begin(OpenGL.GL_QUADS);

            // Barres de l'axe des X
            if (c.GetParametre("X", true))
                for (int y = 0; y < NB_BARRES_Y; y++)
                    for (int z = 0; z < NB_BARRES_Z; z++)
                        Brique(gl, 0, ORIGINE_Y + y * ECART_GRILLE, ORIGINE_Z + z * ECART_GRILLE, NB_BARRES_X * ECART_GRILLE, EPAISSEUR_GRILLE, EPAISSEUR_GRILLE);

            // Barres de l'axe des Y
            if (c.GetParametre("Y", true))
                for (int x = 0; x < NB_BARRES_X; x++)
                    for (int z = 0; z < NB_BARRES_Z; z++)
                        Brique(gl, ORIGINE_X + x * ECART_GRILLE, 0, ORIGINE_Z + z * ECART_GRILLE, EPAISSEUR_GRILLE, NB_BARRES_Y * ECART_GRILLE, EPAISSEUR_GRILLE);

            // Barres de l'axe des Z
            if (c.GetParametre("Z", true))
                for (int x = 0; x < NB_BARRES_X; x++)
                    for (int y = 0; y < NB_BARRES_Y; y++)
                        Brique(gl, ORIGINE_X + x * ECART_GRILLE, ORIGINE_Y + y * ECART_GRILLE, 0, EPAISSEUR_GRILLE, EPAISSEUR_GRILLE, NB_BARRES_Z * ECART_GRILLE);

            gl.End();
        }

    }
}
