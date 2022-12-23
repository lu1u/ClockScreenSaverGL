namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    public class Explosion : Sprite
    {
        private readonly TimerIsole _timer;
        public Explosion(float X, float Y, int Image, int Delai) : base(X, Y, Image, 0.2f)
        {
            _timer = new TimerIsole(Delai);
        }

        public bool Ecoule()
        {
            return _timer.Ecoule();
        }
    }
}
