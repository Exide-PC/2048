using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace _2048.GameUI
{
    class TileStyle: Style
    {
        public TileStyle(string text, Color background, Color foreground)
        {
            TargetType = typeof(NumericRect);

            this.Setters.Add(new Setter(NumericRect.TextProperty, text));
            this.Setters.Add(new Setter(NumericRect.BackgroundProperty, new SolidColorBrush(background)));
            this.Setters.Add(new Setter(NumericRect.ForegroundProperty, new SolidColorBrush(foreground)));
        }
    }
}
