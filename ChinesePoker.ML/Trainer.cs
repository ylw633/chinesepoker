using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
      public float Score { get; set; }
    }

    public class RoundStrengthPrediction
    {
      [Column(nameof(RoundData.Score))]
      public float Score { get; set; }
    }

    public void Go(string dataFileName, string modelPath)
    {
      var mlContext = new MLContext();
      var reader =  mlContext.Data.TextReader(new TextLoader.Arguments()
      {
        Separator = " ",
        HasHeader = false,
        Column = new[]
        {
          new TextLoader.Column(nameof(RoundData.FirstHandStrength), DataKind.R4, 0),
          new TextLoader.Column(nameof(RoundData.MiddleHandStrength), DataKind.R4, 1),
          new TextLoader.Column(nameof(RoundData.LastHandStrength), DataKind.R4, 2),
          new TextLoader.Column(nameof(RoundData.Score), DataKind.R4, 3),
        }
      });

      var dataView = reader.Read(dataFileName);
      var pipeline = mlContext.Transforms.CopyColumns(nameof(RoundData.Score), "Label")
        //.Append(mlContext.Transforms.Categorical.OneHotEncoding(nameof(RoundData.FirstHandStrength)))
        //.Append(mlContext.Transforms.Categorical.OneHotEncoding(nameof(RoundData.MiddleHandStrength)))
        //.Append(mlContext.Transforms.Categorical.OneHotEncoding(nameof(RoundData.LastHandStrength)))
        .Append(mlContext.Transforms.Concatenate("Features",
          nameof(RoundData.FirstHandStrength),
          nameof(RoundData.MiddleHandStrength),
          nameof(RoundData.LastHandStrength)))
        .Append(mlContext.Regression.Trainers.FastForest());
      //var pipeline = mlContext.Transforms.Conversion.MapValueToKey(nameof(RoundData.Score), "Label")
      //  .Append(mlContext.Transforms.Categorical.OneHotEncoding(nameof(RoundData.FirstHandStrength)))
      //  .Append(mlContext.Transforms.Categorical.OneHotEncoding(nameof(RoundData.MiddleHandStrength)))
      //  .Append(mlContext.Transforms.Categorical.OneHotEncoding(nameof(RoundData.LastHandStrength)))
      //  .Append(mlContext.Transforms.Concatenate("Features",
      //    nameof(RoundData.FirstHandStrength),
      //    nameof(RoundData.MiddleHandStrength),
      //    nameof(RoundData.LastHandStrength)))
      //  .AppendCacheCheckpoint(mlContext)
      //  .Append(mlContext.Transforms.Conversion.MapKeyToValue())
      //  .Append(mlContext.MulticlassClassification.Trainers.StochasticDualCoordinateAscent());


      //var preview = dataView.Preview();
      //var transPreview = pipeline.Preview(dataView);
      var model = pipeline.Fit(dataView);

      using (var sw = new FileStream(modelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
      {
        mlContext.Model.Save(model, sw);
      }
    }
  }
}
