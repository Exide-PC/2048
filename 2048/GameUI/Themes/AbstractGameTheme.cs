using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace _2048.GameUI.Themes
{
    abstract class AbstractGameTheme
    {
        public abstract Brush WindowBackground { get; }

        public abstract Brush GridLineBrush { get; }
        public abstract Brush CellBackround { get; }

        public abstract Brush InfoBlockBackground { get; }
        public abstract Brush InfoBlockUpperForeground { get; }
        public abstract Brush InfoBlockLowerForeground { get; }

        public abstract Brush GameOverBackground { get; }
        public abstract Brush GameOverForeground { get; }

        public abstract List<Tuple<Color, Color>> BackForeColorTuples { get; }

        //public abstract void UpdateVisuals(NumericRect tile, int number);
    }
}
