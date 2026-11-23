using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

namespace test5
{
    public partial class MainForm : Form
    {
        // private readonly WireObject _obj = WireObject.CreateCube(1.5f);
        private readonly WireObject _obj = WireObject.CreateDigit3(1.5f, 0.4f);
        private readonly Vector3[] _worldVertices;
        private readonly int[] _trackedVertices;

        public MainForm()
        {
            InitializeComponent();
            KeyPreview = true;
            DoubleBuffered = true;
            ResizeRedraw = true;

            BackColor = Color.Black;
            ClientSize = new Size(1000, 700);
            Text = "Графика 1";

            _worldVertices = new Vector3[_obj.Vertices.Length];
            _trackedVertices = new[] { 0, _worldVertices.Length / 4, -1 };
            UpdateModel();

            _timer = new System.Windows.Forms.Timer { Interval = 16 };
            _timer.Tick += (s, e) =>
            {
                if (!_spiralClock.IsRunning) return;
                UpdateSpiral();
                UpdateModel();
                Invalidate();
            };
            _timer.Start();
        }

        private float _angleX = 0.0f;
        private float _angleY = 0.0f;
        private float _angleZ = 0.0f;
        private float _tx = 0.0f, _ty = 0.0f, _tz = 0.0f;
        private float _scale = 1.0f;
        private bool _usePerspective = true;
        private const float InitialCameraDistance = 5f;
        private const float CameraYaw = 25f * MathF.PI / 180f;
        private const float CameraPitch = 18f * MathF.PI / 180f;
        private float _cameraDistance = InitialCameraDistance;

        // для мыши
        private Point _lastMouse;
        private bool _dragging;

        // Состояние анимации спирали
        private readonly System.Windows.Forms.Timer _timer;
        private readonly Stopwatch _spiralClock = new Stopwatch();
        private char _spiralAxis = 'Y';
        private const float SpiralRadius = 0.95f;          // радиус спирали
        private const float SpiralLength = 1.7f;           // длина вдоль выбранной оси
        private const float SpiralTurns = 3.0f;           // число витков
        private const float SpiralDurationSec = 6.0f;      // длительность одного прохода


        private static string FormatCoordinates(Vector3 point)
        {
            return FormattableString.Invariant($"({point.X:F3}, {point.Y:F3}, {point.Z:F3})");
        }

