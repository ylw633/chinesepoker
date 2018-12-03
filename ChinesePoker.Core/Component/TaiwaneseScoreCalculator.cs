using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Interface;
using ChinesePoker.Core.Model;

namespace ChinesePoker.Core.Component
{
  public class TaiwaneseScoreCalculator : IScoreCalculator
  {
    public bool IsDoubleOnStrike { get; set; } = true;
    public bool IsDoubleOnThreeOfKindInFirstHand { get; set; } = true;
    public bool IsDoubleOnFourOfKind { get; set; } = true;
    public bool IsDoubleOnRoyalFlush { get; set; } = true;
    public void GetScore(Round roundA, Round roundB, out int scoreA, out int socreB)
    {
      scoreA = socreB = 0;
    }
  }
}
