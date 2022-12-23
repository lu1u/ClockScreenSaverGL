using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;
using ClockScreenSaverGL.DisplayedObjects.OpenGLUtils;
using SharpGL;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    internal class CasseBrique : Fond
    {
        #region Configuration
        private const string CAT = "Casse brique";
        private CategorieConfiguration c;
        private float BALLE_VX = 0.015f;
        private float BALLE_VY = 0.012f;
        private float MARGE_BRIQUE = 0.45f;
        private readonly float MIN_VIEWPORT_X = 0;
        private readonly float MIN_VIEWPORT_Y = 0;
        private readonly float MAX_VIEWPORT_X = 1.0f;
        private readonly float MAX_VIEWPORT_Y = 1.0f;
        private float TAILLE_BALLE = 0.01f;
        private int NB_BRIQUES_PAR_LIGNE = 20;
        private int NB_BRIQUES_PAR_COLONNES = 10;
        private int NB_BRIQUES;
        private int NB_MAX_PARTICULE = 10000;
        private int NB_PARTICULE_EXPLOSION = 200;
        private float TAILLE_PARTICULE = 0.0025f;
        #endregion

        sealed private class Particule
        {
            public float x, y, vx, vy, alpha;
            public Color Couleur;
        }

        // Balle
        private Balle _balle;
        private readonly Brique[] _briques;
        private int _nbBriques;
        private readonly List<Particule> _particules = new List<Particule>();

        sealed private class BriqueTombante : Brique
        {
            public BriqueTombante(Brique b, float vx, float vy, float vr) : base(b.X, b.Y, b.Largeur, b.Hauteur, b.ChangeCouleur)
            {
                Vx = vx;
                Vy = vy;
                Vr = vr;
                Rotation = 0;
                Couleur = b.Couleur;
            }
            public float Vx, Vy, Rotation;
            internal float Vr;
        }

        private readonly List<BriqueTombante> _briquesTombantes = new List<BriqueTombante>();
        private readonly TextureAsynchrone _textureBalle;
        private readonly TextureAsynchrone _textureBrique;
        private TimerIsole _timerReinit;
        private int _ligneCourante;

        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                TAILLE_BALLE = c.GetParametre("Taille balle", 0.01f);
                NB_BRIQUES_PAR_LIGNE = c.GetParametre("Nb briques par ligne", 20);
                NB_BRIQUES_PAR_COLONNES = c.GetParametre("Nb briques par colonne", 10);
                NB_BRIQUES = NB_BRIQUES_PAR_COLONNES * NB_BRIQUES_PAR_LIGNE;
                NB_MAX_PARTICULE = c.GetParametre("Nb max particules", 10000, a => NB_MAX_PARTICULE = Convert.ToInt32(a));
                NB_PARTICULE_EXPLOSION = c.GetParametre("Nb particules par explosion", 200, a => NB_PARTICULE_EXPLOSION = Convert.ToInt32(a));
                TAILLE_PARTICULE = c.GetParametre("Taille particule", 0.0025f);
                BALLE_VX = c.GetParametre("Vitesse balle X", 0.015f);
                BALLE_VY = c.GetParametre("Vitesse balle Y", 0.016f);
                MARGE_BRIQUE = c.GetParametre("Marge brique", 0.45f);
            }
            return c;
        }

        public CasseBrique(OpenGL gl) : base(gl)
        {
            c = GetConfiguration();
            _balle = new Balle(FloatRandom(MIN_VIEWPORT_X, MAX_VIEWPORT_X), MIN_VIEWPORT_Y, BALLE_VX, BALLE_VY, TAILLE_BALLE, TAILLE_BALLE);

            _textureBalle = new TextureAsynchrone(gl, Configuration.GetImagePath("balle.png"));
            _textureBalle.Init();
            _textureBrique = new TextureAsynchrone(gl, Configuration.GetImagePath("brique.png"));
            _textureBrique.Init();

            float LargeurColonne = (MAX_VIEWPORT_X - MIN_VIEWPORT_X) / NB_BRIQUES_PAR_LIGNE;
            float HauteurLigne = (MAX_VIEWPORT_Y - MIN_VIEWPORT_Y) * 0.3f / NB_BRIQUES_PAR_COLONNES;
            float LargeurBrique = LargeurColonne * MARGE_BRIQUE;
            float HauteurBrique = HauteurLigne * MARGE_BRIQUE;
            int indice = 0;
            _briques = new Brique[NB_BRIQUES];
            for (int j = 0; j < NB_BRIQUES_PAR_COLONNES; j++)
            {
                float decalageCouleur = j / (float)NB_BRIQUES_PAR_COLONNES;
                float y = (MAX_VIEWPORT_Y * 0.8f) - ((j * HauteurLigne) + (HauteurLigne / 2.0f));
                for (int i = 0; i < NB_BRIQUES_PAR_LIGNE; i++)
                {
                    float x = MIN_VIEWPORT_X + (i * LargeurColonne) + (LargeurColonne / 2.0f);

                    _briques[indice] = new Brique(x, y, LargeurBrique, HauteurBrique, decalageCouleur);
                    indice++;
                }
            }

            _nbBriques = NB_BRIQUES_PAR_COLONNES * NB_BRIQUES_PAR_LIGNE;
        }

        /// <summary>
        /// Fait tout l'affichage du fond 
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="maintenant"></param>
        /// <param name="tailleEcran"></param>
        /// <param name="couleur"></param>
        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            gl.LoadIdentity();
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_DEPTH);
            gl.Enable(OpenGL.GL_BLEND);

            using (new Viewport2D(gl, MIN_VIEWPORT_X, MIN_VIEWPORT_Y, MAX_VIEWPORT_X, MAX_VIEWPORT_Y))
            {
                float[] col = { couleur.R / 256.0f, couleur.G / 256.0f, couleur.B / 256.0f, 1.0f };
                gl.Color(col);

                // Dessine les briques
                if (_textureBrique.Pret)
                {
                    gl.Enable(OpenGL.GL_TEXTURE_2D);
                    gl.Disable(OpenGL.GL_BLEND);
                    _textureBrique.Texture.Bind(gl);
                    foreach (Brique brique in _briques)
                    {
                        if (brique.Visible)
                        {
                            if ((_balle == null) || (_balle.Collision(brique) == Balle.COLLISION.RIEN))
                            {
                                brique.Couleur = GetColorWithHueChange(couleur, brique.ChangeCouleur);
                                gl.Color(brique.Couleur.R / 256.0f, brique.Couleur.G / 256.0f, brique.Couleur.B / 256.0f, 1.0f);
                            }
                            else
                                gl.Color(1.0f, 1.0f, 1.0f, 1.0f);

                            gl.Begin(OpenGL.GL_QUADS);

                            gl.TexCoord(0.0f, 0.0f); gl.Vertex(brique.Gauche, brique.Bas);
                            gl.TexCoord(0.0f, 1.0f); gl.Vertex(brique.Gauche, brique.Haut);
                            gl.TexCoord(1.0f, 1.0f); gl.Vertex(brique.Droite, brique.Haut);
                            gl.TexCoord(1.0f, 0.0f); gl.Vertex(brique.Droite, brique.Bas);
                            gl.End();
                        }
                    }

                    // Briques tombantes
                    foreach (BriqueTombante brique in _briquesTombantes)
                    {
                        SetColorWithHueChange(gl, couleur, brique.ChangeCouleur);
                        gl.PushMatrix();
                        gl.Translate(brique.X, brique.Y, 0);
                        gl.Rotate(0, 0, brique.Rotation);
                        gl.Begin(OpenGL.GL_QUADS);
                        gl.TexCoord(0.0f, 0.0f); gl.Vertex(-brique.Largeur, -brique.Hauteur);
                        gl.TexCoord(0.0f, 1.0f); gl.Vertex(-brique.Largeur, brique.Hauteur);
                        gl.TexCoord(1.0f, 1.0f); gl.Vertex(brique.Largeur, brique.Hauteur);
                        gl.TexCoord(1.0f, 0.0f); gl.Vertex(brique.Largeur, -brique.Hauteur);
                        gl.End();
                        gl.PopMatrix();
                    }
                }

                // Dessine la balle
                if ((_textureBalle.Pret) && (_balle != null))
                {
                    gl.Enable(OpenGL.GL_TEXTURE_2D);
                    gl.Enable(OpenGL.GL_BLEND);
                    gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
                    _textureBalle.Texture.Bind(gl);
                    gl.Begin(OpenGL.GL_QUADS);
                    gl.TexCoord(0.0f, 0.0f); gl.Vertex(_balle.Gauche, _balle.Bas);
                    gl.TexCoord(0.0f, 1.0f); gl.Vertex(_balle.Gauche, _balle.Haut);
                    gl.TexCoord(1.0f, 1.0f); gl.Vertex(_balle.Droite, _balle.Haut);
                    gl.TexCoord(1.0f, 0.0f); gl.Vertex(_balle.Droite, _balle.Bas);
                    gl.End();
                }


                // Particules
                gl.Disable(OpenGL.GL_TEXTURE_2D);
                gl.Enable(OpenGL.GL_BLEND);
                gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE);
                foreach (Particule p in _particules)
                {
                    gl.Color(p.Couleur.R / 256.0f, p.Couleur.G / 256.0f, p.Couleur.B / 256.0f, p.alpha);
                    gl.Begin(OpenGL.GL_QUADS);
                    gl.Vertex(p.x - TAILLE_PARTICULE, p.y + TAILLE_PARTICULE);
                    gl.Vertex(p.x - TAILLE_PARTICULE, p.y - TAILLE_PARTICULE);
                    gl.Vertex(p.x + TAILLE_PARTICULE, p.y - TAILLE_PARTICULE);
                    gl.Vertex(p.x + TAILLE_PARTICULE, p.y + TAILLE_PARTICULE);
                    gl.End();
                }
            }

            LookArcade(gl, couleur);
