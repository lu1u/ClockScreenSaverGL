using ClockScreenSaverGL.Config;
///
/// Particules qui rebondissent les unes contre les autres
///
///
using ClockScreenSaverGL.DisplayedObjects.Fonds.SystemeParticules2D.Modificateurs;
using SharpGL;
using System;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.Particules
{
    internal class RebondParticules : SystemeParticules2D.SystemeParticules2D, IDisposable
    {
        #region Parametres
        private const string CAT = "RebondParticules";
        protected CategorieConfiguration c;
        private float GRAVITE_X;
        private float GRAVITE_Y;
        private float TAILLE_PARTICULE;
        private float VITESSE_PARTICULE;
        #endregion

        public RebondParticules(OpenGL gl) : base(gl)
        {
            GetConfiguration();
            // Ajouter les particules (pas d'emetteur: le nb de particules reste fixe)
            for (int i = 0; i < NB_MAX_PARTICULES; i++)
                AjouteParticule();

            couleurParticules = COULEUR_PARTICULES.BLANC;
            AjouteTexture(c.GetParametre("Balle", Config.Configuration.GetImagePath("balle.png")), 1);

            AjouteModificateur(new ModificateurGravite(GRAVITE_X, GRAVITE_Y));
            AjouteModificateur(new ModificateurRebond(MIN_X, MAX_X, MIN_Y, MAX_Y));
            AjouteModificateur(new ModificateurCollisions());
            AjouteModificateur(new ModificateurVitesseLineaire());
        }

        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                NB_MAX_PARTICULES = c.GetParametre("Nb Particules", 50);
                GRAVITE_X = c.GetParametre("Gravite X", 0.0f);
                GRAVITE_Y = c.GetParametre("Gravite Y", -0.5f);
                TAILLE_PARTICULE = c.GetParametre("TailleParticule", 0.05f);
                VITESSE_PARTICULE = c.GetParametre("VitesseParticule", 0.2f);
            }
            return c;
        }
        /// <summary>
        /// Ajoute une particule
        /// </summary>
        private void AjouteParticule()
        {
            int indice = FindUnusedParticle();
            _particules[indice].x = FloatRandom(MIN_X, MAX_X);
            _particules[indice].y = FloatRandom(MIN_Y, MAX_Y);
            _particules[indice].alpha = 1;

            _particules[indice].vx = FloatRandom(0.01f, VITESSE_PARTICULE) * SigneRandom();
            _particules[indice].vy = FloatRandom(0.01f, VITESSE_PARTICULE) * SigneRandom();
            _particules[indice].taille = FloatRandom(0.75f, 1.25f) * TAILLE_PARTICULE;
            _particules[indice].textureIndex = 0;
            _particules[indice].active = true;
        }
    }
}
