using ClockScreenSaverGL.DisplayedObjects.Ephemerides;
using ClockScreenSaverGL.DisplayedObjects.OpenGLUtils;
using SharpGL;

namespace ClockScreenSaverGL.DisplayedObjects
{
    internal class TextureEphemeride : TextureAsynchrone
    {
        private Ephemeride _ephemeride;
        private float LATITUDE, LONGITUDE;
        private int TIMEZONE;
        private bool HEURE_ETE;
        private int TAILLE_TEXTE_EPHEMERIDE;

        public TextureEphemeride(OpenGL gl, float latitude, float longitude, int timeZone, bool heureEte, int tailleTexteEphemeride) : base(gl, null)
        {
            LATITUDE = latitude;
            LONGITUDE = longitude;
            TIMEZONE = timeZone;
            HEURE_ETE = heureEte;
            TAILLE_TEXTE_EPHEMERIDE = tailleTexteEphemeride;
        }

        /// <summary>
        /// Creation de la bitmap meteo en arriere plan
        /// </summary>
        protected override void InitAsynchrone()
        {
            _ephemeride = new Ephemeride(LATITUDE, LONGITUDE, TIMEZONE, HEURE_ETE);
            _bitmap = _ephemeride.getBitmap(TAILLE_TEXTE_EPHEMERIDE);
            _bitmap = DisplayedObject.BitmapInvert(_bitmap);
        }
    }

}
