using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Interface;
using ChinesePoker.Core.Model;
using Combinatorics.Collections;

namespace ChinesePoker.Core.Helper
{
  public class RoundStrategyExtension
  {
    public static IEnumerable<Round> GetAllPossibleRounds(IGameHandsManager handsManager, IList<Card> cards)
    {
      if (cards.Count != 13) throw new Exception("not enough cards");
      var hand = handsManager.DetermineHand(cards); // if it's a dragon
      if (hand != null)
      {
        yield return new Round(new List<Hand> { hand });
        yield break;
      }

      var hash = new HashSet<string>();
      foreach (var thirdRoundCards in new Combinations<Card>(cards, 5, GenerateOption.WithoutRepetition))
      {
        var thirdRound = handsManager.DetermineHand(thirdRoundCards);
        if (thirdRound == null || thirdRound.Strength < handsManager.MinHandStrengthThreshold) continue; // no need to continue if 3rd round is lower than min threshold
        var secondRoundSet = cards.Except(thirdRoundCards).ToList();
        foreach (var secondRoundCards in new Combinations<Card>(secondRoundSet, 5, GenerateOption.WithoutRepetition))
        {
          var secondRound = handsManager.DetermineHand(secondRoundCards, thirdRound);
          if (secondRound == null) continue;

          var firstRoundCards = secondRoundSet.Except(secondRoundCards).ToList();
          var firstRound = handsManager.DetermineHand(firstRoundCards, secondRound);
          if (firstRound == null) continue;

          // ignore ones with same strength
          var hashKey = thirdRound.Strength.ToString() + secondRound.Strength + firstRound.Strength;
          if (hash.Contains(hashKey))
            continue;
          hash.Add(hashKey);
          yield return new Round(new List<Hand> { firstRound, secondRound, thirdRound });
        }
      }
    }
  }
}
