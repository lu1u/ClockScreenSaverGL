using System.Threading;
/// <summary>
/// Classe de base pour implementer des objets dont l'initialisation doit se faire avec une
/// partie en tache de fond, puis une dans le thread principal
/// </summary>
namespace ClockScreenSaverGL.DisplayedObjects.OpenGLUtils
{
    public abstract class ObjetAsynchrone
    {
        private bool _initAsynchroneTerminé, _initSynchroneTerminé;
        protected abstract void InitAsynchrone();       // Partie asynchrone de l'initialisation (tache de fond)
        protected abstract void InitSynchrone();        // Partie synchrone de l'initialisation (thread principal)

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="priorité">La priorité qu'aura le thread de tache de fond</param>
        public ObjetAsynchrone(ThreadPriority priorité = ThreadPriority.Normal)
        {
            _initAsynchroneTerminé = false;
            _initSynchroneTerminé = false;
            Thread t = new Thread(FonctionThread);
            t.Priority = priorité;
            t.Start();
        }

        /// <summary>
        /// La fonction principale executee en tache de fond
        /// </summary>
        private void FonctionThread()
        {
            InitAsynchrone();
            _initAsynchroneTerminé = true;
        }

        public bool Pret
        {
            get
            {
                lock (this)
                {
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
