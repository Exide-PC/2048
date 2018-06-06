using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace _2048.GameUI
{
    class Gradient
    {
        Color[] colors;
        double step;

        public Gradient(Color[] colors)
        {
            if (colors == null || colors.Length == 1)
                throw new ArgumentException("colors");

            this.colors = colors.ToArray();
            this.step = (double)1 / (colors.Length - 1);
        }

        public Color GetColorAt(double rate)
        {
            if (rate < 0 || rate > 1)
                throw new ArgumentException("rate");

            int firstColorIndex = (int)(rate / step);
            if (rate == 1) firstColorIndex--;

            double newRate = (rate - (firstColorIndex * step)) / step;

            return ColorBetween(colors[firstColorIndex], colors[firstColorIndex + 1], newRate);
        }

        Color ColorBetween(Color color1, Color color2, double rate)
        {
            int deltaA = color2.A - color1.A;
            int deltaR = color2.R - color1.R;
            int deltaG = color2.G - color1.G;
            int deltaB = color2.B - color1.B;

            byte newA = (byte)(color1.A + deltaA * rate);
            byte newR = (byte)(color1.R + deltaR * rate);
            byte newG = (byte)(color1.G + deltaG * rate);
            byte newB = (byte)(color1.B + deltaB * rate);

            return Color.FromArgb(newA, newR, newG, newB);
        }
    }
}
