/***
 * Un 'timer' indépendant de toute autres ressources
 */

using System;

namespace ClockScreenSaverGL
{
    public class TimerIsole
    {
        private DateTime _prochainTick;
        private bool _initial;

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="intervalle">Intervalle en millisecondes</param>
        /// <param name="initial">true si le timer est initialement "écoulé"</param>
        public TimerIsole(double intervalle, bool initial = false)
        {
            Intervalle = intervalle;
            _prochainTick = DateTime.Now.AddMilliseconds(intervalle);
            _initial = initial;
        }

        public double Intervalle { get; set; }

        public bool Ecoule()
        {
            DateTime m = DateTime.Now;
            if (_initial || (m >= _prochainTick))
            {
                _prochainTick = m.AddMilliseconds(Intervalle);
                _initial = false;
                return true;
            }
            else
                return false;
        }
    }
}
