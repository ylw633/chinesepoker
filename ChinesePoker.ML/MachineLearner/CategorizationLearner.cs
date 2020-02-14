using System.IO;
using ChinesePoker.Core.Interface;
using ChinesePoker.Core.Model;
using ChinesePoker.ML.Component;
using ChinesePoker.ML.Model;
using Common;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Text;

namespace ChinesePoker.ML.MachineLearner
{
  public class CategorizationLearner : IMachineLearner
  {
    public string ModelFileName => "cpmodel-categorization.zip";

    public void Training(string dataFileName, string modelPath)
    {
      var mlContext = new MLContext();
      var trainingDataView = mlContext.Data.LoadFromTextFile<RoundData<int>>(dataFileName, ',');

      var dataProcessPipeline = mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(RoundData<int>.Score), keyOrdinality: ValueToKeyMappingEstimator.KeyOrdinality.ByOccurrence)
        //.Append(mlContext.Transforms.NormalizeMeanVariance("R1", nameof(RoundData<int>.FirstHandStrength)))
        //.Append(mlContext.Transforms.NormalizeMeanVariance("R2", nameof(RoundData<int>.MiddleHandStrength)))
        //.Append(mlContext.Transforms.NormalizeMeanVariance("R3", nameof(RoundData<int>.LastHandStrength)))

        //.Append(mlContext.Transforms.Categorical.OneHotEncoding("R1", nameof(RoundData.FirstHandStrength)))
        //.Append(mlContext.Transforms.Categorical.OneHotEncoding("R2", nameof(RoundData.MiddleHandStrength)))
        //.Append(mlContext.Transforms.Categorical.OneHotEncoding("R3", nameof(RoundData.LastHandStrength)))
        //.Append(mlContext.Transforms.Text.FeaturizeText("R4", nameof(RoundData<int>.FirstHandType)))
        //.Append(mlContext.Transforms.Text.FeaturizeText("R5", nameof(RoundData<int>.MiddleHandType)))
        //.Append(mlContext.Transforms.Text.FeaturizeText("R6", nameof(RoundData<int>.LastHandType)))
        .Append(mlContext.Transforms.Text.FeaturizeText("Cards", new TextFeaturizingEstimator.Options(), "Card1", "Card2", "Card3", "Card4", "Card5", "Card6", "Card7", "Card8", "Card9", "Card10", "Card11", "Card12", "Card13"))
        .Append(mlContext.Transforms.Categorical.OneHotEncoding(nameof(RoundData<int>.PlayerIndex)))
        .Append(mlContext.Transforms.Categorical.OneHotEncoding(nameof(RoundData<int>.Player1RoundIndex)))
        .Append(mlContext.Transforms.Categorical.OneHotEncoding(nameof(RoundData<int>.Player2RoundIndex)))
        .Append(mlContext.Transforms.Categorical.OneHotEncoding(nameof(RoundData<int>.Player3RoundIndex)))
        .Append(mlContext.Transforms.Categorical.OneHotEncoding(nameof(RoundData<int>.Player4RoundIndex)))
        .Append(mlContext.Transforms.Concatenate("Features", "Cards", 
          nameof(RoundData<int>.PlayerIndex),
          nameof(RoundData<int>.Player1RoundIndex),
          nameof(RoundData<int>.Player2RoundIndex),
          nameof(RoundData<int>.Player3RoundIndex),
          nameof(RoundData<int>.Player4RoundIndex)));
        //.Append(mlContext.Transforms.Concatenate("Features", "Cards"));

      //ConsoleHelper.PeekDataViewInConsole(mlContext, trainingDataView, dataProcessPipeline);
      //ConsoleHelper.ConsolePressAnyKey();

      var trainingPipeline = dataProcessPipeline.Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
        .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel", "Label"));

      var trainedModel = trainingPipeline.Fit(trainingDataView);

      using (var sw = new FileStream(Path.Combine(modelPath, ModelFileName), FileMode.Create, FileAccess.Write, FileShare.Write))
      {
        mlContext.Model.Save(trainedModel, trainingDataView.Schema, sw);
      }
    }

    public IRoundStrategy GetStrategy(string modelPath)
    {
      return new CategorizationMlStrategy(Path.Combine(modelPath, ModelFileName));
    }
  }
}