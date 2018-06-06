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
    /// Interaction logic for InfoBlock.xaml
    /// </summary>
    public partial class InfoBlock : UserControl
    {
        public string UpperText
        {
            get => (string)GetValue(UpperTextProperty);
            set => SetValue(UpperTextProperty, value);
        }

        public string LowerText
        {
            get => (string)GetValue(LowerTextProperty);
            set => SetValue(LowerTextProperty, value);
        }

        public new Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        public Brush UpperForeground
        {
            get => (Brush)GetValue(UpperForegroundProperty);
            set => SetValue(UpperForegroundProperty, value);
        }

        public Brush LowerForeground
        {
            get => (Brush)GetValue(LowerForegroundProperty);
            set => SetValue(LowerForegroundProperty, value);
        }

        public static readonly DependencyProperty UpperTextProperty;
        public static readonly DependencyProperty LowerTextProperty;
        public static new readonly DependencyProperty BackgroundProperty;
        //public static new readonly DependencyProperty ForegroundProperty;
        public static readonly DependencyProperty UpperForegroundProperty;
        public static readonly DependencyProperty LowerForegroundProperty;
        

        static InfoBlock()
        {
            UpperTextProperty = DependencyProperty.Register("UpperText", typeof(string), typeof(InfoBlock), new PropertyMetadata("Upper text"));
            LowerTextProperty = DependencyProperty.Register("LowerText", typeof(string), typeof(InfoBlock), new PropertyMetadata("Lower text"));

            BackgroundProperty = DependencyProperty.Register("Background", typeof(Brush), typeof(InfoBlock), 
                new PropertyMetadata(new SolidColorBrush(Colors.Indigo)));
            UpperForegroundProperty = DependencyProperty.Register("UpperForeground", typeof(Brush), typeof(InfoBlock),
                new PropertyMetadata(new SolidColorBrush(Colors.White)));
            LowerForegroundProperty = DependencyProperty.Register("LowerForeground", typeof(Brush), typeof(InfoBlock),
                new PropertyMetadata(new SolidColorBrush(Colors.White)));
        }

        public InfoBlock()
        {
            InitializeComponent();
        }
    }
}
