﻿/*
 * Crée par SharpDevelop.
 * Utilisateur: lucien
 * Date: 27/12/2014
 * Heure: 15:20
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using ClockScreenSaverGL.Config;
using SharpGL;
using System;
using System.Drawing;
namespace ClockScreenSaverGL.DisplayedObjects.Metaballes
{
    /// <summary>
    /// Description of Bacteries.
    /// </summary>
    public class Bacteries : Metaballes
    {
        private const String CAT = "Bacteries";
        private CategorieConfiguration c;
        private float TailleMax, TailleMin, IntensiteMax, IntensiteMin;
        private int UnSur = 0;

        public Bacteries(OpenGL gl, int cx, int cy) : base(gl)
        {
            GetConfiguration();
        }


        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
            }
            return c;
        }
        /// <summary>
        /// Lit les preferences a chaque version de metaballes
        /// </summary>
        /// <param name="L"></param>
        /// <param name="H"></param>
        /// <param name="N"></param>
        /// <param name="C"></param>
        protected override void GetPreferences(ref int L, ref int H, ref int N, ref int C)
        {

            base.GetPreferences(ref L, ref H, ref N, ref C);
            L = c.GetParametre("Largeur", 400);
            H = c.GetParametre("Hauteur", 300);
            N = c.GetParametre("Nombre", 30);
            C = c.GetParametre("Niveaux", 512);
        }

        protected override void ConstruitMetaballes()
        {
            TailleMax = c.GetParametre("TailleMax", 30f);
            TailleMin = c.GetParametre("TailleMin", 20f);
            IntensiteMax = c.GetParametre("IntensiteMax", 1.0f);
            IntensiteMin = IntensiteMax / 2.0f;

            for (int i = 0; i < NbMetaballes; i++)
            {
                _metaballes[i] = new MetaBalle(FloatRandom(IntensiteMin, IntensiteMax),
                                      FloatRandom(TailleMin, TailleMax),
                                      FloatRandom(0, Largeur), FloatRandom(0, Hauteur),
                                      FloatRandom(-5, 5), FloatRandom(-5, 5));
            }

        }

        /// <summary>
        /// Changer l'image
        /// </summary>
        /// <param name="maintenant"></param>
        /// <param name="tailleEcran"></param>
        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
            base.Deplace(maintenant, tailleEcran);

            UnSur++;

            if (UnSur % 8 == 0)
            {
                // Changer la trajectoire d'une bacterie
                int Indice = random.Next(NbMetaballes);

                _metaballes[Indice]._Vx = FloatRandom(-5, 5);
                _metaballes[Indice]._Vy = FloatRandom(-5, 5);

                UnSur = 0;
            }
        }
    }
}

