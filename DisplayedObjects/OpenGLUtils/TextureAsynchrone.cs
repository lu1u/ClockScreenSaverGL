using ClockScreenSaverGL.Config;
using SharpGL;
using SharpGL.SceneGraph.Assets;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.OpenGLUtils
{
    public class TextureAsynchrone : ObjetAsynchrone
    {
        public string _nomFichier;
        protected Bitmap _bitmap;
        protected Texture _texture;
        protected OpenGL _gl;

        public TextureAsynchrone(OpenGL gl, string nomfichier)
        {
            _gl = gl;
            _nomFichier = nomfichier;
        }
        /// <summary>
        /// Charger la bitmap depuis un fichier en arriere plan
        /// </summary>
        protected override void InitAsynchrone()
        {
            _bitmap = (Bitmap)Image.FromFile(_nomFichier);// Charger la bitmap depuis un fichier
        }

        /// <summary>
        /// La texture OpenGL doit etre créée dans le thread principal
        /// </summary>
        protected override void InitSynchrone()
        {
            if (_bitmap != null)
            {
                _texture = new Texture();
                if (!_texture.Create(_gl, _bitmap))
                {
                    Log.Instance.Error("TextureAsynchrone: erreur de chargement de la texture: " + _nomFichier);
                }
                _bitmap = null;// Plus besoin de la bitmap
            }
        }

        public Texture Texture
        {
            get
            {
                return _texture;
            }
        }
    }
}
