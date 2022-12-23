using SharpGL;
using System.Diagnostics;
using System.Drawing;
using T_CASE = System.Int16;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    public class PieceTetris
    {

        public const int NB_PIECES = 7;
        public const float CHANGE_COULEUR = 0.75f / NB_PIECES;

        #region Pieces
        // Les differentes formes de pieces Tetris:
        // Piece O, les formes avec rotation sont les memes
        private const float COULEUR_CHANGE_O = CHANGE_COULEUR * 0.0f;
        private const float COULEUR_CHANGE_S = CHANGE_COULEUR * 1.0f;
        private const float COULEUR_CHANGE_L = CHANGE_COULEUR * 2.0f;
        private const float COULEUR_CHANGE_I = CHANGE_COULEUR * 3.0f;
        private const float COULEUR_CHANGE_Z = CHANGE_COULEUR * 4.0f;
        private const float COULEUR_CHANGE_J = CHANGE_COULEUR * 5.0f;
        private const float COULEUR_CHANGE_T = CHANGE_COULEUR * 6.0f;

        private static readonly T_CASE[,] O =
        {
            { 1, 1 } ,
            { 1, 1 }

        };

        // Piece I
        private static readonly T_CASE[,] I_1 =
        {
                { 1 },
                { 1 },
                { 1 },
                { 1 }
        };

        static private readonly T_CASE[,] I_2 =
        {
            { 1, 1, 1, 1 }
        };

        // Piece S
        static private readonly T_CASE[,] S_1 =
        {
            { 0,1,1 },
            { 1,1,0 }
        };
        static private readonly T_CASE[,] S_2 =
        {
            { 1,0 },
            { 1,1 },
            { 0,1 }
        };

        // Piece Z
        static private readonly T_CASE[,] Z_1 =
        {
            { 1,1,0 },
            { 0,1,1 }
        };

        static private readonly T_CASE[,] Z_2 =
        {
            { 0,1 },
            { 1,1 },
            { 1,0 }
        };

        // Piece L
        static private readonly T_CASE[,] L_1 =
        {
            {1,0 },
            {1,0 },
            {1,1 }
        };

        static private readonly T_CASE[,] L_2 =
        {
            {1,1,1 },
            {1,0,0 }
        };
        static private readonly T_CASE[,] L_3 =
        {
            {1,1 },
            {0,1 },
            {0,1 }
        };
        static private readonly T_CASE[,] L_4 =
        {
            {0,0,1 },
            {1,1,1 }
        };

        // Piece J 
        static private readonly T_CASE[,] J_1 =
            {
            {0,1 },
            {0,1 },
            {1,1 }
            };
        static private readonly T_CASE[,] J_2 =
        {
            {1,0,0 },
            {1,1,1 }
        };

        static private readonly T_CASE[,] J_3 =
        {
            {1,1 },
            {1,0 },
            {1,0 }
        };
        static private readonly T_CASE[,] J_4 =
        {
            {1,1,1 },
            {0,0,1 }
        };

        // Piece T
        static private readonly T_CASE[,] T_1 =
        {
            {1,1,1},
            {0,1,0}
        };

        static private readonly T_CASE[,] T_2 =
        {
            {0,1 },
            {1,1 },
            {0,1 }
        };
        static private readonly T_CASE[,] T_3 =
        {
            {0,1,0},
            {1,1,1}
        };

        static private readonly T_CASE[,] T_4 =
        {
            {1,0 },
            {1,1 },
            {1,0 }
        };

        /// <summary>
        /// Retourne le contenu de la case
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal T_CASE Case(int x, int y)
        {
            return _cases[_rotation][y, x];
        }
        #endregion

        public int CaseX;
        public int CaseY;
        public int Rotation { get => _rotation; set => _rotation = value % 4; }

        public int NbColonnes
        {
            get => _cases[_rotation].GetLength(1);
        }
        public int NbLignes
        {
            get => _cases[_rotation].GetLength(0);
        }

        public float ChangeCouleur
        {
            get => _couleurChange;
        }

        public void Dump()
        {
            int rMem = _rotation;
            Debug.WriteLine("======================================================");
            for (_rotation = 0; _rotation < 4; _rotation++)
            {
                Debug.WriteLine("NbLignes: " + NbLignes);
                Debug.WriteLine("NbColonnes:" + NbColonnes);

                for (T_CASE l = 0; l < NbLignes; l++)
                {
                    for (T_CASE c = 0; c < NbColonnes; c++)
                    {
                        Debug.Write(Case(c, l) == 0 ? "." : "O");
                    }
                    Debug.WriteLine("");
                }
            }

            _rotation = rMem;
        }

        /// <summary>
        /// Creer et retourne une piece
        /// </summary>
        /// <param name="i">Numero de la piece, de 0 à NB_PIECES</param>
        /// <returns></returns>
        public static PieceTetris CreerPiece(T_CASE i)
        {
            switch (i)
            {
                case 0: return new PieceTetris(O, O, O, O, COULEUR_CHANGE_O);
                case 1: return new PieceTetris(I_1, I_2, I_1, I_2, COULEUR_CHANGE_I); 
                case 2: return new PieceTetris(S_1, S_2, S_1, S_2, COULEUR_CHANGE_S);
                case 3: return new PieceTetris(Z_1, Z_2, Z_1, Z_2, COULEUR_CHANGE_Z);
                case 4: return new PieceTetris(L_1, L_2, L_3, L_4, COULEUR_CHANGE_L);
                case 5: return new PieceTetris(J_1, J_2, J_3, J_4, COULEUR_CHANGE_J);
                default: return new PieceTetris(T_1, T_2, T_3, T_4, COULEUR_CHANGE_T);
            }
        }

        /// <summary>
        /// Clone la piece
        /// </summary>
        /// <returns></returns>
        public PieceTetris Clone()
        {
            return new PieceTetris(this);
        }

        public void TourneADroite()
        {
            _rotation++;
            if (_rotation >= _cases.Length)
                _rotation = 0;
        }
        public void TourneAGauche()
        {
            _rotation--;
            if (_rotation < 0)
                _rotation = _cases.Length - 1;
        }

        private readonly T_CASE[][,] _cases;
        private int _rotation;
        private readonly float _couleurChange;

        private PieceTetris(PieceTetris p)
        {
            _cases = p._cases;
            CaseX = p.CaseX;
            CaseY = p.CaseY;
            _rotation = p._rotation;
        }
        private PieceTetris(T_CASE[,] T1, T_CASE[,] T2, T_CASE[,] T3, T_CASE[,] T4, float couleurChange)
        {
            _cases = new T_CASE[4][,];
            _cases[0] = T1;
            _cases[1] = T2;
            _cases[2] = T3;
            _cases[3] = T4;

            _couleurChange = couleurChange;
            _rotation = 2;
        }

        /// <summary>
        /// Dessine la piece
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="origineX"></param>
        /// <param name="origineY"></param>
        /// <param name="largeurCase"></param>
        /// <param name="hauteurCase"></param>
        /// <param name="couleur"></param>
        public void Affiche(OpenGL gl, float origineX, float origineY, float largeurCase, float hauteurCase, Color couleur)
        {
            float X = origineX + (CaseX * largeurCase);
            float Y = origineY + (CaseY * hauteurCase);

            gl.PushMatrix();
            gl.Translate(X, Y, 0);
            Fond.SetColorWithHueChange(gl, couleur, _couleurChange);
            gl.Begin(OpenGL.GL_QUADS);
            for (T_CASE i = 0; i < _cases[_rotation].GetLength(0); i++)
                for (T_CASE j = 0; j < _cases[_rotation].GetLength(1); j++)
                    if (_cases[_rotation][i, j] > 0)
                    {
                        gl.TexCoord(0.0f, 0.0f); gl.Vertex(j * largeurCase, i * hauteurCase);
                        gl.TexCoord(0.0f, 1.0f); gl.Vertex((j + 1) * largeurCase, i * hauteurCase);
                        gl.TexCoord(1.0f, 1.0f); gl.Vertex((j + 1) * largeurCase, (i + 1) * hauteurCase);
                        gl.TexCoord(1.0f, 0.0f); gl.Vertex(j * largeurCase, (i + 1) * hauteurCase);
                    }
            gl.End();
            gl.PopMatrix();
        }
    }
}
