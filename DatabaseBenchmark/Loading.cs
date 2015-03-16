using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;

namespace DatabaseBenchmark
{
    public partial class Loading : Form
    {
        private static Thread Worker;

        private float Angle;
        private string Text;

        public Loading(string loadingText)
        {
            InitializeComponent();

            Text = loadingText;
            Angle = 0;
        }

        public static void Start(string loadingText)
        {
            Worker = new Thread(new ParameterizedThreadStart(Work));
            Worker.Start(loadingText);
        }

        public static void Stop()
        {
            if (Worker == null)
                return;

            Worker.Abort();
            Worker = null;
        }

        private static void Work(object text)
        {
            Loading form = new Loading((string)text);

            Application.Run(form);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
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
            graphics.TranslateTransform(Width / 2  - img.Width / 2, Height / 2 - img.Height / 2);
            graphics.TranslateTransform(img.Width / 2, img.Height / 2);
            graphics.RotateTransform(Angle);
            graphics.TranslateTransform(-img.Width / 2, -img.Height / 2);
            graphics.DrawImage(img, Point.Empty);

            graphics.Restore(state);

            Font font = new Font("Times New Roman", 12.0f, FontStyle.Bold);
            graphics.DrawString(Text, font, Brushes.Black, new PointF(Width / 2 - textSize.Width / 2, Height / 2 + img.Height / 2 + 10 ));
        }
    }
}
