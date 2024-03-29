﻿/*
 * Bande:
 * classe de base pour les objets qui affichent heure/minutes/secondes, verticalement ou horizontalement
 */
using SharpGL;
using System;
using System.Drawing;
namespace ClockScreenSaverGL.DisplayedObjects.Bandes
{
    /// <summary>
    /// Description of Bande.
    /// </summary>
    public abstract class Bande : DisplayedObject, IDisposable
    {
        protected int _intervalleTexte;
        protected float _largeurCase;
        public int _hauteurFonte;
        protected int _valeurMax;
        protected Font _fonte;
        protected float _origine;
        protected Trajectoire _trajectoire;
        protected SizeF _taillebande;
        protected byte _alpha;

        /// <summary>
        /// Retourne la valeur a afficher, avec un decalage partiel (ex: decalage partiel par seconde pour afficher
        /// les minutes
        /// </summary>
        /// <param name="maintenant"></param>
        /// <param name="value"></param>
        /// <param name="decalage"></param>
        protected abstract void GetValue(Temps maintenant, out float value, out float decalage);

        protected Bande(OpenGL gl, int valMax, int intervalle, float largeurcase, float origineX, int largeur) :
            base(gl)
        {
            GetConfiguration();
            _valeurMax = valMax;
            _largeurCase = largeurcase;
            //_hauteurFonte = hauteurfonte;
            _origine = origineX;
            _intervalleTexte = intervalle;
            //_alpha = alpha;

            _fonte = new Font(FontFamily.GenericMonospace, _hauteurFonte, FontStyle.Bold, GraphicsUnit.Pixel);
        }

        /// <summary>
        /// Implementation de la fonction virtuelle Deplace: deplacement de l'objet
        /// </summary>
        /// <param name="maintenant"></param>
        /// <param name="tailleEcran"></param>
        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.DEPLACE);
#endif

            _trajectoire.Avance(tailleEcran, _taillebande, maintenant);
#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif

        }

        public override void Dispose()
        {
            _fonte?.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
