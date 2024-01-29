
using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects;
using ClockScreenSaverGL.DisplayedObjects.Fonds;
using ClockScreenSaverGL.DisplayedObjects.PanneauActualites;
using ClockScreenSaverGL.DisplayedObjects.Textes;
using SharpGL;
/*
 * Crée par SharpDevelop.
 * Utilisateur: lucien
 * Date: 23/01/2015
 * Heure: 17:09
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ClockScreenSaverGL
{

    /// <summary>
    /// Description of MainForm.
    /// </summary>
    public partial class MainForm : Form
    {

        #region Parametres
        public const string CAT = "Main";
        static protected CategorieConfiguration c = Configuration.GetCategorie(CAT);
        private const string PARAM_DELAI_CHANGE_FOND = "DelaiChangeFondMinutes";
        private const string PARAM_FONDDESAISON = "FondDeSaison";
        private const string PARAM_TYPEFOND = "TypeFond";

        #endregion

        private readonly CouleurGlobale _couleur = new CouleurGlobale(c.GetParametre("Couleur Globale Hue", 0.5f), c.GetParametre("Couleur Globale Saturation", 0.5f), c.GetParametre("Couleur Globale Luminance", 0.5f));        // La couleur de base pour tous les affichages
        private readonly List<DisplayedObject> _listeObjets = new List<DisplayedObject>();
        private int _jourActuel = -1;                          // Pour forcer un changement de date avant la premiere image

        private bool _fondDeSaison;                           // Vrai si on doit commencer par le fond 'de saison'
        private DateTime _derniereFrame = DateTime.Now;                // Heure de la derniere frame affichee
        private Temps _temps;
        private bool wireframe = false;
        private int INDICE_FOND;
        private int INDICE_TRANSITION;

#if TRACER
        private bool _afficheDebug = c.GetParametre("Debug", true);
        private DateTime lastFrame = DateTime.Now;
        private PanneauMessage _panneau;
        //private readonly Process currentProc = Process.GetCurrentProcess();
#endif
        #region Preview API's

        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out Rectangle lpRect);

        #endregion

        #region Screensaver
        private readonly bool IsPreviewMode = false;
        public MainForm()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            try
            {
                InitializeComponent();

                _temps = new Temps(DateTime.Now, _derniereFrame);
            }
            catch (Exception e)
            {
                _panneau.SetMessage(openGLControl.OpenGL, "Exception:" + e.ToString());
                MessageBox.Show(e.Message + "\ndans : " + e.Source + "\n" + e.StackTrace, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        //This constructor is the handle to the select screensaver dialog preview window
        //It is used when in preview mode (/p)
        public MainForm(IntPtr PreviewHandle)
        {
            try
            {
                InitializeComponent();

                //set the preview window as the parent of this window
                SetParent(this.Handle, PreviewHandle);

                //make this a child window, so when the select screensaver dialog closes, this will also close
                SetWindowLong(this.Handle, -16, new IntPtr(GetWindowLong(this.Handle, -16) | 0x40000000));

                //set our window's size to the size of our window's new parent
                GetClientRect(PreviewHandle, out Rectangle ParentRect);
                this.Size = ParentRect.Size;

                //set our location at (0, 0)
                this.Location = new Point(0, 0);

                IsPreviewMode = true;
            }
            catch (Exception e)
            {
                _panneau.SetMessage(openGLControl.OpenGL, "Exception:" + e.ToString());
                MessageBox.Show(e.Message + "\ndans : " + e.Source, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        #endregion

        /// <summary>
        /// Creer l'objet qui anime le fond d'ecran
        /// </summary>
        /// <returns></returns>
        private Fond CreateBackgroundObject(DisplayedObjectFactory.FONDS type, bool initial)
        {
            OpenGL gl = openGLControl.OpenGL;
            if (!initial)
                gl.PopAttrib();

            //gl.PushAttrib(OpenGL.GL_ENABLE_BIT | OpenGL.GL_FOG_BIT | OpenGL.GL_LIGHTING_BIT);
            Fond ret = DisplayedObjectFactory.GetObjetFond(gl, type, initial, _fondDeSaison);
            ret.Initialisation(gl);
            return ret;
        }






        /// <summary>
        /// Chargement de la fenetre et de ses composants
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoad(object sender, System.EventArgs e)
        {
            try
            {
                UpdateStyles();
                Cursor.Hide();

                timerChangeFond.Interval = c.GetParametre(PARAM_DELAI_CHANGE_FOND, 3) * 60 * 1000;
                timerChangeFond.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\ndans : " + ex.Source, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }


#if TRACER
        /// <summary>
        /// Affichage des informations de debug et performance
        /// </summary>
        /// <param name="g"></param>
        private void RemplitDebug(OpenGL gl)
        {
            double NbMillisec = _temps.temps.Subtract(lastFrame).TotalMilliseconds;
            DisplayedObjects.Console console = DisplayedObjects.Console.GetInstance(gl);
            console.Clear();
#if DEBUG
            console.AddLigne(Color.White, "Version DEBUG ");
#else
            console.AddLigne(Color.White, "Version RELEASE ");
#endif

            console.AddLigne(Color.White, Assembly.GetExecutingAssembly().GetName().Version.ToString());

            try
            {
                DisplayedObjectFactory.FONDS Type = (DisplayedObjectFactory.FONDS)c.GetParametre(PARAM_TYPEFOND, 0);
                console.AddLigne(Color.White, (1000.0 / NbMillisec).ToString("0.0") + " FPS\n\n");
                console.AddLigne(Color.White, "Couleur: " + _couleur.ToString() + "\n\n");
                console.AddLigne(Color.White, "Fond courant: " + _listeObjets[INDICE_FOND].GetType().Name + ":" + (int)Type + "/" + (int)DisplayedObjectFactory.DERNIER_FOND + "\n");
                //console.AddLigne(Color.White, "CPU " + cpuCounter.NextValue().ToString("00") + "%\n");
                //console.AddLigne(Color.White, "Free RAM " + (ramCounter.NextValue() / 1024).ToString("0.00") + "GB\n");
                // console.AddLigne(Color.White, "Utilisation mémoire " + ((currentProc.PrivateMemorySize64 / 1024.0) / 1024.0).ToString("0.0") + "MB\n\n");
                //console.AddLigne(Color.White, "Memory usage " + ((currentProc.PrivateMemorySize64 / 1024.0) / 1024.0).ToString("0.0") + "MB\n\n");
            }
            catch (Exception e)
            {
                _panneau.SetMessage(openGLControl.OpenGL, "Exception:" + e.ToString());
            }

            foreach (DisplayedObject b in _listeObjets)
                console.AddLigne(Color.White, b.DumpRender());

            //g.DrawString(s.ToString(), SystemFonts.DefaultFont, Brushes.LightGray, 0, 10);
            lastFrame = _temps.temps;
        }

#endif
        /// <summary>
        /// Deplacer tous les objets
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeplaceTous()
        {
            Log log = Log.Instance;
            try
            {
                _couleur.AvanceCouleur();
                Rectangle bnd = Bounds;
                foreach (DisplayedObject b in _listeObjets)
                    b.Deplace(_temps, bnd);

                if (_jourActuel != _temps.jourDeLAnnee)
                {
                    // Detection de changement de date, avertir les objets qui sont optimises pour ne changer
                    // qu'une fois par jour

                    OpenGL gl = openGLControl.OpenGL;
                    foreach (DisplayedObject b in _listeObjets)
                        b.DateChangee(gl, _temps);

                    _jourActuel = _temps.jourDeLAnnee;
                }

                _derniereFrame = _temps.temps;
            }
            catch (Exception e)
            {
                _panneau.SetMessage(openGLControl.OpenGL, "Exception:" + e.ToString());
                log.Error("Exception dans MainForm.deplaceTous");
                log.Error(e.Message);
                log.Error(e.StackTrace);
            }
        }

        private void OnOpenGLDraw(object sender, SharpGL.RenderEventArgs args)
        {
            Log log = Log.Instance;
            try
            {
                _temps = new Temps(DateTime.Now, _derniereFrame);

                if (!_initTermine)
                {
                    Debug.WriteLine("Init pas terminé");
                    return;
                }

                DeplaceTous();

                //if (_frameInitiale)
                //{
                //    Debug.WriteLine("Frame Initiale");
                //    _frameInitiale = false;
                //    _derniereFrame = DateTime.Now;
                //    return;
                //}


                OpenGL gl = openGLControl.OpenGL;

#if TRACER
                if (_afficheDebug)
                    RemplitDebug(gl);
#endif
                Color Couleur = _couleur.GetARGB();

                gl.MatrixMode(OpenGL.GL_PROJECTION);                        // Select The Projection Matrix
                gl.LoadIdentity();                                   // Reset The Projection Matrix
                gl.Perspective(60, Bounds.Width / (float)Bounds.Height, .1f, 1000f);
                gl.MatrixMode(OpenGL.GL_MODELVIEW);                         // Select The Modelview Matrix
                gl.LoadIdentity();

                // Deplacer et Afficher tous les objets
                if (_effacerFond)
                    foreach (DisplayedObject b in _listeObjets)
                        if (b.ClearBackGround(gl, Couleur))
                            break;

                if (wireframe)
                {
                    gl.LineWidth(1);
                    gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_LINE);
                }

                // Deplacer et Afficher tous les objets
                foreach (DisplayedObject b in _listeObjets)
                {
                    gl.PushMatrix();
                    gl.PushAttrib(OpenGL.GL_ENABLE_BIT | OpenGL.GL_CURRENT_BIT | OpenGL.GL_DEPTH_BUFFER_BIT | OpenGL.GL_FOG_BIT | OpenGL.GL_COLOR_BUFFER_BIT);
                    b.AfficheOpenGL(gl, _temps, Bounds, Couleur);
                    gl.PopAttrib();
                    gl.PopMatrix();
                }

                if (wireframe)
                    gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);

                if (_afficheDebug)
                {
                    foreach (DisplayedObject b in _listeObjets)
                        if (b is Fond fond)
                            fond.FillConsole(gl);

                    DisplayedObjects.Console console = DisplayedObjects.Console.GetInstance(gl);
                    if (_texteErreur != null)
                        console.AddLigne(Color.Red, _texteErreur);

                    console.Trace(gl, Bounds);
                    console.Clear();
                }

                _panneau.AfficheOpenGL(gl, Couleur);
            }
            catch (Exception e)
            {
                // _panneau.SetMessage(openGLControl.OpenGL, "Exception:" + e.ToString());
                _texteErreur = e.ToString();
                _afficheDebug = true;
                log.Error("Exception dans MainForm.createAllObjects");
                log.Error(e.Message);
                log.Error(e.StackTrace);
            }
        }

        /// <summary>
        /// OpenGL est initialise
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOpenGLInitialized(object sender, System.EventArgs e)
        {
            Debug.WriteLine("onOpenGLInitialized");
            _initTermine = false;

            OpenGL gl = openGLControl.OpenGL;
            gl.Clear(0);
            CreateAllObjects();
            _initTermine = true;
        }


        /// <summary>
        /// Creer tous les objets qui seront affiches
        /// </summary>
        private void CreateAllObjects()
        {
            Log log = Log.Instance;
            log.Verbose(">>> Initialisation");
            try
            {
                OpenGL gl = openGLControl.OpenGL;
                //int TailleHorloge = c.getParametre("TailleCadran", 400);
                //if (IsPreviewMode)
                //{
                //    TailleHorloge = 10;
                //    //_listeObjets.Add(new HorlogeRonde(gl, TailleHorloge, CentreX - TailleHorloge / 2, CentreY - TailleHorloge / 2));
                //    return;
                //}

                _fondDeSaison = c.GetParametre(PARAM_FONDDESAISON, true);
                // Ajout de tous les objets graphiques, en finissant par celui qui sera affiche en dessus des autres
                INDICE_FOND = 0;
                _listeObjets.Add(CreateBackgroundObject((DisplayedObjectFactory.FONDS)c.GetParametre(PARAM_TYPEFOND, 0), true));
                
                INDICE_TRANSITION = 1;
                _listeObjets.Add(new Transition(gl));

                // Copyright
                if (c.GetParametre("Copyright", true))
                    _listeObjets.Add(new TexteCopyright(gl, -4, 100));
                // citations
                if (c.GetParametre("Citation", true))
                    _listeObjets.Add(new Citations(gl, this, 200, 200));

                _listeObjets.Add(new PanneauInfo2(gl));
                _listeObjets.Add(new Actualites(gl));

                using (new Chronometre("Initialisations"))
                {
                    for (int i = 1; i < _listeObjets.Count; i++)
                    {
                        using (var v = new Chronometre("Init " + _listeObjets[i].GetType().Name))
                        {
                            _listeObjets[i].Initialisation(gl);
                        }
                    }
                }
                _panneau = PanneauMessage.Instance;
            }
            catch (Exception e)
            {
                //_panneau.SetMessage(openGLControl.OpenGL, "Exception:" + e.ToString());
                _texteErreur = e.ToString();
                _afficheDebug = true;
                log.Error("Exception dans MainForm.createAllObjects");
                log.Error(e.Message);
                log.Error(e.StackTrace);
            }

            log.Verbose(">>> Fin initialisation");
        }

        private void OnTimerChangeBackground(object sender, EventArgs e)
        {
            DisplayedObjectFactory.FONDS type = (DisplayedObjectFactory.FONDS)c.GetParametre(PARAM_TYPEFOND, 0);
            type = ProchainFond(type);
            ChangeFond(type);
        }

        private static DisplayedObjectFactory.FONDS ProchainFond(DisplayedObjectFactory.FONDS type)
        {
            if (type == DisplayedObjectFactory.DERNIER_FOND)
                return DisplayedObjectFactory.PREMIER_FOND;
            else
                return (DisplayedObjectFactory.FONDS)((int)type + 1);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!IsPreviewMode) //disable exit functions for preview
            {
                switch ((Keys)e.KeyValue)
                {
                    case Keys.F1: _couleur.ChangeHue(1); break;
                    case Keys.F2: _couleur.ChangeHue(-1); break;
                    case Keys.F3: _couleur.ChangeSaturation(1); break;
                    case Keys.F4: _couleur.ChangeSaturation(-1); break;
                    case Keys.F5: _couleur.ChangeValue(1); break;
                    case Keys.F6: _couleur.ChangeValue(-1); break;

                    case DisplayedObject.TOUCHE_REINIT:
                        _panneau.SetMessage(openGLControl.OpenGL, "Réinitialisation du fond d'écran");
                        //_listeObjets[0].Dispose();
                        _listeObjets[0] = CreateBackgroundObject((DisplayedObjectFactory.FONDS)c.GetParametre(PARAM_TYPEFOND, 0), _fondDeSaison);
                        timerChangeFond.Stop();
                        timerChangeFond.Start();
                        break;

                    case DisplayedObject.TOUCHE_EFFACER_FOND:
                        _panneau.SetMessage(openGLControl.OpenGL, "Effacer le fond");
                        _effacerFond = !_effacerFond;
                        break;

                    case DisplayedObject.TOUCHE_WIREFRAME:
                        _panneau.SetMessage(openGLControl.OpenGL, "Fil de fer");
                        wireframe = !wireframe;
                        break;

                    case DisplayedObject.TOUCHE_AIDE:
                        _panneau.SetMessage(openGLControl.OpenGL, DisplayedObject.MESSAGE_AIDE);
                        break;

                    case DisplayedObject.TOUCHE_DE_SAISON:
                        {
                            _panneau.SetMessage(openGLControl.OpenGL, "Fond de saison");
                            // Changement de mode de fond
                            _fondDeSaison = !_fondDeSaison;
                            c.SetParametre(PARAM_FONDDESAISON, _fondDeSaison);
                            _listeObjets[0] = CreateBackgroundObject((DisplayedObjectFactory.FONDS)c.GetParametre(PARAM_TYPEFOND, 0), _fondDeSaison);
                        }
                        break;
                    case DisplayedObject.TOUCHE_PROCHAIN_FOND:
                        {
                            _panneau.SetMessage(openGLControl.OpenGL, "Prochain fond");
                            // Passage en mode manuel
                            timerChangeFond.Enabled = false;
                            DisplayedObjectFactory.FONDS Type = (DisplayedObjectFactory.FONDS)c.GetParametre(PARAM_TYPEFOND, 0);
                            Type = ProchainFond(Type);
                            ChangeFond(Type);
                        }
                        break;

                    case DisplayedObject.TOUCHE_FOND_PRECEDENT:
                        {
                            _panneau.SetMessage(openGLControl.OpenGL, "Fond précédent");

                            // Passage en mode manuel
                            timerChangeFond.Enabled = false;
                            DisplayedObjectFactory.FONDS Type = (DisplayedObjectFactory.FONDS)c.GetParametre(PARAM_TYPEFOND, 0);
                            if (Type == DisplayedObjectFactory.PREMIER_FOND)
                                Type = DisplayedObjectFactory.DERNIER_FOND;
                            else
                                Type--;
                            ChangeFond(Type);
                        }
                        break;

                    case DisplayedObject.TOUCHE_FIGER_FOND:
                        timerChangeFond.Enabled = !timerChangeFond.Enabled;
                        _panneau.SetMessage(openGLControl.OpenGL, "Figer fond");
                        break;
#if TRACER
                    case DisplayedObject.TOUCHE_DEBUG:
                        {
                            _panneau.SetMessage(openGLControl.OpenGL, "Console");
                            _afficheDebug = !_afficheDebug;
                            _texteErreur = null;
                            c.SetParametre("Debug", _afficheDebug);
                        }
                        break;
#endif
                    default:
                        // Proposer la touche a chaque objet affiche
                        bool b = false;
                        foreach (DisplayedObject o in _listeObjets)
                            if (o.KeyDown(this, (Keys)e.KeyValue))
                                b = true;

                        if (!b)
                        {
                            // Touche non reconnue: terminer l'application
                            Cursor.Show();
                            Application.Exit();
                        }
                        break;
                }
            }
        }

        private void ChangeFond(DisplayedObjectFactory.FONDS type)
        {
            c.SetParametre(PARAM_TYPEFOND, (int)type);
            // Remplacer le premier objet de la liste par le nouveau fond
            DisplayedObject dO = _listeObjets[INDICE_FOND];
            DisplayedObject tr = _listeObjets[INDICE_TRANSITION];
            if (tr is Transition transition)
                transition.InitTransition(openGLControl.OpenGL, dO, _temps, Bounds, _couleur.GetARGB());

            _listeObjets[INDICE_FOND] = CreateBackgroundObject(type, false);
        }

        //start off OriginalLoction with an X and Y of int.MaxValue, because
        //it is impossible for the cursor to be at that position. That way, we
        //know if this variable has been set yet.
        private Point OriginalLocation = new Point(int.MaxValue, int.MaxValue);
        private bool _effacerFond = true;
        private bool _initTermine = false;
        private string _texteErreur;

        //private bool _frameInitiale = true;

        public void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!IsPreviewMode) //disable exit functions for preview
            {
                //see if originallocat5ion has been set
                if (OriginalLocation.X == int.MaxValue && OriginalLocation.Y == int.MaxValue)
                {
                    OriginalLocation = e.Location;
                }
                //see if the mouse has moved more than 20 pixels in any direction. If it has, close the application.
                if (Math.Abs(e.X - OriginalLocation.X) > 20 || Math.Abs(e.Y - OriginalLocation.Y) > 20)
                {
                    Cursor.Show();
                    Application.Exit();
                }
            }
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            c.SetParametre("Couleur Globale Hue", (float)_couleur.hue);
            c.SetParametre("Couleur Globale Saturation", (float)_couleur.saturation);
            c.SetParametre("Couleur Globale Luminance", (float)_couleur.luminance);

            Configuration.Instance.Flush();
        }
    }
}