#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }


        /// <summary>
        /// Gere tous les deplacements
        /// </summary>
        /// <param name="maintenant"></param>
        /// <param name="tailleEcran"></param>
        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
            if (_timerReinit != null)
            {
                // Reinitialiser les briques
                if (_timerReinit.Ecoule())
                {
                    if (_ligneCourante < NB_BRIQUES_PAR_COLONNES)
                    {
                        int indice = _ligneCourante * NB_BRIQUES_PAR_LIGNE;
                        for (int i = indice; i < indice + NB_BRIQUES_PAR_LIGNE; i++)
                        {
                            _briques[i].Visible = true;
                        }

                        _ligneCourante++;
                    }
                    else
                    {
                        _nbBriques = NB_BRIQUES_PAR_COLONNES * NB_BRIQUES_PAR_LIGNE;
                        _balle = new Balle(FloatRandom(MIN_VIEWPORT_X, MAX_VIEWPORT_X), MIN_VIEWPORT_Y, BALLE_VX, BALLE_VY, TAILLE_BALLE, TAILLE_BALLE);
                        _timerReinit = null;
                    }
                }
            }
            else
            {
                DeplaceBalle(maintenant);

                if (_nbBriques == 0)
                {
                    _timerReinit = new TimerIsole(1000);
                    _ligneCourante = 0;
                    _balle = null;
                }
            }

            DeplaceBriquesTombantes(maintenant);
            DeplaceParticule(maintenant);
        }

        private void DeplaceBriquesTombantes(Temps maintenant)
        {
            // Deplacer les briques tombantes
            int i = 0;
            while (i < _briquesTombantes.Count)
            {
                BriqueTombante p = _briquesTombantes[i];
                if (p.Y > MIN_VIEWPORT_Y)
                {
                    p.X += p.Vx * maintenant.intervalleDepuisDerniereFrame;
                    p.Y += p.Vy * maintenant.intervalleDepuisDerniereFrame;

                    p.Vy += -1.0f * maintenant.intervalleDepuisDerniereFrame;
                    p.Rotation += p.Vr * maintenant.intervalleDepuisDerniereFrame;
                    i++;
                }
                else
                    _briquesTombantes.RemoveAt(i);
            }
        }

        private void DeplaceBalle(Temps maintenant)
        {
            if (_balle == null)
                return;

            // Deplacer la balle
            float dx = _balle.Vx * maintenant.intervalleDepuisDerniereFrame;
            float dy = _balle.Vy * maintenant.intervalleDepuisDerniereFrame;
            if (_balle.Droite + dx >= MAX_VIEWPORT_X)
                _balle.Vx = SigneMoins(_balle.Vx);
            else
                if (_balle.Gauche + dx <= MIN_VIEWPORT_X)
                _balle.Vx = SignePlus(_balle.Vx);

            if (_balle.Haut + dy >= MAX_VIEWPORT_Y)
                _balle.Vy = SigneMoins(_balle.Vy);
            else
                if (_balle.Bas + dy <= MIN_VIEWPORT_Y)
                _balle.Vy = SignePlus(_balle.Vy);

            // Collisions avec les briques
            foreach (Brique brique in _briques)
            {
                if (brique.Visible)
                    switch (_balle.Collision(brique))
                    {
                        case Balle.COLLISION.DROITE:
                        case Balle.COLLISION.GAUCHE:
                            _nbBriques--;
                            brique.Visible = false;
                            AjouteBriqueTombante(brique, _balle, maintenant);
                            AjouteParticules(brique);
                            _balle.Vx = -_balle.Vx;
                            break;

                        case Balle.COLLISION.HAUT:
                        case Balle.COLLISION.BAS:
                            _nbBriques--;
                            brique.Visible = false;
                            AjouteBriqueTombante(brique, _balle, maintenant);
                            AjouteParticules(brique);
                            _balle.Vy = -_balle.Vy;
                            break;
                    }
            }

            _balle.X += _balle.Vx;
            _balle.Y += _balle.Vy;

            //if (_balle.Droite > MAX_VIEWPORT_X + TAILLE_BALLE)
            //    _balle.X = MAX_VIEWPORT_X - TAILLE_BALLE;
            //
            //if (_balle.Gauche < MIN_VIEWPORT_X - TAILLE_BALLE)
            //    _balle.X = MIN_VIEWPORT_X + TAILLE_BALLE;
            //
            //if (_balle.Haut > MAX_VIEWPORT_Y + TAILLE_BALLE)
            //    _balle.Y = MAX_VIEWPORT_Y - TAILLE_BALLE;
            //if (_balle.Bas < MIN_VIEWPORT_Y - TAILLE_BALLE)
            //    _balle.Y = MIN_VIEWPORT_Y + TAILLE_BALLE;
            //
        }

        private void AjouteBriqueTombante(Brique brique, Balle balle, Temps maintenant)
        {
            BriqueTombante tombante = new BriqueTombante(brique, balle.Vx / (2.0f * maintenant.intervalleDepuisDerniereFrame), balle.Vy / (2.0f * maintenant.intervalleDepuisDerniereFrame), FloatRandom(300, 420) * SigneRandom());
            _briquesTombantes.Add(tombante);
        }

        private void DeplaceParticule(Temps maintenant)
        {
            // Deplacer les particules
            int i = 0;
            while (i < _particules.Count)
            {
                Particule p = _particules[i];
                if ((p.alpha > 0.001f) && (p.x > MIN_VIEWPORT_X) && (p.x < MAX_VIEWPORT_X) && (p.y > MIN_VIEWPORT_Y) && (p.y < MAX_VIEWPORT_Y))
                {
                    p.alpha -= (0.6f * maintenant.intervalleDepuisDerniereFrame);
                    p.x += p.vx * maintenant.intervalleDepuisDerniereFrame;
                    p.y += p.vy * maintenant.intervalleDepuisDerniereFrame;
                    i++;
                }
                else
                    _particules.RemoveAt(i);
            }
        }

        private void AjouteParticules(Brique brique)
        {
            if (_particules.Count > NB_MAX_PARTICULE)
                _particules.RemoveRange(0, NB_PARTICULE_EXPLOSION);

            for (int i = 0; i < NB_PARTICULE_EXPLOSION; i++)
            {
                Particule p = new Particule();
                p.x = FloatRandom(brique.Gauche, brique.Droite);
                p.y = FloatRandom(brique.Bas, brique.Haut);
                float angle = FloatRandom(0, DEUX_PI);
                float vitesse = FloatRandom(0.001f, 0.2f);
                p.vx = vitesse * (float)Math.Cos(angle);
                p.vy = vitesse * (float)Math.Sin(angle);
                p.Couleur = brique.Couleur;
                p.alpha = 1.0f;
                _particules.Add(p);
            }
        }

        public override string DumpRender()
        {
            return base.DumpRender() + " Nb briques:" + _nbBriques;
        }
    }
}
