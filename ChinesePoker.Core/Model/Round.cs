using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChinesePoker.Core.Model
{
  public class Round
  {
    public IList<Hand> Hands { get; private set; }
    public int Strength { get; private set; }

    public Round(IList<Hand> hands, int strength)
    {
      Hands = hands;
      Strength = strength;
    }

    public override string ToString()
    {
      return $"{Strength}\n{string.Join("\n", Hands)}";
    }
  }

  #region for machine learning
  public class RoundData
  {
    public int FirstHandStrength { get; set; }
    public int MiddleHandStrength { get; set; }
    public int LastHandStrength { get; set; }
    public int Score { get; set; }
  }
  #endregion

}
