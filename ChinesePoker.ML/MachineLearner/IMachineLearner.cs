using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Interface;
using Microsoft.ML.Data;

namespace ChinesePoker.ML.MachineLearner
{
  public interface IMachineLearner
  {
    string ModelFileName { get; }
    void Training(string dataFileName, string modelPath);
    IRoundStrategy GetStrategy(string modelPath);
  }
}
