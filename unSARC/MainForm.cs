using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace unSARC
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            OpenFileDialog odiag = new OpenFileDialog();
            odiag.Multiselect = true;
            //SaveFileDialog sdiag = new SaveFileDialog();

            if (odiag.ShowDialog() != DialogResult.OK) return;

            progressBar1.Value = 0;
            Stopwatch stop = new Stopwatch();

            Thread nTh = new Thread(
                new ThreadStart(() =>
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        progressBar1.Maximum = odiag.FileNames.Length;
                    }));

                    stop.Start();

                    for (int i = 0; i < odiag.FileNames.Length; i++)
                    {
                        sarc sn = new sarc(odiag.FileNames[i]);
                        sn.Extract();

                        this.BeginInvoke(new Action(() =>
                        {
                            label1.Text = "SARC File : " + (i) + " of " + odiag.FileNames.Length +" ("+ stop.ElapsedMilliseconds+ " ms)";
                            progressBar1.Value ++;
                        }));
                    }
                    stop.Stop();

                    GC.Collect(9, GCCollectionMode.Forced);
                }));

            nTh.Start();
        }
    }
}
