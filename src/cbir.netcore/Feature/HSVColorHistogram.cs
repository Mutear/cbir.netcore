using CBIR.Netcore.Image;
using System;
using System.Drawing;

namespace CBIR.Netcore.Feature
{
    public class HSVColorHistogram : IFeature
    {
        public const string FeatureName = "HSVCH";
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
            Color[][] matrix = ImageUtil.GetImagePixelMatrix(bitmap, Width, Height);
            if (matrix != null && matrix.Length != 0 && matrix[0].Length != 0)
            {
                HSV[][] hsvMatrix = new HSV[matrix.Length][];
                for (int i = 0; i < matrix.Length; i++)
                {
                    hsvMatrix[i] = new HSV[matrix[i].Length];
                    for (int j = 0; j < matrix[i].Length; j++)
                    {
                        HSV hsv = new HSV();
                        hsv.H = (int)matrix[i][j].GetHue() * 255;
                        hsv.S = (int)matrix[i][j].GetSaturation() * 255;
                        hsv.V = (int)matrix[i][j].GetBrightness() * 255;
                        hsvMatrix[i][j] = hsv;
                    }
                }

                // 统计
                int[][] histogram = new int[3][];
                for (int i = 0; i < 3; i++)
                {
                    histogram[i] = new int[256];
                }
                for (int i = 0; i < hsvMatrix.Length; i++)
                {
                    for (int j = 0; j < hsvMatrix[0].Length; j++)
                    {
                        histogram[0][hsvMatrix[i][j].H]++;
                        histogram[1][hsvMatrix[i][j].S]++;
                        histogram[2][hsvMatrix[i][j].V]++;
                    }
                }
                this.featureMatrix = histogram;
            }
        }

        public virtual double CalculateSimilarity(IFeature feature)
        {
            if (this.featureMatrix == null)
            {
                throw new Exception("This object has not yet extracted the image feature");
            }
            else if (feature is HSVColorHistogram)
            {
                return ImageUtil.CalculateSimilarity(this.featureMatrix, (feature as HSVColorHistogram).featureMatrix);
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
            this.featureMatrix = ImageUtil.ConvertStringToMatrix(index, 3, 256);
        }

        public virtual string GetFeatureName()
        {
            return FeatureName;
        }
    }
}
