using System.Drawing;
using System.Windows.Forms;

namespace test5
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            DoubleBuffered = true;
            BackColor = Color.Black;
            ClientSize = new System.Drawing.Size(1000, 700);
            Text = "Графика 1";

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using var pen = new Pen(Color.White, 2f);
            e.Graphics.DrawLine(pen, 100, 100, 900, 600);


        }



    }
}
