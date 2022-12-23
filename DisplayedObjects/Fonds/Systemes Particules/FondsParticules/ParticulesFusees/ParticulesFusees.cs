using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.SystemeParticules2D;
using ClockScreenSaverGL.DisplayedObjects.Fonds.SystemeParticules2D.Modificateurs;
using SharpGL;
using System;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.Particules
{
    internal class ParticulesFusees : SystemeParticules2D.SystemeParticules2D, IDisposable
    {
        private const string CAT = "ParticulesFusees";
        protected CategorieConfiguration c;
        private int NB_EMETTEURS;
        private float GRAVITE_X;
        private float GRAVITE_Y;
        private float ALPHA_MODIFIEUR;
        private float TAILLE_MODIFIEUR;
        private float TAILLE_PARTICULE;
        private float VITESSE_ANGLE;
        private float VITESSE_PARTICULE;
        private float VITESSE_FUSEE;

        public ParticulesFusees(OpenGL gl) : base(gl)
        {
            GetConfiguration();
            for (int i = 0; i < NB_EMETTEURS; i++)
                AjouteEmetteur(new EmetteurFusee(TAILLE_PARTICULE, VITESSE_ANGLE, VITESSE_PARTICULE, VITESSE_FUSEE));

            AttributBlend = PARTICULES_BLEND_ADDITIVE;
            typeFond = TYPE_FOND.FOND_COULEUR;
            couleurParticules = COULEUR_PARTICULES.BLANC;
            AjouteTexture(c.GetParametre("Nuages petits", Config.Configuration.GetImagePath("nuages_petits.png")), 3);

            AjouteModificateur(new ModificateurExclusion(MIN_X, MIN_Y, MAX_X, MAX_Y,
                ModificateurExclusion.Exclusions.EXCLURE_AU_DESSUS | ModificateurExclusion.Exclusions.EXCLURE_A_DROITE | ModificateurExclusion.Exclusions.EXCLURE_A_GAUCHE));
            AjouteModificateur(new ModificateurLife());
            AjouteModificateur(new ModificateurVitesseLineaire());
            AjouteModificateur(new ModificateurGravite(GRAVITE_X, GRAVITE_Y));
            AjouteModificateur(new ModificateurAlpha(ALPHA_MODIFIEUR));
            AjouteModificateur(new ModificateurTaille(TAILLE_MODIFIEUR));
        }

        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                NB_EMETTEURS = c.GetParametre("Nb Emetteurs", 5);
                NB_MAX_PARTICULES = c.GetParametre("Nb Particules", 1000);
                GRAVITE_X = c.GetParametre("Gravite X", 0.02f);
                GRAVITE_Y = c.GetParametre("Gravite Y", 0.02f);
                ALPHA_MODIFIEUR = c.GetParametre("Modifieur Alpha", 0.6f);
                TAILLE_MODIFIEUR = c.GetParametre("Modifieur Taille", 0.05f);
                TAILLE_PARTICULE = c.GetParametre("TailleParticule", 0.01f);
                VITESSE_ANGLE = c.GetParametre("VitesseAngle", 0.5f);
                VITESSE_PARTICULE = c.GetParametre("VitesseParticule", 0.2f);
                VITESSE_FUSEE = c.GetParametre("VitesseParticule", 0.5f);

            }
            return c;
        }
    }
}
