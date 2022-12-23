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
        #region parametres
        private readonly bool LOG_VERBOSE;
        private readonly bool LOG_WARNING;
        private readonly bool LOG_ERROR;
        #endregion

        /// <summary>
        /// Constructeur privé du singleton
        /// </summary>
        private Log()
        {
            CategorieConfiguration c = Configuration.GetCategorie(CAT);
            LOG_VERBOSE = c.GetParametre("Verbose", false);
            LOG_WARNING = c.GetParametre("Warning", false);
            LOG_ERROR = c.GetParametre("Error", true);
        }

        private string GetLogName()
        {
            string res = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString(), Configuration.NOM_PROGRAMME);

            if (!Directory.Exists(res))
                Directory.CreateDirectory(res);

            return Path.Combine(res, "journal.txt");
        }

        public static Log Instance
        {
            get
            {
                if (_instance == null)
                    return _instance = new Log();

                return _instance;
            }
        }

        public void Verbose(string message)
        {
            if (LOG_VERBOSE)
                Write("W:" + Date() + " " + message);
        }

        public void Warning(string message)
        {
            if (LOG_WARNING)
                Write("W:" + Date() + " " + message);
        }

        private void Write(string v)
        {
            try
            {
                TextWriter tw;
                tw = new StreamWriter(GetLogName(), true);
                tw.WriteLine(v);
                tw.Close();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }

        }

        public void Error(string message)
        {
            if (LOG_ERROR)
                Write("E:" + Date() + " " + message);
        }

        private string Date()
        {
            return DateTime.Now.ToString("o");
        }
    }
}
