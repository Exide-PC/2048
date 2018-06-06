using _2048.GameUI;
using _2048.GameUI.Themes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace _2048
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IObserver<Game.GameArgs>
    {
        string cfgPath = "2048.xml";
        IniSettings cfg;
        Point startPoint;
        AbstractGameTheme theme;
        Game game;
        bool canMove = true;

        public MainWindow()
        {
            InitializeComponent();
        }

        // Утилитарное свойство, т.к. Point нельзя задать как null
        Point DefaultPoint => new Point(-1, -1);

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            XmlSerializer iniSerializer = new XmlSerializer(typeof(IniSettings));

            if (!File.Exists(cfgPath))
            {
                IniSettings defaultSettings = new IniSettings();

                using (FileStream fs = File.OpenWrite(cfgPath))
                    iniSerializer.Serialize(fs, defaultSettings);
            }

            using (FileStream fs = File.OpenRead(cfgPath))
                this.cfg = (IniSettings)iniSerializer.Deserialize(fs);

            // Создаем экземпляр игрового поля
            this.game = new Game(cfg.Height, cfg.Width);
            // Задаем тему
            this.theme = new Theme2048();

            // подписываемся на игровые события
            this.game.Subscribe(this);
            this.startPoint = DefaultPoint;

            // Задаем фон приложения
            this.mainGrid.Background = this.theme.WindowBackground;

            // Разово задаем цвет линий поля и фон поля
            this.Resources["GridLineBrush"] = this.theme.GridLineBrush;
            this.Resources["GridCellBackground"] = this.theme.CellBackround;

            // Задаем цвета информационных блоков согласно теме
            this.undoInfoBlock.Background = this.theme.InfoBlockBackground;
            this.undoInfoBlock.UpperForeground = this.theme.InfoBlockUpperForeground;
            this.undoInfoBlock.LowerForeground = this.theme.InfoBlockLowerForeground;

            UpdateResources(); // обновляем первый раз ресурсы
            GenerateField(); // и по заданным значениям заполним поле ячейками
        }

        void UpdateGame()
        {
            NumericRect[] rects = grid.Children.OfType<NumericRect>().ToArray();

            // обновляем информацию о числе попыток
            //this.rollbackTextblock.Text = $"Попыток: {this.game.RollbackCount}";
            this.undoInfoBlock.UpperText = $"{this.game.RollbackCount} left";
            // обновляем счет
            this.scoreInfoBlock.LowerText = $"{CalculateScore(this.game.MergeMap)}";

            // Удаляем все цифровые блоки
            foreach (NumericRect rect in rects)
                this.grid.Children.Remove(rect);

            // Удаляем экран GameOver, если такой есть
            Border gameOverBorder = grid.Children.OfType<Border>().FirstOrDefault();
            if (gameOverBorder != null)
                this.grid.Children.Remove(gameOverBorder);

            Viewbox gameOverText = grid.Children.OfType<Viewbox>().FirstOrDefault();
            if (gameOverText != null)
                this.grid.Children.Remove(gameOverText);


            for (int i = 0; i < this.game.Height; i++)
            {
                for (int j = 0; j < this.game.Width; j++)
                {
                    int value = this.game.Matrix[i, j];

                    // Находим прямоугольник, отвечающий за задний фон
                    //Rectangle backgroundRect = this.grid.Children.OfType<Rectangle>().First(
                    //        r => Grid.GetRow(r) == i && Grid.GetColumn(r) == j);

                    if (value > 0)
                    {
                        // Скрываем его, если в этой ячейке есть значение
                        //backgroundRect.Visibility = Visibility.Collapsed;

                        NumericRect rect = new NumericRect();
                        //rect.Text = value.ToString();
                        // задаем визуальную составляющую плитки
                        UpdateVisuals(rect, value);

                        grid.Children.Add(rect);

                        Grid.SetRow(rect, i);
                        Grid.SetColumn(rect, j);
                    }
                    else
                    {
                        // Если значения нет, то делаем прямоугольник, отвечающий за задний фон видимым
                        //backgroundRect.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        void GameOverDialog()
        {
            // Элемент, служащий для заливки игрового поля цветом согласно теме
            Border hugeBorder = new Border();
            hugeBorder.Background = this.theme.GameOverBackground;
            hugeBorder.Opacity = 0.4;

            grid.Children.Add(hugeBorder);

            Grid.SetColumnSpan(hugeBorder, grid.ColumnDefinitions.Count);
            Grid.SetRowSpan(hugeBorder, grid.RowDefinitions.Count);

            // Создаем viewbox для удобного масштабирования текста, задаем его цвет согласно теме
            Viewbox textViewBox = new Viewbox();
            TextBlock textBlock = new TextBlock();
            textBlock.Foreground = this.theme.GameOverForeground;
            textBlock.Text = "Game over";
            textViewBox.Child = textBlock;

            grid.Children.Add(textViewBox);
            Grid.SetColumn(textViewBox, 1);
            Grid.SetRow(textViewBox, 1);
            Grid.SetRowSpan(textViewBox, grid.RowDefinitions.Count - 2);
            Grid.SetColumnSpan(textViewBox, grid.ColumnDefinitions.Count - 2);
        }

        void UpdateResources()
        {
            if (cfg == null) return;

            double numRectRatio = 0.85;
            double gridLineRatio = 1 - numRectRatio;

            var gridSize = grid.RenderSize;
            double rowLength = gridSize.Height / cfg.Height;
            double colLength = gridSize.Width / cfg.Width;

            double least = rowLength > colLength ? colLength : rowLength;
            double singleLineThickness = least * gridLineRatio / 2;

            double numRectHeight;
            double numRectWidth;

            if (rowLength > colLength)
            {
                // если растянуто больше вертикально
                numRectWidth = colLength * numRectRatio;
                numRectHeight = rowLength - 2 * singleLineThickness;
            }
            else
            {
                // если сильно растянуто горизонтально
                numRectHeight = rowLength * numRectRatio;
                numRectWidth = colLength - 2 * singleLineThickness;
            }

            this.Resources["GridThickness"] = singleLineThickness;
            this.Resources["NumericRectHeight"] = numRectHeight;
            this.Resources["NumericRectWidth"] = numRectWidth;

            double cornerRadius = NumericRect.CalculateCornerRadius(numRectHeight, numRectWidth);
            this.Resources["CornerRadius"] = cornerRadius;
        }

        void GenerateField()
        {
            this.grid.Children.Clear();
            this.grid.RowDefinitions.Clear();
            this.grid.ColumnDefinitions.Clear();

            // добавляем строки
            for (int i = 0; i < this.cfg.Height; i++)
                this.grid.RowDefinitions.Add(new RowDefinition());

            // добавляем колонки
            for (int i = 0; i < this.cfg.Width; i++)
                this.grid.ColumnDefinitions.Add(new ColumnDefinition());

            // добавляем рамку в каждую ячейку таблицы
            for (int i = 0; i < this.cfg.Height; i++)
            {
                for (int j = 0; j < this.cfg.Width; j++)
                {
                    Rectangle cell = new Rectangle();
                    this.grid.Children.Add(cell);

                    Grid.SetRow(cell, i);
                    Grid.SetColumn(cell, j);
                }
            }
        }

        public void OnNext(Game.GameArgs gameArgs) // Вызывается в другом потоке, запущенном из обработчика нажатия клавиши
        {
            switch (gameArgs.Evt)
            {
                case Game.GameArgs.EvtType.GameStarted:
                    {
                        this.Dispatcher.Invoke(() => UpdateGame());
                        break;
                    }
                case Game.GameArgs.EvtType.Move:
                    {
                        // UI поток не блокируется, т.к. это другой поток
                        AnimateMotionAsync(gameArgs.SourceDestinationTuples, gameArgs.MergerPoints)
                            .Wait();

                        //this.Dispatcher.Invoke(() => UpdateGame());
                        break;
                    }
                case Game.GameArgs.EvtType.RandomAdded:
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            int height = (int)gameArgs.RandomPoint.Y;
                            int width = (int)gameArgs.RandomPoint.X;
                            int value = this.game.Matrix[height, width];
                            CreateNumberTile(height, width, value);
                        });
                        break;
                    }
                case Game.GameArgs.EvtType.Rollback:
                    {
                        this.Dispatcher.Invoke(() => UpdateGame());
                        break;
                    }
                case Game.GameArgs.EvtType.GameFinished:
                    {
                        this.Dispatcher.Invoke(() => GameOverDialog());                        
                        break;
                    }
            }
        }

        Task AnimateMotionAsync(IEnumerable<Tuple<Point, Point>> sourceDestinationPoints, IEnumerable<Point> mergerPoints)
        {
            return Task.Factory.StartNew(() =>
            {
                //Переменная, отвечающая подсчет завершившихся анимаций
                int finishedAnimationCount = 0;
                // Необходимое число анимаций
                int targetAnimationCount = sourceDestinationPoints.Count();
                // Вычисляемый флаг для получения информации о том, завершены ли все анимации

                // Узнаем высоту и ширину одной ячейки сетки ячейки
                double cellHeight = this.grid.ActualHeight / this.grid.RowDefinitions.Count;
                double cellWidth = this.grid.ActualWidth / this.grid.ColumnDefinitions.Count;

                // Как только задача будет запущена - из главного потока настроим и запустим все анимации
                this.grid.Dispatcher.Invoke(() =>
                {
                    // Временная шкала для всех анимаций
                    Storyboard storyboard = new Storyboard();

                    // Получим все элементы грида нужного типа               
                    IEnumerable<NumericRect> visibleRects = this.grid.Children.OfType<NumericRect>();

                    foreach (NumericRect visibleRect in visibleRects)
                    {
                        // Определим для текущего квадрата его строку и колонку в таблице
                        int rowNum = Grid.GetRow(visibleRect);
                        int colNum = Grid.GetColumn(visibleRect);

                        // Найдем кортеж в котором первый элемент - координаты откуда надо переместить квадрат
                        // Item1 = откуда надо переместить, т.е. исходные координаты квадрата на поле
                        Tuple<Point, Point> targetTuple = sourceDestinationPoints.FirstOrDefault(
                                tuple => (int)tuple.Item1.X == colNum && (int)tuple.Item1.Y == rowNum);

                        // Если текущий квадрат не входит в число тех, которые надо переместить, т.е. нужный кортеж не найден 
                        if (targetTuple == null) continue;

                        // Выделяем из кортежа информацию откуда и куда надо переместить квадрат
                        Point pFrom = targetTuple.Item1;
                        Point pTo = targetTuple.Item2;

                        // Вычисляем отступ
                        double vMargin = (cellHeight - visibleRect.ActualHeight) / 2;
                        double hMargin = (cellWidth - visibleRect.ActualWidth) / 2;

                        // В эти переменные запишутся вычисленные стартовые и конечные значения отступов
                        Thickness marginFrom;
                        Thickness marginTo;

                        // Если двиг произошел вдоль строки, т.е. изменилось значение X
                        if (pFrom.X != pTo.X)
                        {
                            int delta = (int)Math.Abs(pTo.X - pFrom.X); // Вычислим разницу между началом и концом
                            double highestMargin = hMargin + delta * cellWidth; // Определяем самый дальний отступ (направляющий)
                            Grid.SetColumnSpan(visibleRect, delta + 1); // Зададим допустимое значение ячеек, которое может занимать квадрат

                            if (pFrom.X < pTo.X) // движение вправо
                            {
                                marginFrom = new Thickness(hMargin, vMargin, highestMargin, vMargin);
                                marginTo = new Thickness(highestMargin, vMargin, hMargin, vMargin);
                            }
                            else
                            {
                                Grid.SetColumn(visibleRect, (int)pTo.X); // Зададим квадрату самую левую колонку, чтобы анимация отработала правильно
                                marginFrom = new Thickness(highestMargin, vMargin, hMargin, vMargin);
                                marginTo = new Thickness(hMargin, vMargin, highestMargin, vMargin);
                            }

                        }
                        else
                        {
                            // Иначе вдоль колонки
                            int delta = (int)Math.Abs(pTo.Y - pFrom.Y);
                            double highestMargin = vMargin + delta * cellHeight;
                            Grid.SetRowSpan(visibleRect, delta + 1);

                            if (pFrom.Y < pTo.Y) // движение вниз
                            {
                                marginFrom = new Thickness(hMargin, vMargin, hMargin, highestMargin);
                                marginTo = new Thickness(hMargin, highestMargin, hMargin, vMargin);
                            }
                            else
                            {
                                Grid.SetRow(visibleRect, (int)pTo.Y); // Изменим значение строки на самое верхнее, т.к. анимация идет снизу вверх
                                marginFrom = new Thickness(hMargin, highestMargin, hMargin, vMargin);
                                marginTo = new Thickness(hMargin, vMargin, hMargin, highestMargin);
                            }
                        }

                        ThicknessAnimation thicknessAnimation = new ThicknessAnimation(marginFrom, marginTo, TimeSpan.FromSeconds(0.15));
                        // По умолчанию стоит HoldEnd, которое не дает нам изменить вручную в дальнейшем, не подходит
                        thicknessAnimation.FillBehavior = FillBehavior.Stop;

                        thicknessAnimation.Completed += (_, __) =>
                        {
                            // вернем обратно допустимое число ячеек, которое может занимать элемент (1)
                            //if (pFrom.X != pTo.X) 
                            Grid.SetColumnSpan(visibleRect, 1); // если движение было по строке - восстановим макс. занимаемое число колонок
                            //else
                            Grid.SetRowSpan(visibleRect, 1); // иначе строк

                            // и зададим верные конечные координаты
                            Grid.SetRow(visibleRect, (int)pTo.Y);
                            Grid.SetColumn(visibleRect, (int)pTo.X);

                            // Исправим отступы на нулевые для сброса значения, установленного анимацией
                            visibleRect.Margin = new Thickness(0);

                            finishedAnimationCount++; // учтем, что текущая анимация завершена      
                        };

                        Storyboard.SetTarget(thicknessAnimation, visibleRect);
                        Storyboard.SetTargetProperty(thicknessAnimation, new PropertyPath(NumericRect.MarginProperty));
                        storyboard.Children.Add(thicknessAnimation);
                        //visibleRect.BeginAnimation(FrameworkElement.MarginProperty, null);
                        //visibleRect.BeginAnimation(FrameworkElement.MarginProperty, thicknessAnimation);
                    }

                    // Не блокируем значения по завершению анимаций
                    storyboard.FillBehavior = FillBehavior.Stop;
                    // Запустим анимации перемещения
                    storyboard.Begin();
                });

                // После запуска анимаций ждём пока все анимации не выполнятся, 
                // сравнивая переменные из потока нашей задачи

                while (finishedAnimationCount != targetAnimationCount)
                    Thread.Sleep(5);

                // После окончания анимации передвижения запустим анимации слияния так же из главного потока
                // А так же обновим текущий счет
                this.grid.Dispatcher.Invoke(() =>
                {
                    if (mergerPoints.Count() == 0) return;

                    // Таймлайн анимаций слияния
                    Storyboard mergerStoryboard = new Storyboard();
                    // Сбрасываем счетчик анимаций и заново устанавливаем необходимое их число
                    finishedAnimationCount = 0;
                    targetAnimationCount = mergerPoints.Count() * 2; // Для одного слияния у нас 2 анимации

                    // Обновим добавляемый счет как разницу между новым и записанным в интерфейсе
                    int newScore = CalculateScore(this.game.MergeMap);
                    int prevScore = int.Parse(this.scoreInfoBlock.LowerText);
                    int appendedScore = newScore - prevScore;
                    this.scoreInfoBlock.LowerText = $"{newScore}";

                    double expandedValueKeyTime = 0.08;
                    double defaultValueKeyTime = 0.16;

                    foreach (Point mergerPoint in mergerPoints)
                    {
                        int height = (int)mergerPoint.Y;
                        int width = (int)mergerPoint.X;

                        // Находим все квадраты, которые находятся по нужным координатам
                        NumericRect[] mergedRects = this.grid.Children.OfType<NumericRect>()
                            .Where(r => Grid.GetRow(r) == height && Grid.GetColumn(r) == width).ToArray();

                        if (mergedRects.Length != 2)
                            throw new InvalidDataException("В координатах слияния не находится 2 квадрата");

                        // Первый просто удаляем
                        this.grid.Children.Remove(mergedRects[0]);

                        // Выделим для удобства анимируемый квадрат
                        NumericRect animatedRect = mergedRects[1];

                        // Второй преобразуем согласно новому значению
                        UpdateVisuals(animatedRect, this.game.Matrix[height, width]);

                        // Зададим стандартные и расширенные значения для анимаций высоты и ширины
                        double defaultHeight = animatedRect.Height;
                        double expadedHeight = cellHeight;

                        double defaultWidth = animatedRect.Width;
                        double expandedWidth = cellWidth;

                        // Настраиваем анимацию высоты так, чтобы половину времени она 
                        // увеличивалась и половину возвращалась до стандартного значения
                        DoubleAnimationUsingKeyFrames heightAnim = new DoubleAnimationUsingKeyFrames();
                        heightAnim.Duration = TimeSpan.FromSeconds(defaultValueKeyTime);
                        heightAnim.KeyFrames.Add(
                            new LinearDoubleKeyFrame(expadedHeight, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(expandedValueKeyTime))));
                        heightAnim.KeyFrames.Add(
                            new LinearDoubleKeyFrame(defaultHeight, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(defaultValueKeyTime))));
                        heightAnim.FillBehavior = FillBehavior.Stop;

                        Storyboard.SetTarget(heightAnim, animatedRect);
                        Storyboard.SetTargetProperty(heightAnim, new PropertyPath(NumericRect.HeightProperty));

                        // Так же настраиваем анимацию ширины
                        DoubleAnimationUsingKeyFrames widthAnim = new DoubleAnimationUsingKeyFrames();
                        widthAnim.Duration = TimeSpan.FromSeconds(defaultValueKeyTime);
                        widthAnim.KeyFrames.Add(
                            new LinearDoubleKeyFrame(expandedWidth, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(expandedValueKeyTime))));
                        widthAnim.KeyFrames.Add(
                            new LinearDoubleKeyFrame(defaultWidth, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(defaultValueKeyTime))));
                        widthAnim.FillBehavior = FillBehavior.Stop;

                        Storyboard.SetTarget(widthAnim, animatedRect);
                        Storyboard.SetTargetProperty(widthAnim, new PropertyPath(NumericRect.WidthProperty));

                        // Добавляем обе анимации на таймлайн
                        mergerStoryboard.Children.Add(heightAnim);
                        mergerStoryboard.Children.Add(widthAnim);

                        // Учет завершившихся анимаций
                        heightAnim.Completed += (_, __) =>
                        {
                            finishedAnimationCount++;
                        };

                        widthAnim.Completed += (_, __) =>
                        {
                            finishedAnimationCount++;
                        };
                    }

                    // После всех вычислений начинаем процесс анимации
                    mergerStoryboard.Begin();
                });

                while (finishedAnimationCount != targetAnimationCount)
                    Thread.Sleep(5);
            });
        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void CreateNumberTile(int height, int width, int value)
        {            
            NumericRect rect = new NumericRect();
            UpdateVisuals(rect, value);

            this.grid.Children.Add(rect);
            Grid.SetRow(rect, height);
            Grid.SetColumn(rect, width);
        }


        
        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateResources();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.game.Restart();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;

            if (key == Key.Right || key == Key.D)
                Move_s(Game.SlideDirection.Right);
            else if (key == Key.Up || key == Key.W)
                Move_s(Game.SlideDirection.Up);
            else if (key == Key.Left || key == Key.A)
                Move_s(Game.SlideDirection.Left);
            else if (key == Key.Down || key == Key.S)
                Move_s(Game.SlideDirection.Down);
        }

        /// <summary>
        /// Потокобезопасная обработка движения игрового поля, не блокирующая UI поток
        /// </summary>
        /// <param name="direction"></param>
        async void Move_s(Game.SlideDirection direction)
        {
            // Если какие-то процессы отрисовки не закончены, 
            // либо неверное состояние экземпляра игры - не обрабатываем нажатие
            if (!canMove || game == null || !game.IsGameActive) return;

            canMove = false; // Запрещаем обработку событий во избежание ошибок

            // Асинхронно запускаем обработку движения в другом потоке, не блокируя UI поток
            await Task.Factory.StartNew(() =>
            {
                game.Move(direction);
            });

            canMove = true; // Разрешаем обработку событий
        }

        private void undoInfoBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.game?.Rollback();
        }

        int CalculateScore(Dictionary<int, int> mergerMap)
        {
            // обновим счет
            int score = 0;
            foreach (KeyValuePair<int, int> pair in this.game.MergeMap)
            {
                int mergedValue = (int)Math.Pow(2, pair.Key);
                score += mergedValue * pair.Value;
            }
            return score;
        }

        void UpdateVisuals(NumericRect tile, int number)
        {
            int valueToShow = (int)Math.Pow(2, number);
            tile.Text = valueToShow.ToString();

            Tuple<Color, Color> selectedTuple;

            if (number > 0 && number <= this.theme.BackForeColorTuples.Count)
                selectedTuple = this.theme.BackForeColorTuples[number - 1];
            else
                // Если соответствия номеру в базе не нашлось, то возвращаем первый элемент
                selectedTuple = this.theme.BackForeColorTuples[0];

            tile.Background = new SolidColorBrush(selectedTuple.Item1);
            tile.Foreground = new SolidColorBrush(selectedTuple.Item2);

            // Определяем, нужен ли эффект блюра вокруг квадрата
            int lowerBlurBound = 7;
            int upperBlurBound = 11;

            if (number >= lowerBlurBound && number <= upperBlurBound)
            {
                // Вычисляем возможное число состояний, т.к. если границы 7 и 11, то состояний будет 5, а не 4
                int possibleStateCount = upperBlurBound - lowerBlurBound + 1;
                // Вычисляем номер текущего состояния. Если число равно 7, то его номер 1, т.к. иначе мы получим прозрачность 0
                int currentStateNum = number - lowerBlurBound + 1;

                double opacity = ((double)currentStateNum / possibleStateCount) * 1; // 1 - максимальная прозрачность

                System.Windows.Media.Effects.DropShadowEffect shadow =
                    new System.Windows.Media.Effects.DropShadowEffect();

                shadow.Opacity = opacity;
                shadow.Color = this.theme.BackForeColorTuples[number - 1].Item1; //selectedTuple.Item1;
                shadow.BlurRadius = 15;
                shadow.ShadowDepth = 0;

                tile.Effect = shadow;
            }
        }
    }
}
