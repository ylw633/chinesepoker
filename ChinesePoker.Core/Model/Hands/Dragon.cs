using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Combinatorics.Collections;

namespace ChinesePoker.Core.Model.Hands
{
  public class Dragon : HandBase
  {
    public override int ScoreMultiplier => 2;

    public override IEnumerable<IEnumerable<Card>> Pick(IEnumerable<Card> cards)
    {
      var cardList = cards.OrderBy(c => c.RankOrdinal).ToList();
      if (cardList.Count < 13) return new List<List<Card>>();

      return new Combinations<Card>(cardList, 13, GenerateOption.WithoutRepetition).Where(Predicate);
    }

    private bool Predicate(IList<Card> cards)
    {
      cards = cards.OrderBy(c => c.RankOrdinal).ToList();
      var gotit = true;
      for (var i = 0; gotit && i < 12; i++)
        gotit &= cards[i].RankOrdinal < cards[i + 1].RankOrdinal;

      return gotit;
    }
  }
}
