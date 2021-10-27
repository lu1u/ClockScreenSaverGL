using System;
using System.IO;

namespace ClockScreenSaverGL.Config
{
    /// <summary>
    /// Journal de traces
    /// </summary>
    public class Log 
    {
        private static Log _instance;
        public const string CAT = "Log";
        private CategorieConfiguration c;
        #region parametres
        bool LOG_VERBOSE;
        bool LOG_WARNING;
        bool LOG_ERROR;
        #endregion

        /// <summary>
        /// Constructeur privé du singleton
        /// </summary>
        private Log()
        {
            c = Configuration.getCategorie(CAT);
            LOG_VERBOSE = c.getParametre("Verbose", false);
            LOG_WARNING = c.getParametre("Warning", false);
            LOG_ERROR = c.getParametre("Error", true);
        }

        private string getLogName()
        {
            string res = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString(), Configuration.NOM_PROGRAMME);

            if (!Directory.Exists(res))
                Directory.CreateDirectory(res);

            return Path.Combine(res, "journal.txt");
        }

        public static Log instance
        {
            get
            {
                if (_instance == null)
                    return _instance = new Log();

                return _instance;
            }
        }

        public void verbose(string message)
        {
            if (LOG_VERBOSE)
                Write("W:" + date() + " " + message);
        }

        public void warning(string message)
        {
            if (LOG_WARNING)
                Write("W:" + date() + " " + message);
        }

        private void Write(string v)
        {
            try
            {
                TextWriter tw;
                tw = new StreamWriter(getLogName(), true);
                tw.WriteLine(v);
                tw.Close();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }

        }

        public void error(string message)
        {
            if (LOG_ERROR)
                Write("E:" + date() + " " + message);
        }

        private string date()
        {
            return DateTime.Now.ToString("o");
        }
    }
}
