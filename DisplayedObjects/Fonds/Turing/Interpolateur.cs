using System;
// Interpolateur a la mode Android: 
// Equations: https://titanwolf.org/Network/Articles/Article?AID=248e19ac-44bb-40b0-8b24-b190445fcc9d#gsc.tab=0
//              http://cogitolearning.co.uk/2013/10/android-animations-tutorial-5-more-on-interpolators/
namespace ClockScreenSaverGL.DisplayedObjects.Fonds.Turing
{
    internal class Interpolateur
    {
        private TimeSpan _duree;
        private DateTime _dateDepart;
        private bool _demarré;

        public Interpolateur(TimeSpan duree)
        {
            _duree = duree;
            _demarré = false;
        }

        public Interpolateur(int nbMillisecondes)
        {
            _duree = new TimeSpan(0, 0, 0, 0, nbMillisecondes);
            _demarré = false;
        }

        public void Start()
        {
            _dateDepart = DateTime.Now;
            _demarré = true;
        }

        public void Stop()
        {
            _demarré = false;
        }

        public bool estFini()
        {
            return (!_demarré) || (DateTime.Now.Subtract(_dateDepart) > _duree);
        }


        private float getProgression()
        {
            if (estFini())
                return 1.0f;
            if (!_demarré)
                return 0.0f;

            DateTime maintenant = DateTime.Now;

            float progress = (float)((maintenant - _dateDepart).TotalMilliseconds / _duree.TotalMilliseconds);
            if (progress < 0)
                progress = 0;
            if (progress > 1.0f)
                progress = 1.0f;
            return progress;
        }
        public float interpolationLineaire()
        {
            float progress = getProgression();
            return progress;
        }

        public float interpolationAccelere(float facteurAcceleration = 1.0f)
        {
            return (float)Math.Pow(getProgression(), 2.0 * facteurAcceleration);
        }

        public float interpolationDecelere(float facteurAcceleration = 1.0f)
        {
            return (float)(1.0 - Math.Pow(1.0 - getProgression(), 2.0 * facteurAcceleration));
        }

        public float interpolationAccelereDecelere()
        {
            return (float)(Math.Cos((getProgression() + 1) * Math.PI) / 2.0 + 0.5);
        }

        public float interpolationAnticipe(float facteur = 2.0f)
        {
            float t = getProgression();
            return (float)((facteur + 1) * Math.Pow(t, 3) - facteur * Math.Pow(t, 2));
        }

        public float interpolationDepasse(float T)
        {
            float t = getProgression();
            return (float)((T + 1) * Math.Pow(t - 1, 3) + T * Math.Pow(t - 1, 2) + 1);
        }

        public float interpolationAnticipeDepasse(float T = 2.0f)
        {
            float t = getProgression();
            if (t < 0.5f)
                return (float)(0.5 * ((T + 1) * Math.Pow(2.0 * t, 3) - T * Math.Pow(2 * t, 2)));
            else
                return (float)(0.5 * ((T + 1) * Math.Pow(2 * t - 2, 3) + T * Math.Pow(2 * t - 2, 2)) + 1);
        }

        public float interpolationRebondi()
        {
            float t = getProgression();
            if (t < 0.31489)
                return (float)(8.0 * Math.Pow(1.1226 * t, 2));
            else
                if (t < 0.6599)
                return (float)(8.0 * Math.Pow(1.1226 * t - 0.54719, 2) + 0.7);
            else
                if (t < 0.85908)
                return (float)(8.0 * Math.Pow(1.1226 * t - 0.8526, 2) + 0.9);
            else
                return (float)(8.0 * Math.Pow(1.1226 * t - 1.0435, 2) + 0.95);
        }
    }
}
