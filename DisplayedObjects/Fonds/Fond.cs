/*
 * Crée par SharpDevelop.
 * Utilisateur: lucien
 * Date: 22/12/2014
 * Heure: 15:06
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;
using ClockScreenSaverGL.DisplayedObjects.OpenGLUtils;
using SharpGL;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{

    /// <summary>
    /// Description of Class1.
    /// </summary>
    public abstract class Fond : DisplayedObject
    {
        private CategorieConfiguration c;
        private TextureAsynchrone _textureArcade, _textureEcran;
        private float DERIVE_Y, ALPHA_ARCADE;
        private float RATIO_TEXTURE;
        private bool APPLIQUER_LOOK_ARCADE;

        protected Fond(OpenGL gl) : base(gl)
        {
        }

        public override bool ClearBackGround(OpenGL gl, Color c)
        {
            gl.ClearColor(0, 0, 0, 0);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            return true;
        }

        //public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        //{
        //	base.AfficheOpenGL(gl, maintenant, tailleEcran, couleur);
        //}

        public virtual void FillConsole(OpenGL gl)
        {
            //getConfiguration()?.fillConsole(gl);
            string[] lignes = GetConfiguration().GetLignesParametres();
            Console console = Console.GetInstance(gl);
            console.AddLigne(Color.LightGreen, "");
            //c.AddLigne(Color.LightGreen, getConfiguration().);
            console.AddLigne(Color.LightGreen, "");
            console.AddLigne(Color.LightGreen, "F1..F6: changer couleur");
            console.AddLigne(Color.LightGreen, "8/2 : changer le parametre courant");
            console.AddLigne(Color.LightGreen, "4/6 : modifier la valeur du parametre courant");
            console.AddLigne(Color.LightGreen, "Les valeurs en gris nécessitent de redémarrer le fond (touche R)");
            console.AddLigne(Color.LightGreen, "");

            foreach (string ligne in lignes)
                if (ligne.Length > 1)
                {
                    Color col;
                    switch (ligne[0])
                    {
                        case 'Y':
                            col = Color.Yellow; break;
                        case 'G':
                            col = Color.Green; break;
                        case 'W':
                            col = Color.White; break;
                        default:
                            col = Color.Gray; break;
                    }

                    console.AddLigne(col, ligne.Substring(1));
                }
        }

        public override bool KeyDown(Form f, Keys k)
        {
            CategorieConfiguration conf = GetConfiguration();
            if (conf?.KeyDown(k) == true)
                return true;

            return base.KeyDown(f, k);
        }

        static public Color GetColorWithHueChange(Color couleur, double change)
        {
            return new CouleurGlobale(couleur).GetColorWithHueChange(change);
        }

        static public Color GetColorWithLuminanceChange(Color couleur, double change)
        {
            return new CouleurGlobale(couleur).GetColorWithValueChange(change);
        }
        public static void SetColorWithHueChange(OpenGL gl, Color couleur, double change)
        {
            Color cG = GetColorWithHueChange(couleur, change);
            gl.Color(cG.R / 256.0f, cG.G / 256.0f, cG.B / 256.0f, cG.A / 256.0f);
        }

        public static void SetColorWithLuminanceChange(OpenGL gl, Color couleur, double change)
        {
            Color cG = GetColorWithLuminanceChange(couleur, change);
            gl.Color(cG.R / 256.0f, cG.G / 256.0f, cG.B / 256.0f, cG.A / 256.0f);
        }


        protected void LookArcade(OpenGL gl, Color couleur)
        {
            if (c == null)
            {
                c = GetConfiguration();
                APPLIQUER_LOOK_ARCADE = c.GetParametre("Arcade Appliquer", true, a => APPLIQUER_LOOK_ARCADE = Convert.ToBoolean(a));
                DERIVE_Y = c.GetParametre("Arcade Dérive Y", 0.01f, a => DERIVE_Y = (float)Convert.ToDouble(a));
                ALPHA_ARCADE = c.GetParametre("Arcade Alpha", 0.5f, a => ALPHA_ARCADE = (float)Convert.ToDouble(a));
                RATIO_TEXTURE = c.GetParametre("Arcade Ratio texture", 16.0f, a => RATIO_TEXTURE = (float)Convert.ToDouble(a));
            }

            if (!APPLIQUER_LOOK_ARCADE)
                return;

            if (_textureArcade == null)
            {
                _textureArcade = new TextureAsynchrone(gl, Configuration.GetImagePath("Arcade\\arcade.png"));
                _textureArcade.Init();
            }

            if (_textureEcran == null)
            {
                _textureEcran = new TextureAsynchrone(gl, Configuration.GetImagePath("Arcade\\ecran.png"));
                _textureEcran.Init();
            }

            using (new Viewport2D(gl, 0, 0, 1, 1))
            {
                gl.Enable(OpenGL.GL_BLEND);
                gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
                gl.Enable(OpenGL.GL_TEXTURE_2D);
                gl.Disable(OpenGL.GL_DEPTH);
                if (_textureArcade.Pret)
                {
                    _textureArcade.Texture.Bind(gl);
                    gl.Color(0, 0, 0, ALPHA_ARCADE);
                    using (new GLBegin(gl, OpenGL.GL_QUADS))
                    {
                        float yCoord = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height / RATIO_TEXTURE;
                        gl.TexCoord(0.0f, yCoord); gl.Vertex(0, FloatRandom(1 - DERIVE_Y, 1 + DERIVE_Y));
                        gl.TexCoord(1.0f, yCoord); gl.Vertex(1, FloatRandom(1 - DERIVE_Y, 1 + DERIVE_Y));
                        gl.TexCoord(1.0f, 0.0f); gl.Vertex(1, FloatRandom(0 - DERIVE_Y, 0 + DERIVE_Y));
                        gl.TexCoord(0.0f, 0.0f); gl.Vertex(0, FloatRandom(0 - DERIVE_Y, 0 + DERIVE_Y));
                    }
                }

                if (_textureEcran.Pret)
                {
                    OpenGLColor.Couleur(gl, couleur, 0.5f);
                    _textureEcran.Texture.Bind(gl);
                    using (new GLBegin(gl, OpenGL.GL_QUADS))
                    {
                        gl.TexCoord(0.0f, 1.0f); gl.Vertex(0, 0);
                        gl.TexCoord(1.0f, 1.0f); gl.Vertex(1, 0);
                        gl.TexCoord(1.0f, 0.0f); gl.Vertex(1, 1);
                        gl.TexCoord(0.0f, 0.0f); gl.Vertex(0, 1);
                    }
                }
            }
        }
    }
}
