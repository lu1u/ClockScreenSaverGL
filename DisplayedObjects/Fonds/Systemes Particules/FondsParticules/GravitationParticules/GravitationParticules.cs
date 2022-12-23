
using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.SystemeParticules2D.Modificateurs;

using SharpGL;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.Particules
{
    public class GravitationParticules : SystemeParticules2D.SystemeParticules2D
    {
        #region Parametres
        private const string CAT = "GravitationParticules";
        private CategorieConfiguration c;
        private int TIMER_CREATE;
        private float G = 0.01f;
        private float MULT_DIST;
        private float VITESSE_RECENTRE;
        #endregion

        public GravitationParticules(OpenGL gl) : base(gl)
        {
            GetConfiguration();
            AjouteTexture(c.GetParametre("Particule", Configuration.GetImagePath("particuleTexture.png")), 1);

            AjouteEmetteur(new EmetteurGravitation(G, MULT_DIST, TIMER_CREATE));

            AjouteModificateur(new ModificateurVitesseLineaire());
            AjouteModificateur(new ModificateurAttracteurMutuelle(G, MULT_DIST));
            AjouteModificateur(new ModificateurVitesseLineaire());
            AjouteModificateur(new ModificateurRecentre(VITESSE_RECENTRE));
            AjouteModificateur(new ModificateurExclusion(MIN_X * 1.5f, MIN_Y * 1.5f, MAX_X * 1.5f, MAX_Y * 1.5f, ModificateurExclusion.Exclusions.EXCLURE_TOUT));
        }
        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                NB_MAX_PARTICULES = c.GetParametre("Nb particules", 200);
                TIMER_CREATE = c.GetParametre("Delai creation particule", 200);
                G = c.GetParametre("G", 0.01f);
                MULT_DIST = c.GetParametre("Mult dist", 10.0f);
                VITESSE_RECENTRE = c.GetParametre("Vitesse recentre", 0.2f);
            }
            return c;
        }
    }

}
