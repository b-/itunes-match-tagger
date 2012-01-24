using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace iTunesMatchTagger
{
    public partial class Info : Form
    {
        public Info()
        {
            InitializeComponent();

            linkLabel1.Links.Add(0, linkLabel1.Text.Length, linkLabel1.Text);
            linkLabel2.Links.Add(0, linkLabel2.Text.Length, linkLabel2.Text);
        }

        private void LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
            }
            catch (Exception) {}
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
