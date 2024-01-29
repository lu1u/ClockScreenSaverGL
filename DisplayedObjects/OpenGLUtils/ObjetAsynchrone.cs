using System.Threading;
/// <summary>
/// Classe de base pour implementer des objets dont l'initialisation doit se faire avec une
/// partie en tache de fond, puis une dans le thread principal (par exemple, creation de texture OpenGL
/// qui doit se faire dans le thread principal, mais dont la preparation peut se faire dans un thread)
/// 
/// Utilisation:
/// o = new ObjectAsynchrone();
/// o.Init();
/// 
/// </summary>
namespace ClockScreenSaverGL.DisplayedObjects.OpenGLUtils
{
    public abstract class ObjetAsynchrone
    {
        private bool _initAsynchroneTerminé, _initSynchroneTerminé, _initLancé;
        private readonly object _lock = new object();
        protected abstract void InitAsynchrone();       // Partie asynchrone de l'initialisation (tache de fond)
        protected abstract void InitSynchrone();        // Partie synchrone de l'initialisation (thread principal)
        protected ThreadPriority _priorité;
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="priorité">La priorité qu'aura le thread de tache de fond</param>
        public ObjetAsynchrone(ThreadPriority priorité = ThreadPriority.Normal)
        {
            _initAsynchroneTerminé = false;
            _initSynchroneTerminé = false;
            _initLancé = false;
            _priorité = priorité;
        }

        /// <summary>
        /// A appeler impérativement pour lancer l'initialisation
        /// </summary>
        public void Init()
        {
            _initLancé = true;
            Thread t = new Thread(FonctionThread)
            {
                Priority = _priorité
            };
            t.Start();
        }

        /// <summary>
        /// La fonction principale executee en tache de fond
        /// </summary>
        private void FonctionThread()
        {
            InitAsynchrone();
            //Thread.Sleep(1000);
            _initAsynchroneTerminé = true;
        }

        /// <summary>
        /// Retourne TRUE si les initialisations Asynchrone et synchrone sont terminées
        /// </summary>
        public bool Pret
        {
            get
            {
                lock (_lock)
                {
                    if (!_initLancé)
                    {
                        Init();
                        return false;
                    }

                    if (!_initAsynchroneTerminé)
                        // Partie asynchrone pas encore terminée
                        return false;

                    if (!_initSynchroneTerminé)
                    {
                        // Partie asynchrone terminée, appeler la partie synchrone
                        InitSynchrone();
                        _initSynchroneTerminé = true;   // Pour ne pas la rappeler une deuxieme fois
                    }
                }
                return true;
            }
        }
    }
}
