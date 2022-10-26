using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;
using ClockScreenSaverGL.DisplayedObjects.Fonds.Turing;
using SharpGL;
using SharpGL.SceneGraph.Assets;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ClockScreenSaverGL.DisplayedObjects
{
    class PanneauMessage
    {
        private static PanneauMessage _instance;


        public const string CAT = "Panneau Messages";
        protected CategorieConfiguration c;
        private Texture _texture = null;
        private Bitmap _bitmap;
        Interpolateur _interpolateur;

        public CategorieConfiguration getConfiguration()
        {
            if (c == null)
            {
                c = Configuration.getCategorie(CAT);

            }
            return c;
        }


        private PanneauMessage()
        {
            c = getConfiguration();
        }
        public static PanneauMessage instance
        {
            get
            {
                if (_instance == null)
                    return _instance = new PanneauMessage();

                return _instance;
            }
        }
        public void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
            if (_texture != null)
            {
                gl.Disable(OpenGL.GL_LIGHTING);
                gl.Disable(OpenGL.GL_DEPTH);
                using (new Viewport2D(gl, -1.0f, -1.0f, 1.0f, 1.0f))
                {
                    gl.Enable(OpenGL.GL_BLEND);
                    gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
                    gl.Enable(OpenGL.GL_TEXTURE_2D);
                    _texture.Bind(gl);
                    float[] col = { couleur.R / 512.0f, couleur.G / 512.0f, couleur.B / 512.0f, 1.0f - _interpolateur.interpolationAccelere() };
                    gl.Color(col);
                    gl.Begin(OpenGL.GL_QUADS);
                    gl.TexCoord(0, 0.0f); gl.Vertex(-0.25f, 0.25f, 0);
                    gl.TexCoord(0, 1.0f); gl.Vertex(-0.25f, -0.25f, 0);
                    gl.TexCoord(1.0f, 1.0f); gl.Vertex(0.25f, -0.25f, 0);
                    gl.TexCoord(1.0f, 0.0f); gl.Vertex(0.25f, 0.25f, 0);
                    gl.End();
                }

                if (_interpolateur.estFini())
                    _texture = null;
            }
        }

        internal void SetMessage(OpenGL gl, string message)
        {
            _bitmap?.Dispose();

            SizeF taille;
            Font fonte = new Font(FontFamily.GenericSansSerif, 64, FontStyle.Bold, GraphicsUnit.Pixel);
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                taille = g.MeasureString(message, fonte);

            _bitmap = new Bitmap((int)Math.Max(taille.Width, 300), (int)Math.Max(taille.Height, 300), PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(_bitmap))
            {
                g.Clear(Color.Gray);
                float x = (float)(_bitmap.Width - taille.Width) / 2.0f;
                float y = (float)(_bitmap.Height - taille.Height) / 2.0f;

                g.DrawString(message, fonte, Brushes.White, x, y);
            }

            _texture = new Texture();
            _texture.Create(gl, _bitmap);
            _interpolateur = new Interpolateur(3000);
            _interpolateur.Start();
        }
    }
}
