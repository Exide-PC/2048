using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace _2048.GameUI.Themes
{
    class Theme2048 : AbstractGameTheme
    {
        /// <summary>
        /// Кортеж цветов для переданной плитки. 1 элемент - цвет заливки, 2 - цвет текста
        /// </summary>
        public override List<Tuple<Color, Color>> BackForeColorTuples { get; } = new List<Tuple<Color, Color>>();

        public override Brush WindowBackground => new SolidColorBrush(Color.FromArgb(0xff, 0xfa, 0xf8, 0xef));

        public override Brush GridLineBrush => new SolidColorBrush(Color.FromArgb(0xff, 0xbb, 0xad, 0xa0));
        public override Brush CellBackround => new SolidColorBrush(Color.FromArgb(0xff, 0xcd, 0xc0, 0xb4));

        public override Brush InfoBlockBackground => new SolidColorBrush(Colors.IndianRed);
        public override Brush InfoBlockUpperForeground => new SolidColorBrush(Colors.LightGray);
        public override Brush InfoBlockLowerForeground => new SolidColorBrush(Colors.White);

        public override Brush GameOverBackground => new SolidColorBrush(Colors.IndianRed);
        public override Brush GameOverForeground => new SolidColorBrush(Colors.White);

        public Theme2048()
        {
            Color darkFore = Color.FromArgb(0xff, 0x77, 0x6e, 0x65);
            Color lightFore = Color.FromArgb(0xff, 0xf9, 0xf6, 0xf2);

            BackForeColorTuples.Add(new Tuple<Color, Color>(Color.FromArgb(0xff, 0xee, 0xe4, 0xda), darkFore));
            BackForeColorTuples.Add(new Tuple<Color, Color>(Color.FromArgb(0xff, 0xed, 0xe0, 0xc8), darkFore));
            BackForeColorTuples.Add(new Tuple<Color, Color>(Color.FromArgb(0xff, 0xf2, 0xb1, 0x79), lightFore));
            BackForeColorTuples.Add(new Tuple<Color, Color>(Color.FromArgb(0xff, 0xf5, 0x95, 0x63), lightFore));
            BackForeColorTuples.Add(new Tuple<Color, Color>(Color.FromArgb(0xff, 0xf6, 0x7c, 0x5f), lightFore));
            BackForeColorTuples.Add(new Tuple<Color, Color>(Color.FromArgb(0xff, 0xf6, 0x5e, 0x3b), lightFore));
            BackForeColorTuples.Add(new Tuple<Color, Color>(Color.FromArgb(0xff, 0xed, 0xcf, 0x72), lightFore));
            BackForeColorTuples.Add(new Tuple<Color, Color>(Color.FromArgb(0xff, 0xed, 0xcc, 0x61), lightFore));
            BackForeColorTuples.Add(new Tuple<Color, Color>(Color.FromArgb(0xff, 0xed, 0xc8, 0x50), lightFore));
            BackForeColorTuples.Add(new Tuple<Color, Color>(Color.FromArgb(0xff, 0xed, 0xc5, 0x3f), lightFore));
            BackForeColorTuples.Add(new Tuple<Color, Color>(Color.FromArgb(0xff, 0xed, 0xc2, 0x2e), lightFore));
        }

        //public override void UpdateVisuals(NumericRect tile, int number)
        //{
        //    int valueToShow = (int)Math.Pow(2, number);
        //    tile.Text = valueToShow.ToString();

        //    Tuple<Color, Color> selectedTuple;

        //    if (number > 0 && number <= this.BackForeColorTuples.Count)
        //        selectedTuple = BackForeColorTuples[number - 1];
        //    else
        //        // Если соответствия номеру в базе не нашлось, то возвращаем первый элемент
        //        selectedTuple = BackForeColorTuples[0];

        //    tile.Background = new SolidColorBrush(selectedTuple.Item1);
        //    tile.Foreground = new SolidColorBrush(selectedTuple.Item2);

        //    // Определяем, нужен ли эффект блюра вокруг квадрата
        //    int lowerBlurBound = 7;
        //    int upperBlurBound = 11;

        //    if (number >= lowerBlurBound && number <= upperBlurBound)
        //    {
        //        // Вычисляем возможное число состояний, т.к. если границы 7 и 11, то состояний будет 5, а не 4
        //        int possibleStateCount = upperBlurBound - lowerBlurBound + 1;
        //        // Вычисляем номер текущего состояния. Если число равно 7, то его номер 1, т.к. иначе мы получим прозрачность 0
        //        int currentStateNum = number - lowerBlurBound + 1;

        //        double opacity = ((double)currentStateNum / possibleStateCount) * 1; // 1 - максимальная прозрачность

        //        System.Windows.Media.Effects.DropShadowEffect shadow = 
        //            new System.Windows.Media.Effects.DropShadowEffect();

        //        shadow.Opacity = opacity;
        //        shadow.Color = BackForeColorTuples[number - 1].Item1; //selectedTuple.Item1;
        //        shadow.BlurRadius = 15;
        //        shadow.ShadowDepth = 0;

        //        tile.Effect = shadow;
        //    }
        //}
    }
}
