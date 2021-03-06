
using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects;
using ClockScreenSaverGL.DisplayedObjects.Fonds;
using ClockScreenSaverGL.DisplayedObjects.Fonds.FontaineParticulesPluie;
using ClockScreenSaverGL.DisplayedObjects.Fonds.Gravity;
using ClockScreenSaverGL.DisplayedObjects.Fonds.Particules;
using ClockScreenSaverGL.DisplayedObjects.Fonds.Printemps;
using ClockScreenSaverGL.DisplayedObjects.Fonds.Saisons.Ete;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD.Boids;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD.Grilles;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD.MarchingCubes;
using ClockScreenSaverGL.DisplayedObjects.Metaballes;
using ClockScreenSaverGL.DisplayedObjects.Meteo;
using ClockScreenSaverGL.DisplayedObjects.PanneauActualites;
using ClockScreenSaverGL.DisplayedObjects.Saisons;
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
using System.Threading.Tasks;
using ClockScreenSaverGL.DisplayedObjects.Fonds.Turing;

namespace ClockScreenSaverGL
{

    /// <summary>
    /// Description of MainForm.
    /// </summary>
    public partial class MainForm : Form, IDisposable
    {

        #region Parametres
        public const string CAT = "Main";
        static protected CategorieConfiguration c = Config.Configuration.getCategorie(CAT);
        const string PARAM_DELAI_CHANGE_FOND = "DelaiChangeFondMinutes";
        const string PARAM_FONDDESAISON = "FondDeSaison";
        const string PARAM_TYPEFOND = "TypeFond";
        static readonly int PRINTEMPS = c.getParametre("Printemps", 80);
        static readonly int ETE = c.getParametre("Ete", 172);
        static readonly int AUTOMNE = c.getParametre("Automne", 265);
        static readonly int HIVER = c.getParametre("Hiver", 356);
        static bool MULTISAMPLE = c.getParametre("Multisample", true);
        #endregion

        CouleurGlobale _couleur = new CouleurGlobale();        // La couleur de base pour tous les affichages
        private List<DisplayedObject> _listeObjets = new List<DisplayedObject>();
        private int _jourActuel = -1;                          // Pour forcer un changement de date avant la premiere image
        //private bool _afficherAide = false;                    // Vrai si on doit afficher le message d'aide

        private bool _fondDeSaison;                           // Vrai si on doit commencer par le fond 'de saison'
        DateTime _derniereFrame = DateTime.Now;                // Heure de la derniere frame affichee
        DateTime _debut = DateTime.Now;
        Temps _temps;
        private bool wireframe = false;
        int INDICE_FOND;
        int INDICE_TRANSITION;

        #region Fonds
        enum FONDS
        {
            ESPACE, TROISDPIPES, COURONNES, GRILLE, PARTICULES_GRAVITATION, METABALLES, BOIDS_OISEAUX, MULTICHAINES, NUAGES, MOLECULE, PARTICULES_PLUIE, CARRE_ESPACE, ENCRE, REBOND, ESCALIER, TUNNEL, NEIGE_META, DOUBLE_PENDULE,LIFE, TERRE,
            BACTERIES, PARTICULES1, COULEUR, FUSEES, ARTIFICE, NOIR, ATTRACTEUR, NEBULEUSE, VIELLES_TELES, GRAVITE, ENGRENAGES, CUBES, BOIDS_POISSONS,
            MYRIADE, CONSOLE, MOTO, MARCHING_CUBES, TRIANGLES, EPICYCLE, TURING, /*MOIRE,*/ ADN, SINUSOIDE
        };

        const FONDS PREMIER_FOND = FONDS.ESPACE;
        const FONDS DERNIER_FOND = FONDS.SINUSOIDE;
        #endregion

        enum SAISON { HIVER = 0, PRINTEMPS = 1, ETE = 2, AUTOMNE = 3 };
#if TRACER
        bool _afficheDebug = c.getParametre("Debug", true);
        DateTime lastFrame = DateTime.Now;
        PanneauMessage _panneau;
        Process currentProc = Process.GetCurrentProcess();
        PerformanceCounter cpuCounter;
        PerformanceCounter ramCounter;
#endif
        #region Preview API's

        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, out Rectangle lpRect);

