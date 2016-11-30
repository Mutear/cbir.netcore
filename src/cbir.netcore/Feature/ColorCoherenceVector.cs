using CBIR.Netcore.Image;
using System;

namespace CBIR.Netcore.Feature
{
    /// <summary>
    /// <para>Color Coherence Vector</para>
    /// </summary>
    public class ColorCoherenceVector : IFeature
    {
        public const string FeatureName = "CCV";
        protected const int BinWidth = 4;
        /// <summary>
        /// <para>The size of the image when the feature is extracted</para>
        /// </summary>
        protected const int Width = 200, Height = 200;
        /// <summary>
        /// The matrix of this image feature
        /// </summary>
        protected int[][] featureMatrix = null;

        public virtual void Extract(System.Drawing.Bitmap bitmap)
        {
            int[][] grayPixelMatrix = ImageUtil.GetGrayPixelMatrix(bitmap, Width, Height);
            if (grayPixelMatrix != null && grayPixelMatrix.Length != 0 && grayPixelMatrix[0].Length != 0)
            {
                for (int i = 0; i < grayPixelMatrix.Length; i++)
                {
                    for (int j = 0; j < grayPixelMatrix[0].Length; j++)
                    {
                        grayPixelMatrix[i][j] /= BinWidth;
                    }
                }

                var tuple = this.GroupMatrix(grayPixelMatrix);
                int groupNum = tuple.Item1;
                int[][] groupNums = tuple.Item2;

                int[] groupCount = new int[groupNum];
                for (int i = 0; i < grayPixelMatrix.Length; i++)
                {
                    for (int j = 0; j < grayPixelMatrix[0].Length; j++)
                    {
                        groupCount[groupNums[i][j]]++;
                    }
                }

                int threshold = Width * Height / 100;
                for (int i = 0; i < groupNum; i++)
                {
                    if (groupCount[i] < threshold)
                    {
                        groupCount[i] = 0;
                    }
                    else
                    {
                        groupCount[i] = 1;
                    }
                }

                for (int i = 0; i < grayPixelMatrix.Length; i++)
                {
                    for (int j = 0; j < grayPixelMatrix[0].Length; j++)
                    {
                        groupNums[i][j] = groupCount[groupNums[i][j]];
                    }
                }

                this.featureMatrix = new int[2][];
                for (int i = 0; i < 2; i++)
                {
                    this.featureMatrix[i] = new int[(255 / BinWidth) + 1];
                }
                for (int i = 0; i < grayPixelMatrix.Length; i++)
                {
                    for (int j = 0; j < grayPixelMatrix[0].Length; j++)
                    {
                        if (groupNums[i][j] == 0)
                        {
                            this.featureMatrix[0][grayPixelMatrix[i][j] / BinWidth]++;
                        }
                        else
                        {
                            this.featureMatrix[1][grayPixelMatrix[i][j] / BinWidth]++;
                        }
                    }
                }
            }
        }

        public virtual double CalculateSimilarity(IFeature feature)
        {
            if (this.featureMatrix == null)
            {
                throw new Exception("This object has not yet extracted the image feature");
            }
            else if (feature is ColorCoherenceVector)
            {
                return ImageUtil.CalculateSimilarity(this.featureMatrix, (feature as ColorCoherenceVector).featureMatrix);
            }
            else
            {
                throw new ArgumentException("The type of feature does not match this object");
            }
        }

        public virtual string GenerateIndexWithFeature()
        {
            if (this.featureMatrix == null)
            {
                throw new Exception("This object has not yet extracted the image feature");
            }
            else
                return ImageUtil.ConvertMatrixToString(this.featureMatrix);
        }

        public virtual void GenerateFeatureWithIndex(string index)
        {
            this.featureMatrix = ImageUtil.ConvertStringToMatrix(index, 2, 255 / BinWidth + 1);
        }

        public virtual string GetFeatureName()
        {
            return FeatureName;
        }

