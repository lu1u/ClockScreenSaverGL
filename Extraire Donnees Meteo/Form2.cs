using System;
using System.Windows.Forms;

using Config;

namespace Extraire_Donnees_Meteo
{
	public partial class Form2 : Form
    {
        public const string CAT = "ExtraitMeteo";
        static protected Config.CategorieConfiguration c = Configuration.getCategorie(CAT);
        static readonly int DELAI_TIMER = c.getParametre("delai lecture info (secondes)", 60 * 60);
        //static readonly string UU = c.getParametre("uddrl", @"http://www.meteofrance.com/previsions-meteo-france/saint-pancrasse/38660");


        private Actualites _actualites;

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            Cursor cur = this.Cursor;
            timer1.Interval = DELAI_TIMER * 1000;
            this.Cursor = Cursors.WaitCursor;
            extraitInformationsMeteo();
            _actualites = new Actualites();
            _actualites.extraireActualites();

            notifyIcon.Text = $"Dernière extraction {GetDate()}";
            this.Cursor = cur;
        }



        private void timer1_Tick(object sender, EventArgs e)
        {
            extraitInformationsMeteo();
            extraireInformationsActualites();

            notifyIcon.Text = $"Dernière extraction {GetDate()}";
        }

        private void extraireMétéoMaintenantToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor c = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            extraitInformationsMeteo();
            _actualites.extraireActualites();
            this.Cursor = c;
        }

        private void quitterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ouvrirRépertoireDeConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _actualites.ouvreRepertoireConfig();
        }

        private void ouvrirRépertoireDesActualitésChargéesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _actualites.ouvreRepertoireActualites();
        }
    }
}
