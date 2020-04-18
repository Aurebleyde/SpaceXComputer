using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpaceXComputer
{
    public partial class FalconSupervisor : Form
    {
        public static FalconSupervisor Instance { get; private set; }
        public FalconSupervisor()
        {
            InitializeComponent();
        }

        private void FalconSupervisor_Load(object sender, EventArgs e)
        {
            Instance = this;

            lb_PowerCentral.ForeColor = Color.Black;
            lb_PowerCentral.BackColor = Color.White;
            lb_PowerCentral.Text = "kN";
        }

        public static void Execute(Action method)
        {
            try
            {
                Instance?.Invoke(method);
            }
            catch
            { }
        }

        private void lb_Debug_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void lb_PowerCentral_Click(object sender, EventArgs e)
        {

        }
    }
}
