using SharpGL;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.Fourmilliere
{
    internal class Marqueur
    {
        private float _x, _y, _cap;
        private float _force, _forceMax;

        public float X
        {
            get => _x;
            set => _x = value;
        }

        public float Y
        {
            get => _y;
            set => _y = value;
        }


        public float Cap
        {
            get => _cap;
            set => _cap = value;
        }


        public float Force => _force;

        public float Alpha
        {
            get
            {
                if (_force <= 0)
                    return 0;

                if (_force >= _forceMax)
                    return 1.0f;

                return _force / _forceMax;
            }
        }
        /// <summary>
        /// Diminue la force du marqueue
        /// </summary>
        /// <param name="duree"></param>
        /// <returns>true si le marqueur est arrivé à zéro</returns>
        public bool Evapore(float duree)
        {
            _force -= duree;
            return _force <= 0;
        }


        public Marqueur(float x, float y, float cap, int force)
        {
            _x = x;
            _y = y;
            _force = force;
            _forceMax = force;
            _cap = cap;
        }

        internal void Affiche(OpenGL gl, float taille)
        {
            gl.PushMatrix();
            gl.Translate(_x, _y, 0);
            gl.Rotate(0, 0, _cap * Fond.RADIAN_TO_DEG);
            gl.Begin(OpenGL.GL_LINES);

            gl.Vertex(0, 0);
            gl.Vertex(taille, 0);

            gl.Vertex(taille * 0.6f, -0.6f * taille);
            gl.Vertex(taille, 0);
            gl.Vertex(taille * 0.6f, 0.6f * taille);

            gl.End();
            gl.PopMatrix();
        }

        internal void RenforceMarqueur()
        {
            _force = _forceMax;
        }
    }
}
