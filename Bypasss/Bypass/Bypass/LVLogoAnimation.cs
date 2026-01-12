using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace Bypass
{
    public class LVMarqueeAnimation
    {
        private Guna2Panel _panel;
        private System.Windows.Forms.Timer _animationTimer;
        private List<LVLogo> _logos;
        private Color _logoColor;
        private float _speed = 2.0f;
        private int _spacing = 150; // Spazio tra i loghi
        private Random _random;

        public Color LogoColor
        {
            get => _logoColor;
            set
            {
                _logoColor = value;
                UpdateLogosColor();
            }
        }

        public float Speed
        {
            get => _speed;
            set => _speed = value;
        }

        // Costruttore
        public LVMarqueeAnimation(Guna2Panel panel)
        {
            _panel = panel ?? throw new ArgumentNullException(nameof(panel));
            _logoColor = Color.FromArgb(255, 91, 90); // Colore specificato
            _random = new Random();
            _logos = new List<LVLogo>();

            SetupPanel();
            InitializeTimer();
            CreateLogoSequence();
        }

        private void SetupPanel()
        {
            _panel.Paint += Panel_Paint;
            _panel.Resize += (s, e) => ResetAnimation();
        }

        private void InitializeTimer()
        {
            _animationTimer = new System.Windows.Forms.Timer();
            _animationTimer.Interval = 16; // ~60 FPS
            _animationTimer.Tick += AnimationTimer_Tick;
        }

        private void CreateLogoSequence()
        {
            _logos.Clear();

            // Calcola quanti loghi servono per coprire l'intera larghezza
            int logoCount = (_panel.Width / _spacing) + 2; // +2 per quelli fuori schermo

            for (int i = 0; i < logoCount; i++)
            {
                var logo = new LVLogo
                {
                    Position = new PointF(i * _spacing, _panel.Height / 2),
                    Size = 60 + _random.Next(-10, 11), // Dimensione variabile tra 50-70
                    Speed = _speed,
                    Color = _logoColor,
                    LogoType = (i % 2 == 0) ? LVLogoType.Classic : LVLogoType.Modern,
                    Opacity = 0.7f + (float)_random.NextDouble() * 0.3f // Opacità variabile
                };

                // Posiziona alcuni loghi sopra e altri sotto per un effetto più dinamico
                if (i % 3 == 0)
                    logo.Position = new PointF(i * _spacing, _panel.Height / 3);
                else if (i % 3 == 1)
                    logo.Position = new PointF(i * _spacing, (2 * _panel.Height) / 3);

                _logos.Add(logo);
            }
        }

        private void UpdateLogosColor()
        {
            foreach (var logo in _logos)
            {
                logo.Color = _logoColor;
            }
            _panel.Invalidate();
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            bool needsRedraw = false;

            foreach (var logo in _logos)
            {
                // Muovi il logo verso sinistra
                logo.Position = new PointF(logo.Position.X - logo.Speed, logo.Position.Y);

                // Se il logo è completamente uscito a sinistra, riportalo a destra
                if (logo.Position.X + logo.Size < 0)
                {
                    logo.Position = new PointF(_panel.Width, logo.Position.Y);

                    // Cambia occasionalmente il tipo di logo e altre proprietà
                    if (_random.Next(0, 10) == 0)
                        logo.LogoType = (logo.LogoType == LVLogoType.Classic) ? LVLogoType.Modern : LVLogoType.Classic;

                    logo.Size = 60 + _random.Next(-10, 11);
                    logo.Opacity = 0.7f + (float)_random.NextDouble() * 0.3f;
                }

                needsRedraw = true;
            }

            if (needsRedraw)
                _panel.Invalidate();
        }

        private void Panel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Sfondo trasparente
            e.Graphics.Clear(Color.Transparent);

            // Disegna ogni logo con opacità
            foreach (var logo in _logos)
            {
                DrawLVLogo(e.Graphics, logo);
            }
        }

        private void DrawLVLogo(Graphics g, LVLogo logo)
        {
            // Crea un rettangolo per il logo
            RectangleF rect = new RectangleF(
                logo.Position.X,
                logo.Position.Y - logo.Size / 2, // Centra verticalmente
                logo.Size,
                logo.Size
            );

            // Crea colore con opacità
            Color logoColorWithAlpha = Color.FromArgb(
                (int)(logo.Opacity * 255),
                logo.Color.R,
                logo.Color.G,
                logo.Color.B
            );

            using (Pen pen = new Pen(logoColorWithAlpha, 2))
            using (Brush brush = new SolidBrush(Color.FromArgb((int)(logo.Opacity * 50), logo.Color)))
            {
                if (logo.LogoType == LVLogoType.Classic)
                {
                    DrawClassicLVLogo(g, rect, pen, brush);
                }
                else
                {
                    DrawModernLVLogo(g, rect, pen, brush);
                }
            }
        }

        private void DrawClassicLVLogo(Graphics g, RectangleF rect, Pen pen, Brush brush)
        {
            // Disegna sfondo ovale
            g.FillEllipse(brush, rect);
            g.DrawEllipse(pen, rect);

            // Disegna la "L"
            float padding = rect.Width * 0.2f;

            // L verticale
            g.DrawLine(pen,
                rect.Left + rect.Width * 0.3f,
                rect.Top + padding,
                rect.Left + rect.Width * 0.3f,
                rect.Bottom - padding
            );

            // L orizzontale
            g.DrawLine(pen,
                rect.Left + rect.Width * 0.3f,
                rect.Bottom - padding,
                rect.Left + rect.Width * 0.5f - padding / 2,
                rect.Bottom - padding
            );

            // Disegna la "V"
            // Linea sinistra della V
            g.DrawLine(pen,
                rect.Left + rect.Width * 0.5f + padding / 2,
                rect.Top + padding,
                rect.Left + rect.Width * 0.7f,
                rect.Bottom - padding
            );

            // Linea destra della V
            g.DrawLine(pen,
                rect.Left + rect.Width * 0.7f,
                rect.Bottom - padding,
                rect.Right - padding,
                rect.Top + padding
            );
        }

        private void DrawModernLVLogo(Graphics g, RectangleF rect, Pen pen, Brush brush)
        {
            // Disegna un quadrato con angoli arrotondati
            float cornerRadius = rect.Width * 0.1f;

            using (GraphicsPath path = CreateRoundedRectangle(rect, cornerRadius))
            {
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }

            // Disegna "LV" stilizzato
            float padding = rect.Width * 0.2f;

            // L stilizzata (più spessa)
            using (Pen thickPen = new Pen(pen.Color, 3))
            {
                // L verticale
                g.DrawLine(thickPen,
                    rect.Left + padding,
                    rect.Top + padding,
                    rect.Left + padding,
                    rect.Bottom - padding
                );

                // L orizzontale
                g.DrawLine(thickPen,
                    rect.Left + padding,
                    rect.Bottom - padding,
                    rect.Left + rect.Width * 0.45f,
                    rect.Bottom - padding
                );

                // V stilizzata
                g.DrawLine(thickPen,
                    rect.Left + rect.Width * 0.55f,
                    rect.Top + padding,
                    rect.Left + rect.Width * 0.7f,
                    rect.Bottom - padding
                );

                g.DrawLine(thickPen,
                    rect.Left + rect.Width * 0.7f,
                    rect.Bottom - padding,
                    rect.Right - padding,
                    rect.Top + padding * 1.5f
                );
            }
        }

        private GraphicsPath CreateRoundedRectangle(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();

            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();

            return path;
        }

        // Metodi pubblici
        public void StartAnimation()
        {
            if (!_animationTimer.Enabled)
            {
                _animationTimer.Start();
            }
        }

        public void StopAnimation()
        {
            _animationTimer.Stop();
        }

        public void PauseAnimation()
        {
            _animationTimer.Stop();
        }

        public void ResumeAnimation()
        {
            _animationTimer.Start();
        }

        public void ResetAnimation()
        {
            CreateLogoSequence();
            _panel.Invalidate();
        }

        public void SetDirection(bool leftToRight)
        {
            _speed = Math.Abs(_speed) * (leftToRight ? -1 : 1);
            foreach (var logo in _logos)
            {
                logo.Speed = _speed;
            }
        }

        public void SetLogoSize(int minSize, int maxSize)
        {
            foreach (var logo in _logos)
            {
                logo.Size = _random.Next(minSize, maxSize + 1);
            }
            _panel.Invalidate();
        }

        public void Dispose()
        {
            if (_animationTimer != null)
            {
                _animationTimer.Stop();
                _animationTimer.Dispose();
            }
        }
    }

    // Classe helper per il logo
    public class LVLogo
    {
        public PointF Position { get; set; }
        public float Size { get; set; }
        public float Speed { get; set; }
        public Color Color { get; set; }
        public LVLogoType LogoType { get; set; }
        public float Opacity { get; set; } = 1.0f;
    }

    public enum LVLogoType
    {
        Classic,
        Modern
    }
}