using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Model;

namespace ChinesePoker.Core.Interface
{
  public interface IHand
  {
    bool IsValid(IEnumerable<Card> cards);
    IEnumerable<IEnumerable<Card>> Pick(IEnumerable<Card> cards);

    int ScoreMultiplier { get; }
  }
}
