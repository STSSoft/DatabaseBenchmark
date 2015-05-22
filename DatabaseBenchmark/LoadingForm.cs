using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;

namespace DatabaseBenchmark
{
    public partial class LoadingForm : Form
    {
        private static bool Stopped;
        private static Thread Worker;

        private float Angle;

        public LoadingForm(string loadingText, Rectangle bounds)
        {
            InitializeComponent();

            Bounds = bounds;
            Text = loadingText;
            Angle = 0;
        }

        public static void Start(string loadingText, Rectangle formBounds)
        {
            Stopped = false;

            Worker = new Thread(new ParameterizedThreadStart(Work));
            Worker.Start(new KeyValuePair<string, Rectangle>(loadingText, formBounds));
        }

        public static void Stop()
        {
            if (Worker == null)
                return;

            Stopped = true;

            try
            {
                Worker.Join(300);
            }
            finally
            {
                Worker = null;
            }
        }

        private static void Work(object settings)
        {
            KeyValuePair<string, Rectangle> kv = (KeyValuePair<string, Rectangle>)settings;
            LoadingForm form = new LoadingForm(kv.Key, kv.Value);

            Application.Run(form);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Stopped)
            {
                Close();
                return;
            }

            Angle += 30;

            Invalidate();
        }

        private void Loading_Paint(object sender, PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            Image img = DatabaseBenchmark.Properties.Resources.loading_throbber_icon;
            SizeF textSize = graphics.MeasureString(Text, Font);

            GraphicsState state = graphics.Save();

            graphics.SmoothingMode = SmoothingMode.HighQuality;

            // Draw and rotate image.
            graphics.TranslateTransform(Width / 2 - img.Width / 2, Height / 2 - img.Height / 2);
            graphics.TranslateTransform(img.Width / 2, img.Height / 2);
            graphics.RotateTransform(Angle);
            graphics.TranslateTransform(-img.Width / 2, -img.Height / 2);
            graphics.DrawImage(img, Point.Empty);

            graphics.Restore(state);

            Font font = new Font("Times New Roman", 12.0f, FontStyle.Bold);
            graphics.DrawString(Text, font, Brushes.Black, new PointF(Width / 2 - textSize.Width / 2, Height / 2 + img.Height / 2 + 10));
        }
    }
}
