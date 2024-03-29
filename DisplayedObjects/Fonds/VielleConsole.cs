﻿/*
 * Affiche du texte sur une console text a l'ancienne
 */
using ClockScreenSaverGL.Config;
using SharpGL;
using System;
using System.Drawing;
using System.IO;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    /// <summary>
    /// Description of Noir.
    /// </summary>
    public class VielleConsole : Fond
    {
        private const string NO_LIGNE = "Numero Ligne";
        private const string DEBUT_BALISE = "<<<";
        private const string FIN_BALISE = ">>>";
        public readonly char[] SEPARATOR = { ';' };

        #region Parametres
        public const string CAT = "VielleConsole";
        private CategorieConfiguration c;
        private int NB_LIGNES;
        private int NB_COLONNES;
        private int TAILLE_CHAR;
        private float rR;
        private float rG;
        private float rB;
        private int noLigne;
        #endregion

        sealed private class Caractere
        {
            public char caractere = ' ';
            public float rR = 1.0f, rG = 1.0f, rB = 1.0f;
            public bool clignotant = false;
            internal void Affecte(Caractere car)
            {
                caractere = car.caractere;
                rR = car.rR;
                rG = car.rG;
                rB = car.rB;
                clignotant = car.clignotant;
            }
        }

        private readonly OpenGLFonte fonte;
        private readonly Caractere[,] console;
        private int curseurX = 0;
        private int curseurY = 0;
        private readonly TimerIsole timer = new TimerIsole(1);
        private readonly TimerIsole timerCurseur = new TimerIsole(200);
        private bool clignotantEnCours = false;
        private readonly string script;
        private int posScript = 0;
        private readonly int TAILLE_DEBUT_BALISE = DEBUT_BALISE.Length;
        private readonly int TAILLE_FIN_BALISE = FIN_BALISE.Length;
        private bool clignotant = false;

        /// <summary>
		/// Constructeur
		/// </summary>
		/// <param name="gl"></param>
        public VielleConsole(OpenGL gl) : base(gl)
        {
            GetConfiguration();
            script = File.ReadAllText(Configuration.GetDataDirectory() + @"\script console.txt");
            fonte = new OpenGLFonte(gl, OpenGLFonte.CARACTERES, TAILLE_CHAR, FontFamily.GenericMonospace, FontStyle.Bold);
            console = new Caractere[NB_LIGNES, NB_COLONNES];
            for (int i = 0; i < NB_LIGNES; i++)
                for (int j = 0; j < NB_COLONNES; j++)
                {
                    console[i, j] = new Caractere();
                }

            posScript = chercheDebutLigne(noLigne);
        }

        private int chercheDebutLigne(int noLigne)
        {
            int ligneCourante = 0;
            int pos = 0;
            while ((pos < script.Length) && (ligneCourante < noLigne))
            {
                if (script[pos] == '\n')
                    ligneCourante++;
                pos++;
            }

            return pos;
        }

        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                NB_LIGNES = c.GetParametre("NB Lignes", 40);
                NB_COLONNES = c.GetParametre("NB Colonnes", 120);
                TAILLE_CHAR = c.GetParametre("Taille caracteres", 16);
                rR = c.GetParametre("R", 0.5f, a => { rR = (float)Convert.ToDouble(a); });
                rG = c.GetParametre("G", 0.5f, a => { rG = (float)Convert.ToDouble(a); });
                rB = c.GetParametre("B", 0.5f, a => { rB = (float)Convert.ToDouble(a); });
                noLigne = c.GetParametre(NO_LIGNE, 0);
            }
            return c;
        }

        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            gl.ClearColor(0, 0, 0, 1);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();

            gl.PushMatrix();
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.PushMatrix();
            gl.LoadIdentity();
            gl.Ortho2D(0, NB_COLONNES * TAILLE_CHAR, 0, NB_LIGNES * TAILLE_CHAR);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_DEPTH);
            gl.Disable(OpenGL.GL_DEPTH_TEST);
            gl.Disable(OpenGL.GL_ALPHA_TEST);
            gl.Disable(OpenGL.GL_BLEND);
            for (int ligne = 0; ligne < NB_LIGNES; ligne++)
            {
                for (int colonne = 0; colonne < NB_COLONNES; colonne++)
                    if (console[ligne, colonne].caractere != ' ')
                        if (clignotantEnCours || (!console[ligne, colonne].clignotant))
                        {
                            fonte.drawOpenGL(gl, console[ligne, colonne].caractere, colonne * TAILLE_CHAR, (NB_LIGNES - (ligne + 1)) * TAILLE_CHAR - (TAILLE_CHAR / 2),
                                console[ligne, colonne].rR * couleur.R / 256.0f,
                                console[ligne, colonne].rG * couleur.G / 256.0f,
                                console[ligne, colonne].rB * couleur.B / 256.0f);
                        }
            }

            if (clignotantEnCours)
                fonte.drawOpenGL(gl, "_", curseurX * TAILLE_CHAR, (NB_LIGNES - (curseurY + 1)) * TAILLE_CHAR - (TAILLE_CHAR / 2), couleur);


            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.PopMatrix();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.PopMatrix();

            LookArcade(gl, couleur);
