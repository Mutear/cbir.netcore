using CBIR.Netcore.Image;
using System;

namespace CBIR.Netcore.Feature
{
    public class RHash : IFeature
    {
        public const string FeatureName = "RHash";
        protected string featureValue = null;

        public virtual void Extract(System.Drawing.Bitmap bitmap)
        {
            int[][] grayPixelMatrix = ImageUtil.GetGrayPixelMatrix(bitmap, 8, 8);
            if (grayPixelMatrix != null && grayPixelMatrix.Length != 0 && grayPixelMatrix[0].Length != 0)
            {
                double average = 0;
                for(int i = 0; i < 8; i++){
                    for(int j = 0; j < 8; j++){
                        average += grayPixelMatrix[i][j];
                    }
                }
                average /= 64.0;
                this.featureValue = GetFeature(grayPixelMatrix, average);
            }
        }

        public virtual double CalculateSimilarity(IFeature feature)
        {
            if (string.IsNullOrEmpty(this.featureValue))
            {
                throw new Exception("This object has not yet extracted the image feature");
            }
            else if (feature is RHash)
            {
                return ImageUtil.CalculateSimilarity(this.featureValue, (feature as RHash).featureValue);
            }
            else
            {
                throw new ArgumentException("The type of feature does not match this object");
            }
        }

        public virtual string GenerateIndexWithFeature()
        {
            return this.featureValue;
        }

        public virtual void GenerateFeatureWithIndex(string index)
        {
            this.featureValue = index;
        }

        public virtual string GetFeatureName()
        {
            return FeatureName;
        }

        private String GetFeature(int[][] matrix, double average)
        {
            String featureValue = "";
            int[] r = { 2, 4, 6, 8 };
            for (int i = 0; i < 4; i++)
            {
                int start = (8 - r[i]) / 2;
                int feature = 0;
                for (int j = start; j < start + r[i]; j++)
                {
                    feature = matrix[start][j] < average ? feature << 1 : (feature << 1) + 1;
                }
                for (int j = start + 1; j < start + r[i]; j++)
                {
                    feature = matrix[j][start + r[i] - 1] < average ? feature << 1 : (feature << 1) + 1;
                }
                for (int j = start + r[i] - 2; j >= start; j--)
                {
                    feature = matrix[start + r[i] - 1][j] < average ? feature << 1 : (feature << 1) + 1;
                }
                for (int j = start + r[i] - 2; j > start; j--)
                {
                    feature = matrix[j][start] < average ? feature << 1 : (feature << 1) + 1;
                }
                featureValue += GetMinFeature(feature, 4 * (r[i] - 1));
            }
            return featureValue;
        }

        private String GetMinFeature(int feature, int bitNum)
        {
            int max = 1;
            for (int i = 1; i < bitNum; i++)
            {
                max = (max << 1) + 1;
            }

            int min = feature;
            for (int i = 0; i < bitNum - 1; i++)
            {
                feature = (feature >> 1 | feature << (bitNum - 1)) & max;
                if (feature < min) min = feature;
            }

            String result = "";
            for (int i = 0; i < bitNum; i++)
            {
                if (min % 2 == 0)
                {
                    result = "0" + result;
                }
                else
                {
                    result = "1" + result;
                }
                min >>= 1;
            }
            return result;
        }
    }
}
