using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace _2048
{
    public class Matrix<T>: IEquatable<Matrix<T>> where T: IEquatable<T>
    {
        public int Height { get; }
        public int Width { get; }

        T[,] data;
        
        public Matrix(int height, int width)
        {
            this.Height = height;
            this.Width = width;

            this.data = new T[height, width];
        }

        public Matrix(Matrix<T> matrixToCopy): this(matrixToCopy.Height, matrixToCopy.Width)
        {
            for (int i = 0; i < this.Height; i++)
            {
                for (int j = 0; j < this.Width; j++)
                {
                    this[i, j] = matrixToCopy[i, j];
                }
            }
        }

        public T this[int height, int width]
        {
            get => data[height, width];
            set => data[height, width] = value;
        }

        public Matrix<T> GetTransposed()
        {
            Matrix<T> transposed = new Matrix<T>(this.Width, this.Height);

            for (int i = 0; i < this.Height; i++)
            {
                for (int j = 0; j < this.Width; j++)
                {
                    transposed[j, i] = this[i, j]; // строки становятся колонками и наоборот
                }
            }

            return transposed;
        }

        public Matrix<T> GetHorizontalInversed()
        {
            Matrix<T> inversed = new Matrix<T>(this.Height, this.Width);

            for (int i = 0; i < this.Height; i++)
                for (int j = 0; j < this.Width; j++) // 0123456 
                    inversed[i, this.Width - j - 1] = this[i, j];

            return inversed;
        }

        public Matrix<T> GetVerticalInversed()
        {
            Matrix<T> inversed = new Matrix<T>(this.Height, this.Width);

            for (int i = 0; i < this.Height; i++)
                for (int j = 0; j < this.Width; j++)
                    inversed[this.Height - i - 1, j] = this[i, j];

            return inversed;
        }

        public bool Equals(Matrix<T> other)
        {
            if (this.Height != other.Height || this.Width != other.Width) return false;

            for (int i = 0; i < this.Height; i++)
            {
                for (int j = 0; j < this.Width; j++)
                {
                    if (!this[i, j].Equals(other[i, j])) return false;
                }
            }

            return true;
        }
    }
}
