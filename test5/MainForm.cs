using System.Drawing;
using System.Drawing.Text;
using System.Numerics;
using System.Windows.Forms;

namespace test5
{
    public partial class MainForm : Form
    {
        private readonly WireObject _obj = WireObject.CreateCube(1.5f);


        public MainForm()
        {
            InitializeComponent();
            DoubleBuffered = true;
            BackColor = Color.Black;
            ClientSize = new System.Drawing.Size(1000, 700);
            Text = "Графика 1";

        }

        private float _angleZ = 0.5f; // 28 градусов
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);


            int w = ClientSize.Width;
            int h= ClientSize.Height;
            float cx = w / 2f;
            float cy = h / 2f;


            const float k = 200f; // кол-во пикселей на единицу длины

            Mat4 model = Mat4.RotationZ(_angleZ); // матрица модели

            var screen = new PointF[_obj.Vertices.Length];
            for (int i = 0; i < _obj.Vertices.Length; i++)
            { 
                var v = _obj.Vertices[i];

                Vector4 t = model.Transform(new Vector4(v.X, v.Y, v.Z, 1f));

                screen[i] = new PointF(cx+t.X * k, cy -t.Y * k);
            }

            using var pen = new Pen(Color.White, 1.5f);
            foreach (var (a, b) in _obj.Edges)
                e.Graphics.DrawLine(pen, screen[a], screen[b]);

        }



    }
}
