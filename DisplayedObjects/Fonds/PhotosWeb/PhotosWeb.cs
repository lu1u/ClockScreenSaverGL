using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;
using ClockScreenSaverGL.DisplayedObjects.OpenGLUtils;
using SharpGL;
using SharpGL.SceneGraph.Assets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net.Http;
using ClockScreenSaverGL.Utils;

///
/// Affiche des images telechargées au hasard sur des sites internet 
/// (liste d'url dans un fichier de configuration)
namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    internal class PhotosWeb : Fond
    {
        #region Parametres
        public const string CAT = "PhotosWeb";
        private CategorieConfiguration c;
        private int DELAI_TIMER;
        int NB_MAX_IMAGES;
        int TAILLE_CADRE, TAILLE_OMBRE;
        float VITESSE_Y;
        float TAILLE_IMAGE;
        float VARIATION_MAX, VITESSE_VARIATION, VITESSE_ANGLE;
        #endregion

        public const string NOM_FICHIER_URL = "random pictures.txt";

        class Photo
        {
            public Texture _texture;
            public float _width, _height;
            public float _x, _y, _angle, _vx, _vy, _va;
        }


        private readonly List<Photo> _photos = new List<Photo>();       // La liste des photos a afficher
        private TimerIsole _timer;                                      // Timer pour creer de nouvelles photos
        private List<string> _urls;                                     // Liste d'url extraite du fichier de configuration
        public PhotosWeb(OpenGL gl) : base(gl)
        {
        }

        protected override void Init(OpenGL gl)
        {
            base.Init(gl);
            _urls = FichiersUtils.LitFichierChaines(NOM_FICHIER_URL);
        }


        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                NB_MAX_IMAGES = c.GetParametre("Nb Max Images", 10, (a) => { NB_MAX_IMAGES = Convert.ToInt32(a); });
                TAILLE_CADRE = c.GetParametre("Taille Cadre", 10, (a) => { TAILLE_CADRE = Convert.ToInt32(a); });
                TAILLE_OMBRE = c.GetParametre("Taille Ombre", 10, (a) => { TAILLE_OMBRE = Convert.ToInt32(a); });
                TAILLE_IMAGE = c.GetParametre("Taille Image", 0.0005f, (a) => { TAILLE_IMAGE = (float)Convert.ToDouble(a); });
                VITESSE_Y = c.GetParametre("Vitesse Y", 0.02f, (a) => { VITESSE_Y = (float)Convert.ToDouble(a); });
                VARIATION_MAX = c.GetParametre("Variation max vx", 0.05f, (a) => { VARIATION_MAX = (float)Convert.ToDouble(a); });
                VITESSE_VARIATION = c.GetParametre("Vitesse variation vx", 0.3f, (a) => { VITESSE_VARIATION = (float)Convert.ToDouble(a); });
                VITESSE_ANGLE = c.GetParametre("Vitesse angle", 1.0f, (a) => { VITESSE_ANGLE = (float)Convert.ToDouble(a); });
                DELAI_TIMER = c.GetParametre("Delai images", 5000, (a) => { _timer = new TimerIsole(Convert.ToInt32(a), false); });
                _timer = new TimerIsole(DELAI_TIMER, true);
            }
            return c;
        }




        /// <summary>
        /// Charge une photo depuis internet et l'ajoute dans le tableau
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="ratioEcran"></param>
        private async void ChargePhoto(OpenGL gl, float ratioEcran)
        {
            if (_photos.Count >= NB_MAX_IMAGES)
                return;

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = _urls[random.Next(_urls.Count)]; 
                    // Get the image stream from the URL
                    using (Stream stream = await client.GetStreamAsync(url))
                    {
                        Bitmap bitmap = DessineCadre(new Bitmap(stream));
                        int largeur = bitmap.Width;
                        int hauteur = bitmap.Height;
                        Texture texture = new Texture();
                        if (texture.Create(gl, bitmap))
                        {
                            Photo photo = new Photo()
                            {
                                _texture = texture,
                                _width = largeur * TAILLE_IMAGE,
                                _height = hauteur * TAILLE_IMAGE,
                                _x = FloatRandom(0, ratioEcran),
                                _y = -hauteur * TAILLE_IMAGE,
                                _angle = 0,
                                _vx = FloatRandom(-0.05f, 0.05f),
                                _vy = FloatRandom(VITESSE_Y, VITESSE_Y * 2.0f),
                                _va = FloatRandom(VITESSE_ANGLE, VITESSE_ANGLE * 2.0f) * SigneRandom()
                            };
                            _photos.Add(photo);
                        }
                        else
                        {
                            Log.Instance.Error("PhotosWeb.ChargePhoto: texture.Create: " + url);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Instance.Error("Erreur dans PhotosWeb.ChargePhoto");
                    Log.Instance.Error(ex.Message);
                }
            }
        }

        /// <summary>
        /// Dessine un cadre blanc et une ombre autour de l'image qu'on recoit
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private Bitmap DessineCadre(Bitmap bitmap)
        {
            Size size = new Size(bitmap.Width + TAILLE_CADRE + TAILLE_CADRE + TAILLE_OMBRE, bitmap.Height + TAILLE_CADRE + TAILLE_CADRE + TAILLE_OMBRE);

            Bitmap bmp = new Bitmap(bitmap, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(0,0,0,0));
                g.SmoothingMode = SmoothingMode.None;
                g.FillRectangle(new SolidBrush(Color.FromArgb(96, 0, 0, 0)), TAILLE_OMBRE*2, TAILLE_OMBRE * 2, bitmap.Width + TAILLE_OMBRE * 3, bitmap.Height + TAILLE_OMBRE * 3);
                g.FillRectangle(Brushes.White, 0, 0, bitmap.Width + TAILLE_CADRE + TAILLE_CADRE, bitmap.Height + TAILLE_CADRE + TAILLE_CADRE);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawImage(bitmap, TAILLE_CADRE, TAILLE_CADRE, bitmap.Width, bitmap.Height);
            }

            return bmp;
        }

        public override bool ClearBackGround(OpenGL gl, Color c)
        {
            gl.ClearColor(c.R / 1024.0f, c.G / 1024.0f, c.B / 1024.0f, 1);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            return true;
        }

        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            float ratio = (float)tailleEcran.Width / (float)tailleEcran.Height;

            if (_timer.Ecoule())
                ChargePhoto(gl, ratio);

            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
            gl.Disable(OpenGL.GL_DEPTH);
            gl.Disable(OpenGL.GL_DEPTH_TEST);
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            float[] col = { couleur.R / 256.0f, couleur.G / 256.0f, couleur.B / 256.0f, 1 };
            gl.Color(col);

            using (new Viewport2D(gl, 0, 1, ratio, 0))
                foreach (Photo p in _photos)
                {
                    if (p._texture != null)
                        p._texture.Bind(gl);

                    gl.Translate(p._x, p._y, 0);
                    gl.Rotate(0, 0, p._angle);
                    using (new GLBegin(gl, OpenGL.GL_QUADS))
                    {
                        gl.TexCoord(0.0f, 0.0f); gl.Vertex(-p._width, -p._height);
                        gl.TexCoord(1.0f, 0.0f); gl.Vertex(+p._width, -p._height);
                        gl.TexCoord(1.0f, 1.0f); gl.Vertex(+p._width, +p._height);
                        gl.TexCoord(0.0f, 1.0f); gl.Vertex(-p._width, +p._height);
                    }
                    gl.Rotate(0, 0, -p._angle);
                    gl.Translate(-p._x, -p._y, 0);
                }
#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

        /// <summary>
        /// Deplace les photos, supprime celles qui sont arrivées en bas
        /// </summary>
        /// <param name="maintenant"></param>
        /// <param name="tailleEcran"></param>
        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.DEPLACE);
#endif
            for (int i = _photos.Count - 1; i >= 0; i--)
            {
                _photos[i]._x += _photos[i]._vx * maintenant.intervalleDepuisDerniereFrame;
                _photos[i]._y += _photos[i]._vy * maintenant.intervalleDepuisDerniereFrame;
                _photos[i]._angle += _photos[i]._va * maintenant.intervalleDepuisDerniereFrame;
                Varie(ref _photos[i]._vx, -VARIATION_MAX, VARIATION_MAX, VITESSE_VARIATION, maintenant.intervalleDepuisDerniereFrame);

                if ((_photos[i]._y - _photos[i]._height > 1) && (_photos[i]._y - _photos[i]._width > 1))
                    _photos.RemoveAt(i);
            }
#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif
        }

        public override string DumpRender()
        {
            return base.DumpRender() + $" Nb Images: {_photos.Count}/{NB_MAX_IMAGES}";
        }
    }
}
