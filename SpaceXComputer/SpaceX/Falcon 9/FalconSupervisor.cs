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
        }

        public static void Execute(Action method)
        {
            try
            {
                Console.WriteLine("Invoke");
                Instance?.BeginInvoke(method);
            }
            catch
            { }
        }
    }
}
