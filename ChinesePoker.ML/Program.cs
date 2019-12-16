using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChinesePoker.ML
{
  class Program
  {
    public const string RawDataPath = @"D:\ws\temp\cpRecords.txt";
    public const string TrainedModelPath = @"D:\ws\temp\cpModel2.zip";

    static void Main(string[] args)
    {
      //GenerateData();
      //Train();
      Prediction();
    }

    static void GenerateData()
    {
      var gen = new PlayRecordGenerator();
      gen.Go(RawDataPath, 4_000_000);
    }

    static void Train()
    {
      var trainer = new Trainer();
      trainer.RegressionTraining(RawDataPath, TrainedModelPath);
      //trainer.CategorizationTraining(RawDataPath, TrainedModelPath);
    }

    static void Prediction()
    {
      var predictor = new Predictor();
      //predictor.SimulationComparison(TrainedModelPath);
      predictor.Go(TrainedModelPath);
    }
  }
}
