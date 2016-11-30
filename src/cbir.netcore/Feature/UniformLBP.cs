using CBIR.Netcore.Image;
using System;

namespace CBIR.Netcore.Feature
{
    public class UniformLBP : IFeature
    {
        public const string FeatureName = "ULBP";
        /// <summary>
        /// <para>The size of the image when the feature is extracted</para>
        /// </summary>
        protected const int Width = 200, Height = 200;
        protected int[] featureVector = null;

        public virtual void Extract(System.Drawing.Bitmap bitmap)
        {
            int[][] grayMatrix = ImageUtil.GetGrayPixelMatrix(bitmap, Width, Height);

            int[] groupNums = GroupFeatureValue();

            if (grayMatrix == null || grayMatrix.Length == 0 || grayMatrix[0].Length == 0) return;
            int[] vector = new int[59];
            for (int i = 1; i < grayMatrix.Length - 1; i++)
            {
                for (int j = 1; j < grayMatrix[0].Length - 1; j++)
                {
                    int center = grayMatrix[i][j];
                    int feature = 0;
                    feature = grayMatrix[i - 1][j - 1] >= center ? (feature << 1) + 1 : (feature << 1);
                    feature = grayMatrix[i][j - 1] >= center ? (feature << 1) + 1 : (feature << 1);
                    feature = grayMatrix[i + 1][j - 1] >= center ? (feature << 1) + 1 : (feature << 1);
                    feature = grayMatrix[i + 1][j] >= center ? (feature << 1) + 1 : (feature << 1);
                    feature = grayMatrix[i + 1][j + 1] >= center ? (feature << 1) + 1 : (feature << 1);
                    feature = grayMatrix[i][j + 1] >= center ? (feature << 1) + 1 : (feature << 1);
                    feature = grayMatrix[i - 1][j + 1] >= center ? (feature << 1) + 1 : (feature << 1);
                    feature = grayMatrix[i - 1][j] >= center ? (feature << 1) + 1 : (feature << 1);
                    feature = GetMinFeature(feature);
                    vector[groupNums[feature]]++;
                }
            }

            this.featureVector = vector;
        }

        public virtual double CalculateSimilarity(IFeature feature)
        {
            if (this.featureVector == null)
            {
                throw new Exception("This object has not yet extracted the image feature");
            }
            else if (feature is UniformLBP)
            {
                return ImageUtil.CalculateSimilarity(this.featureVector, (feature as UniformLBP).featureVector);
            }
            else
            {
                throw new ArgumentException("The type of feature does not match this object");
            }
        }

        public virtual string GenerateIndexWithFeature()
        {
            if (this.featureVector == null)
            {
                throw new Exception("This object has not yet extracted the image feature");
            }
            else
                return ImageUtil.ConvertVectorToString(this.featureVector);
        }

        public virtual void GenerateFeatureWithIndex(string index)
        {
            this.featureVector = ImageUtil.ConvertStringToVector(index);
        }

        public virtual string GetFeatureName()
        {
            return FeatureName;
        }

        private int[] GroupFeatureValue()
        {
            int[] groupNums = new int[256];
            int num = 1;
            for (int i = 0; i <= 255; i++)
            {
                if (GetHopCount(i) <= 2)
                {
                    groupNums[i] = num;
                    num++;
                }
            }
            return groupNums;
        }

        private int GetHopCount(int i)
        {
            int[] a = new int[8];
            int cnt = 0;
            int k = 7;
            while (i > 0)
            {
                a[k] = i & 1;
                i = i >> 1;
                --k;
            }
            for (k = 0; k < 7; k++)
            {
                if (a[k] != a[k + 1])
                {
                    ++cnt;
                }
            }
            if (a[0] != a[7])
            {
                ++cnt;
            }
            return cnt;
        }

        private int GetMinFeature(int feature)
        {
            int minFeature = feature;
            for (int i = 0; i < 7; i++)
            {
                feature = (feature >> 1 | feature << 7) & 0xff;
                if (feature < minFeature) minFeature = feature;
            }

            return minFeature;
        }
    }
}
