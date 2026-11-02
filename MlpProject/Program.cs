using System;
using MlpProject;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Console/command-line integration can be placed here when the custom MLP is available:
        // PreparedDataset data = DataPreprocessor.Prepare("data.csv");
        // // Initialize your custom MLP here.
        // TrainingResult result = TrainingLoop.Run(
        //     data,
        //     trainSample: (features, targets) => yourNetwork.Train(features, targets),
        //     predict: features => yourNetwork.Predict(features),
        //     epochCount: 1000,
        //     logInterval: 100);
        // Console.WriteLine(result.TestMetrics);

        Application.Run(new Form1());
    }
}