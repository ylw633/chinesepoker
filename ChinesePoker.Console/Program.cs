using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.ML;
using ChinesePoker.ML.Component;
using ChinesePoker.ML.MachineLearner;

namespace ChinesePoker.Console
{
  class Program
  {
    public const string RawDataPath = @"D:\ws\temp\cpRecords_validate.txt";
    public const string TrainedModelPath = @"D:\ws\temp\";

    public static IList<IMachineLearner> Learners { get; } = new List<IMachineLearner> { new RegressionLearner(), new CategorizationLearner() };
    private static IMachineLearner ActiveLearner { get; set; }

    static void Main(string[] args)
    {
      //GenerateData();
      //return;
      ActiveLearner = Learners[1];
      Train();
      //Prediction();
    }

    static void GenerateData()
    {
      var gen = new PlayRecordGenerator();
      gen.Go(RawDataPath, 2_000_000);
    }

    static void Train()
    {
      ActiveLearner.Training(RawDataPath, TrainedModelPath);
    }

    static void Prediction()
    {
      var predictor = new Predictor();
      predictor.SimulationComparison(ActiveLearner.GetStrategy(TrainedModelPath));
      predictor.Go(ActiveLearner.GetStrategy(TrainedModelPath));
    }
  }
}
