using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace _2048
{
    public class Game: IObservable<Game.GameArgs>
    {
        public enum SlideDirection { Left, Up, Right, Down }

        public int Height => matrix.Height;
        public int Width => matrix.Width;

        public int RollbackCount { get; private set; }
        public int DefaultRollbackCount { get; set; } = 3;
        public Dictionary<int, int> MergeMap => mergeMap.ToDictionary(p => p.Key, p => p.Value);
        public bool IsGameActive { get; private set; }

        public Matrix<int> Matrix => this.matrix;

        Matrix<int> matrix;
        Matrix<int> previousMatrix;
        Random random = new Random();
        List<IObserver<GameArgs>> observers = new List<IObserver<GameArgs>>();
        Dictionary<int, int> mergeMap = new Dictionary<int, int>();

        public Game(int height, int width)
        {
            this.matrix = new Matrix<int>(height, width);
        }

        public Game(Matrix<int> matrix)
        {
            this.matrix = new Matrix<int>(matrix);
        }

        public void Rollback()
        {
            // если попыток не осталось, либо откат только что был сделан,
            // либо это начало игры, то ничего не далаем
            if (this.RollbackCount == 0 || this.previousMatrix == null) return;

            this.RollbackCount--; // уменьшаем число откатов

            this.matrix = previousMatrix;
            previousMatrix = null;

            foreach (IObserver<GameArgs> observer in this.observers)
                observer.OnNext(new GameArgs(GameArgs.EvtType.Rollback));
        }

        public void Restart()
        {
            // очищаем матрицу, заполняя нулями
            for (int i = 0; i < this.Height; i++)
            {
                for (int j = 0; j < this.Width; j++)
                {
                    this.matrix[i, j] = 0;
                }
            }

            // сбрасываем флаги и восстанавливаем попытки до дефолтного количества
            this.IsGameActive = true;
            this.RollbackCount = this.DefaultRollbackCount;
            this.mergeMap = new Dictionary<int, int>();

            // добавляем два первых рандомнох значения
            AddRandom();
            AddRandom();

            // и оповещаем о начале игры
            foreach (IObserver<GameArgs> observer in this.observers)
                observer.OnNext(new GameArgs(GameArgs.EvtType.GameStarted));
        }

        void AddRandom()
        {
            Point[] freePoints = GetUnoccupiedCells();
            int count = freePoints.Length;

            if (count > 0)
            {
                int rndIndex = random.Next() % count;
                Point targetPoint = freePoints[rndIndex];
                
                int generatedValue = 1 + random.Next() % 2;
                this.matrix[(int)targetPoint.Y, (int)targetPoint.X] = generatedValue;

                foreach (IObserver<GameArgs> observer in this.observers)
                {
                    GameArgs args = new GameArgs(GameArgs.EvtType.RandomAdded);
                    args.RandomPoint = targetPoint;

                    observer.OnNext(args);
                }
            }
            else
                throw new InvalidDataException("Нет пустой ячейки, чтобы добавить случайное значение");
        }

        void CheckDeadlock()
        {
            // Тупик, пока не доказано обратное
            this.IsGameActive = false;
            // Тупик - это если нет пустых ячеек и рядом не стоят ячейки с одинаковым значением
            // и при этом нет откатов

            if (this.RollbackCount > 0)
            {
                this.IsGameActive = true;
                return;
            }

            // Проверяем, есть ли пустые ячейки
            Point[] unoccupied = this.GetUnoccupiedCells();
            // если хотя бы одна не занята - значит не тупик
            if (unoccupied.Length > 0)
            {
                this.IsGameActive = true;
                return;
            }

            // если нет пустых ячеек, то проверяем, 
            // стоят ли горизонтально рядом ячейки с равным значением
            for (int i = 0; i < this.Height; i++)
            {
                for (int j = 0; j < this.Width - 1; j++) // -1, т.к. сравниваем текущую и стоящую справа
                {
                    if (matrix[i, j] == matrix[i, j + 1])
                    {
                        this.IsGameActive = true;
                        return;
                    }
                }
            }

            // если не получилось, то транспонируем матрицу и повторяем снова
            Matrix<int> transposed = this.matrix.GetTransposed();

            for (int i = 0; i < transposed.Height; i++)
            {
                for (int j = 0; j < transposed.Width - 1; j++) // -1, т.к. сравниваем текущую и стоящую справа
                {
                    if (transposed[i, j] == transposed[i, j + 1])
                    {
                        this.IsGameActive = true;
                        return;
                    }
                }
            }
        }

        public void Move(SlideDirection direction)
        {
            if (!this.IsGameActive) return;

            // Преобразуем матрицу к такому виду, чтобы работать 
            // только со смещением горизонтально вправо
            Matrix<int> convertedMatrix = ConvertForMovement(this.matrix, direction, true);
            List<Tuple<Point, Point>> commonSourceDestinationTuple = new List<Tuple<Point, Point>>();
            List<Point> commonMergerPointList = new List<Point>();

            for (int i = 0; i < convertedMatrix.Height; i++)
            {
                // получаем все значения занятых ячеек в список
                // в конце вершиной стека будет последний элемент строки
                Stack<int> valueStack = new Stack<int>();

                // Список с сохраненными координатами точек
                List<Point> sourcePoints = new List<Point>();

                for (int j = 0; j < this.Width; j++)
                {
                    // запоминаем значение, которое было, анализируем
                    int value = convertedMatrix[i, j];
                    if (value > 0)
                    {
                        // Если было что-то больше нуля, то запоминаем значение
                        // И добавляем точку в список
                        valueStack.Push(value);
                        sourcePoints.Add(new Point(j, i)); // j - X, i - Y
                    }
                        
                    // после чего зануляем
                    convertedMatrix[i, j] = 0;
                }

                // Список с перемещенными точками
                List<Point> destinationPoints = new List<Point>();
                // Список с координатами, где произошло объединение значений
                List<Point> mergedPoints = new List<Point>();
                // Стек результирующих значений
                Stack<int> afterMergedValues = new Stack<int>();

                // объединяем рядом стоящие начиная с конца
                while (valueStack.Count > 0)
                {
                    int currentValue = valueStack.Pop();
                    // Вычисляем индекс в который переместится значение
                    int newIndex = convertedMatrix.Width - 1 - afterMergedValues.Count;

                    // По меньшей мере один раз запоминаем в какую точку сместилось значение
                    destinationPoints.Add(new Point(newIndex, i)); // X - newIndex, Y - i

                    // если стек все еще не пустой и следующее значение равно текущему,
                    // т.е. у нас пара, которую необходимо объединить в новое значение
                    if (valueStack.Count > 0 && valueStack.Peek() == currentValue)
                    {
                        // то в новый стек добавляем увеличенное значение
                        afterMergedValues.Push(currentValue + 1);
                        // и удаляем следующее значение
                        valueStack.Pop();
                        
                        // т.к. мы объединили - дублируем предыдущее значение в списке перемещений
                        // и указываем в каких координатах произошло это объединение
                        destinationPoints.Add(destinationPoints.Last());
                        mergedPoints.Add(destinationPoints.Last());
                    }
                    else
                    {
                        // иначе в новый стек добавляем просто текущее значение
                        afterMergedValues.Push(currentValue);
                    }
                }

                // т.к. исходные и перемещенные координаты хранятся 
                // в обратном порядке - инвертируем один список
                destinationPoints.Reverse();

                // Узнаем какие точки изменили свои координаты
                // Здесь будут храниться координаты откуда и куда переместились квадраты
                List<Tuple<Point, Point>> sourceDestinationPointTuples = new List<Tuple<Point, Point>>();

                for (int k = 0; k < sourcePoints.Count; k++) // Используем k, чтобы избежать путаницы
                {
                    Point source = sourcePoints[k];
                    Point destination = destinationPoints[k];

                    // Значения Y у нас гарантированно одинаковые
                    if (source.X != destination.X)
                    {
                        sourceDestinationPointTuples.Add(new Tuple<Point, Point>(source, destination));
                    }
                }

                // сохраняем значения в результирующую матрицу, учитывая сдвиг вправо
                for (int j = convertedMatrix.Width - afterMergedValues.Count; j < convertedMatrix.Width; j++)
                {
                    convertedMatrix[i, j] = afterMergedValues.Pop();
                }

                // Преобразуем значения к виду, совместимому с исходной матрицей
                mergedPoints = mergedPoints.Select(p => ConvertForMovement(p, convertedMatrix, direction, false)).ToList();
                sourceDestinationPointTuples = sourceDestinationPointTuples.Select(
                    t => new Tuple<Point, Point>(
                        ConvertForMovement(t.Item1, convertedMatrix, direction, false),
                        ConvertForMovement(t.Item2, convertedMatrix, direction, false))).ToList();

                // И заносим в общие для всех строк структуры
                foreach (Point mergerPoint in mergedPoints)
                    commonMergerPointList.Add(mergerPoint);

                foreach (Tuple<Point, Point> srcDstnPointTuple in sourceDestinationPointTuples)
                    commonSourceDestinationTuple.Add(srcDstnPointTuple);
            }

            // Конвертируем матрицу обратно к нормальному виду
            Matrix<int> convertedBack = ConvertForMovement(convertedMatrix, direction, false);
            
            // И если по факту изменений не было, то ничего не делаем вообще
            if (this.matrix.Equals(convertedBack)) return;

            // Иначе сохраняем новую матрицу
            this.previousMatrix = matrix; // Сохраняем предыдущую матрицу
            this.matrix = convertedBack; // И создаем новую

            // Обновляем карту числа слияний по значениям
            foreach (Point mergerPoint in commonMergerPointList)
            {
                int value = this.matrix[(int)mergerPoint.Y, (int)mergerPoint.X];
                if (this.mergeMap.ContainsKey(value))
                    this.mergeMap[value]++;
                else
                    this.mergeMap.Add(value, 1);
            }
            
            // Оповещаем после сделанного хода и задаем дополнительную информацию для этого
            foreach (IObserver<GameArgs> observer in this.observers)
            {
                GameArgs args = new GameArgs(GameArgs.EvtType.Move);
                args.MergerPoints = commonMergerPointList.ToArray();
                args.SourceDestinationTuples = commonSourceDestinationTuple.ToArray();

                observer.OnNext(args);
            }

            // добавляем случайное значение, 
            // т.к. был сделан ход и освободилась минимум 1 ячейка
            // и в этом же методе оповещаем всех, что появилось новое значение
            AddRandom();

            // проверяем, что после хода мог образоваться тупик и при этом нет попыток
            CheckDeadlock();

            // если тупик - оповещаем всех, кому интересно, что игра закончена
            if (!this.IsGameActive)
            {
                foreach (IObserver<GameArgs> observer in this.observers)
                    observer.OnNext(new GameArgs(GameArgs.EvtType.GameFinished));
            }
        }

        Point[] GetUnoccupiedCells()
        {
            List<Point> pointList = new List<Point>();

            for (int i = 0; i < this.Height; i++)
            {
                for (int j = 0; j < this.Width; j++)
                {
                    if (this.matrix[i, j] == 0)
                        pointList.Add(new Point(j, i)); // j - X, i - Y
                }
            }

            return pointList.ToArray();
        }

        static Point ConvertForMovement(Point point, Matrix<int> matrix, SlideDirection direction, bool isPreparation)
        {
            switch (direction)
            {
                case SlideDirection.Right:
                    {
                        return point;
                    }
                case SlideDirection.Left:
                    {
                        return InverseCoordsHorizontally(point, matrix.Width);
                    }
                case SlideDirection.Down:
                    {
                        return TransposeCoords(point);
                    }
                case SlideDirection.Up:
                    {
                        if (isPreparation)
                            return InverseCoordsHorizontally(TransposeCoords(point), matrix.Height); //matrix.GetTransposed().GetHorizontalInversedPoint((int)point.X, (int)point.Y);
                        else
                            return TransposeCoords(InverseCoordsHorizontally(point, matrix.Width)); //matrix.GetHorizontalInversed().GetTransposedPoint((int)point.X, (int)point.Y);
                    }
                default:
                    throw new ArgumentException(nameof(direction));
            }
        }

        static Matrix<int> ConvertForMovement(Matrix<int> matrix, SlideDirection direction, bool isPreparation)
        {
            Matrix<int> convertedMatrix;

            switch (direction)
            {
                case SlideDirection.Right:
                    {
                        convertedMatrix = new Matrix<int>(matrix);
                        break;
                    }
                case SlideDirection.Left:
                    {
                        convertedMatrix = matrix.GetHorizontalInversed();
                        break;
                    }
                case SlideDirection.Down:
                    {
                        convertedMatrix = matrix.GetTransposed();
                        break;
                    }
                case SlideDirection.Up:
                    {
                        if (isPreparation)
                            convertedMatrix = matrix.GetTransposed().GetHorizontalInversed();
                        else
                            convertedMatrix = matrix.GetHorizontalInversed().GetTransposed();
                        break;
                    }
                default:
                    throw new ArgumentException(nameof(direction));
            }

            return convertedMatrix;
        }

        public IDisposable Subscribe(IObserver<GameArgs> observer)
        {
            this.observers.Add(observer);
            return new Unsubscriber<GameArgs>(this.observers, observer);
        }

        public class GameArgs
        {
            public enum EvtType { GameStarted, GameFinished, Move, RandomAdded, Rollback }
            /// <summary>
            /// Кортежи пар значений "откуда" и "куда" переместилось значение
            /// </summary>
            public Tuple<Point, Point>[] SourceDestinationTuples { get; set; } = null;
            /// <summary>
            /// Массив координат, где произошло объединение значений
            /// </summary>
            public Point[] MergerPoints { get; set; } = null;
            /// <summary>
            /// Координаты, которые были выбраны для создания нового значения
            /// </summary>
            public Point RandomPoint { get; set; }

            public EvtType Evt { get; }

            public GameArgs(EvtType evt)
            {
                this.Evt = evt;
            }
        }

        static Point TransposeCoords(Point point)
        {
            return new Point(point.Y, point.X);
        }

        static Point InverseCoordsHorizontally(Point point, int width)
        {
            return new Point(width - 1 - point.X, point.Y);
        }
    }
}
