using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            Worker.Abort();
        }

        private static void Work(object text)
        {
            Loading form = new Loading((string)text);
            form.DoubleBuffered = true;

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
            SizeF textSize = e.Graphics.MeasureString(Text, Font);

            GraphicsState state = e.Graphics.Save();

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
