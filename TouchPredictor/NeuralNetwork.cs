using System;
using System.Collections.Generic;
using System.IO;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using Newtonsoft.Json.Linq;

namespace InsLab
{
    internal class NeuralNetwork
    {
        private const float epsilon = 1E-3F;

        private readonly Matrix<float> w1, b1, w2, b2, bnMean, bnStdReciprocal, bnGamma, bnBeta;
        private Matrix<float> layer1Output, layer2Output;

        public NeuralNetwork(string weightPath)
        {
            var weights = LoadWeights(weightPath);
            w1 = weights["dense/kernel"];
            b1 = weights["dense/bias"];
            w2 = weights["dense_1/kernel"];
            b2 = weights["dense_1/bias"];
            bnMean = weights["batch_normalization/moving_mean"];
            bnStdReciprocal = 1 / (weights["batch_normalization/moving_variance"] + epsilon).PointwiseSqrt();
            bnGamma = weights["batch_normalization/gamma"];
            bnBeta = weights["batch_normalization/beta"];

            layer1Output = new DenseMatrix(1, w1.ColumnCount);
            layer2Output = new DenseMatrix(1, w2.ColumnCount);
        }

        private Dictionary<string, Matrix<float>> LoadWeights(string path)
        {
            string json = File.ReadAllText(path);
            JObject o = JObject.Parse(json);

            var weights = new Dictionary<string, Matrix<float>>
            {
                ["batch_normalization/beta"] = DenseVector.OfArray(o["batch_normalization/beta"].ToObject<float[]>()).ToRowMatrix(),
                ["batch_normalization/gamma"] = DenseVector.OfArray(o["batch_normalization/gamma"].ToObject<float[]>()).ToRowMatrix(),
                ["batch_normalization/moving_mean"] = DenseVector.OfArray(o["batch_normalization/moving_mean"].ToObject<float[]>()).ToRowMatrix(),
                ["batch_normalization/moving_variance"] = DenseVector.OfArray(o["batch_normalization/moving_variance"].ToObject<float[]>()).ToRowMatrix(),
                ["dense/bias"] = DenseVector.OfArray(o["dense/bias"].ToObject<float[]>()).ToRowMatrix(),
                ["dense/kernel"] = DenseMatrix.OfArray(o["dense/kernel"].ToObject<float[,]>()),
                ["dense_1/bias"] = DenseVector.OfArray(o["dense_1/bias"].ToObject<float[]>()).ToRowMatrix(),
                ["dense_1/kernel"] = DenseMatrix.OfArray(o["dense_1/kernel"].ToObject<float[,]>())
            };

            return weights;
        }

        public void Predict(Matrix<float> x, Matrix<float> result)
        {
            // Layer 1
            x.Multiply(w1, layer1Output);
            layer1Output.Add(b1, layer1Output);
            BatchNormalization(layer1Output, bnMean, bnStdReciprocal, bnGamma, bnBeta, layer1Output);
            ReLU(layer1Output, layer1Output);

            // Output layer
            layer1Output.Multiply(w2, layer2Output);
            layer2Output.Add(b2, layer2Output);
            layer2Output.PointwiseTanh(result);
        }

        private void BatchNormalization(Matrix<float> x, Matrix<float> mu, Matrix<float> stdReciprocal, Matrix<float> gamma, Matrix<float> beta, Matrix<float> result)
        {
            x.Subtract(mu, result);
            result.PointwiseMultiply(stdReciprocal, result);
            result.PointwiseMultiply(gamma, result);
            result.Add(beta, result);
        }

        private void ReLU(Matrix<float> x, Matrix<float> result)
        {
            x.PointwiseMaximum(0, result);
        }
    }
}
