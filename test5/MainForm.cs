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
            KeyPreview = true;
            DoubleBuffered = true;
            BackColor = Color.Black;
            ClientSize = new System.Drawing.Size(1000, 700);
            Text = "Графика 1";

        }

        private float _angleX = 0.0f;
        private float _angleY = 0.0f;


        private float _tx = 0.0f, _ty = 0.0f, _tz = 0.0f;
        private float _scale = 1.0f;

        private bool _usePerspective = true;
        private float _cameraZ = 5f;

        // для мыши
        private Point _lastMouse;
        private bool _dragging;



        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);


            int w = ClientSize.Width;
            int h= ClientSize.Height;
            float cx = w / 2f;
            float cy = h / 2f;


            const float k = 200f; // кол-во пикселей на единицу длины

            Mat4 model =
                Mat4.Translation(_tx, _ty, _tz)
                * Mat4.RotationY(_angleY)
                * Mat4.RotationX(_angleX)
                * Mat4.Scale(_scale, _scale, _scale);



            Mat4 view = Mat4.Translation(0, 0, -_cameraZ);

            float aspect = (float)w / h;

            Mat4 proj = _usePerspective 
                ? Mat4.Perspective(MathF.PI / 3f, aspect, 0.1f, 100f)
                : Mat4.Orthographic(-2f * aspect, 2f * aspect, -2f, 2f, 0.1f, 100f);

            // итоговая матрица
            Mat4 mvp = proj * view * model;


            var screen = new PointF[_obj.Vertices.Length];
            for (int i = 0; i < _obj.Vertices.Length; i++)
            { 
                var v = _obj.Vertices[i];

                Vector4 t = mvp.Transform(new Vector4(v.X, v.Y, v.Z, 1f));
                if (MathF.Abs(t.W) < 1e-6f) { screen[i] = new PointF(float.NaN, float.NaN); continue; }

                float ndcX = t.X / t.W;
                float ndcY = t.Y / t.W;

                // w пока 1
                screen[i] = new PointF
                    (
                        cx + ndcX * cx, // Рястягиваем ndc в пиксели
                        cy - ndcY * cy // инвертирование y
                    );
            }

            using var pen = new Pen(Color.White, 1.5f);
            foreach (var (a, b) in _obj.Edges)
                e.Graphics.DrawLine(pen, screen[a], screen[b]);

        }




        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            const float step = 0.1f;

            switch (e.KeyCode)
            { 
                case Keys.A: _tx -= step; break;
                case Keys.D: _tx += step; break;
                case Keys.W: _ty += step; break;
                case Keys.S: _ty -= step; break;
                case Keys.Q: _cameraZ -= step; break;
                case Keys.E: _cameraZ += step; break;

                case Keys.J: _angleY -= 0.1f; break;
                case Keys.L: _angleY += 0.1f; break;
                case Keys.I: _angleX -= 0.1f; break;
                case Keys.K: _angleX += 0.1f; break;

                case Keys.Oemplus:
                case Keys.Add: _scale *= 1.1f; break;
                case Keys.OemMinus:
                case Keys.Subtract: _scale /= 1.1f; break;

                case Keys.P: _usePerspective = !_usePerspective; break;

                case Keys.Space:
                    _tx = _ty = 0; _angleX = _angleY = 0; _scale = 1f; _cameraZ = 5f;
                    break;
            }
            Invalidate();



        }


        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) 
            { _dragging = true; _lastMouse = e.Location; }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) _dragging = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!_dragging) return;
            float dx = e.X - _lastMouse.X;
            float dy = e.Y - _lastMouse.Y;
            _lastMouse = e.Location;

            _angleY += dx * 0.01f;
            _angleX += dy * 0.01f;
            Invalidate();
        }






    }
}
