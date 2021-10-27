using System;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClockScreenSaverGL.Config;
using SharpGL;
using SharpGL.SceneGraph.Assets;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    internal class Moire : Fond, IDisposable
    {
        #region Parametres
        public const string CAT = "Moire";
        private CategorieConfiguration c;
        private int LARGEUR_BITMAP;
        private int HAUTEUR_BITMAP;
        private int ECART_MOIRE;
        private int LARGEUR_MOIRE;
        private int NB_COUCHES;
        #endregion
        private Bitmap _bitmapMoire;
        private Texture _textureMoire;

        private class Couche
        {
            public float x, y, angle, vitesse;
        }

        private Couche[] _couches;

        public override CategorieConfiguration getConfiguration()
        {
            if (c == null)
            {
                c = Configuration.getCategorie(CAT);
                LARGEUR_BITMAP = 400;//.getParametre("Largeur Bitmap", 2000);
                HAUTEUR_BITMAP = 400;//c.getParametre("Hauteur Bitmap", 2000);
                ECART_MOIRE = c.getParametre("Ecart Moire", 10);
                LARGEUR_MOIRE = c.getParametre("Largeur Moire", 5);
                NB_COUCHES = 4;// c.getParametre("Nb Couches", 2);
            }
            return c;
        }

        public Moire(OpenGL gl) : base(gl)
        {
            getConfiguration();
            InitGrille(gl);
            InitCouches();
        }

        private void InitCouches()
        {
            _couches = new Couche[NB_COUCHES];
            for (int i = 0; i < NB_COUCHES; i++)
            {
                _couches[i] = new Couche();
                _couches[i].x = FloatRandom(0.4f, 0.6f);
                _couches[i].y = FloatRandom(0.4f, 0.6f);
                _couches[i].angle = FloatRandom(0, 360);
                _couches[i].vitesse = FloatRandom(-5, 5);
            }
        }

        private void InitGrille(OpenGL gl)
        {
            if (_bitmapMoire != null)
            {
                _bitmapMoire.Dispose();
                _bitmapMoire = null;
            }
            _bitmapMoire = new Bitmap(LARGEUR_BITMAP, HAUTEUR_BITMAP, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(_bitmapMoire))
            {
                g.SmoothingMode = SmoothingMode.HighSpeed;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;

                // Dessine le moire
                using ( Pen p = new Pen( Color.White, LARGEUR_MOIRE))
                for (int x = 0; x < LARGEUR_BITMAP; x += ECART_MOIRE)
                {
                    //g.FillRectangle(Brushes.Yellow, x, 0, LARGEUR_MOIRE, HAUTEUR_BITMAP);
                    g.DrawLine(p, x, 0, x, HAUTEUR_BITMAP);
                }
            }

            _textureMoire = new Texture();
            _textureMoire.Create(gl, _bitmapMoire);
        }

        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
            for (int i = 0; i < NB_COUCHES; i++)
            {
                _couches[i].angle += _couches[i].vitesse * maintenant.intervalleDepuisDerniereFrame;
            }
        }

        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.Clear(OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.PushMatrix();
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.PushMatrix();
            gl.LoadIdentity();
            gl.Ortho2D(0.0, LARGEUR_BITMAP, 0.0, HAUTEUR_BITMAP);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_DEPTH);
            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            float[] col = { couleur.R / 256.0f, couleur.G / 256.0f, couleur.B / 256.0f, 1 };
            gl.Color(col);
            gl.Translate(LARGEUR_BITMAP / 2, HAUTEUR_BITMAP / 2, 0);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);

            foreach (Couche c in _couches)
            {
                gl.Translate(c.x, c.y, 0);
                gl.Rotate(0, 0, c.angle);
                gl.Begin(OpenGL.GL_QUADS);
                gl.TexCoord(0.0f, 0.0f); gl.Vertex(-LARGEUR_BITMAP, +HAUTEUR_BITMAP);
                gl.TexCoord(0.0f, 1.0f); gl.Vertex(+LARGEUR_BITMAP, +HAUTEUR_BITMAP);
                gl.TexCoord(1.0f, 1.0f); gl.Vertex(+LARGEUR_BITMAP, -HAUTEUR_BITMAP);
                gl.TexCoord(1.0f, 0.0f); gl.Vertex(-LARGEUR_BITMAP, -HAUTEUR_BITMAP);
                gl.End();
                gl.Rotate(0, 0, -c.angle);
                gl.Translate(-c.x, -c.y, 0);
            }
        }
    }
}
