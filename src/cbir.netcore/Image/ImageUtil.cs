using System;
using System.Drawing;

namespace CBIR.Netcore.Image
{
    public class ImageUtil
    {
        public static int[][] GetGrayPixelMatrix(Bitmap bitmap, int width, int height)
        {
            try
            {
                Color[][] colors = ShrinkBitmap(bitmap, width, height);
                int[][] grayPixelMatrix = new int[height][];
                for (int i = 0; i < height; i++)
                {
                    grayPixelMatrix[i] = new int[width];
                    for (int j = 0; j < width; j++)
                    {
                        Color color = colors[i][j];
                        grayPixelMatrix[i][j] = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
                    }
                }
                return grayPixelMatrix;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Color[][] GetImagePixelMatrix(Bitmap bitmap, int width, int height)
        {
                return ShrinkBitmap(bitmap, width, height);
        }

        public static Color[][] ShrinkBitmap(Bitmap bitmap, int width, int height)
        {
            if (bitmap != null)
            {
                int bHeight = bitmap.Height;
                int bWidth = bitmap.Width;
                if (bHeight < height || bWidth < width)
                {
                    return null;
                }
                else
                {
                    Color[][] colors = new Color[height][];
                    double u = (double)bHeight / height;
                    double v = (double)bWidth / width;
                    for (int i = 0; i < height; i++)
                    {
                        colors[i] = new Color[width];
                        for (int j = 0; j < width; j++)
                        {
                            double x = u * i;
                            double y = v * j;
                            int x1 = (int)x, y1 = (int)y, x2 = (int)(x + u), y2 = (int)(y + v);
                            if (x2 >= bHeight)
                            {
                                x2 = bHeight - 1;
                            }
                            if (y2 >= bWidth)
                            {
                                y2 = bWidth - 1;
                            }
                            Color c1 = bitmap.GetPixel(y1, x1), c2 = bitmap.GetPixel(y2, x1), c3 = bitmap.GetPixel(y1, x2), c4 = bitmap.GetPixel(y2, x2);
                            int r = (c1.R + c2.R + c3.R + c4.R) / 4;
                            int g = (c1.G + c2.G + c3.G + c4.G) / 4;
                            int b = (c1.B + c2.B + c3.B + c4.B) / 4;
                            colors[i][j] = Color.FromArgb(r, g, b);
                        }
                    }
                    return colors;
                }
            }
            return null;
        }

        //public static Bitmap ResizeBitmap(Bitmap bitmap, int width, int height)
        //{
        //    Bitmap newBitmap = new Bitmap(width, height);
        //    using (Graphics graphics = Graphics.FromImage(newBitmap))
        //    {
        //        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
        //        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        //        graphics.Clear(Color.Transparent);
        //        graphics.DrawImage(bitmap, new Rectangle(0, 0, width, height),
        //            new Rectangle(0, 0, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel);
        //        return newBitmap;
        //    }
        //}

        public static double CalculateSimilarity(int[][] matrix1, int[][] matrix2)
        {
            return CalculateSimilarity(ConvertMatrixToVector(matrix1), ConvertMatrixToVector(matrix2));
        }

        public static double CalculateSimilarity(int[] vector1, int[] vector2)
        {
            if (vector1 == null || vector2 == null)
            {
                throw new NullReferenceException();
            }
            else if (vector1.Length != vector2.Length)
            {
                throw new ArgumentException("Two vector lengths are not equal");
            }
            else
            {
                double num1 = 0, num2 = 0, numerator = 0;
                for (int i = 0; i < vector1.Length; i++)
                {
                    num1 += Math.Pow(vector1[i], 2);
                    num2 += Math.Pow(vector2[i], 2);
                    numerator += vector1[i] * vector2[i];
                }
                num1 = Math.Sqrt(num1);
                num2 = Math.Sqrt(num2);
                return numerator / (num1 * num2);
            }
        }

        public static double CalculateSimilarity(string str1, string str2)
        {
            int num = 0;
            for (int i = 0; i < 64; i++)
            {
                if (str1[i] == str2[i])
                {
                    num++;
                }
            }
            return ((double)num) / 64.0;
        }

        public static int[] ConvertMatrixToVector(int[][] matrix)
        {
            if (matrix != null && matrix.Length != 0 && matrix[0].Length != 0)
            {
                int[] vector = new int[matrix.Length * matrix[0].Length];
                for (int i = 0, index = 0; i < matrix.Length; i++)
                {
                    for (int j = 0; j < matrix[0].Length; j++, index++)
                    {
                        vector[index] = matrix[i][j];
                    }
                }
                return vector;
            }
            return null;
        }

        public static string ConvertMatrixToString(int[][] matrix)
        {
            return ConvertVectorToString(ConvertMatrixToVector(matrix));
        }

        public static string ConvertVectorToString(int[] vector)
        {
            if (vector != null && vector.Length != 0)
            {
                string str = vector[0].ToString();
                for (int i = 1; i < str.Length; i++)
                {
                    str += ' ' + vector[i];
                }
                return str;
            }
            return null;
        }

        public static int[] ConvertStringToVector(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            else
            {
                string[] strs = str.Split(' ');
                int[] vector = new int[strs.Length];
                for (int i = 0; i < strs.Length; i++)
                {
                    vector[i] = int.Parse(strs[i]);
                }
                return vector;
            }
        }

        public static int[][] ConvertStringToMatrix(string str, int row, int column)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            else
            {
                string[] strs = str.Split(' ');
                int[][] matrix = new int[row][];
                for (int i = 0, index = 0; i < row; i++)
                {
                    matrix[i] = new int[column];
                    for (int j = 0; j < column; j++, index++)
                    {
                        matrix[i][j] = int.Parse(strs[index]);
                    }
                }
                return matrix;
            }
        }
    }
}
