using _2048.GameUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _2048
{
    /// <summary>
    /// Interaction logic for NumericRect.xaml
    /// </summary>
    public partial class NumericRect : UserControl
    {
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public new Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        public new Brush Foreground
        {
            get => (Brush)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        //public new Thickness Margin
        //{
        //    get => (Thickness)GetValue(MarginProperty);
        //    set => SetValue(MarginProperty, value);
        //}


        public static readonly DependencyProperty TextProperty;
        public static new readonly DependencyProperty BackgroundProperty;
        public static new readonly DependencyProperty ForegroundProperty;
        //public static new readonly DependencyProperty MarginProperty;

        static double CIRCLE_RATIO = 0.3;
        
        public static double CalculateCornerRadius(double height, double width)
        {
            // Определяем что меньше, высота или ширина.
            double least = height < width ? height : width;

            // По результату вычисляем радиус круга, описывающего угол
            double radius = least * CIRCLE_RATIO / 2;
            return radius;
        }

      

        static NumericRect()
        {
            TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(NumericRect), new PropertyMetadata("SomeText"));
            BackgroundProperty = DependencyProperty.Register("Background", typeof(Brush),
                typeof(NumericRect), new PropertyMetadata(new SolidColorBrush(Colors.Coral)));
            ForegroundProperty = DependencyProperty.Register("Foreground", typeof(Brush),
                typeof(NumericRect), new PropertyMetadata(new SolidColorBrush(Colors.White)));
        }

        public NumericRect()
        {
            InitializeComponent();

            this.SizeChanged += (obj, args) =>
            {
                double height = args.NewSize.Height;
                double width = args.NewSize.Width;
                
                double radius = CalculateCornerRadius(height, width);
                this.Resources["CornerRadius"] = new CornerRadius(radius); //cornerRadius;
            };
        }
    }
}
