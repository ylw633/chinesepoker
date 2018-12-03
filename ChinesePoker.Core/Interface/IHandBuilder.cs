using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Model;

namespace ChinesePoker.Core.Interface
{
  public interface IHandBuilder
  {
    string HandName { get; }
    bool TestIsHand(IList<Card> cards);
    IList<Card> SortCards(IList<Card> cards);
    int GetStrength(IList<Card> cards);
    int CompareHands(IList<Card> srcCards, IList<Card> targetCards);
  }
}
