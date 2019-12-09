using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Model;

namespace ChinesePoker.Core.Interface
{
  public interface IStrengthStrategy
  {
    Hand GetAHand(IEnumerable<Card> cards);
    int CompareHands(Hand x, Hand y);

    int ComputeHandsStrength(IEnumerable<Hand> hands);
  }
}
