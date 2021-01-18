using System.Linq;
using System.Collections.Generic;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace InsLab
{
    public class TouchPredictor
    {
        private Dictionary<string, Matrix<float>> weights = new Dictionary<string, Matrix<float>>();
        private NeuralNetwork model;

        public TouchPredictor(string weightPath)
        {
            model = new NeuralNetwork(weightPath);
        }

        private Matrix<float> inputVector = new DenseMatrix(1, 16);
        private Matrix<float> outputVector = new DenseMatrix(1, 2);

        public TouchEvent<float> Predict(TouchEvent<float>[] touchEvents)
        {
            var normalized = NormalizeTouchEvents(touchEvents, out float xPtp, out float yPtp, out float xMin, out float yMin);		

            for (int i = 0; i < 7; i++)
            {
                // x, y speed
                inputVector[0, i] = normalized[i + 1].X - normalized[i].X;
                inputVector[0, i + 7] = normalized[i + 1].Y - normalized[i].Y;
            }

            inputVector[0, 14] = 1 / xPtp;
            inputVector[0, 15] = 1 / yPtp;

            model.Predict(inputVector, outputVector);

            var lastNormalziedTouchEvent = normalized[normalized.Length - 1];
            var normalizedPredictionX = lastNormalziedTouchEvent.X + outputVector[0, 0];
            var normalizedPredictionY = lastNormalziedTouchEvent.Y + outputVector[0, 1];
            var predictedX = (normalizedPredictionX + 0.45F) * xPtp / 0.9F + xMin;
            var predictedY = (normalizedPredictionY + 0.45F) * yPtp / 0.9F + yMin;

            return new TouchEvent<float>(predictedX, predictedY, lastNormalziedTouchEvent.Timestamp);
        }

        private TouchEvent<float>[] NormalizeTouchEvents(TouchEvent<float>[] touchEvents, out float xPtp, out float yPtp, out float xMin, out float yMin)
        {
            TouchEvent<float>[] norm = (TouchEvent<float>[])touchEvents.Clone();
            var xPoints = new float[norm.Length];
            var yPoints = new float[norm.Length];

            for (int i = 0; i < norm.Length; i++)
            {
                xPoints[i] = norm[i].X;
                yPoints[i] = norm[i].Y;
            }
            //var xPoints = norm.Select((touchEvent) => touchEvent.X);
            //var yPoints = norm.Select((touchEvent) => touchEvent.Y);

            xMin = xPoints.Min();
            yMin = yPoints.Min();
            xPtp = xPoints.Max() - xMin;
            yPtp = yPoints.Max() - yMin;

            for (int i = 0; i < norm.Length; i++)
            {
                norm[i].X = 0.9F * (xPoints[i] - xMin) / xPtp - 0.45F;
                norm[i].Y = 0.9F * (yPoints[i] - yMin) / yPtp - 0.45F;
            }

            return norm;
        }
    }
}
