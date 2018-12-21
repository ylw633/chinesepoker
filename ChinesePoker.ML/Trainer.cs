using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Microsoft.ML;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;

namespace ChinesePoker.ML
{
  public class Trainer
  {
    public class RoundData
    {
      public int FirstHandStrength { get; set; }
      public int MiddleHandStrength { get; set; }
      public int LastHandStrength { get; set; }
      public int Score { get; set; }
    }

    public void CategorizationTraining(string dataFileName, string modelPath)
    {
      var mlContext = new MLContext();
      var reader =  mlContext.Data.TextReader(new TextLoader.Arguments()
      {
        Separator = " ",
        HasHeader = false,
        Column = new[]
        {
          new TextLoader.Column(nameof(RoundData.FirstHandStrength), DataKind.I4, 0),
          new TextLoader.Column(nameof(RoundData.MiddleHandStrength), DataKind.I4, 1),
          new TextLoader.Column(nameof(RoundData.LastHandStrength), DataKind.I4, 2),
          new TextLoader.Column(nameof(RoundData.Score), DataKind.I4, 3),
        }
      });

      var dataView = reader.Read(dataFileName);
      var dataPipeline = mlContext.Transforms.Conversion.MapValueToKey(nameof(RoundData.Score), "Label")
        .Append(mlContext.Transforms.Categorical.OneHotEncoding(nameof(RoundData.FirstHandStrength)))
        .Append(mlContext.Transforms.Categorical.OneHotEncoding(nameof(RoundData.MiddleHandStrength)))
        .Append(mlContext.Transforms.Categorical.OneHotEncoding(nameof(RoundData.LastHandStrength)))
        .Append(mlContext.Transforms.Concatenate("Features",
          nameof(RoundData.FirstHandStrength),
          nameof(RoundData.MiddleHandStrength),
          nameof(RoundData.LastHandStrength)))
        .AppendCacheCheckpoint(mlContext);
      //var trainer = mlContext.MulticlassClassification.Trainers.StochasticDualCoordinateAscent();

      var averagedPerceptronBinaryTrainer = mlContext.BinaryClassification.Trainers.AveragedPerceptron(numIterations: 10);
      var trainer = mlContext.MulticlassClassification.Trainers.OneVersusAll(averagedPerceptronBinaryTrainer);

      var trainerPipeline = dataPipeline.Append(trainer).Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));


      var preview = dataView.Preview();
      var transPreview = trainerPipeline.Preview(dataView);

      //var crossValidationResults = mlContext.MulticlassClassification.CrossValidate(dataView, trainerPipeline);
      //ConsoleHelper.PrintMulticlassClassificationFoldsAverageMetrics(trainer.ToString(), crossValidationResults);

      var model = trainerPipeline.Fit(dataView);

      using (var sw = new FileStream(modelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
      {
        mlContext.Model.Save(model, sw);
      }

      ConsoleHelper.ConsolePressAnyKey();
    }

    public void RegressionTraining(string dataFileName, string modelPath)
    {
      var mlContext = new MLContext();
      var reader =  mlContext.Data.TextReader(new TextLoader.Arguments()
      {
        Separator = " ",
        HasHeader = false,
        Column = new[]
        {
          new TextLoader.Column(nameof(RoundData.FirstHandStrength), DataKind.I4, 0),
          new TextLoader.Column(nameof(RoundData.MiddleHandStrength), DataKind.I4, 1),
          new TextLoader.Column(nameof(RoundData.LastHandStrength), DataKind.I4, 2),
          new TextLoader.Column(nameof(RoundData.Score), DataKind.R4, 3),
        }
      });

      var dataView = reader.Read(dataFileName);

      var pipeline = mlContext.Transforms.CopyColumns(nameof(RoundData.Score), "Label")
        .Append(mlContext.Transforms.Categorical.OneHotEncoding(nameof(RoundData.FirstHandStrength)))
        .Append(mlContext.Transforms.Categorical.OneHotEncoding(nameof(RoundData.MiddleHandStrength)))
        .Append(mlContext.Transforms.Categorical.OneHotEncoding(nameof(RoundData.LastHandStrength)))
        .Append(mlContext.Transforms.Concatenate("Features",
          nameof(RoundData.FirstHandStrength),
          nameof(RoundData.MiddleHandStrength),
          nameof(RoundData.LastHandStrength)))
        .Append(mlContext.Regression.Trainers.FastForest());

      var preview = dataView.Preview();
      var transPreview = pipeline.Preview(dataView);
      var model = pipeline.Fit(dataView);

      using (var sw = new FileStream(modelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
      {
        mlContext.Model.Save(model, sw);
      }
    }
  }
}
