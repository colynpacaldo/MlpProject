using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace MlpProject
{
    /// <summary>One named loss curve (one loss value per epoch).</summary>
    public sealed class LossSeries
    {
        /// <summary>Creates a series.</summary>
        /// <param name="name">Legend label.</param>
        /// <param name="values">Loss after each epoch; index 0 is epoch 1.</param>
        public LossSeries(string name, IReadOnlyList<double> values)
        {
            Name = name;
            Values = values ?? new double[0];
        }

        /// <summary>Gets the legend label.</summary>
        public string Name { get; }

        /// <summary>Gets the loss values.</summary>
        public IReadOnlyList<double> Values { get; }
    }

    /// <summary>
    /// A small dependency-free line chart (GDI+) that plots training loss against epoch.
    /// Shows a legend automatically when more than one series is supplied.
    /// </summary>
    public sealed class LossChartPanel : Panel
    {
        private static readonly Color[] Palette =
        {
            Color.FromArgb(31, 119, 180), Color.FromArgb(214, 39, 40), Color.FromArgb(44, 160, 44),
            Color.FromArgb(255, 127, 14), Color.FromArgb(148, 103, 189), Color.FromArgb(23, 190, 207),
            Color.FromArgb(127, 127, 127), Color.FromArgb(188, 189, 34)
        };

        private IList<LossSeries> _series = new List<LossSeries>();

        /// <summary>Creates the chart with a white, double-buffered surface.</summary>
        public LossChartPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.White;
        }

        /// <summary>Replaces the plotted curves and redraws.</summary>
        public void SetSeries(IEnumerable<LossSeries> series)
        {
            _series = series == null
                ? new List<LossSeries>()
                : series.Where(s => s != null && s.Values.Count > 0).ToList();
            Invalidate();
        }

        /// <inheritdoc />
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var titleFont = new Font("Segoe UI", 9.5F, FontStyle.Bold))
            using (var textFont = new Font("Segoe UI", 8F))
            using (var textBrush = new SolidBrush(Color.FromArgb(70, 70, 70)))
            using (var axisPen = new Pen(Color.FromArgb(120, 120, 120)))
            using (var gridPen = new Pen(Color.FromArgb(228, 228, 228)))
            {
                g.DrawString("Training loss (binary cross-entropy) per epoch", titleFont, textBrush, 8, 4);

                if (_series.Count == 0)
                {
                    g.DrawString("Run training or tuning to plot the loss curve.", textFont, textBrush, 60, Height / 2f - 8);
                    return;
                }

                int rightMargin = _series.Count > 1 ? 210 : 20;
                var plot = new Rectangle(55, 30, Math.Max(50, Width - 55 - rightMargin), Math.Max(50, Height - 30 - 40));

                double yMax = _series.Max(s => s.Values.Where(v => !double.IsNaN(v) && !double.IsInfinity(v)).DefaultIfEmpty(1.0).Max());
                if (yMax <= 0.0) yMax = 1.0;
                yMax *= 1.05;
                int maxPoints = _series.Max(s => s.Values.Count);
                const int ticks = 5;

                for (int i = 0; i <= ticks; i++)
                {
                    float y = plot.Bottom - (float)i / ticks * plot.Height;
                    g.DrawLine(gridPen, plot.Left, y, plot.Right, y);

                    string yLabel = (yMax * i / ticks).ToString("0.00");
                    SizeF ySize = g.MeasureString(yLabel, textFont);
                    g.DrawString(yLabel, textFont, textBrush, plot.Left - ySize.Width - 2, y - ySize.Height / 2);

                    float x = plot.Left + (float)i / ticks * plot.Width;
                    double epoch = maxPoints <= 1 ? 1 : 1 + (maxPoints - 1.0) * i / ticks;
                    string xLabel = Math.Round(epoch).ToString("0");
                    SizeF xSize = g.MeasureString(xLabel, textFont);
                    g.DrawString(xLabel, textFont, textBrush, x - xSize.Width / 2, plot.Bottom + 3);
                }

                g.DrawString("Epoch", textFont, textBrush, plot.Left + plot.Width / 2f - 15, plot.Bottom + 20);
                g.DrawRectangle(axisPen, plot);

                for (int s = 0; s < _series.Count; s++)
                {
                    IReadOnlyList<double> values = _series[s].Values;
                    var points = new PointF[values.Count];
                    for (int i = 0; i < values.Count; i++)
                    {
                        double v = double.IsNaN(values[i]) || double.IsInfinity(values[i]) ? yMax : values[i];
                        float x = plot.Left + (maxPoints <= 1 ? 0f : (float)i / (maxPoints - 1) * plot.Width);
                        float y = plot.Bottom - (float)(v / yMax) * plot.Height;
                        points[i] = new PointF(x, y);
                    }

                    using (var pen = new Pen(Palette[s % Palette.Length], 2F))
                    {
                        if (points.Length >= 2)
                        {
                            g.DrawLines(pen, points);
                        }
                        else
                        {
                            g.DrawEllipse(pen, points[0].X - 2, points[0].Y - 2, 4, 4);
                        }

                        if (_series.Count > 1)
                        {
                            float legendY = plot.Top + 8 + s * 18;
                            g.DrawLine(pen, plot.Right + 12, legendY, plot.Right + 32, legendY);
                            g.DrawString(_series[s].Name, textFont, textBrush, plot.Right + 36, legendY - 7);
                        }
                    }
                }
            }
        }
    }
}
