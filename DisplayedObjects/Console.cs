using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;
using SharpGL;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects
{
    public class Console : IDisposable
    {
        private readonly OpenGLFonte _fonte;
        sealed private class Ligne
        {
            public Ligne(Color c, string s)
            {
                couleur = c;
                texte = s;
            }
            public Color couleur;
            public string texte;
        };

        private readonly List<Ligne> _lignes;

        private static Console INSTANCE = null;

        static public Console GetInstance(OpenGL gl)
        {
            if (INSTANCE == null)
                INSTANCE = new Console(gl);

            return INSTANCE;
        }

        private Console(OpenGL gl)
        {

            _fonte = new OpenGLFonte(gl, OpenGLFonte.CARACTERES, 12, FontFamily.GenericSansSerif, FontStyle.Regular);
            _lignes = new List<Ligne>();
        }

        public void Dispose()
        {
            _fonte?.Dispose();
        }

        public void Clear()
        {
            _lignes.Clear();
        }

        public void AddLigne(Color couleur, string Texte)
        {
            _lignes.Add(new Ligne(couleur, Texte));
        }

        public void Trace(OpenGL gl, Rectangle tailleEcran, float X = 0, float Y = 0)
        {
            using (Viewport2D v = new Viewport2D(gl, 0, 0, tailleEcran.Width, tailleEcran.Height))
            {
                gl.Disable(OpenGL.GL_LIGHTING);
                gl.Disable(OpenGL.GL_DEPTH);
                gl.Disable(OpenGL.GL_COLOR_MATERIAL);

                gl.Enable(OpenGL.GL_TEXTURE_2D);
                gl.Enable(OpenGL.GL_BLEND);
                gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);

                float y = tailleEcran.Height - _fonte.Hauteur() - Y;
                foreach (Ligne ligne in _lignes)
                {
                    _fonte.drawOpenGL(gl, ligne.texte, X, y, ligne.couleur);
                    y -= _fonte.Hauteur();
                }

            }
        }
    }
}
