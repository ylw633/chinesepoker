using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Component;
using ChinesePoker.Core.Interface;
using ChinesePoker.ML.Component;
using ChinesePoker.ML.Model;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace ChinesePoker.ML.MachineLearner
{
  public class RegressionLearner : IMachineLearner
  {
    public string ModelFileName => "cpmodel-regression.zip";

    public void Training(string dataFileName, string modelPath)
    {
      var mlContext = new MLContext();
      var trainingDataView = mlContext.Data.LoadFromTextFile<RoundData>(dataFileName, ' ');

      var dataProcessPipeline = mlContext.Transforms.CopyColumns("Label", nameof(RoundData.Score))
        .Append(mlContext.Transforms.Categorical.OneHotEncoding("R1", nameof(RoundData.FirstHandStrength)))
        .Append(mlContext.Transforms.Categorical.OneHotEncoding("R2", nameof(RoundData.MiddleHandStrength)))
        .Append(mlContext.Transforms.Categorical.OneHotEncoding("R3", nameof(RoundData.LastHandStrength)))
        .Append(mlContext.Transforms.Concatenate("Features", "R1", "R2", "R3"));

      var trainingPipeline = dataProcessPipeline.Append(mlContext.Regression.Trainers.Sdca());

      var trainedModel = trainingPipeline.Fit(trainingDataView);

      using (var sw = new FileStream(Path.Combine(modelPath, ModelFileName), FileMode.Create, FileAccess.Write, FileShare.Write))
      {
        mlContext.Model.Save(trainedModel, trainingDataView.Schema, sw);
      }
    }

    public IRoundStrategy GetStrategy(string modelPath)
    {
      return new RegressionMlStrategy(Path.Combine(modelPath, ModelFileName));
    }
  }
}
