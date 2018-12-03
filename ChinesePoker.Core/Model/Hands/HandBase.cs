using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Interface;

namespace ChinesePoker.Core.Model.Hands
{
  public abstract class HandBase : IHand
  {
    public bool IsValid(IEnumerable<Card> cards)
    {
      return Pick(cards).Any();
    }
    public abstract IEnumerable<IEnumerable<Card>> Pick(IEnumerable<Card> cards);
    public virtual int ScoreMultiplier => 1;
  }
}
