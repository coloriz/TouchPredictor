Touch Predictor
===

MLP-based touch point prediction algorithm implemented in C#.

This project is developed in conjunction with [this project](https://github.com/coloriz/touch-extrapolation-mlp).

## Example

```csharp
using System;
using System.Diagnostics;
using InsLab;

class Program
{
    static void Main(string[] args)
    {
        var stopwatch = new Stopwatch();
        var predictor = new TouchPredictor(@"data\weights.json");

        var stroke = new TouchEvent<float>[] {
            new TouchEvent<float>(1212.8483F, 327.3748F, 25034722),
            new TouchEvent<float>(1219.1581F, 331.75656F, 25034739),
            new TouchEvent<float>(1225.443F, 336.12112F, 25034755),
            new TouchEvent<float>(1231.1128F, 342.81012F, 25034772),
            new TouchEvent<float>(1235.2523F, 349.27823F, 25034789),
            new TouchEvent<float>(1240.191F, 358.7311F, 25034806),
            new TouchEvent<float>(1241.5713F, 365.625F, 25034823),
            new TouchEvent<float>(1242.8572F, 374.9256F, 25034839),
        };
        
        stopwatch.Start();
        // Predict function takes 8 consecutive touch points as an input.
        // and returns the predicted 9th touch point.
        var predictedPoint = predictor.Predict(stroke);
        stopwatch.Stop();
        // 12ms 
        // relatively slow at first execution probably due to initial loading...?
        Console.WriteLine($"elapsed (single) : {stopwatch.ElapsedMilliseconds} ms");
        
        // Perform multiple predictions
        stopwatch.Restart();
        for (int i = 0; i < 6; i++)
        {
            predictedPoint = predictor.Predict(stroke);
        }
        stopwatch.Stop();
        // < 1ms
        Console.WriteLine($"elapsed (multiple (6)) : {stopwatch.ElapsedMilliseconds} ms");
        
        // 1244.94875, 383.45914
        Console.WriteLine(predictedPoint);
    }
}
```