        #endregion

        #region Screensaver
        bool IsPreviewMode = false;
        public MainForm()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            try
            {
                InitializeComponent();

                _temps = new Temps(DateTime.Now, _derniereFrame);
                _fondDeSaison = c.getParametre(PARAM_FONDDESAISON, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\ndans : " + ex.Source + "\n" + ex.StackTrace, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                Rectangle ParentRect;
                GetClientRect(PreviewHandle, out ParentRect);
                this.Size = ParentRect.Size;

                //set our location at (0, 0)
                this.Location = new Point(0, 0);

                IsPreviewMode = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\ndans : " + ex.Source, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        #endregion

        /// <summary>
        /// Creer l'objet qui anime le fond d'ecran
        /// </summary>
        /// <returns></returns>
        private Fond createBackgroundObject(FONDS type, bool initial)
        {
            OpenGL gl = openGLControl.OpenGL;
            if (!initial)
                gl.PopAttrib();

            gl.PushAttrib(OpenGL.GL_ENABLE_BIT | OpenGL.GL_FOG_BIT | OpenGL.GL_LIGHTING_BIT);
            Fond ret = getObjetFond(type, initial);
            ret.Init(gl);
            return ret;
        }

        private Fond getObjetFond(FONDS Type, bool initial)
        {
            OpenGL gl = openGLControl.OpenGL;
            if (_fondDeSaison && initial)
            {
                // Si l'option 'fond de saison' est selectionnee, l'economiseur commence par celui ci
                // Note: il n'apparaissent plus dans le cycle de changement du fond
                switch (getSaison())
                {
                    case SAISON.HIVER:
                        return new Hiver(gl);
                    case SAISON.PRINTEMPS:
                        return new Printemps(gl, SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
                    case SAISON.ETE:
                        return new Ete(gl, SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
                    case SAISON.AUTOMNE:
                        return new Automne(gl);
                }
            }

            //Type = FONDS.SINUSOIDE ;

            switch (Type)
            {
                case FONDS.METABALLES: return new Neige(gl, SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
                case FONDS.ENCRE: return new Encre(gl, SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
                case FONDS.BACTERIES: return new Bacteries(gl, SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
                case FONDS.LIFE: return new Life(gl);
                case FONDS.NOIR: return new Noir(gl, SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
                case FONDS.COURONNES: return new Couronnes(gl);
                case FONDS.COULEUR: return new Couleur(gl, SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
                case FONDS.ESPACE: return new Espace(gl);
                case FONDS.TUNNEL: return new Tunnel(gl);
                case FONDS.CARRE_ESPACE: return new CarresEspace(gl);
                case FONDS.PARTICULES_GRAVITATION: return new GravitationParticules(gl);
                case FONDS.NUAGES: return new Nuages2(gl);
                case FONDS.TERRE: return new TerreOpenGL(gl);
                case FONDS.PARTICULES1: return new ParticulesGalaxie(gl);
                case FONDS.PARTICULES_PLUIE: return new FontaineParticulesPluie(gl);
                case FONDS.FUSEES: return new ParticulesFusees(gl);
                case FONDS.MULTICHAINES: return new MultiplesChaines(gl);
                case FONDS.VIELLES_TELES: return new ViellesTeles(gl);
                case FONDS.ARTIFICE: return new FeuDArtifice(gl);
                case FONDS.ATTRACTEUR: return new AttracteurParticules(gl);
                case FONDS.GRAVITE: return new Gravitation(gl);
                case FONDS.REBOND: return new RebondParticules(gl);
                case FONDS.ENGRENAGES: return new Engrenages(gl);
                case FONDS.CUBES: return new Cubes(gl);
                case FONDS.NEBULEUSE: return new Nebuleuse(gl);
                case FONDS.ADN: return new ADN(gl);
                case FONDS.BOIDS_OISEAUX: return new BoidsOiseaux(gl);
                case FONDS.BOIDS_POISSONS: return new BoidsPoissons(gl);
                case FONDS.MOLECULE: return new Molecule(gl);
                case FONDS.MYRIADE: return new Myriade(gl);
                case FONDS.CONSOLE: return new VielleConsole(gl);
                case FONDS.GRILLE: return new Grille(gl);
                case FONDS.ESCALIER: return new Escaliers(gl);
                case FONDS.MOTO: return new Moto(gl);
                case FONDS.MARCHING_CUBES: return new MarchingCubes(gl);
                case FONDS.TRIANGLES: return new Triangles(gl);
                case FONDS.EPICYCLE: return new Epicycle(gl);
                case FONDS.DOUBLE_PENDULE: return new PenduleDouble(gl);
                case FONDS.TROISDPIPES: return new TroisDPipes(gl);
                //case FONDS.MOIRE: return new Moire(gl);
                case FONDS.TURING: return new MachineDeTuring(gl);
                case FONDS.SINUSOIDE: return new Sinusoides(gl);
                default:
                    return new Metaballes(gl);
            }

        }


        /// <summary>
        /// Retourne la saison, (calcul tres approximatif)
        /// </summary>
        /// <returns></returns>
        private static SAISON getSaison()
        {
            int forceSaison = c.getParametre("Force saison", -1);
            if (forceSaison != -1)
                // Forcage de la saison
                return (SAISON)forceSaison;

            DateTime date = DateTime.Now;

            int quantieme = date.DayOfYear;
            // Hiver : jusqu'a l'equinoxe de printemps
            if (quantieme < PRINTEMPS)
                return SAISON.HIVER;

            // Printemps: jusqu'au solstice d'ete
            if (quantieme <= ETE)
                return SAISON.PRINTEMPS;

            // Ete: jusqu'a l'equinoxe d'automne
            if (quantieme < AUTOMNE)
                return SAISON.ETE;

            // Automne : jusqu'au solstice d'hiver
            if (quantieme < HIVER)
                return SAISON.AUTOMNE;

            return SAISON.HIVER;
        }

        /// <summary>
        /// Chargement de la fenetre et de ses composants
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void onLoad(object sender, System.EventArgs e)
        {
            try
            {
                UpdateStyles();
                Cursor.Hide();
                _fontHelp = new Font(FontFamily.GenericSansSerif, 20);

                timerChangeFond.Interval = c.getParametre(PARAM_DELAI_CHANGE_FOND, 3) * 60 * 1000;
                timerChangeFond.Enabled = true;
#if TRACER
                cpuCounter = new PerformanceCounter();
                cpuCounter.CategoryName = "Processor";
                cpuCounter.CounterName = "% Processor Time";
                cpuCounter.InstanceName = "_Total";
                ramCounter = new PerformanceCounter("Memory", "Available MBytes");
#endif
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
        private void remplitDebug(OpenGL gl)
        {
            double NbMillisec = _temps.temps.Subtract(lastFrame).TotalMilliseconds;
            DisplayedObjects.Console console = DisplayedObjects.Console.getInstance(gl);
            console.Clear();
#if DEBUG
            console.AddLigne(Color.White, "Version DEBUG ");
#else
            console.AddLigne(Color.White, "Version RELEASE ");
#endif

            console.AddLigne(Color.White, Assembly.GetExecutingAssembly().GetName().Version.ToString());

            try
            {
                console.AddLigne(Color.White, (1000.0 / NbMillisec).ToString("0.0") + " FPS\n\n");
                console.AddLigne(Color.White, "Couleur: " + _couleur.ToString() + "\n\n");
                //console.AddLigne(Color.White, "CPU " + cpuCounter.NextValue().ToString("00") + "%\n");
                console.AddLigne(Color.White, "Free RAM " + (ramCounter.NextValue() / 1024).ToString("0.00") + "GB\n");
                console.AddLigne(Color.White, "Memory usage " + ((currentProc.PrivateMemorySize64 / 1024.0) / 1024.0).ToString("0.0") + "MB\n\n");
            }
            catch (Exception)
            {

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
        void moveAll()
        {
            _couleur.AvanceCouleur();

            _temps = new Temps(DateTime.Now, _derniereFrame);

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

        void onOpenGLDraw(object sender, SharpGL.RenderEventArgs args)
        {
            OpenGL gl = openGLControl.OpenGL;

#if TRACER
            if (_afficheDebug)
                remplitDebug(gl);
#endif
            // Get the OpenGL object, just to clean up the code.
            Color Couleur = _couleur.GetRGB();

            gl.MatrixMode(OpenGL.GL_PROJECTION);                        // Select The Projection Matrix
            gl.LoadIdentity();                                   // Reset The Projection Matrix
            gl.Perspective(60, Bounds.Width / (float)Bounds.Height, .1f, 1000f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);                         // Select The Modelview Matrix
            gl.LoadIdentity();
            gl.Disable(OpenGL.GL_MULTISAMPLE);
            gl.Hint(OpenGL.GL_LINE_SMOOTH_HINT, OpenGL.GL_FASTEST);

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

            //Log log = Log.getInstance();

            // Deplacer et Afficher tous les objets
            foreach (DisplayedObject b in _listeObjets)
            {
                //DateTime av = DateTime.Now;
                gl.PushMatrix();
                gl.PushAttrib(OpenGL.GL_ENABLE_BIT | OpenGL.GL_CURRENT_BIT | OpenGL.GL_DEPTH_BUFFER_BIT | OpenGL.GL_FOG_BIT | OpenGL.GL_COLOR_BUFFER_BIT);
                b.AfficheOpenGL(gl, _temps, Bounds, Couleur);
                gl.PopAttrib();
                gl.PopMatrix();
                //DateTime ap = DateTime.Now;
                //Debug.WriteLine("Affichage " + b.GetType().Name + " " + (ap-av).TotalSeconds + "s" );
            }

            if (wireframe)
                gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);

            if (_afficheDebug)
            {
                foreach (DisplayedObject b in _listeObjets)
                    if (b is Fond)
                        ((Fond)b).fillConsole(gl);

                DisplayedObjects.Console c = DisplayedObjects.Console.getInstance(gl);
                c.trace(gl, Bounds);
                c.Clear();
            }

            _panneau.AfficheOpenGL(gl, _temps, Bounds, Couleur);
            gl.Finish();
            moveAll();
        }

        /// <summary>
        /// OpenGL est initialise
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void onOpenGLInitialized(object sender, System.EventArgs e)
        {
            createAllObjects();

            OpenGL gl = openGLControl.OpenGL;
            gl.Clear(0);
        }


        /// <summary>
        /// Creer tous les objets qui seront affiches
        /// </summary>
        private void createAllObjects()
        {
            Log log = Log.instance;
            log.verbose(">>> Initialisation");

            OpenGL gl = openGLControl.OpenGL;
            int CentreX = Bounds.Width / 2;
            int CentreY = Bounds.Height / 2;
            int TailleHorloge = c.getParametre("TailleCadran", 400);
            if (IsPreviewMode)
            {
                TailleHorloge = 10;
                _listeObjets.Add(new HorlogeRonde(gl, TailleHorloge, CentreX - TailleHorloge / 2, CentreY - TailleHorloge / 2));
                return;
            }

            _fondDeSaison = c.getParametre(PARAM_FONDDESAISON, true);
            // Ajout de tous les objets graphiques, en finissant par celui qui sera affiche en dessus des autres
            INDICE_FOND = 0;
            _listeObjets.Add( createBackgroundObject((FONDS)c.getParametre(PARAM_TYPEFOND, 0), true));

            INDICE_TRANSITION = 1;
            _listeObjets.Add( new Transition(gl));

            // Copyright
            if (c.getParametre("Copyright", true))
                _listeObjets.Add(new TexteCopyright(gl, -4, 100));
            // citations
            if (c.getParametre("Citation", true))
                _listeObjets.Add(new Citations(gl, this, 200, 200));

           _listeObjets.Add(new Actualites(gl));
           //////  Meteo
           _listeObjets.Add(new PanneauInfos(gl));

            for (int i = 1; i < _listeObjets.Count; i++)
            {
                Stopwatch w = new Stopwatch();
                w.Start();
                _listeObjets[i].Initialisation(gl);
                w.Stop();
                long l = w.ElapsedMilliseconds;
                Debug.WriteLine("Duree initialisation " + _listeObjets[i].GetType().Name  + " = " + l + "ms");

            }
            _panneau = PanneauMessage.instance ;

            log.verbose(">>> Fin initialisation");
        }

        void onTimerChangeBackground(object sender, EventArgs e)
        {
            FONDS Type = (FONDS)c.getParametre(PARAM_TYPEFOND, 0);
            Type = ProchainFond(Type);
            ChangeFond(Type);
        }

        private static FONDS ProchainFond(FONDS type)
        {
            if (type == DERNIER_FOND)
                return PREMIER_FOND;
            else
                return (FONDS)((int)type + 1);
        }

        private void onKeyDown(object sender, KeyEventArgs e)
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

                    //case Keys.H: _afficherAide = !_afficherAide; break;
                    case DisplayedObject.TOUCHE_REINIT:
                        _panneau.SetMessage(openGLControl.OpenGL, "Réinitialisation du fond d'écran");
                        _listeObjets[0].Dispose();
                        _listeObjets[0] =  createBackgroundObject((FONDS)c.getParametre(PARAM_TYPEFOND, 0), _fondDeSaison);
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

                    case DisplayedObject.TOUCHE_DE_SAISON:
                        {
                            _panneau.SetMessage(openGLControl.OpenGL, "Fond de saison");
                            // Changement de mode de fond
                            _fondDeSaison = !_fondDeSaison;
                            c.setParametre(PARAM_FONDDESAISON, _fondDeSaison);
                            _listeObjets[0] =  createBackgroundObject((FONDS)c.getParametre(PARAM_TYPEFOND, 0), _fondDeSaison);
                        }
                        break;
                    case DisplayedObject.TOUCHE_PROCHAIN_FOND:
                        {
                            _panneau.SetMessage(openGLControl.OpenGL, "Prochain fond");
                            // Passage en mode manuel
                            timerChangeFond.Enabled = false;
                            FONDS Type = (FONDS)c.getParametre(PARAM_TYPEFOND, 0);
                            Type = ProchainFond(Type);
                            ChangeFond(Type);
                        }
                        break;

                    case DisplayedObject.TOUCHE_FOND_PRECEDENT:
                        {
                            _panneau.SetMessage(openGLControl.OpenGL, "Fond précédent");

                            // Passage en mode manuel
                            timerChangeFond.Enabled = false;
                            FONDS Type = (FONDS)c.getParametre(PARAM_TYPEFOND, 0);
                            if (Type == PREMIER_FOND)
                                Type = DERNIER_FOND;
                            else
                                Type--;
                            ChangeFond(Type);
                        }
                        break;
#if TRACER
                    case Keys.D:
                        {
                            _panneau.SetMessage(openGLControl.OpenGL, "Debug");
                            _afficheDebug = !_afficheDebug;
                            c.setParametre("Debug", _afficheDebug);
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

        private void ChangeFond(FONDS type)
        {
            c.setParametre(PARAM_TYPEFOND, (int)type);
            // Remplacer le premier objet de la liste par le nouveau fond
            DisplayedObject dO = _listeObjets[INDICE_FOND];
            DisplayedObject tr = _listeObjets[INDICE_TRANSITION];
            if (tr is Transition)
            {
                ((Transition)tr).InitTransition(openGLControl.OpenGL, dO, _temps, Bounds, _couleur.GetRGB());
            }

            _listeObjets[INDICE_FOND] = createBackgroundObject(type, false);
        }

        //start off OriginalLoction with an X and Y of int.MaxValue, because
        //it is impossible for the cursor to be at that position. That way, we
        //know if this variable has been set yet.
        Point OriginalLocation = new Point(int.MaxValue, int.MaxValue);
        private Font _fontHelp;
        private bool _effacerFond = true ;

        public void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!IsPreviewMode) //disable exit functions for preview
            {
                //see if originallocat5ion has been set
                if (OriginalLocation.X == int.MaxValue & OriginalLocation.Y == int.MaxValue)
                {
                    OriginalLocation = e.Location;
                }
                //see if the mouse has moved more than 20 pixels in any direction. If it has, close the application.
                if (Math.Abs(e.X - OriginalLocation.X) > 20 | Math.Abs(e.Y - OriginalLocation.Y) > 20)
                {
                    Cursor.Show();
                    Application.Exit();
                }
            }
        }

        private void onFormClosed(object sender, FormClosedEventArgs e)
        {
            Configuration.Instance.flush();
        }
    }
}
