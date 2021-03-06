﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Helper;
using ChinesePoker.Core.Interface;
using ChinesePoker.Core.Model;
using Combinatorics.Collections;

namespace ChinesePoker.Core.Component
{
  public class SimpleRoundStrategy : IRoundStrategy
  {
    public IGameHandsManager GameHandsManager { get; set; } = new PokerHandBuilderManager();
    // we have a lot of combo in all possible rounds, but only several types of hands combinations, GetSensibleRounds returns the strongest round from each type of hand type combination rounds
    public IEnumerable<Round> GetPossibleRounds(IList<Card> cards)
    {
      var allPossibleArrangements = GameHandsManager.GetAllPossibleRounds(cards);
      return allPossibleArrangements.GroupBy(r => string.Join("_", r.Hands.Select(h => h.Name))).Select(typeCombo => typeCombo.OrderByDescending(r => r.Strength).First());
    }
    public Round GetBestRound(IList<Card> cards)
    {
      return GetBestRounds(cards).First();
    }

    public IEnumerable<Round> GetBestRounds(IList<Card> cards, int take = 1)
    {
      return GetPossibleRounds(cards).OrderByDescending(r => r.Strength).Take(take);
    }

    public IEnumerable<KeyValuePair<Round, int>> GetBestRoundsWithScore(IList<Card> cards, int take = 1)
    {
      return GetBestRounds(cards, take).ToDictionary(r => r, r => r.Strength);
    }
  }
}