        private void UpdateModel()
        {
            Mat4 model =
                Mat4.Translation(_tx, _ty, _tz)
                * Mat4.RotationZ(_angleZ)
                * Mat4.RotationY(_angleY)
                * Mat4.RotationX(_angleX)
                * Mat4.Scale(_scale, _scale, _scale);


            for (int i = 0; i < _obj.Vertices.Length; i++)
            {
                Vector4 point = model.Transform(new Vector4(_obj.Vertices[i], 1f));
                _worldVertices[i] = new Vector3(point.X, point.Y, point.Z);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int w = ClientSize.Width;
            int h = ClientSize.Height;
            if (w <= 0 || h <= 0) return;

   
            Mat4 view = Mat4.Translation(0, 0, -_cameraDistance)
                * Mat4.RotationX(CameraPitch)
                * Mat4.RotationY(-CameraYaw);

            float aspect = (float)w / h;

            Mat4 proj = _usePerspective
                ? Mat4.Perspective(MathF.PI / 3f, aspect, 0.1f, 100f)
                : Mat4.Orthographic(-2f * aspect, 2f * aspect, -2f, 2f, 0.1f, 100f);

            Mat4 viewProjection = proj * view;
            using var font = new Font("Consolas", 10);
            DrawWorldAxes(e.Graphics, font, viewProjection, w, h);

            var clipVertices = new Vector4[_worldVertices.Length];
            for (int i = 0; i < _worldVertices.Length; i++)
                clipVertices[i] = viewProjection.Transform(new Vector4(_worldVertices[i], 1f));

            using var pen = new Pen(Color.White, 1.5f);
            foreach (var (a, b) in _obj.Edges)
            {
                Vector4 start = clipVertices[a];
                Vector4 end = clipVertices[b];
                if (ClipDepth(ref start, ref end))
                    e.Graphics.DrawLine(pen, ToScreen(start, w, h), ToScreen(end, w, h));
            }

            string coordinates = "Мировые координаты вершин (X, Y, Z)";
            foreach (int index in _trackedVertices)
            {
                Vector3 worldPos;
                string label;
                Brush brush = Brushes.Yellow;

                if (index == -1)
                {
                    worldPos = new Vector3(_tx, _ty, _tz);  
                    label = "Центр";
                    brush = Brushes.Orange;
                }
                else
                {
                    worldPos = _worldVertices[index];
                    label = $"№{index + 1}";
                }

                coordinates += $"\n{label,5} {FormatCoordinates(worldPos)}";

                Vector4 clipPoint = viewProjection.Transform(new Vector4(worldPos, 1f));
                if (MathF.Abs(clipPoint.Z) <= clipPoint.W)
                {
                    PointF p = ToScreen(clipPoint, w, h);
                    e.Graphics.FillEllipse(brush, p.X - 3f, p.Y - 3f, 6f, 6f);
                    e.Graphics.DrawString(label, font, brush, p.X + 6f, p.Y - 12f);
                }
            }

            string info =
                $"Проекция — {(_usePerspective ? "перспективная" : "ортографическая")}\n" +
                $"Перенос (X, Y, Z) {FormatCoordinates(new Vector3(_tx, _ty, _tz))}  Масштаб {_scale:F3}\n" +
                $"Углы X {_angleX:F2}  Y {_angleY:F2}  Z {_angleZ:F2} рад  Камера {_cameraDistance:F2} ед. от начала\n" +
                "WASD — сдвиг по X/Y, R/F — по Z, IJKL — поворот, +/- — масштаб\n" +
                "Q/E — ближе/дальше, P — проекция, Space — сброс\n" +
                $"Спираль вдоль {_spiralAxis}  1/2/3 — ось X/Y/Z, M — запуск/пауза\n\n" +
                coordinates;
            e.Graphics.DrawString(info, font, Brushes.Lime, 10, 10);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            const float step = 0.1f;

            switch (e.KeyCode)
            {
                case Keys.A: MoveObject(-step, 0f, 0f); break;
                case Keys.D: MoveObject(step, 0f, 0f); break;
                case Keys.W: MoveObject(0f, step, 0f); break;
                case Keys.S: MoveObject(0f, -step, 0f); break;
                case Keys.R: MoveObject(0f, 0f, step); break;
                case Keys.F: MoveObject(0f, 0f, -step); break;
                case Keys.Q: _cameraDistance = MathF.Max(step, _cameraDistance - step); break;
                case Keys.E: _cameraDistance += step; break;

                case Keys.J: _angleY -= 0.1f; break;
                case Keys.L: _angleY += 0.1f; break;
                case Keys.I: _angleX -= 0.1f; break;
                case Keys.K: _angleX += 0.1f; break;

                case Keys.Oemplus:
                case Keys.Add: _scale *= 1.1f; break;
                case Keys.OemMinus:
                case Keys.Subtract: _scale /= 1.1f; break;

                case Keys.P: _usePerspective = !_usePerspective; break;

                case Keys.D1:
                case Keys.NumPad1: SelectSpiralAxis('X'); break;
                case Keys.D2:
                case Keys.NumPad2: SelectSpiralAxis('Y'); break;
                case Keys.D3:
                case Keys.NumPad3: SelectSpiralAxis('Z'); break;
                case Keys.M: ToggleSpiral(); break;

                case Keys.Space:
                    _spiralClock.Reset();
                    _tx = _ty = _tz = 0f;
                    _angleX = _angleY = _angleZ = 0f;
                    _scale = 1f;
                    _cameraDistance = InitialCameraDistance;
                    break;
                default:
                    return;
            }
            e.SuppressKeyPress = true;
            UpdateModel();
            Invalidate();
        }

        private void MoveObject(float dx, float dy, float dz)
        {
            _spiralClock.Reset();
            _tx += dx;
            _ty += dy;
            _tz += dz;
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
            UpdateModel();
            Invalidate();
        }

        private static PointF ToScreen(Vector4 point, int w, int h)
        {
            return new PointF(
                w / 2f + (point.X / point.W) * (w / 2f),
                h / 2f - (point.Y / point.W) * (h / 2f)
            );
        }

        private static bool ClipDepth(ref Vector4 start, ref Vector4 end)
        {
            return ClipAgainstPlane(ref start, ref end, start.W + start.Z, end.W + end.Z)
                && ClipAgainstPlane(ref start, ref end, start.W - start.Z, end.W - end.Z);
        }

        private static bool ClipAgainstPlane(ref Vector4 start, ref Vector4 end, float startDistance, float endDistance)
        {
            if (startDistance < 0f && endDistance < 0f) return false;
            if (startDistance < 0f)
                start = Vector4.Lerp(start, end, startDistance / (startDistance - endDistance));
            else if (endDistance < 0f)
                end = Vector4.Lerp(start, end, startDistance / (startDistance - endDistance));
            return true;
        }

        private static void DrawWorldAxes(Graphics g, Font font, Mat4 viewProjection, int w, int h)
        {
            Vector4 origin = viewProjection.Transform(new Vector4(0f, 0f, 0f, 1f));

            var axes = new (Vector3 end, string label, Brush brush)[]
            {
                (new Vector3(2f, 0f, 0f), "X", Brushes.Salmon),
                (new Vector3(0f, 2f, 0f), "Y", Brushes.LimeGreen),
                (new Vector3(0f, 0f, 2f), "Z", Brushes.DeepSkyBlue)
            };
            foreach (var (end, label, brush) in axes)
            {
                Vector4 axisStart = origin;
                Vector4 axisEnd = viewProjection.Transform(new Vector4(end, 1f));
                if (!ClipDepth(ref axisStart, ref axisEnd)) continue;

                PointF projectedEnd = ToScreen(axisEnd, w, h);
                using var pen = new Pen(brush, 1f);
                g.DrawLine(pen, ToScreen(axisStart, w, h), projectedEnd);
                float labelX = Math.Clamp(projectedEnd.X + 5f, 0f, Math.Max(0f, w - 16f));
                float labelY = Math.Clamp(projectedEnd.Y - 12f, 0f, Math.Max(0f, h - 18f));
                g.DrawString(label, font, brush, labelX, labelY);
            }
        }

        private void SelectSpiralAxis(char axis)
        {
            if (_spiralAxis == axis) return;
            _spiralAxis = axis;
            _spiralClock.Reset();
        }

        private void ToggleSpiral()
        {
            if (_spiralClock.IsRunning)
                _spiralClock.Stop();
            else
            {
                if (_spiralClock.Elapsed.TotalSeconds >= SpiralDurationSec)
                    _spiralClock.Reset();
                _spiralClock.Start();
            }
            UpdateSpiral();
        }

        private void UpdateSpiral()
        {
            double elapsed = _spiralClock.Elapsed.TotalSeconds;
            if (elapsed >= SpiralDurationSec) _spiralClock.Stop();

            float t = (float)Math.Min(elapsed / SpiralDurationSec, 1d);
            // Замедляются и подъём вдоль оси, и вращение вокруг неё.
            float progress = t * (2f - t);
            float theta = 2f * MathF.PI * SpiralTurns * progress;
            float along = SpiralLength * (progress - 0.5f);
            float radialCos = SpiralRadius * MathF.Cos(theta);
            float radialSin = SpiralRadius * MathF.Sin(theta);

            _angleX = _angleY = _angleZ = 0f;
            switch (_spiralAxis)
            {
                case 'X':
                    (_tx, _ty, _tz) = (along, radialCos, radialSin);
                    _angleX = theta;
                    break;
                case 'Y':
                    (_tx, _ty, _tz) = (radialCos, along, -radialSin);
                    _angleY = theta;
                    break;
                case 'Z':
                    (_tx, _ty, _tz) = (radialCos, radialSin, along);
                    _angleZ = theta;
                    break;
            }
            _scale = 1f;
        }
    }
}







































//using System.Drawing;
//using System.Drawing.Text;
//using System.Numerics;
//using System.Windows.Forms;

//namespace test5
//{
//    public partial class MainForm : Form
//    {
//        // private readonly WireObject _obj = WireObject.CreateCube(1.5f);
//        private readonly WireObject _obj = WireObject.CreateDigit3(1.5f, 0.4f);

//        public MainForm()
//        {
//            InitializeComponent();
//            KeyPreview = true;
//            DoubleBuffered = true;

//            BackColor = Color.Black;
//            ClientSize = new System.Drawing.Size(1000, 700);
//            Text = "Графика 1";

//            _timer = new System.Windows.Forms.Timer { Interval = 16 }; 
//            _timer.Tick += (s, e) =>
//            {
//                UpdateAnimation(0.016f);
//                Invalidate();
//            };
//            _timer.Start();
//        }

//        private float _angleX = 0.0f;
//        private float _angleY = 0.0f;
//        private float _tx = 0.0f, _ty = 0.0f, _tz = 0.0f;
//        private float _scale = 1.0f;
//        private bool _usePerspective = true;
//        private float _cameraZ = 5f;

//        // для мыши
//        private Point _lastMouse;
//        private bool _dragging;

//        // Состояние анимации спирали
//        private bool _animationEnabled = false;
//        private readonly System.Windows.Forms.Timer _timer;
//        private float _spiralT = 0f;                       // прогресс
//        private const float SpiralRadius = 1.5f;           // радиус спирали
//        private const float SpiralHeight = 3.0f;           // высота подъёма
//        private const float SpiralTurns = 3.0f;           // число витков
//        private const float SpiralDurationSec = 6.0f;      // длительность одного прохода


//        protected override void OnPaint(PaintEventArgs e)
//        {
//            base.OnPaint(e);


//            int w = ClientSize.Width;
//            int h= ClientSize.Height;
//            float cx = w / 2f;
//            float cy = h / 2f;


//            const float k = 200f; // кол-во пикселей на единицу длины

//            Mat4 model =
//                Mat4.Translation(_tx, _ty, _tz)
//                * Mat4.RotationY(_angleY)
//                * Mat4.RotationX(_angleX)
//                * Mat4.Scale(_scale, _scale, _scale);



//            Mat4 view = Mat4.Translation(0, 0, -2*_cameraZ);

//            float aspect = (float)w / h;

//            Mat4 proj = _usePerspective 
//                ? Mat4.Perspective(MathF.PI / 3f, aspect, 0.1f, 100f)
//                : Mat4.Orthographic(-2f * aspect, 2f * aspect, -2f, 2f, 0.1f, 100f);

//            // итоговая матрица
//            Mat4 mvp = proj * view * model;
//            DrawAxes(e.Graphics, mvp, w, h);


//            var screen = new PointF[_obj.Vertices.Length];
//            for (int i = 0; i < _obj.Vertices.Length; i++)
//            { 
//                var v = _obj.Vertices[i];

//                Vector4 t = mvp.Transform(new Vector4(v.X, v.Y, v.Z, 1f));
//                if (MathF.Abs(t.W) < 1e-6f) { screen[i] = new PointF(float.NaN, float.NaN); continue; }

//                float ndcX = t.X / t.W;
//                float ndcY = t.Y / t.W;

//                screen[i] = new PointF
//                    (
//                        cx + ndcX * cx, // Рястягиваем ndc в пиксели
//                        cy - ndcY * cy // инвертирование y
//                    );
//            }

//            using var pen = new Pen(Color.White, 1.5f);
//            foreach (var (a, b) in _obj.Edges)
//                e.Graphics.DrawLine(pen, screen[a], screen[b]);


//            using var font = new Font("Consolas", 10);
//            string info =
//                $"Projection: {(_usePerspective ? "Перспективная" : "ортографическая")}\n" +
//                $"T = ({_tx:F2}, {_ty:F2})  Scale = {_scale:F2}  CamZ = {_cameraZ:F2}\n" +
//                $"RotX = {_angleX:F2}  RotY = {_angleY:F2}\n" +
//                $"WASD — сдвиг, QE — камера, IJKL — вращение, +/- — масштаб, P — проекция, Space — сброс";
//            e.Graphics.DrawString(info, font, Brushes.Lime, 10, 10);
//        }

//        protected override void OnKeyDown(KeyEventArgs e)
//        {
//            base.OnKeyDown(e);
//            const float step = 0.1f;

//            switch (e.KeyCode)
//            { 
//                case Keys.A: _tx -= step; break;
//                case Keys.D: _tx += step; break;
//                case Keys.W: _ty += step; break;
//                case Keys.S: _ty -= step; break;
//                case Keys.Q: _cameraZ -= step; break;
//                case Keys.E: _cameraZ += step; break;

//                case Keys.J: _angleY -= 0.1f; break;
//                case Keys.L: _angleY += 0.1f; break;
//                case Keys.I: _angleX -= 0.1f; break;
//                case Keys.K: _angleX += 0.1f; break;

//                case Keys.Oemplus:
//                case Keys.Add: _scale *= 1.1f; break;
//                case Keys.OemMinus:
//                case Keys.Subtract: _scale /= 1.1f; break;

//                case Keys.P: _usePerspective = !_usePerspective; break;

//                case Keys.M: _animationEnabled = !_animationEnabled;  break;

//                case Keys.Space:
//                    _tx = _ty = 0; _angleX = _angleY = 0; _scale = 1f; _cameraZ = 5f;
//                    break;
//            }
//            Invalidate();
//        }

//        protected override void OnMouseDown(MouseEventArgs e)
//        {
//            if (e.Button == MouseButtons.Left) 
//            { _dragging = true; _lastMouse = e.Location; }
//        }

//        protected override void OnMouseUp(MouseEventArgs e)
//        {
//            if (e.Button == MouseButtons.Left) _dragging = false;
//        }

//        protected override void OnMouseMove(MouseEventArgs e)
//        {
//            if (!_dragging) return;
//            float dx = e.X - _lastMouse.X;
//            float dy = e.Y - _lastMouse.Y;
//            _lastMouse = e.Location;

//            _angleY += dx * 0.01f;
//            _angleX += dy * 0.01f;
//            Invalidate();
//        }

//        private PointF? Project(Mat4 mvp, Vector3 v, int w, int h)
//        {
//            Vector4 t = mvp.Transform(new Vector4(v.X, v.Y, v.Z, 1f));
//            if (MathF.Abs(t.W) < 1e-6f) return null;
//            return new PointF(
//                w / 2f + (t.X / t.W) * (w / 2f),
//                h / 2f - (t.Y / t.W) * (h / 2f)
//            );
//        }

//        private void DrawAxes(Graphics g, Mat4 mvp, int w, int h)
//        {
//            var axes = new (Vector3 a, Vector3 b, Color c)[]
//            {
//        (new Vector3(0,0,0), new Vector3(2,0,0), Color.Red),
//        (new Vector3(0,0,0), new Vector3(0,2,0), Color.Green),
//        (new Vector3(0,0,0), new Vector3(0,0,2), Color.Blue),
//            };
//            foreach (var (a, b, c) in axes)
//            {
//                var pa = Project(mvp, a, w, h);
//                var pb = Project(mvp, b, w, h);
//                if (pa.HasValue && pb.HasValue)
//                    using (var pen = new Pen(c, 1f)) g.DrawLine(pen, pa.Value, pb.Value);
//            }
//        }

//        private void UpdateAnimation(float dt)
//        {
//            if (!_animationEnabled) return;

//            _spiralT += dt / SpiralDurationSec;
//            if (_spiralT > 1f) _spiralT = 0f; // зацикливаем

//            //  Замедление
//            float tEased = 1f - (1f - _spiralT) * (1f - _spiralT);

//            // Угол вдоль спирали
//            float theta = 2f * MathF.PI * SpiralTurns * tEased;

//            // Позиция центра объекта — по винтовой линии вокруг Y
//            _tx = SpiralRadius * MathF.Cos(theta);
//            _tz = SpiralRadius * MathF.Sin(theta);
//            _ty = SpiralHeight * tEased - SpiralHeight / 2f; // центрируем по вертикали

//            // вращение
//            _angleY = theta;
//            _angleX = 0f;
//            _scale = 1f;
//        }
//    }
//}
