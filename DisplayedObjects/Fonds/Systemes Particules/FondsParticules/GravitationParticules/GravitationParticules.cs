using System;

using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.SystemeParticules2D.Modificateurs;

using SharpGL;
using System.Threading.Tasks;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.Particules
{
	public class GravitationParticules : SystemeParticules2D.SystemeParticules2D
    {
		#region Parametres
		const string CAT = "GravitationParticules";
		CategorieConfiguration c;

		int TIMER_CREATE;
		float G = 0.01f;
		float MULT_DIST;
		float VITESSE_RECENTRE;
		#endregion

		public GravitationParticules(OpenGL gl) : base(gl)
        {
			getConfiguration();
            AjouteTexture(c.getParametre( "Particule", Configuration.getImagePath( "particuleTexture.png" ) ), 1);

            AjouteEmetteur(new EmetteurGravitation(G, MULT_DIST, TIMER_CREATE));

            AjouteModificateur(new ModificateurVitesseLineaire());
            AjouteModificateur(new ModificateurAttracteurMutuelle(G, MULT_DIST));
            AjouteModificateur(new ModificateurVitesseLineaire());
            AjouteModificateur(new ModificateurRecentre(VITESSE_RECENTRE));
            AjouteModificateur(new ModificateurExclusion(MIN_X * 1.5f, MIN_Y * 1.5f, MAX_X * 1.5f, MAX_Y * 1.5f, ModificateurExclusion.Exclusions.EXCLURE_TOUT));
        }
        public override CategorieConfiguration getConfiguration()
        {
			if ( c == null)
			{
				c = Configuration.getCategorie(CAT);
				NB_MAX_PARTICULES = c.getParametre("Nb particules", 200);
				TIMER_CREATE = c.getParametre("Delai creation particule", 200);
				G = c.getParametre("G", 0.01f);
				MULT_DIST =c.getParametre("Mult dist", 10.0f);
				VITESSE_RECENTRE = c.getParametre("Vitesse recentre", 0.2f);
			}
			return c;
        }
    }

}
