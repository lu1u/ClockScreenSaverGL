using ClockScreenSaverGL.Config;
using SharpGL;
using System.Diagnostics;
using System.Drawing;
using T_CASE = System.Int16;
namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    /// <summary>
    /// Representation du plateau du jeu Tetris
    /// </summary>
    internal class Plateau
    {
        public const T_CASE CASE_VIDE = 0;

        public static float COEFF_LIGNES_VIDES = 10.0f; // Maximiser le nombre de lignes vides en haut du jeu
        public static float COEFF_TROUS = -50.0f;       // Minimiser le nombre de trous
        public static float COEFF_LIGNE_HAUTE = -20.0f;       // Privilegier la depose sur une ligne basse (indice élevé)

        /// <summary>
        /// Classe pour representer une case
        /// </summary>
        private class Case
        {
            public T_CASE contenu;
            public float changeCouleur;
            public Case()
            {
                contenu = CASE_VIDE;
            }

            public Case Clone()
            {
                Case clone = new Case();
                clone.contenu = contenu;
                return clone;
            }
        }

        private int _nbLignes, _nbColonnes;
        private Case[,] _cases;

        /// <summary>
        /// Constructeur public
        /// </summary>
        /// <param name="nbLignes"></param>
        /// <param name="nbColonnes"></param>
        public Plateau(int nbLignes, int nbColonnes)
        {
            _nbLignes = nbLignes;
            _nbColonnes = nbColonnes;
            _cases = new Case[_nbLignes, _nbColonnes];
            for (int i = 0; i < _nbLignes; i++)
                for (int j = 0; j < _nbColonnes; j++)
                    _cases[i, j] = new Case();
        }

        /// <summary>
        /// Constructeur de clonage
        /// </summary>
        /// <param name="p"></param>
        private Plateau(Plateau p)
        {
            _nbLignes = p._nbLignes;
            _nbColonnes = p._nbColonnes;
            _cases = new Case[_nbLignes, _nbColonnes];
            for (int i = 0; i < _nbLignes; i++)
                for (int j = 0; j < _nbColonnes; j++)
                    _cases[i, j] = p._cases[i, j].Clone();
        }

        /// <summary>
        /// Afficher le plateau de jeu
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="largeurCase"></param>
        /// <param name="hauteurCase"></param>
        /// <param name="couleur"></param>
        /// <param name="clignoterCompletes"></param>
        public void affiche(OpenGL gl, float X, float Y, float largeurCase, float hauteurCase, Color couleur, bool clignoterCompletes)
        {
            gl.PushMatrix();
            gl.Translate(X, Y, 0);
            gl.Begin(OpenGL.GL_QUADS);
            for (int ligne = 0; ligne < _nbLignes; ligne++)
            {
                bool afficherSurbrillance = clignoterCompletes ? LigneComplete(ligne) : false;
                for (int colonne = 0; colonne < _nbColonnes; colonne++)
                    if (_cases[ligne, colonne].contenu != CASE_VIDE)
                    {
                        Color Couleur = afficherSurbrillance ? Color.White : Fond.getColorWithHueChange(couleur, _cases[ligne, colonne].changeCouleur);
                        gl.Color(Couleur.R / 256.0f, Couleur.G / 256.0f, Couleur.B / 256.0f, 1.0f);

                        gl.TexCoord(0.0f, 0.0f); gl.Vertex(colonne * largeurCase, ligne * hauteurCase);
                        gl.TexCoord(0.0f, 1.0f); gl.Vertex((colonne + 1) * largeurCase, ligne * hauteurCase);
                        gl.TexCoord(1.0f, 1.0f); gl.Vertex((colonne + 1) * largeurCase, (ligne + 1) * hauteurCase);
                        gl.TexCoord(1.0f, 0.0f); gl.Vertex(colonne * largeurCase, (ligne + 1) * hauteurCase);
                    }
            }
            gl.End();
            gl.PopMatrix();
        }

        /// <summary>
        /// Retourne VRAI si la place est disponible sur le plateau pour la piece
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="caseX"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        internal bool Disponible(PieceTetris piece, int PieceX, int PieceY)
        {
            int LargeurPiece = piece.NbColonnes;
            int HauteurPiece = piece.NbLignes;

            // Arrivé en bas?
            if (PieceY + HauteurPiece > _nbLignes)
                return false;

            // Toutes les cases de la piece ont une place libre sur le plateau
            int MaxX = Min(_nbColonnes, PieceX + LargeurPiece);
            int MaxY = Min(_nbLignes, PieceY + HauteurPiece);
            for (int x = PieceX; x < MaxX; x++)
                for (int y = PieceY; y < MaxY; y++)
                    if (piece.Case(x - PieceX, y - PieceY) > 0) // Case occupee sur la piece
                        if (_cases[y, x].contenu != CASE_VIDE)      // Case occupee sur le plateau
                            return false;

            return true;
        }

        /// <summary>
        /// Retourne VRAI si la place est disponible sur le plateau pour la piece
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="caseX"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        internal bool Disponible(PieceTetris piece)
        {
            int LargeurPiece = piece.NbColonnes;
            int HauteurPiece = piece.NbLignes;
            int PieceX = piece.CaseX;
            int PieceY = piece.CaseY;

            // Arrivé en bas?
            if (PieceY + HauteurPiece > _nbLignes)
                return false;

            // Toutes les cases de la piece ont une place libre sur le plateau
            int MaxX = Min(_nbColonnes, PieceX + LargeurPiece);
            int MaxY = Min(_nbLignes, PieceY + HauteurPiece);
            for (int x = PieceX; x < MaxX; x++)
                for (int y = PieceY; y < MaxY; y++)
                    if (piece.Case(x - PieceX, y - PieceY) > 0) // Case occupee sur la piece
                        if (_cases[y, x].contenu != CASE_VIDE)      // Case occupee sur le plateau
                            return false;

            return true;
        }

        /// <summary>
        /// Depose une piece sur le plateau: chaque case occupee de la piece est
        /// copiée sur le tableau
        /// </summary>
        /// <param name="piece"></param>
        internal void DeposePiece(PieceTetris piece)
        {
            Log.instance.verbose("Tetris: depose piece");
            Log.instance.verbose("Piece X,Y " + piece.CaseX + " x " + piece.CaseY);

            int LargeurPiece = piece.NbColonnes;
            int HauteurPiece = piece.NbLignes;
            int PieceX = piece.CaseX;
            int PieceY = piece.CaseY;
            float changeCouleur = piece.ChangeCouleur;

            int MaxX = Min(_nbColonnes, PieceX + LargeurPiece);
            int MaxY = Min(_nbLignes, PieceY + HauteurPiece);

            for (int ligne = PieceY; ligne < MaxY; ligne++)
            {
                Log.instance.verbose("Depose ligne " + ligne);
                for (int colonne = PieceX; colonne < MaxX; colonne++)
                {
                    T_CASE casePiece = piece.Case(colonne - PieceX, ligne - PieceY);
                    if (casePiece != CASE_VIDE)
                        if (_cases[ligne, colonne].contenu == CASE_VIDE)
                        {
                            _cases[ligne, colonne].contenu = casePiece;
                            _cases[ligne, colonne].changeCouleur = changeCouleur;
                        }
                }
            }
        }

        private int Min(int a, int b) => a > b ? b : a;

        /// <summary>
        /// Calcule la colonne optimale pour faire tomber la piece
        /// </summary>
        /// <param name="piece"></param>
        public void CalculeCible(PieceTetris piece, out int colonneCible, out int rotationCible)
        {
            //using (new Chronometre("Plateau Tetris: evaluation"))
            //{
            //    Debug.WriteLine("*************************** CalculeCible **************************************************");
            PieceTetris clonePiece = piece.Clone();
            float score = float.MinValue;
            colonneCible = 0;
            rotationCible = 0;

            for (int rotation = 0; rotation < 4; rotation++)     // Tester toutes les rotations
            {
                clonePiece.Rotation = rotation;
                int largeurPiece = clonePiece.NbColonnes;

                for (int colonne = 0; colonne <= (_nbColonnes - largeurPiece); colonne++) // Tester toutes les colonnes
                {
                    clonePiece.CaseX = colonne;
                    Plateau plateauClone = Clone();
                    int niveau = plateauClone.DeposeAuPlusBas(clonePiece);
                    float scoreColonne = plateauClone.EvaluePlateau() + (COEFF_LIGNE_HAUTE * niveau);

                    //Debug.WriteLine("========================================");
                    //plateauClone.Dump();
                    //Debug.WriteLine("Score: " + scoreColonne);
                    //Debug.WriteLine("========================================");

                    if (scoreColonne > score)
                    {
                        score = scoreColonne;
                        colonneCible = colonne;
                        rotationCible = rotation;
                    }
                }
            }
            //}
        }

        internal void Vide()
        {
            for (int ligne = 0; ligne < _nbLignes; ligne++)
                for (int colonne = 0; colonne < _nbColonnes; colonne++)
                    _cases[ligne, colonne].contenu = CASE_VIDE;
        }

        /// <summary>
        /// Evaluer le score du plateau pour choisir la meilleure strategie,
        /// plus le score est haut, mieux c'est
        /// </summary>
        /// <returns>Scolre</returns>
        private float EvaluePlateau()
        {
            VideLignesCompletes();

            // Garder le maximum de lignes vides en haut
            int ligneVideMax = 0;
            while ((ligneVideMax < _nbLignes) && LigneVide(ligneVideMax))
                ligneVideMax++;

            // Limiter le nombre de trous
            int nbTrous = 0;
            for (int ligne = ligneVideMax + 1; ligne < _nbLignes; ligne++)
                for (int colonne = 0; colonne < _nbColonnes; colonne++)
                    if (_cases[ligne, colonne].contenu == CASE_VIDE)
                        if (_cases[ligne - 1, colonne].contenu != CASE_VIDE)
                            nbTrous++;

            return (ligneVideMax * COEFF_LIGNES_VIDES) + (nbTrous * COEFF_TROUS);
        }

        /// <summary>
        /// Place la piece le plus bas possible sur le plateau, sans changer sa colonne
        /// </summary>
        /// <param name="piece"></param>
        /// <returns>Numero de la ligne ou la piece a été posée</returns>
        private int DeposeAuPlusBas(PieceTetris piece)
        {
            for (int i = 0; i < _nbLignes - 1; i++)
            {
                piece.CaseY = i + 1;
                if (!Disponible(piece))
                {
                    // La ligne suivante serait non disponible: deposer la piece ici
                    piece.CaseY = i;
                    DeposePiece(piece);
                    return i;
                }
            }
            return 0;
        }

        /// <summary>
        /// Faire une copie du plateau
        /// </summary>
        /// <returns></returns>
        private Plateau Clone()
        {
            return new Plateau(this);
        }

        /// <summary>
        /// Vider toutes les lignes complètes
        /// </summary>
        internal void VideLignesCompletes()
        {
            for (int ligne = 0; ligne < _nbLignes; ligne++)
                if (LigneComplete(ligne))
                    VideLigne(ligne);
        }

        /// <summary>
        /// Supprime la ligne en faisant tomber les lignes du dessus
        /// </summary>
        /// <param name="i"></param>
        private void VideLigne(int i)
        {
            // Decaler les lignes vers le bas
            for (int ligne = i; ligne > 0; ligne--)
                for (int colonne = 0; colonne < _nbColonnes; colonne++)
                    _cases[ligne, colonne] = _cases[ligne - 1, colonne];

            // Vider la ligne du haut
            for (int colonne = 0; colonne < _nbColonnes; colonne++)
                _cases[0, colonne].contenu = 0;
        }

        /// <summary>
        /// Retourne true si une des lignes est complète
        /// </summary>
        /// <returns></returns>
        internal bool LignesCompletes()
        {
            for (int i = 0; i < _nbLignes; i++)
                if (LigneComplete(i))
                    return true;

            return false;
        }

        /// <summary>
        /// Retourne TRUE si toutes les cases de la ligne sont occupées
        /// </summary>
        /// <param name="ligne">Indice de la ligne</param>
        /// <returns>TRUE si ligne complete</returns>
        public bool LigneComplete(int ligne)
        {
            for (int i = 0; i < _nbColonnes; i++)
                if (_cases[ligne, i].contenu == CASE_VIDE)
                    return false;

            return true;
        }

        /// <summary>
        /// Retourne TRUE si toutes les cases de la ligne sont vides
        /// </summary>
        /// <param name="ligne">Indice de la ligne</param>
        /// <returns>TRUE si ligne vide</returns>
        public bool LigneVide(int ligne)
        {
            for (int i = 0; i < _nbColonnes; i++)
                if (_cases[ligne, i].contenu != CASE_VIDE)
                    return false;

            return true;
        }


        public void Dump()
        {
            for (int l = 0; l < _nbLignes; l++)
            {
                for (int c = 0; c < _nbColonnes; c++)
                    Debug.Write(_cases[l, c].contenu == 0 ? "." : "#");

                Debug.WriteLine("");
            }
        }
    }
}
