using CBIR.Netcore.Image;
using System;
using System.Collections.Generic;

namespace CBIR.Netcore.Feature
{
    /// <summary>
    /// <para>Annular Color Layout Histogram</para>
    /// <para>References:</para>
    /// <para>Rao A, Srihari R K, Zhang Z. Spatial color histograms for content-based image retrieval[J]. 1999:183-186.</para>
    /// <para>孙君顶, 毋小省. 基于颜色分布特征的图像检索[J]. 光电子·激光, 2006, 17(8):1009-1013.</para>
    /// </summary>
    public class AnnularColorLayoutHistogram : IFeature
    {
        public const string FeatureName = "ACLH";
        /// <summary>
        /// <para>Number of concentric circles</para>
        /// <para>Default value is 10</para>
        /// </summary>
        protected const int N = 10;
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
            // Get gray pixel matrix
            int[][] grayPixelMatrix = ImageUtil.GetGrayPixelMatrix(bitmap, Width, Height);
            if (grayPixelMatrix != null && grayPixelMatrix.Length != 0 && grayPixelMatrix[0].Length != 0)
            {
                // Group pixels according to the gray value of pixel
                Dictionary<int, List<Tuple<int, int>>> groupedPixels = new Dictionary<int, List<Tuple<int, int>>>();
                for (int i = 0; i < grayPixelMatrix.Length; i++)
                {
                    for (int j = 0; j < grayPixelMatrix[0].Length; j++)
                    {
                        int gray = grayPixelMatrix[i][j];
                        Tuple<int, int> tuple = new Tuple<int, int>(i, j);
                        List<Tuple<int, int>> list = null;
                        if (groupedPixels.ContainsKey(gray))
                        {
                            list = groupedPixels[gray];
                            list.Add(tuple);
                        }
                        else
                        {
                            list = new List<Tuple<int, int>>();
                            list.Add(tuple);
                            groupedPixels.Add(gray, list);
                        }
                    }
                }
                // Calculate the centroid for different gray values
                Tuple<double, double>[] centroids = new Tuple<double, double>[256];
                for (int i = 0; i < 256; i++)
                {
                    if (groupedPixels.ContainsKey(i))
                    {
                        List<Tuple<int, int>> list = groupedPixels[i];
                        double x = 0, y = 0;
                        foreach (var tuple in list)
                        {
                            x += tuple.Item1;
                            y += tuple.Item2;
                        }
                        x = x / list.Count;
                        y = y / list.Count;
                        centroids[i] = new Tuple<double, double>(x, y);
                    }
                }
                // Calculate the distance to the centroid for each pixel
                double[][] distances = new double[grayPixelMatrix.Length][];
                for (int i = 0; i < grayPixelMatrix.Length; i++)
                {
                    distances[i] = new double[grayPixelMatrix[i].Length];
                    for (int j = 0; j < grayPixelMatrix[i].Length; j++)
                    {
                        // Get the centroid
                        Tuple<double, double> centroid = centroids[grayPixelMatrix[i][j]];
                        distances[i][j] = Math.Sqrt(Math.Pow(i - centroid.Item1, 2) + Math.Pow(j - centroid.Item2, 2));
                    }
                }
                // Get the max distance
                double[] maxDistances = new double[256];
                for (int i = 0; i < 256; i++)
                {
                    if (groupedPixels.ContainsKey(i))
                    {
                        var list = groupedPixels[i];
                        double max = 0;
                        for (int j = 0; j < list.Count; j++)
                        {
                            var tuple = list[j];
                            double distance = distances[tuple.Item1][tuple.Item2];
                            if (distance > max) max = distance;
                        }
                        maxDistances[i] = max;
                    }
                }
                // Counts the number of pixels contained in a concentric circle with different distances
                this.featureMatrix = new int[256][];
                for (int i = 0; i < 256; i++)
                {
                    this.featureMatrix[i] = new int[N];
                    if (groupedPixels.ContainsKey(i))
                    {
                        var list = groupedPixels[i];
                        for (int k = 0; k < list.Count; k++)
                        {
                            var tuple = list[k];
                            double dis = distances[tuple.Item1][tuple.Item2];
                            double quot = dis / (maxDistances[i] / N);
                            this.featureMatrix[i][(int)Math.Floor(quot)]++;
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
            else if(feature is AnnularColorLayoutHistogram)
            {
                return ImageUtil.CalculateSimilarity(this.featureMatrix, (feature as AnnularColorLayoutHistogram).featureMatrix);
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
            this.featureMatrix = ImageUtil.ConvertStringToMatrix(index, 256, N);
        }

        public virtual string GetFeatureName()
        {
            return FeatureName;
        }
    }
}