#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {

#if TRACER
            RenderStart(CHRONO_TYPE.DEPLACE);
#endif
            base.Deplace(maintenant, tailleEcran);
            if (timer.Ecoule())
            {
                // Ajoute un caractere
                if (posScript >= script.Length)
                {
                    posScript = 0;
                    noLigne = 0;
                    c.SetParametre(NO_LIGNE, noLigne);
                }

                TraiteBalise();

                Ajoute(script[posScript]);
                posScript++;
            }

            if (timerCurseur.Ecoule())
                clignotantEnCours = !clignotantEnCours;
#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif
        }

        private void TraiteBalise()
        {
            if (posScript > script.Length - TAILLE_DEBUT_BALISE + TAILLE_FIN_BALISE)
                return;

            if (script.Substring(posScript).StartsWith(DEBUT_BALISE))
            {
                int indiceFin = script.IndexOf(FIN_BALISE, posScript + TAILLE_DEBUT_BALISE);
                if (indiceFin != -1)
                {

                    string code = script.Substring(posScript + TAILLE_DEBUT_BALISE, indiceFin - (posScript + TAILLE_DEBUT_BALISE));
                    posScript = indiceFin + TAILLE_FIN_BALISE;
                    string[] morceaux = code.Split(SEPARATOR);
                    if (morceaux?.Length > 0)
                    {
                        switch (morceaux[0].ToUpper())
                        {
                            case "COLOR":
                                if (morceaux.Length > 3)
                                {
                                    rR = (float)Double.Parse(morceaux[1]);
                                    rG = (float)Double.Parse(morceaux[2]);
                                    rB = (float)Double.Parse(morceaux[3]);
                                }
                                break;

                            case "BLINK":
                                {
                                    if (morceaux.Length > 1)
                                        switch (morceaux[1].ToUpper())
                                        {
                                            case "OUI":
                                            case "YES":
                                            case "ON":
                                                clignotant = true;
                                                break;

                                            default:
                                                clignotant = false;
                                                break;
                                        }
                                }
                                break;
                        }
                    }
                }
            }
        }

        private void Scroll()
        {
            // Scroller
            for (int i = 0; i < NB_LIGNES - 1; i++)
                for (int j = 0; j < NB_COLONNES; j++)
                    console[i, j].Affecte(console[i + 1, j]);

            for (int i = 0; i < NB_COLONNES; i++)
                console[NB_LIGNES - 1, i].caractere = ' ';
        }
        private void Ajoute(char v)
        {
            switch (v)
            {
                case '\n':
                    curseurX = 0;
                    noLigne++;
                    c.SetParametre(NO_LIGNE, noLigne);
                    c.Flush();

                    if (curseurY < NB_LIGNES - 1)
                        curseurY++;
                    else
                        Scroll();
                    break;

                default:

                    if (curseurX < NB_COLONNES - 1)
                        curseurX++;
                    else
                    {
                        curseurX = 0;
                        if (curseurY < NB_LIGNES - 1)
                            curseurY++;
                        else
                            Scroll();
                    }

                    console[curseurY, curseurX].caractere = v;
                    console[curseurY, curseurX].rR = rR;
                    console[curseurY, curseurX].rG = rG;
                    console[curseurY, curseurX].rB = rB;
                    console[curseurY, curseurX].clignotant = clignotant;
                    break;
            }
        }

    }
}