        private Tuple<int, int[][]> GroupMatrix(int[][] matrix)
        {
            if (matrix == null || matrix.Length == 0 || matrix[0].Length == 0)
            {
                return null;
            }
            else
            {
                int[][] groupNums = new int[matrix.Length][];
                // initial, -1 means the pixel is not yet grouped
                for (int i = 0; i < groupNums.Length; i++)
                {
                    groupNums[i] = new int[groupNums[i].Length];
                    for (int j = 0; j < groupNums[i].Length; j++)
                    {
                        groupNums[i][j] = -1;
                    }
                }

                int groupNum = 0;
                for (int i = 0; i < groupNums.Length; i++)
                {
                    for (int j = 0; j < groupNums[0].Length; j++)
                    {
                        if (groupNums[i][j] < 0)
                        {
                            // Group pixel in matrix
                            groupNums[i][j] = groupNum;
                            GroupPixels(matrix, groupNums, i, j, groupNum);
                            groupNum++;
                        }
                    }
                }
                Tuple<int, int[][]> tuple = new Tuple<int, int[][]>(groupNum + 1, groupNums);
                return tuple;
            }
        }

        /// <summary>
        /// Group pixels that have the same value
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="groupNums"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="groupNum"></param>
        private void GroupPixels(int[][] matrix, int[][] groupNums, int i, int j, int groupNum)
        {
            if (matrix == null || matrix.Length == 0 || matrix[0].Length == 0) return;
            if (groupNums == null || groupNums.Length == 0 || groupNums[0].Length == 0) return;
            int num = matrix[i][j];
            int x = i - 1, y = j - 1;
            int maxX = matrix.Length, maxY = matrix[0].Length;
            if (x >= 0 && y >= 0 && x < maxX && y < maxY && groupNums[x][y] < 0 && matrix[x][y] == num)
            {
                groupNums[x][y] = groupNum;
                GroupPixels(matrix, groupNums, x, y, groupNum);
            }
            y = j;
            if (x >= 0 && y >= 0 && x < maxX && y < maxY && groupNums[x][y] < 0 && matrix[x][y] == num)
            {
                groupNums[x][y] = groupNum;
                GroupPixels(matrix, groupNums, x, y, groupNum);
            }
            y = j + 1;
            if (x >= 0 && y >= 0 && x < maxX && y < maxY && groupNums[x][y] < 0 && matrix[x][y] == num)
            {
                groupNums[x][y] = groupNum;
                GroupPixels(matrix, groupNums, x, y, groupNum);
            }
            x = i; y = j - 1;
            if (x >= 0 && y >= 0 && x < maxX && y < maxY && groupNums[x][y] < 0 && matrix[x][y] == num)
            {
                groupNums[x][y] = groupNum;
                GroupPixels(matrix, groupNums, x, y, groupNum);
            }
            y = j + 1;
            if (x >= 0 && y >= 0 && x < maxX && y < maxY && groupNums[x][y] < 0 && matrix[x][y] == num)
            {
                groupNums[x][y] = groupNum;
                GroupPixels(matrix, groupNums, x, y, groupNum);
            }
            x = i + 1; y = j - 1;
            if (x >= 0 && y >= 0 && x < maxX && y < maxY && groupNums[x][y] < 0 && matrix[x][y] == num)
            {
                groupNums[x][y] = groupNum;
                GroupPixels(matrix, groupNums, x, y, groupNum);
            }
            y = j;
            if (x >= 0 && y >= 0 && x < maxX && y < maxY && groupNums[x][y] < 0 && matrix[x][y] == num)
            {
                groupNums[x][y] = groupNum;
                GroupPixels(matrix, groupNums, x, y, groupNum);
            }
            y = j + 1;
            if (x >= 0 && y >= 0 && x < maxX && y < maxY && groupNums[x][y] < 0 && matrix[x][y] == num)
            {
                groupNums[x][y] = groupNum;
                GroupPixels(matrix, groupNums, x, y, groupNum);
            }
        }
    }
}
