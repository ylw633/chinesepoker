using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using ChinesePoker.Core.Helper;
using ChinesePoker.Core.Interface;
using ChinesePoker.Core.Model;

namespace ChinesePoker.Core.Component.HandBuilders
{
  public class HandStats
  {
    private const int Scale = 100000;
    public HandTypes Type { get; }
    public Dictionary<string, int> HandStrength { get; }

    public HandStats(HandTypes type, IList<string> handStrength, double probabilityBase, double probabilityCeiling, Func<IList<string>, double, double, Dictionary<string, int>> buildStrengthFunc = null)
    {
      Type = type;
      HandStrength = buildStrengthFunc == null ? DefaultGetStrengthLookup(handStrength, probabilityBase, probabilityCeiling) : buildStrengthFunc(handStrength, probabilityBase, probabilityCeiling);
    }

    public static Dictionary<string, int> DefaultGetStrengthLookup(IList<string> sortedHands, double probabilityBase, double probabilityCeiling)
    {
      var i = 0;
      return sortedHands.ToDictionary(s => s, s => PercentileToStrength(probabilityBase + (i++ / (double) sortedHands.Count * (probabilityCeiling - probabilityBase))));
    }

    public static Dictionary<string, int> CardCountGetStrengthLookup(IList<string> sortedHands, double probabilityBase, double probabilityCeiling, int count)
    {
      var ret = new Dictionary<string, int>();
      foreach (var hand in sortedHands)
      {
        var rank = hand.GroupBy(c => c).FirstOrDefault(g => g.Count() == count)?.Key.ToString();
        if (string.IsNullOrEmpty(rank))
          throw new Exception($"Expecting hand {hand} to have {count} of same rank but is not found.");
        ret.Add(hand, PercentileToStrength(probabilityBase + (RankToStrength(rank[0]) - 2) / (double)13 * (probabilityCeiling - probabilityBase)));
      }

      return ret;
    }

    public static int PercentileToStrength(double percentile)
    {
      return (int)Math.Round(percentile * Scale);
    }

    public static double StrengthToPercentile(int strength)
    {
      return strength / (double)Scale;
    }

    public static int RankToStrength(char rank)
    {
      var strength = Card.RankToOrdinal(rank);
      return char.ToUpper(rank) == 'A' ? 14 : strength;
    }
  }

	public class PokerStrengthStrategy : IHandStrengthArbiter
	{
		#region static preparation

		public static Dictionary<HandTypes, HandStats> FiveCardsStrengthLookup { get; } = new Dictionary<HandTypes, HandStats>();
    public static Dictionary<HandTypes, HandStats> ThreeCardsStrengthLookup { get; } = new Dictionary<HandTypes, HandStats>();
		public static Dictionary<HandTypes, HandStats> ThirteenCardsStrengthLookup { get; } = new Dictionary<HandTypes, HandStats>();

    static PokerStrengthStrategy()
    {
      ThreeCardsStrengthLookup.Add(HandTypes.HighCard, new HandStats(HandTypes.HighCard, AllHighCards3Sorted, 0, 0.82786));
      ThreeCardsStrengthLookup.Add(HandTypes.OnePair, new HandStats(HandTypes.OnePair, AllOnePair3Sorted, 0.82786, 0.997195));
      ThreeCardsStrengthLookup.Add(HandTypes.ThreeOfAKind, new HandStats(HandTypes.ThreeOfAKind, AllThreeOfAKind3Sorted, 0.997195, 1, (list, b, c) => HandStats.CardCountGetStrengthLookup(list, b, c, 3)));

      FiveCardsStrengthLookup.Add(HandTypes.HighCard, new HandStats(HandTypes.HighCard, AllHighCards5Sorted, 0, 0.501177));
      FiveCardsStrengthLookup.Add(HandTypes.OnePair, new HandStats(HandTypes.OnePair, AllOnePair5Sorted, 0.501177, 0.923746));
      FiveCardsStrengthLookup.Add(HandTypes.TwoPairs, new HandStats(HandTypes.TwoPairs, AllTwoPairsSorted, 0.923746, 0.971285));
      FiveCardsStrengthLookup.Add(HandTypes.ThreeOfAKind, new HandStats(HandTypes.ThreeOfAKind, AllThreeOfAKind5Sorted, 0.971285, 0.992413, (list, b, c) => HandStats.CardCountGetStrengthLookup(list, b, c, 3)));
      FiveCardsStrengthLookup.Add(HandTypes.Straight, new HandStats(HandTypes.Straight, AllStraightSorted, 0.992413, 0.996338));
      FiveCardsStrengthLookup.Add(HandTypes.Flush, new HandStats(HandTypes.Flush, AllFlushSorted, 0.996338, 0.998303));
      FiveCardsStrengthLookup.Add(HandTypes.FullHouse, new HandStats(HandTypes.FullHouse, AllFullHouseSorted, 0.998303, 0.999744, (list, b, c) => HandStats.CardCountGetStrengthLookup(list, b, c, 3)));
      FiveCardsStrengthLookup.Add(HandTypes.FourOfAKind, new HandStats(HandTypes.FourOfAKind, AllFourOfAKindSorted, 0.999744, 0.999984, (list, b, c) => HandStats.CardCountGetStrengthLookup(list, b, c, 4)));
      FiveCardsStrengthLookup.Add(HandTypes.StraightFlush, new HandStats(HandTypes.StraightFlush, AllStraightFlushSorted, 0.999984, 1));

			ThirteenCardsStrengthLookup.Add(HandTypes.Dragon, new HandStats(HandTypes.Dragon, AllDragon, 1, 1));
		}
		#endregion

    public int CardRankToStrength(char rank)
    {
      return HandStats.RankToStrength(rank);
    }

    public Hand GetAHand(IEnumerable<Card> cards)
    {
      if (cards == null) return null;
      cards = cards.ToList();
      if (cards.Distinct().Count() != cards.Count())
        throw new ArgumentException($"duplicate cards! => {string.Join(" ", cards.Select(c => c.ToString()))} ", nameof(cards));

      var cardList = cards.OrderBy(c => c.Ordinal).ThenBy(c => c.Suit).ToList();
      var cardRank = new string(cardList.Select(c => c.Rank).ToArray());

      switch (cardList.Count)
      {
        case 13:
					if (ThirteenCardsStrengthLookup[HandTypes.Dragon].HandStrength.ContainsKey(cardRank))
						return new Hand($"{nameof(HandTypes.Dragon)}", cardList, ThirteenCardsStrengthLookup[HandTypes.Dragon].HandStrength[cardRank]);
          break;

        case 5:
          foreach (var kv in FiveCardsStrengthLookup.Reverse())
          {
            if (!kv.Value.HandStrength.ContainsKey(cardRank)) continue;

            if (kv.Key == HandTypes.StraightFlush && !IsSameSuit(cardList)) continue;
            if (kv.Key == HandTypes.Flush && !IsSameSuit(cardList)) continue;

            return new Hand($"{kv.Key:G}", cardList, FiveCardsStrengthLookup[kv.Key].HandStrength[cardRank]);
          }
          break;

        case 3:
          foreach (var kv in ThreeCardsStrengthLookup.Reverse())
          {
            if (!kv.Value.HandStrength.ContainsKey(cardRank)) continue;

            return new Hand($"{kv.Key:G}", cardList, ThreeCardsStrengthLookup[kv.Key].HandStrength[cardRank]);
          }
					break;
      }

      return null;
    }

    public int CompareHands(Hand x, Hand y)
    {
			// both hands are of same count
      if (x.Cards.Count == y.Cards.Count) return x.Strength.CompareTo(y.Strength);

      var handA = (HandTypes)Enum.Parse(typeof(HandTypes), x.Name, true);
      var handB = (HandTypes)Enum.Parse(typeof(HandTypes), y.Name, true);

      var compareHandTypes = handA.CompareTo(handB);
      if (compareHandTypes != 0) return compareHandTypes;

			// both hands are of different count but same type, only possible scenarios would be HighCards, OnePair, and ThreeOfAKind
			var rankA = new string(x.Cards.Select(c => c.Rank).ToArray());
      var rankB = new string(y.Cards.Select(c => c.Rank).ToArray());
			
      switch (handA)
      {
        case HandTypes.HighCard:
          return new CardComparer(CardRankToStrength).Compare(rankA, rankB);
        case HandTypes.OnePair:
          return new CardComparer(CardRankToStrength, new Func<string, string>[]{ s => s.GroupBy(c => c).OrderByDescending(g => g.Count()).First().Key.ToString(), s => s.GroupBy(c => c).Where(g => g.Count() == 1).OrderByDescending(g => CardRankToStrength(g.Key)).First().Key.ToString()}).Compare(rankA, rankB);
        case HandTypes.ThreeOfAKind:
          return new CardComparer(CardRankToStrength, new Func<string, string>[] { s => s.GroupBy(c => c).First(g => g.Count() == 3).Key.ToString() }).Compare(rankA, rankB);
        default:
          throw new Exception("impossible scenario, something is wrong");
      }
    }

    public int ComputeHandsStrength(IEnumerable<Hand> hands)
    {
      return (int)Math.Round(hands.Average(h => h.Strength));
    }

    public bool IsSameSuit(IList<Card> cards)
    {
      var firstSuit = cards[0].Suit;
      return cards.Skip(1).All(c => c.Suit == firstSuit);
		}

    public static IEnumerable<(string type, string cards)> StrengthToCards(int strength, int round)
    {
      if (round == 1)
      {
        return GetMatchingHands(ThirteenCardsStrengthLookup).Concat(GetMatchingHands(ThreeCardsStrengthLookup));
      }

      return round < 4 ? GetMatchingHands(FiveCardsStrengthLookup) : Enumerable.Empty<(string type, string cards)>();

      IEnumerable<(string type, string cards)> GetMatchingHands(Dictionary<HandTypes, HandStats> src)
      {
        return from l in src.Values
          let t = l.Type
          from s in l.HandStrength
          where s.Value == strength
          select (l.Type.ToString(), s.Key);
      }
    }

    #region all possible hands
		public static List<string> AllHighCards5Sorted { get; } = new List<string>
		{
      "23457","23467","23567","24567","23458","23468","23568","24568","34568","23478","23578","24578","34578","23678","24678","25678","34678","35678","23459","23469",
      "23569","24569","34569","23479","23579","24579","34579","23679","24679","25679","34679","35679","45679","23489","23589","24589","34589","23689","24689","25689",
      "34689","35689","45689","23789","24789","25789","26789","34789","35789","36789","45789","46789","2345T","2346T","2356T","2456T","3456T","2347T","2357T","2457T",
      "3457T","2367T","2467T","2567T","3467T","3567T","4567T","2348T","2358T","2458T","3458T","2368T","2468T","2568T","3468T","3568T","4568T","2378T","2478T","2578T",
      "2678T","3478T","3578T","3678T","4578T","4678T","5678T","2349T","2359T","2459T","3459T","2369T","2469T","2569T","3469T","3569T","4569T","2379T","2479T","2579T",
      "2679T","3479T","3579T","3679T","4579T","4679T","5679T","2389T","2489T","2589T","2689T","2789T","3489T","3589T","3689T","3789T","4589T","4689T","4789T","5689T",
      "5789T","2345J","2346J","2356J","2456J","3456J","2347J","2357J","2457J","3457J","2367J","2467J","2567J","3467J","3567J","4567J","2348J","2358J","2458J","3458J",
      "2368J","2468J","2568J","3468J","3568J","4568J","2378J","2478J","2578J","2678J","3478J","3578J","3678J","4578J","4678J","5678J","2349J","2359J","2459J","3459J",
      "2369J","2469J","2569J","3469J","3569J","4569J","2379J","2479J","2579J","2679J","3479J","3579J","3679J","4579J","4679J","5679J","2389J","2489J","2589J","2689J",
      "2789J","3489J","3589J","3689J","3789J","4589J","4689J","4789J","5689J","5789J","6789J","234TJ","235TJ","245TJ","345TJ","236TJ","246TJ","256TJ","346TJ","356TJ",
      "456TJ","237TJ","247TJ","257TJ","267TJ","347TJ","357TJ","367TJ","457TJ","467TJ","567TJ","238TJ","248TJ","258TJ","268TJ","278TJ","348TJ","358TJ","368TJ","378TJ",
      "458TJ","468TJ","478TJ","568TJ","578TJ","678TJ","239TJ","249TJ","259TJ","269TJ","279TJ","289TJ","349TJ","359TJ","369TJ","379TJ","389TJ","459TJ","469TJ","479TJ",
      "489TJ","569TJ","579TJ","589TJ","679TJ","689TJ","2345Q","2346Q","2356Q","2456Q","3456Q","2347Q","2357Q","2457Q","3457Q","2367Q","2467Q","2567Q","3467Q","3567Q",
      "4567Q","2348Q","2358Q","2458Q","3458Q","2368Q","2468Q","2568Q","3468Q","3568Q","4568Q","2378Q","2478Q","2578Q","2678Q","3478Q","3578Q","3678Q","4578Q","4678Q",
      "5678Q","2349Q","2359Q","2459Q","3459Q","2369Q","2469Q","2569Q","3469Q","3569Q","4569Q","2379Q","2479Q","2579Q","2679Q","3479Q","3579Q","3679Q","4579Q","4679Q",
      "5679Q","2389Q","2489Q","2589Q","2689Q","2789Q","3489Q","3589Q","3689Q","3789Q","4589Q","4689Q","4789Q","5689Q","5789Q","6789Q","234TQ","235TQ","245TQ","345TQ",
      "236TQ","246TQ","256TQ","346TQ","356TQ","456TQ","237TQ","247TQ","257TQ","267TQ","347TQ","357TQ","367TQ","457TQ","467TQ","567TQ","238TQ","248TQ","258TQ","268TQ",
      "278TQ","348TQ","358TQ","368TQ","378TQ","458TQ","468TQ","478TQ","568TQ","578TQ","678TQ","239TQ","249TQ","259TQ","269TQ","279TQ","289TQ","349TQ","359TQ","369TQ",
      "379TQ","389TQ","459TQ","469TQ","479TQ","489TQ","569TQ","579TQ","589TQ","679TQ","689TQ","789TQ","234JQ","235JQ","245JQ","345JQ","236JQ","246JQ","256JQ","346JQ",
      "356JQ","456JQ","237JQ","247JQ","257JQ","267JQ","347JQ","357JQ","367JQ","457JQ","467JQ","567JQ","238JQ","248JQ","258JQ","268JQ","278JQ","348JQ","358JQ","368JQ",
      "378JQ","458JQ","468JQ","478JQ","568JQ","578JQ","678JQ","239JQ","249JQ","259JQ","269JQ","279JQ","289JQ","349JQ","359JQ","369JQ","379JQ","389JQ","459JQ","469JQ",
      "479JQ","489JQ","569JQ","579JQ","589JQ","679JQ","689JQ","789JQ","23TJQ","24TJQ","25TJQ","26TJQ","27TJQ","28TJQ","29TJQ","34TJQ","35TJQ","36TJQ","37TJQ","38TJQ",
      "39TJQ","45TJQ","46TJQ","47TJQ","48TJQ","49TJQ","56TJQ","57TJQ","58TJQ","59TJQ","67TJQ","68TJQ","69TJQ","78TJQ","79TJQ","2345K","2346K","2356K","2456K","3456K",
      "2347K","2357K","2457K","3457K","2367K","2467K","2567K","3467K","3567K","4567K","2348K","2358K","2458K","3458K","2368K","2468K","2568K","3468K","3568K","4568K",
      "2378K","2478K","2578K","2678K","3478K","3578K","3678K","4578K","4678K","5678K","2349K","2359K","2459K","3459K","2369K","2469K","2569K","3469K","3569K","4569K",
      "2379K","2479K","2579K","2679K","3479K","3579K","3679K","4579K","4679K","5679K","2389K","2489K","2589K","2689K","2789K","3489K","3589K","3689K","3789K","4589K",
      "4689K","4789K","5689K","5789K","6789K","234TK","235TK","245TK","345TK","236TK","246TK","256TK","346TK","356TK","456TK","237TK","247TK","257TK","267TK","347TK",
      "357TK","367TK","457TK","467TK","567TK","238TK","248TK","258TK","268TK","278TK","348TK","358TK","368TK","378TK","458TK","468TK","478TK","568TK","578TK","678TK",
      "239TK","249TK","259TK","269TK","279TK","289TK","349TK","359TK","369TK","379TK","389TK","459TK","469TK","479TK","489TK","569TK","579TK","589TK","679TK","689TK",
      "789TK","234JK","235JK","245JK","345JK","236JK","246JK","256JK","346JK","356JK","456JK","237JK","247JK","257JK","267JK","347JK","357JK","367JK","457JK","467JK",
      "567JK","238JK","248JK","258JK","268JK","278JK","348JK","358JK","368JK","378JK","458JK","468JK","478JK","568JK","578JK","678JK","239JK","249JK","259JK","269JK",
      "279JK","289JK","349JK","359JK","369JK","379JK","389JK","459JK","469JK","479JK","489JK","569JK","579JK","589JK","679JK","689JK","789JK","23TJK","24TJK","25TJK",
      "26TJK","27TJK","28TJK","29TJK","34TJK","35TJK","36TJK","37TJK","38TJK","39TJK","45TJK","46TJK","47TJK","48TJK","49TJK","56TJK","57TJK","58TJK","59TJK","67TJK",
      "68TJK","69TJK","78TJK","79TJK","89TJK","234QK","235QK","245QK","345QK","236QK","246QK","256QK","346QK","356QK","456QK","237QK","247QK","257QK","267QK","347QK",
      "357QK","367QK","457QK","467QK","567QK","238QK","248QK","258QK","268QK","278QK","348QK","358QK","368QK","378QK","458QK","468QK","478QK","568QK","578QK","678QK",
      "239QK","249QK","259QK","269QK","279QK","289QK","349QK","359QK","369QK","379QK","389QK","459QK","469QK","479QK","489QK","569QK","579QK","589QK","679QK","689QK",
      "789QK","23TQK","24TQK","25TQK","26TQK","27TQK","28TQK","29TQK","34TQK","35TQK","36TQK","37TQK","38TQK","39TQK","45TQK","46TQK","47TQK","48TQK","49TQK","56TQK",
      "57TQK","58TQK","59TQK","67TQK","68TQK","69TQK","78TQK","79TQK","89TQK","23JQK","24JQK","25JQK","26JQK","27JQK","28JQK","29JQK","2TJQK","34JQK","35JQK","36JQK",
      "37JQK","38JQK","39JQK","3TJQK","45JQK","46JQK","47JQK","48JQK","49JQK","4TJQK","56JQK","57JQK","58JQK","59JQK","5TJQK","67JQK","68JQK","69JQK","6TJQK","78JQK",
      "79JQK","7TJQK","89JQK","8TJQK","A2346","A2356","A2456","A3456","A2347","A2357","A2457","A3457","A2367","A2467","A2567","A3467","A3567","A4567","A2348","A2358",
      "A2458","A3458","A2368","A2468","A2568","A3468","A3568","A4568","A2378","A2478","A2578","A2678","A3478","A3578","A3678","A4578","A4678","A5678","A2349","A2359",
      "A2459","A3459","A2369","A2469","A2569","A3469","A3569","A4569","A2379","A2479","A2579","A2679","A3479","A3579","A3679","A4579","A4679","A5679","A2389","A2489",
      "A2589","A2689","A2789","A3489","A3589","A3689","A3789","A4589","A4689","A4789","A5689","A5789","A6789","A234T","A235T","A245T","A345T","A236T","A246T","A256T",
      "A346T","A356T","A456T","A237T","A247T","A257T","A267T","A347T","A357T","A367T","A457T","A467T","A567T","A238T","A248T","A258T","A268T","A278T","A348T","A358T",
      "A368T","A378T","A458T","A468T","A478T","A568T","A578T","A678T","A239T","A249T","A259T","A269T","A279T","A289T","A349T","A359T","A369T","A379T","A389T","A459T",
      "A469T","A479T","A489T","A569T","A579T","A589T","A679T","A689T","A789T","A234J","A235J","A245J","A345J","A236J","A246J","A256J","A346J","A356J","A456J","A237J",
      "A247J","A257J","A267J","A347J","A357J","A367J","A457J","A467J","A567J","A238J","A248J","A258J","A268J","A278J","A348J","A358J","A368J","A378J","A458J","A468J",
      "A478J","A568J","A578J","A678J","A239J","A249J","A259J","A269J","A279J","A289J","A349J","A359J","A369J","A379J","A389J","A459J","A469J","A479J","A489J","A569J",
      "A579J","A589J","A679J","A689J","A789J","A23TJ","A24TJ","A25TJ","A26TJ","A27TJ","A28TJ","A29TJ","A34TJ","A35TJ","A36TJ","A37TJ","A38TJ","A39TJ","A45TJ","A46TJ",
      "A47TJ","A48TJ","A49TJ","A56TJ","A57TJ","A58TJ","A59TJ","A67TJ","A68TJ","A69TJ","A78TJ","A79TJ","A89TJ","A234Q","A235Q","A245Q","A345Q","A236Q","A246Q","A256Q",
      "A346Q","A356Q","A456Q","A237Q","A247Q","A257Q","A267Q","A347Q","A357Q","A367Q","A457Q","A467Q","A567Q","A238Q","A248Q","A258Q","A268Q","A278Q","A348Q","A358Q",
      "A368Q","A378Q","A458Q","A468Q","A478Q","A568Q","A578Q","A678Q","A239Q","A249Q","A259Q","A269Q","A279Q","A289Q","A349Q","A359Q","A369Q","A379Q","A389Q","A459Q",
      "A469Q","A479Q","A489Q","A569Q","A579Q","A589Q","A679Q","A689Q","A789Q","A23TQ","A24TQ","A25TQ","A26TQ","A27TQ","A28TQ","A29TQ","A34TQ","A35TQ","A36TQ","A37TQ",
      "A38TQ","A39TQ","A45TQ","A46TQ","A47TQ","A48TQ","A49TQ","A56TQ","A57TQ","A58TQ","A59TQ","A67TQ","A68TQ","A69TQ","A78TQ","A79TQ","A89TQ","A23JQ","A24JQ","A25JQ",
      "A26JQ","A27JQ","A28JQ","A29JQ","A2TJQ","A34JQ","A35JQ","A36JQ","A37JQ","A38JQ","A39JQ","A3TJQ","A45JQ","A46JQ","A47JQ","A48JQ","A49JQ","A4TJQ","A56JQ","A57JQ",
      "A58JQ","A59JQ","A5TJQ","A67JQ","A68JQ","A69JQ","A6TJQ","A78JQ","A79JQ","A7TJQ","A89JQ","A8TJQ","A9TJQ","A234K","A235K","A245K","A345K","A236K","A246K","A256K",
      "A346K","A356K","A456K","A237K","A247K","A257K","A267K","A347K","A357K","A367K","A457K","A467K","A567K","A238K","A248K","A258K","A268K","A278K","A348K","A358K",
      "A368K","A378K","A458K","A468K","A478K","A568K","A578K","A678K","A239K","A249K","A259K","A269K","A279K","A289K","A349K","A359K","A369K","A379K","A389K","A459K",
      "A469K","A479K","A489K","A569K","A579K","A589K","A679K","A689K","A789K","A23TK","A24TK","A25TK","A26TK","A27TK","A28TK","A29TK","A34TK","A35TK","A36TK","A37TK",
      "A38TK","A39TK","A45TK","A46TK","A47TK","A48TK","A49TK","A56TK","A57TK","A58TK","A59TK","A67TK","A68TK","A69TK","A78TK","A79TK","A89TK","A23JK","A24JK","A25JK",
      "A26JK","A27JK","A28JK","A29JK","A2TJK","A34JK","A35JK","A36JK","A37JK","A38JK","A39JK","A3TJK","A45JK","A46JK","A47JK","A48JK","A49JK","A4TJK","A56JK","A57JK",
      "A58JK","A59JK","A5TJK","A67JK","A68JK","A69JK","A6TJK","A78JK","A79JK","A7TJK","A89JK","A8TJK","A9TJK","A23QK","A24QK","A25QK","A26QK","A27QK","A28QK","A29QK",
      "A2TQK","A2JQK","A34QK","A35QK","A36QK","A37QK","A38QK","A39QK","A3TQK","A3JQK","A45QK","A46QK","A47QK","A48QK","A49QK","A4TQK","A4JQK","A56QK","A57QK","A58QK",
      "A59QK","A5TQK","A5JQK","A67QK","A68QK","A69QK","A6TQK","A6JQK","A78QK","A79QK","A7TQK","A7JQK","A89QK","A8TQK","A8JQK","A9TQK","A9JQK",
		};

    public static List<string> AllHighCards3Sorted { get; } = new List<string>
    {
      "234","235","245","345","236","246","346","256","356","456","237","247","347","257","357","457","267","367","467","567",
      "238","248","348","258","358","458","268","368","468","568","278","378","478","578","678","239","249","349","259","359",
      "459","269","369","469","569","279","379","479","579","679","289","389","489","589","689","789","23T","24T","34T","25T",
      "35T","45T","26T","36T","46T","56T","27T","37T","47T","57T","67T","28T","38T","48T","58T","68T","78T","29T","39T","49T",
      "59T","69T","79T","89T","23J","24J","34J","25J","35J","45J","26J","36J","46J","56J","27J","37J","47J","57J","67J","28J",
      "38J","48J","58J","68J","78J","29J","39J","49J","59J","69J","79J","89J","2TJ","3TJ","4TJ","5TJ","6TJ","7TJ","8TJ","9TJ",
      "23Q","24Q","34Q","25Q","35Q","45Q","26Q","36Q","46Q","56Q","27Q","37Q","47Q","57Q","67Q","28Q","38Q","48Q","58Q","68Q",
      "78Q","29Q","39Q","49Q","59Q","69Q","79Q","89Q","2TQ","3TQ","4TQ","5TQ","6TQ","7TQ","8TQ","9TQ","2JQ","3JQ","4JQ","5JQ",
      "6JQ","7JQ","8JQ","9JQ","TJQ","23K","24K","34K","25K","35K","45K","26K","36K","46K","56K","27K","37K","47K","57K","67K",
      "28K","38K","48K","58K","68K","78K","29K","39K","49K","59K","69K","79K","89K","2TK","3TK","4TK","5TK","6TK","7TK","8TK",
      "9TK","2JK","3JK","4JK","5JK","6JK","7JK","8JK","9JK","TJK","2QK","3QK","4QK","5QK","6QK","7QK","8QK","9QK","TQK","JQK",
      "A23","A24","A34","A25","A35","A45","A26","A36","A46","A56","A27","A37","A47","A57","A67","A28","A38","A48","A58","A68",
      "A78","A29","A39","A49","A59","A69","A79","A89","A2T","A3T","A4T","A5T","A6T","A7T","A8T","A9T","A2J","A3J","A4J","A5J",
      "A6J","A7J","A8J","A9J","ATJ","A2Q","A3Q","A4Q","A5Q","A6Q","A7Q","A8Q","A9Q","ATQ","AJQ","A2K","A3K","A4K","A5K","A6K",
      "A7K","A8K","A9K","ATK","AJK","AQK",
    };

    public static List<string> AllOnePair5Sorted { get; } = new List<string>
    {
      "22345","22346","22356","22456","22347","22357","22457","22367","22467","22567","22348","22358","22458","22368","22468","22568","22378","22478","22578","22678",
      "22349","22359","22459","22369","22469","22569","22379","22479","22579","22679","22389","22489","22589","22689","22789","2234T","2235T","2245T","2236T","2246T",
      "2256T","2237T","2247T","2257T","2267T","2238T","2248T","2258T","2268T","2278T","2239T","2249T","2259T","2269T","2279T","2289T","2234J","2235J","2245J","2236J",
      "2246J","2256J","2237J","2247J","2257J","2267J","2238J","2248J","2258J","2268J","2278J","2239J","2249J","2259J","2269J","2279J","2289J","223TJ","224TJ","225TJ",
      "226TJ","227TJ","228TJ","229TJ","2234Q","2235Q","2245Q","2236Q","2246Q","2256Q","2237Q","2247Q","2257Q","2267Q","2238Q","2248Q","2258Q","2268Q","2278Q","2239Q",
      "2249Q","2259Q","2269Q","2279Q","2289Q","223TQ","224TQ","225TQ","226TQ","227TQ","228TQ","229TQ","223JQ","224JQ","225JQ","226JQ","227JQ","228JQ","229JQ","22TJQ",
      "2234K","2235K","2245K","2236K","2246K","2256K","2237K","2247K","2257K","2267K","2238K","2248K","2258K","2268K","2278K","2239K","2249K","2259K","2269K","2279K",
      "2289K","223TK","224TK","225TK","226TK","227TK","228TK","229TK","223JK","224JK","225JK","226JK","227JK","228JK","229JK","22TJK","223QK","224QK","225QK","226QK",
      "227QK","228QK","229QK","22TQK","22JQK","A2234","A2235","A2245","A2236","A2246","A2256","A2237","A2247","A2257","A2267","A2238","A2248","A2258","A2268","A2278",
      "A2239","A2249","A2259","A2269","A2279","A2289","A223T","A224T","A225T","A226T","A227T","A228T","A229T","A223J","A224J","A225J","A226J","A227J","A228J","A229J",
      "A22TJ","A223Q","A224Q","A225Q","A226Q","A227Q","A228Q","A229Q","A22TQ","A22JQ","A223K","A224K","A225K","A226K","A227K","A228K","A229K","A22TK","A22JK","A22QK",
      "23345","23346","23356","33456","23347","23357","33457","23367","33467","33567","23348","23358","33458","23368","33468","33568","23378","33478","33578","33678",
      "23349","23359","33459","23369","33469","33569","23379","33479","33579","33679","23389","33489","33589","33689","33789","2334T","2335T","3345T","2336T","3346T",
      "3356T","2337T","3347T","3357T","3367T","2338T","3348T","3358T","3368T","3378T","2339T","3349T","3359T","3369T","3379T","3389T","2334J","2335J","3345J","2336J",
      "3346J","3356J","2337J","3347J","3357J","3367J","2338J","3348J","3358J","3368J","3378J","2339J","3349J","3359J","3369J","3379J","3389J","233TJ","334TJ","335TJ",
      "336TJ","337TJ","338TJ","339TJ","2334Q","2335Q","3345Q","2336Q","3346Q","3356Q","2337Q","3347Q","3357Q","3367Q","2338Q","3348Q","3358Q","3368Q","3378Q","2339Q",
      "3349Q","3359Q","3369Q","3379Q","3389Q","233TQ","334TQ","335TQ","336TQ","337TQ","338TQ","339TQ","233JQ","334JQ","335JQ","336JQ","337JQ","338JQ","339JQ","33TJQ",
      "2334K","2335K","3345K","2336K","3346K","3356K","2337K","3347K","3357K","3367K","2338K","3348K","3358K","3368K","3378K","2339K","3349K","3359K","3369K","3379K",
      "3389K","233TK","334TK","335TK","336TK","337TK","338TK","339TK","233JK","334JK","335JK","336JK","337JK","338JK","339JK","33TJK","233QK","334QK","335QK","336QK",
      "337QK","338QK","339QK","33TQK","33JQK","A2334","A2335","A3345","A2336","A3346","A3356","A2337","A3347","A3357","A3367","A2338","A3348","A3358","A3368","A3378",
      "A2339","A3349","A3359","A3369","A3379","A3389","A233T","A334T","A335T","A336T","A337T","A338T","A339T","A233J","A334J","A335J","A336J","A337J","A338J","A339J",
      "A33TJ","A233Q","A334Q","A335Q","A336Q","A337Q","A338Q","A339Q","A33TQ","A33JQ","A233K","A334K","A335K","A336K","A337K","A338K","A339K","A33TK","A33JK","A33QK",
      "23445","23446","24456","34456","23447","24457","34457","24467","34467","44567","23448","24458","34458","24468","34468","44568","24478","34478","44578","44678",
      "23449","24459","34459","24469","34469","44569","24479","34479","44579","44679","24489","34489","44589","44689","44789","2344T","2445T","3445T","2446T","3446T",
      "4456T","2447T","3447T","4457T","4467T","2448T","3448T","4458T","4468T","4478T","2449T","3449T","4459T","4469T","4479T","4489T","2344J","2445J","3445J","2446J",
      "3446J","4456J","2447J","3447J","4457J","4467J","2448J","3448J","4458J","4468J","4478J","2449J","3449J","4459J","4469J","4479J","4489J","244TJ","344TJ","445TJ",
      "446TJ","447TJ","448TJ","449TJ","2344Q","2445Q","3445Q","2446Q","3446Q","4456Q","2447Q","3447Q","4457Q","4467Q","2448Q","3448Q","4458Q","4468Q","4478Q","2449Q",
      "3449Q","4459Q","4469Q","4479Q","4489Q","244TQ","344TQ","445TQ","446TQ","447TQ","448TQ","449TQ","244JQ","344JQ","445JQ","446JQ","447JQ","448JQ","449JQ","44TJQ",
      "2344K","2445K","3445K","2446K","3446K","4456K","2447K","3447K","4457K","4467K","2448K","3448K","4458K","4468K","4478K","2449K","3449K","4459K","4469K","4479K",
      "4489K","244TK","344TK","445TK","446TK","447TK","448TK","449TK","244JK","344JK","445JK","446JK","447JK","448JK","449JK","44TJK","244QK","344QK","445QK","446QK",
      "447QK","448QK","449QK","44TQK","44JQK","A2344","A2445","A3445","A2446","A3446","A4456","A2447","A3447","A4457","A4467","A2448","A3448","A4458","A4468","A4478",
      "A2449","A3449","A4459","A4469","A4479","A4489","A244T","A344T","A445T","A446T","A447T","A448T","A449T","A244J","A344J","A445J","A446J","A447J","A448J","A449J",
      "A44TJ","A244Q","A344Q","A445Q","A446Q","A447Q","A448Q","A449Q","A44TQ","A44JQ","A244K","A344K","A445K","A446K","A447K","A448K","A449K","A44TK","A44JK","A44QK",
      "23455","23556","24556","34556","23557","24557","34557","25567","35567","45567","23558","24558","34558","25568","35568","45568","25578","35578","45578","55678",
      "23559","24559","34559","25569","35569","45569","25579","35579","45579","55679","25589","35589","45589","55689","55789","2355T","2455T","3455T","2556T","3556T",
      "4556T","2557T","3557T","4557T","5567T","2558T","3558T","4558T","5568T","5578T","2559T","3559T","4559T","5569T","5579T","5589T","2355J","2455J","3455J","2556J",
      "3556J","4556J","2557J","3557J","4557J","5567J","2558J","3558J","4558J","5568J","5578J","2559J","3559J","4559J","5569J","5579J","5589J","255TJ","355TJ","455TJ",
      "556TJ","557TJ","558TJ","559TJ","2355Q","2455Q","3455Q","2556Q","3556Q","4556Q","2557Q","3557Q","4557Q","5567Q","2558Q","3558Q","4558Q","5568Q","5578Q","2559Q",
      "3559Q","4559Q","5569Q","5579Q","5589Q","255TQ","355TQ","455TQ","556TQ","557TQ","558TQ","559TQ","255JQ","355JQ","455JQ","556JQ","557JQ","558JQ","559JQ","55TJQ",
      "2355K","2455K","3455K","2556K","3556K","4556K","2557K","3557K","4557K","5567K","2558K","3558K","4558K","5568K","5578K","2559K","3559K","4559K","5569K","5579K",
      "5589K","255TK","355TK","455TK","556TK","557TK","558TK","559TK","255JK","355JK","455JK","556JK","557JK","558JK","559JK","55TJK","255QK","355QK","455QK","556QK",
      "557QK","558QK","559QK","55TQK","55JQK","A2355","A2455","A3455","A2556","A3556","A4556","A2557","A3557","A4557","A5567","A2558","A3558","A4558","A5568","A5578",
      "A2559","A3559","A4559","A5569","A5579","A5589","A255T","A355T","A455T","A556T","A557T","A558T","A559T","A255J","A355J","A455J","A556J","A557J","A558J","A559J",
      "A55TJ","A255Q","A355Q","A455Q","A556Q","A557Q","A558Q","A559Q","A55TQ","A55JQ","A255K","A355K","A455K","A556K","A557K","A558K","A559K","A55TK","A55JK","A55QK",
      "23466","23566","24566","34566","23667","24667","25667","34667","35667","45667","23668","24668","25668","34668","35668","45668","26678","36678","46678","56678",
      "23669","24669","25669","34669","35669","45669","26679","36679","46679","56679","26689","36689","46689","56689","66789","2366T","2466T","2566T","3466T","3566T",
      "4566T","2667T","3667T","4667T","5667T","2668T","3668T","4668T","5668T","6678T","2669T","3669T","4669T","5669T","6679T","6689T","2366J","2466J","2566J","3466J",
      "3566J","4566J","2667J","3667J","4667J","5667J","2668J","3668J","4668J","5668J","6678J","2669J","3669J","4669J","5669J","6679J","6689J","266TJ","366TJ","466TJ",
      "566TJ","667TJ","668TJ","669TJ","2366Q","2466Q","2566Q","3466Q","3566Q","4566Q","2667Q","3667Q","4667Q","5667Q","2668Q","3668Q","4668Q","5668Q","6678Q","2669Q",
      "3669Q","4669Q","5669Q","6679Q","6689Q","266TQ","366TQ","466TQ","566TQ","667TQ","668TQ","669TQ","266JQ","366JQ","466JQ","566JQ","667JQ","668JQ","669JQ","66TJQ",
      "2366K","2466K","2566K","3466K","3566K","4566K","2667K","3667K","4667K","5667K","2668K","3668K","4668K","5668K","6678K","2669K","3669K","4669K","5669K","6679K",
      "6689K","266TK","366TK","466TK","566TK","667TK","668TK","669TK","266JK","366JK","466JK","566JK","667JK","668JK","669JK","66TJK","266QK","366QK","466QK","566QK",
      "667QK","668QK","669QK","66TQK","66JQK","A2366","A2466","A2566","A3466","A3566","A4566","A2667","A3667","A4667","A5667","A2668","A3668","A4668","A5668","A6678",
      "A2669","A3669","A4669","A5669","A6679","A6689","A266T","A366T","A466T","A566T","A667T","A668T","A669T","A266J","A366J","A466J","A566J","A667J","A668J","A669J",
      "A66TJ","A266Q","A366Q","A466Q","A566Q","A667Q","A668Q","A669Q","A66TQ","A66JQ","A266K","A366K","A466K","A566K","A667K","A668K","A669K","A66TK","A66JK","A66QK",
      "23477","23577","24577","34577","23677","24677","25677","34677","35677","45677","23778","24778","25778","26778","34778","35778","36778","45778","46778","56778",
      "23779","24779","25779","26779","34779","35779","36779","45779","46779","56779","27789","37789","47789","57789","67789","2377T","2477T","2577T","2677T","3477T",
      "3577T","3677T","4577T","4677T","5677T","2778T","3778T","4778T","5778T","6778T","2779T","3779T","4779T","5779T","6779T","7789T","2377J","2477J","2577J","2677J",
      "3477J","3577J","3677J","4577J","4677J","5677J","2778J","3778J","4778J","5778J","6778J","2779J","3779J","4779J","5779J","6779J","7789J","277TJ","377TJ","477TJ",
      "577TJ","677TJ","778TJ","779TJ","2377Q","2477Q","2577Q","2677Q","3477Q","3577Q","3677Q","4577Q","4677Q","5677Q","2778Q","3778Q","4778Q","5778Q","6778Q","2779Q",
      "3779Q","4779Q","5779Q","6779Q","7789Q","277TQ","377TQ","477TQ","577TQ","677TQ","778TQ","779TQ","277JQ","377JQ","477JQ","577JQ","677JQ","778JQ","779JQ","77TJQ",
      "2377K","2477K","2577K","2677K","3477K","3577K","3677K","4577K","4677K","5677K","2778K","3778K","4778K","5778K","6778K","2779K","3779K","4779K","5779K","6779K",
      "7789K","277TK","377TK","477TK","577TK","677TK","778TK","779TK","277JK","377JK","477JK","577JK","677JK","778JK","779JK","77TJK","277QK","377QK","477QK","577QK",
      "677QK","778QK","779QK","77TQK","77JQK","A2377","A2477","A2577","A2677","A3477","A3577","A3677","A4577","A4677","A5677","A2778","A3778","A4778","A5778","A6778",
      "A2779","A3779","A4779","A5779","A6779","A7789","A277T","A377T","A477T","A577T","A677T","A778T","A779T","A277J","A377J","A477J","A577J","A677J","A778J","A779J",
      "A77TJ","A277Q","A377Q","A477Q","A577Q","A677Q","A778Q","A779Q","A77TQ","A77JQ","A277K","A377K","A477K","A577K","A677K","A778K","A779K","A77TK","A77JK","A77QK",
      "23488","23588","24588","34588","23688","24688","25688","34688","35688","45688","23788","24788","25788","26788","34788","35788","36788","45788","46788","56788",
      "23889","24889","25889","26889","27889","34889","35889","36889","37889","45889","46889","47889","56889","57889","67889","2388T","2488T","2588T","2688T","2788T",
      "3488T","3588T","3688T","3788T","4588T","4688T","4788T","5688T","5788T","6788T","2889T","3889T","4889T","5889T","6889T","7889T","2388J","2488J","2588J","2688J",
      "2788J","3488J","3588J","3688J","3788J","4588J","4688J","4788J","5688J","5788J","6788J","2889J","3889J","4889J","5889J","6889J","7889J","288TJ","388TJ","488TJ",
      "588TJ","688TJ","788TJ","889TJ","2388Q","2488Q","2588Q","2688Q","2788Q","3488Q","3588Q","3688Q","3788Q","4588Q","4688Q","4788Q","5688Q","5788Q","6788Q","2889Q",
      "3889Q","4889Q","5889Q","6889Q","7889Q","288TQ","388TQ","488TQ","588TQ","688TQ","788TQ","889TQ","288JQ","388JQ","488JQ","588JQ","688JQ","788JQ","889JQ","88TJQ",
      "2388K","2488K","2588K","2688K","2788K","3488K","3588K","3688K","3788K","4588K","4688K","4788K","5688K","5788K","6788K","2889K","3889K","4889K","5889K","6889K",
      "7889K","288TK","388TK","488TK","588TK","688TK","788TK","889TK","288JK","388JK","488JK","588JK","688JK","788JK","889JK","88TJK","288QK","388QK","488QK","588QK",
      "688QK","788QK","889QK","88TQK","88JQK","A2388","A2488","A2588","A2688","A2788","A3488","A3588","A3688","A3788","A4588","A4688","A4788","A5688","A5788","A6788",
      "A2889","A3889","A4889","A5889","A6889","A7889","A288T","A388T","A488T","A588T","A688T","A788T","A889T","A288J","A388J","A488J","A588J","A688J","A788J","A889J",
      "A88TJ","A288Q","A388Q","A488Q","A588Q","A688Q","A788Q","A889Q","A88TQ","A88JQ","A288K","A388K","A488K","A588K","A688K","A788K","A889K","A88TK","A88JK","A88QK",
      "23499","23599","24599","34599","23699","24699","25699","34699","35699","45699","23799","24799","25799","26799","34799","35799","36799","45799","46799","56799",
      "23899","24899","25899","26899","27899","34899","35899","36899","37899","45899","46899","47899","56899","57899","67899","2399T","2499T","2599T","2699T","2799T",
      "2899T","3499T","3599T","3699T","3799T","3899T","4599T","4699T","4799T","4899T","5699T","5799T","5899T","6799T","6899T","7899T","2399J","2499J","2599J","2699J",
      "2799J","2899J","3499J","3599J","3699J","3799J","3899J","4599J","4699J","4799J","4899J","5699J","5799J","5899J","6799J","6899J","7899J","299TJ","399TJ","499TJ",
      "599TJ","699TJ","799TJ","899TJ","2399Q","2499Q","2599Q","2699Q","2799Q","2899Q","3499Q","3599Q","3699Q","3799Q","3899Q","4599Q","4699Q","4799Q","4899Q","5699Q",
      "5799Q","5899Q","6799Q","6899Q","7899Q","299TQ","399TQ","499TQ","599TQ","699TQ","799TQ","899TQ","299JQ","399JQ","499JQ","599JQ","699JQ","799JQ","899JQ","99TJQ",
      "2399K","2499K","2599K","2699K","2799K","2899K","3499K","3599K","3699K","3799K","3899K","4599K","4699K","4799K","4899K","5699K","5799K","5899K","6799K","6899K",
      "7899K","299TK","399TK","499TK","599TK","699TK","799TK","899TK","299JK","399JK","499JK","599JK","699JK","799JK","899JK","99TJK","299QK","399QK","499QK","599QK",
      "699QK","799QK","899QK","99TQK","99JQK","A2399","A2499","A2599","A2699","A2799","A2899","A3499","A3599","A3699","A3799","A3899","A4599","A4699","A4799","A4899",
      "A5699","A5799","A5899","A6799","A6899","A7899","A299T","A399T","A499T","A599T","A699T","A799T","A899T","A299J","A399J","A499J","A599J","A699J","A799J","A899J",
      "A99TJ","A299Q","A399Q","A499Q","A599Q","A699Q","A799Q","A899Q","A99TQ","A99JQ","A299K","A399K","A499K","A599K","A699K","A799K","A899K","A99TK","A99JK","A99QK",
      "234TT","235TT","245TT","345TT","236TT","246TT","256TT","346TT","356TT","456TT","237TT","247TT","257TT","267TT","347TT","357TT","367TT","457TT","467TT","567TT",
      "238TT","248TT","258TT","268TT","278TT","348TT","358TT","368TT","378TT","458TT","468TT","478TT","568TT","578TT","678TT","239TT","249TT","259TT","269TT","279TT",
      "289TT","349TT","359TT","369TT","379TT","389TT","459TT","469TT","479TT","489TT","569TT","579TT","589TT","679TT","689TT","789TT","23TTJ","24TTJ","25TTJ","26TTJ",
      "27TTJ","28TTJ","29TTJ","34TTJ","35TTJ","36TTJ","37TTJ","38TTJ","39TTJ","45TTJ","46TTJ","47TTJ","48TTJ","49TTJ","56TTJ","57TTJ","58TTJ","59TTJ","67TTJ","68TTJ",
      "69TTJ","78TTJ","79TTJ","89TTJ","23TTQ","24TTQ","25TTQ","26TTQ","27TTQ","28TTQ","29TTQ","34TTQ","35TTQ","36TTQ","37TTQ","38TTQ","39TTQ","45TTQ","46TTQ","47TTQ",
      "48TTQ","49TTQ","56TTQ","57TTQ","58TTQ","59TTQ","67TTQ","68TTQ","69TTQ","78TTQ","79TTQ","89TTQ","2TTJQ","3TTJQ","4TTJQ","5TTJQ","6TTJQ","7TTJQ","8TTJQ","9TTJQ",
      "23TTK","24TTK","25TTK","26TTK","27TTK","28TTK","29TTK","34TTK","35TTK","36TTK","37TTK","38TTK","39TTK","45TTK","46TTK","47TTK","48TTK","49TTK","56TTK","57TTK",
      "58TTK","59TTK","67TTK","68TTK","69TTK","78TTK","79TTK","89TTK","2TTJK","3TTJK","4TTJK","5TTJK","6TTJK","7TTJK","8TTJK","9TTJK","2TTQK","3TTQK","4TTQK","5TTQK",
      "6TTQK","7TTQK","8TTQK","9TTQK","TTJQK","A23TT","A24TT","A25TT","A26TT","A27TT","A28TT","A29TT","A34TT","A35TT","A36TT","A37TT","A38TT","A39TT","A45TT","A46TT",
      "A47TT","A48TT","A49TT","A56TT","A57TT","A58TT","A59TT","A67TT","A68TT","A69TT","A78TT","A79TT","A89TT","A2TTJ","A3TTJ","A4TTJ","A5TTJ","A6TTJ","A7TTJ","A8TTJ",
      "A9TTJ","A2TTQ","A3TTQ","A4TTQ","A5TTQ","A6TTQ","A7TTQ","A8TTQ","A9TTQ","ATTJQ","A2TTK","A3TTK","A4TTK","A5TTK","A6TTK","A7TTK","A8TTK","A9TTK","ATTJK","ATTQK",
      "234JJ","235JJ","245JJ","345JJ","236JJ","246JJ","256JJ","346JJ","356JJ","456JJ","237JJ","247JJ","257JJ","267JJ","347JJ","357JJ","367JJ","457JJ","467JJ","567JJ",
      "238JJ","248JJ","258JJ","268JJ","278JJ","348JJ","358JJ","368JJ","378JJ","458JJ","468JJ","478JJ","568JJ","578JJ","678JJ","239JJ","249JJ","259JJ","269JJ","279JJ",
      "289JJ","349JJ","359JJ","369JJ","379JJ","389JJ","459JJ","469JJ","479JJ","489JJ","569JJ","579JJ","589JJ","679JJ","689JJ","789JJ","23TJJ","24TJJ","25TJJ","26TJJ",
      "27TJJ","28TJJ","29TJJ","34TJJ","35TJJ","36TJJ","37TJJ","38TJJ","39TJJ","45TJJ","46TJJ","47TJJ","48TJJ","49TJJ","56TJJ","57TJJ","58TJJ","59TJJ","67TJJ","68TJJ",
      "69TJJ","78TJJ","79TJJ","89TJJ","23JJQ","24JJQ","25JJQ","26JJQ","27JJQ","28JJQ","29JJQ","2TJJQ","34JJQ","35JJQ","36JJQ","37JJQ","38JJQ","39JJQ","3TJJQ","45JJQ",
      "46JJQ","47JJQ","48JJQ","49JJQ","4TJJQ","56JJQ","57JJQ","58JJQ","59JJQ","5TJJQ","67JJQ","68JJQ","69JJQ","6TJJQ","78JJQ","79JJQ","7TJJQ","89JJQ","8TJJQ","9TJJQ",
      "23JJK","24JJK","25JJK","26JJK","27JJK","28JJK","29JJK","2TJJK","34JJK","35JJK","36JJK","37JJK","38JJK","39JJK","3TJJK","45JJK","46JJK","47JJK","48JJK","49JJK",
      "4TJJK","56JJK","57JJK","58JJK","59JJK","5TJJK","67JJK","68JJK","69JJK","6TJJK","78JJK","79JJK","7TJJK","89JJK","8TJJK","9TJJK","2JJQK","3JJQK","4JJQK","5JJQK",
      "6JJQK","7JJQK","8JJQK","9JJQK","TJJQK","A23JJ","A24JJ","A25JJ","A26JJ","A27JJ","A28JJ","A29JJ","A2TJJ","A34JJ","A35JJ","A36JJ","A37JJ","A38JJ","A39JJ","A3TJJ",
      "A45JJ","A46JJ","A47JJ","A48JJ","A49JJ","A4TJJ","A56JJ","A57JJ","A58JJ","A59JJ","A5TJJ","A67JJ","A68JJ","A69JJ","A6TJJ","A78JJ","A79JJ","A7TJJ","A89JJ","A8TJJ",
      "A9TJJ","A2JJQ","A3JJQ","A4JJQ","A5JJQ","A6JJQ","A7JJQ","A8JJQ","A9JJQ","ATJJQ","A2JJK","A3JJK","A4JJK","A5JJK","A6JJK","A7JJK","A8JJK","A9JJK","ATJJK","AJJQK",
      "234QQ","235QQ","245QQ","345QQ","236QQ","246QQ","256QQ","346QQ","356QQ","456QQ","237QQ","247QQ","257QQ","267QQ","347QQ","357QQ","367QQ","457QQ","467QQ","567QQ",
      "238QQ","248QQ","258QQ","268QQ","278QQ","348QQ","358QQ","368QQ","378QQ","458QQ","468QQ","478QQ","568QQ","578QQ","678QQ","239QQ","249QQ","259QQ","269QQ","279QQ",
      "289QQ","349QQ","359QQ","369QQ","379QQ","389QQ","459QQ","469QQ","479QQ","489QQ","569QQ","579QQ","589QQ","679QQ","689QQ","789QQ","23TQQ","24TQQ","25TQQ","26TQQ",
      "27TQQ","28TQQ","29TQQ","34TQQ","35TQQ","36TQQ","37TQQ","38TQQ","39TQQ","45TQQ","46TQQ","47TQQ","48TQQ","49TQQ","56TQQ","57TQQ","58TQQ","59TQQ","67TQQ","68TQQ",
      "69TQQ","78TQQ","79TQQ","89TQQ","23JQQ","24JQQ","25JQQ","26JQQ","27JQQ","28JQQ","29JQQ","2TJQQ","34JQQ","35JQQ","36JQQ","37JQQ","38JQQ","39JQQ","3TJQQ","45JQQ",
      "46JQQ","47JQQ","48JQQ","49JQQ","4TJQQ","56JQQ","57JQQ","58JQQ","59JQQ","5TJQQ","67JQQ","68JQQ","69JQQ","6TJQQ","78JQQ","79JQQ","7TJQQ","89JQQ","8TJQQ","9TJQQ",
      "23QQK","24QQK","25QQK","26QQK","27QQK","28QQK","29QQK","2TQQK","2JQQK","34QQK","35QQK","36QQK","37QQK","38QQK","39QQK","3TQQK","3JQQK","45QQK","46QQK","47QQK",
      "48QQK","49QQK","4TQQK","4JQQK","56QQK","57QQK","58QQK","59QQK","5TQQK","5JQQK","67QQK","68QQK","69QQK","6TQQK","6JQQK","78QQK","79QQK","7TQQK","7JQQK","89QQK",
      "8TQQK","8JQQK","9TQQK","9JQQK","TJQQK","A23QQ","A24QQ","A25QQ","A26QQ","A27QQ","A28QQ","A29QQ","A2TQQ","A2JQQ","A34QQ","A35QQ","A36QQ","A37QQ","A38QQ","A39QQ",
      "A3TQQ","A3JQQ","A45QQ","A46QQ","A47QQ","A48QQ","A49QQ","A4TQQ","A4JQQ","A56QQ","A57QQ","A58QQ","A59QQ","A5TQQ","A5JQQ","A67QQ","A68QQ","A69QQ","A6TQQ","A6JQQ",
      "A78QQ","A79QQ","A7TQQ","A7JQQ","A89QQ","A8TQQ","A8JQQ","A9TQQ","A9JQQ","ATJQQ","A2QQK","A3QQK","A4QQK","A5QQK","A6QQK","A7QQK","A8QQK","A9QQK","ATQQK","AJQQK",
      "234KK","235KK","245KK","345KK","236KK","246KK","256KK","346KK","356KK","456KK","237KK","247KK","257KK","267KK","347KK","357KK","367KK","457KK","467KK","567KK",
      "238KK","248KK","258KK","268KK","278KK","348KK","358KK","368KK","378KK","458KK","468KK","478KK","568KK","578KK","678KK","239KK","249KK","259KK","269KK","279KK",
      "289KK","349KK","359KK","369KK","379KK","389KK","459KK","469KK","479KK","489KK","569KK","579KK","589KK","679KK","689KK","789KK","23TKK","24TKK","25TKK","26TKK",
      "27TKK","28TKK","29TKK","34TKK","35TKK","36TKK","37TKK","38TKK","39TKK","45TKK","46TKK","47TKK","48TKK","49TKK","56TKK","57TKK","58TKK","59TKK","67TKK","68TKK",
      "69TKK","78TKK","79TKK","89TKK","23JKK","24JKK","25JKK","26JKK","27JKK","28JKK","29JKK","2TJKK","34JKK","35JKK","36JKK","37JKK","38JKK","39JKK","3TJKK","45JKK",
      "46JKK","47JKK","48JKK","49JKK","4TJKK","56JKK","57JKK","58JKK","59JKK","5TJKK","67JKK","68JKK","69JKK","6TJKK","78JKK","79JKK","7TJKK","89JKK","8TJKK","9TJKK",
      "23QKK","24QKK","25QKK","26QKK","27QKK","28QKK","29QKK","2TQKK","2JQKK","34QKK","35QKK","36QKK","37QKK","38QKK","39QKK","3TQKK","3JQKK","45QKK","46QKK","47QKK",
      "48QKK","49QKK","4TQKK","4JQKK","56QKK","57QKK","58QKK","59QKK","5TQKK","5JQKK","67QKK","68QKK","69QKK","6TQKK","6JQKK","78QKK","79QKK","7TQKK","7JQKK","89QKK",
      "8TQKK","8JQKK","9TQKK","9JQKK","TJQKK","A23KK","A24KK","A25KK","A26KK","A27KK","A28KK","A29KK","A2TKK","A2JKK","A2QKK","A34KK","A35KK","A36KK","A37KK","A38KK",
      "A39KK","A3TKK","A3JKK","A3QKK","A45KK","A46KK","A47KK","A48KK","A49KK","A4TKK","A4JKK","A4QKK","A56KK","A57KK","A58KK","A59KK","A5TKK","A5JKK","A5QKK","A67KK",
      "A68KK","A69KK","A6TKK","A6JKK","A6QKK","A78KK","A79KK","A7TKK","A7JKK","A7QKK","A89KK","A8TKK","A8JKK","A8QKK","A9TKK","A9JKK","A9QKK","ATJKK","ATQKK","AJQKK",
      "AA234","AA235","AA245","AA345","AA236","AA246","AA256","AA346","AA356","AA456","AA237","AA247","AA257","AA267","AA347","AA357","AA367","AA457","AA467","AA567",
      "AA238","AA248","AA258","AA268","AA278","AA348","AA358","AA368","AA378","AA458","AA468","AA478","AA568","AA578","AA678","AA239","AA249","AA259","AA269","AA279",
      "AA289","AA349","AA359","AA369","AA379","AA389","AA459","AA469","AA479","AA489","AA569","AA579","AA589","AA679","AA689","AA789","AA23T","AA24T","AA25T","AA26T",
      "AA27T","AA28T","AA29T","AA34T","AA35T","AA36T","AA37T","AA38T","AA39T","AA45T","AA46T","AA47T","AA48T","AA49T","AA56T","AA57T","AA58T","AA59T","AA67T","AA68T",
      "AA69T","AA78T","AA79T","AA89T","AA23J","AA24J","AA25J","AA26J","AA27J","AA28J","AA29J","AA2TJ","AA34J","AA35J","AA36J","AA37J","AA38J","AA39J","AA3TJ","AA45J",
      "AA46J","AA47J","AA48J","AA49J","AA4TJ","AA56J","AA57J","AA58J","AA59J","AA5TJ","AA67J","AA68J","AA69J","AA6TJ","AA78J","AA79J","AA7TJ","AA89J","AA8TJ","AA9TJ",
      "AA23Q","AA24Q","AA25Q","AA26Q","AA27Q","AA28Q","AA29Q","AA2TQ","AA2JQ","AA34Q","AA35Q","AA36Q","AA37Q","AA38Q","AA39Q","AA3TQ","AA3JQ","AA45Q","AA46Q","AA47Q",
      "AA48Q","AA49Q","AA4TQ","AA4JQ","AA56Q","AA57Q","AA58Q","AA59Q","AA5TQ","AA5JQ","AA67Q","AA68Q","AA69Q","AA6TQ","AA6JQ","AA78Q","AA79Q","AA7TQ","AA7JQ","AA89Q",
      "AA8TQ","AA8JQ","AA9TQ","AA9JQ","AATJQ","AA23K","AA24K","AA25K","AA26K","AA27K","AA28K","AA29K","AA2TK","AA2JK","AA2QK","AA34K","AA35K","AA36K","AA37K","AA38K",
      "AA39K","AA3TK","AA3JK","AA3QK","AA45K","AA46K","AA47K","AA48K","AA49K","AA4TK","AA4JK","AA4QK","AA56K","AA57K","AA58K","AA59K","AA5TK","AA5JK","AA5QK","AA67K",
      "AA68K","AA69K","AA6TK","AA6JK","AA6QK","AA78K","AA79K","AA7TK","AA7JK","AA7QK","AA89K","AA8TK","AA8JK","AA8QK","AA9TK","AA9JK","AA9QK","AATJK","AATQK","AAJQK",
    };

    public static List<string> AllOnePair3Sorted { get; } = new List<string>
    {
      "223","224","225","226","227","228","229","22T","22J","22Q","22K","A22","233","334","335","336","337","338","339","33T",
      "33J","33Q","33K","A33","244","344","445","446","447","448","449","44T","44J","44Q","44K","A44","255","355","455","556",
      "557","558","559","55T","55J","55Q","55K","A55","266","366","466","566","667","668","669","66T","66J","66Q","66K","A66",
      "277","377","477","577","677","778","779","77T","77J","77Q","77K","A77","288","388","488","588","688","788","889","88T",
      "88J","88Q","88K","A88","299","399","499","599","699","799","899","99T","99J","99Q","99K","A99","2TT","3TT","4TT","5TT",
      "6TT","7TT","8TT","9TT","TTJ","TTQ","TTK","ATT","2JJ","3JJ","4JJ","5JJ","6JJ","7JJ","8JJ","9JJ","TJJ","JJQ","JJK","AJJ",
      "2QQ","3QQ","4QQ","5QQ","6QQ","7QQ","8QQ","9QQ","TQQ","JQQ","QQK","AQQ","2KK","3KK","4KK","5KK","6KK","7KK","8KK","9KK",
      "TKK","JKK","QKK","AKK","AA2","AA3","AA4","AA5","AA6","AA7","AA8","AA9","AAT","AAJ","AAQ","AAK",
    };

    public static List<string> AllTwoPairsSorted { get; } = new List<string>
    {
      "22334","22335","22336","22337","22338","22339","2233T","2233J","2233Q","2233K","A2233","22344","22445","22446","22447","22448","22449","2244T","2244J","2244Q",
      "2244K","A2244","22355","22455","22556","22557","22558","22559","2255T","2255J","2255Q","2255K","A2255","22366","22466","22566","22667","22668","22669","2266T",
      "2266J","2266Q","2266K","A2266","22377","22477","22577","22677","22778","22779","2277T","2277J","2277Q","2277K","A2277","22388","22488","22588","22688","22788",
      "22889","2288T","2288J","2288Q","2288K","A2288","22399","22499","22599","22699","22799","22899","2299T","2299J","2299Q","2299K","A2299","223TT","224TT","225TT",
      "226TT","227TT","228TT","229TT","22TTJ","22TTQ","22TTK","A22TT","223JJ","224JJ","225JJ","226JJ","227JJ","228JJ","229JJ","22TJJ","22JJQ","22JJK","A22JJ","223QQ",
      "224QQ","225QQ","226QQ","227QQ","228QQ","229QQ","22TQQ","22JQQ","22QQK","A22QQ","223KK","224KK","225KK","226KK","227KK","228KK","229KK","22TKK","22JKK","22QKK",
      "A22KK","23344","33445","33446","33447","33448","33449","3344T","3344J","3344Q","3344K","A3344","23355","33455","33556","33557","33558","33559","3355T","3355J",
      "3355Q","3355K","A3355","23366","33466","33566","33667","33668","33669","3366T","3366J","3366Q","3366K","A3366","23377","33477","33577","33677","33778","33779",
      "3377T","3377J","3377Q","3377K","A3377","23388","33488","33588","33688","33788","33889","3388T","3388J","3388Q","3388K","A3388","23399","33499","33599","33699",
      "33799","33899","3399T","3399J","3399Q","3399K","A3399","233TT","334TT","335TT","336TT","337TT","338TT","339TT","33TTJ","33TTQ","33TTK","A33TT","233JJ","334JJ",
      "335JJ","336JJ","337JJ","338JJ","339JJ","33TJJ","33JJQ","33JJK","A33JJ","233QQ","334QQ","335QQ","336QQ","337QQ","338QQ","339QQ","33TQQ","33JQQ","33QQK","A33QQ",
      "233KK","334KK","335KK","336KK","337KK","338KK","339KK","33TKK","33JKK","33QKK","A33KK","24455","34455","44556","44557","44558","44559","4455T","4455J","4455Q",
      "4455K","A4455","24466","34466","44566","44667","44668","44669","4466T","4466J","4466Q","4466K","A4466","24477","34477","44577","44677","44778","44779","4477T",
      "4477J","4477Q","4477K","A4477","24488","34488","44588","44688","44788","44889","4488T","4488J","4488Q","4488K","A4488","24499","34499","44599","44699","44799",
      "44899","4499T","4499J","4499Q","4499K","A4499","244TT","344TT","445TT","446TT","447TT","448TT","449TT","44TTJ","44TTQ","44TTK","A44TT","244JJ","344JJ","445JJ",
      "446JJ","447JJ","448JJ","449JJ","44TJJ","44JJQ","44JJK","A44JJ","244QQ","344QQ","445QQ","446QQ","447QQ","448QQ","449QQ","44TQQ","44JQQ","44QQK","A44QQ","244KK",
      "344KK","445KK","446KK","447KK","448KK","449KK","44TKK","44JKK","44QKK","A44KK","25566","35566","45566","55667","55668","55669","5566T","5566J","5566Q","5566K",
      "A5566","25577","35577","45577","55677","55778","55779","5577T","5577J","5577Q","5577K","A5577","25588","35588","45588","55688","55788","55889","5588T","5588J",
      "5588Q","5588K","A5588","25599","35599","45599","55699","55799","55899","5599T","5599J","5599Q","5599K","A5599","255TT","355TT","455TT","556TT","557TT","558TT",
      "559TT","55TTJ","55TTQ","55TTK","A55TT","255JJ","355JJ","455JJ","556JJ","557JJ","558JJ","559JJ","55TJJ","55JJQ","55JJK","A55JJ","255QQ","355QQ","455QQ","556QQ",
      "557QQ","558QQ","559QQ","55TQQ","55JQQ","55QQK","A55QQ","255KK","355KK","455KK","556KK","557KK","558KK","559KK","55TKK","55JKK","55QKK","A55KK","26677","36677",
      "46677","56677","66778","66779","6677T","6677J","6677Q","6677K","A6677","26688","36688","46688","56688","66788","66889","6688T","6688J","6688Q","6688K","A6688",
      "26699","36699","46699","56699","66799","66899","6699T","6699J","6699Q","6699K","A6699","266TT","366TT","466TT","566TT","667TT","668TT","669TT","66TTJ","66TTQ",
      "66TTK","A66TT","266JJ","366JJ","466JJ","566JJ","667JJ","668JJ","669JJ","66TJJ","66JJQ","66JJK","A66JJ","266QQ","366QQ","466QQ","566QQ","667QQ","668QQ","669QQ",
      "66TQQ","66JQQ","66QQK","A66QQ","266KK","366KK","466KK","566KK","667KK","668KK","669KK","66TKK","66JKK","66QKK","A66KK","27788","37788","47788","57788","67788",
      "77889","7788T","7788J","7788Q","7788K","A7788","27799","37799","47799","57799","67799","77899","7799T","7799J","7799Q","7799K","A7799","277TT","377TT","477TT",
      "577TT","677TT","778TT","779TT","77TTJ","77TTQ","77TTK","A77TT","277JJ","377JJ","477JJ","577JJ","677JJ","778JJ","779JJ","77TJJ","77JJQ","77JJK","A77JJ","277QQ",
      "377QQ","477QQ","577QQ","677QQ","778QQ","779QQ","77TQQ","77JQQ","77QQK","A77QQ","277KK","377KK","477KK","577KK","677KK","778KK","779KK","77TKK","77JKK","77QKK",
      "A77KK","28899","38899","48899","58899","68899","78899","8899T","8899J","8899Q","8899K","A8899","288TT","388TT","488TT","588TT","688TT","788TT","889TT","88TTJ",
      "88TTQ","88TTK","A88TT","288JJ","388JJ","488JJ","588JJ","688JJ","788JJ","889JJ","88TJJ","88JJQ","88JJK","A88JJ","288QQ","388QQ","488QQ","588QQ","688QQ","788QQ",
      "889QQ","88TQQ","88JQQ","88QQK","A88QQ","288KK","388KK","488KK","588KK","688KK","788KK","889KK","88TKK","88JKK","88QKK","A88KK","299TT","399TT","499TT","599TT",
      "699TT","799TT","899TT","99TTJ","99TTQ","99TTK","A99TT","299JJ","399JJ","499JJ","599JJ","699JJ","799JJ","899JJ","99TJJ","99JJQ","99JJK","A99JJ","299QQ","399QQ",
      "499QQ","599QQ","699QQ","799QQ","899QQ","99TQQ","99JQQ","99QQK","A99QQ","299KK","399KK","499KK","599KK","699KK","799KK","899KK","99TKK","99JKK","99QKK","A99KK",
      "2TTJJ","3TTJJ","4TTJJ","5TTJJ","6TTJJ","7TTJJ","8TTJJ","9TTJJ","TTJJQ","TTJJK","ATTJJ","2TTQQ","3TTQQ","4TTQQ","5TTQQ","6TTQQ","7TTQQ","8TTQQ","9TTQQ","TTJQQ",
      "TTQQK","ATTQQ","2TTKK","3TTKK","4TTKK","5TTKK","6TTKK","7TTKK","8TTKK","9TTKK","TTJKK","TTQKK","ATTKK","2JJQQ","3JJQQ","4JJQQ","5JJQQ","6JJQQ","7JJQQ","8JJQQ",
      "9JJQQ","TJJQQ","JJQQK","AJJQQ","2JJKK","3JJKK","4JJKK","5JJKK","6JJKK","7JJKK","8JJKK","9JJKK","TJJKK","JJQKK","AJJKK","2QQKK","3QQKK","4QQKK","5QQKK","6QQKK",
      "7QQKK","8QQKK","9QQKK","TQQKK","JQQKK","AQQKK","AA223","AA224","AA225","AA226","AA227","AA228","AA229","AA22T","AA22J","AA22Q","AA22K","AA233","AA334","AA335",
      "AA336","AA337","AA338","AA339","AA33T","AA33J","AA33Q","AA33K","AA244","AA344","AA445","AA446","AA447","AA448","AA449","AA44T","AA44J","AA44Q","AA44K","AA255",
      "AA355","AA455","AA556","AA557","AA558","AA559","AA55T","AA55J","AA55Q","AA55K","AA266","AA366","AA466","AA566","AA667","AA668","AA669","AA66T","AA66J","AA66Q",
      "AA66K","AA277","AA377","AA477","AA577","AA677","AA778","AA779","AA77T","AA77J","AA77Q","AA77K","AA288","AA388","AA488","AA588","AA688","AA788","AA889","AA88T",
      "AA88J","AA88Q","AA88K","AA299","AA399","AA499","AA599","AA699","AA799","AA899","AA99T","AA99J","AA99Q","AA99K","AA2TT","AA3TT","AA4TT","AA5TT","AA6TT","AA7TT",
      "AA8TT","AA9TT","AATTJ","AATTQ","AATTK","AA2JJ","AA3JJ","AA4JJ","AA5JJ","AA6JJ","AA7JJ","AA8JJ","AA9JJ","AATJJ","AAJJQ","AAJJK","AA2QQ","AA3QQ","AA4QQ","AA5QQ",
      "AA6QQ","AA7QQ","AA8QQ","AA9QQ","AATQQ","AAJQQ","AAQQK","AA2KK","AA3KK","AA4KK","AA5KK","AA6KK","AA7KK","AA8KK","AA9KK","AATKK","AAJKK","AAQKK",
    };

    public static List<string> AllThreeOfAKind3Sorted { get; } = new List<string>
    {
      "222","333","444","555","666","777","888","999","TTT","JJJ","QQQ","KKK","AAA",
    };

    public static List<string> AllThreeOfAKind5Sorted { get; } = new List<string>
    {
      "22234","22235","22245","22236","22246","22256","22237","22247","22257","22267","22238","22248","22258","22268","22278","22239","22249","22259","22269","22279",
      "22289","2223T","2224T","2225T","2226T","2227T","2228T","2229T","2223J","2224J","2225J","2226J","2227J","2228J","2229J","222TJ","2223Q","2224Q","2225Q","2226Q",
      "2227Q","2228Q","2229Q","222TQ","222JQ","2223K","2224K","2225K","2226K","2227K","2228K","2229K","222TK","222JK","222QK","A2223","A2224","A2225","A2226","A2227",
      "A2228","A2229","A222T","A222J","A222Q","A222K","23334","23335","33345","23336","33346","33356","23337","33347","33357","33367","23338","33348","33358","33368",
      "33378","23339","33349","33359","33369","33379","33389","2333T","3334T","3335T","3336T","3337T","3338T","3339T","2333J","3334J","3335J","3336J","3337J","3338J",
      "3339J","333TJ","2333Q","3334Q","3335Q","3336Q","3337Q","3338Q","3339Q","333TQ","333JQ","2333K","3334K","3335K","3336K","3337K","3338K","3339K","333TK","333JK",
      "333QK","A2333","A3334","A3335","A3336","A3337","A3338","A3339","A333T","A333J","A333Q","A333K","23444","24445","34445","24446","34446","44456","24447","34447",
      "44457","44467","24448","34448","44458","44468","44478","24449","34449","44459","44469","44479","44489","2444T","3444T","4445T","4446T","4447T","4448T","4449T",
      "2444J","3444J","4445J","4446J","4447J","4448J","4449J","444TJ","2444Q","3444Q","4445Q","4446Q","4447Q","4448Q","4449Q","444TQ","444JQ","2444K","3444K","4445K",
      "4446K","4447K","4448K","4449K","444TK","444JK","444QK","A2444","A3444","A4445","A4446","A4447","A4448","A4449","A444T","A444J","A444Q","A444K","23555","24555",
      "34555","25556","35556","45556","25557","35557","45557","55567","25558","35558","45558","55568","55578","25559","35559","45559","55569","55579","55589","2555T",
      "3555T","4555T","5556T","5557T","5558T","5559T","2555J","3555J","4555J","5556J","5557J","5558J","5559J","555TJ","2555Q","3555Q","4555Q","5556Q","5557Q","5558Q",
      "5559Q","555TQ","555JQ","2555K","3555K","4555K","5556K","5557K","5558K","5559K","555TK","555JK","555QK","A2555","A3555","A4555","A5556","A5557","A5558","A5559",
      "A555T","A555J","A555Q","A555K","23666","24666","34666","25666","35666","45666","26667","36667","46667","56667","26668","36668","46668","56668","66678","26669",
      "36669","46669","56669","66679","66689","2666T","3666T","4666T","5666T","6667T","6668T","6669T","2666J","3666J","4666J","5666J","6667J","6668J","6669J","666TJ",
      "2666Q","3666Q","4666Q","5666Q","6667Q","6668Q","6669Q","666TQ","666JQ","2666K","3666K","4666K","5666K","6667K","6668K","6669K","666TK","666JK","666QK","A2666",
      "A3666","A4666","A5666","A6667","A6668","A6669","A666T","A666J","A666Q","A666K","23777","24777","34777","25777","35777","45777","26777","36777","46777","56777",
      "27778","37778","47778","57778","67778","27779","37779","47779","57779","67779","77789","2777T","3777T","4777T","5777T","6777T","7778T","7779T","2777J","3777J",
      "4777J","5777J","6777J","7778J","7779J","777TJ","2777Q","3777Q","4777Q","5777Q","6777Q","7778Q","7779Q","777TQ","777JQ","2777K","3777K","4777K","5777K","6777K",
      "7778K","7779K","777TK","777JK","777QK","A2777","A3777","A4777","A5777","A6777","A7778","A7779","A777T","A777J","A777Q","A777K","23888","24888","34888","25888",
      "35888","45888","26888","36888","46888","56888","27888","37888","47888","57888","67888","28889","38889","48889","58889","68889","78889","2888T","3888T","4888T",
      "5888T","6888T","7888T","8889T","2888J","3888J","4888J","5888J","6888J","7888J","8889J","888TJ","2888Q","3888Q","4888Q","5888Q","6888Q","7888Q","8889Q","888TQ",
      "888JQ","2888K","3888K","4888K","5888K","6888K","7888K","8889K","888TK","888JK","888QK","A2888","A3888","A4888","A5888","A6888","A7888","A8889","A888T","A888J",
      "A888Q","A888K","23999","24999","34999","25999","35999","45999","26999","36999","46999","56999","27999","37999","47999","57999","67999","28999","38999","48999",
      "58999","68999","78999","2999T","3999T","4999T","5999T","6999T","7999T","8999T","2999J","3999J","4999J","5999J","6999J","7999J","8999J","999TJ","2999Q","3999Q",
      "4999Q","5999Q","6999Q","7999Q","8999Q","999TQ","999JQ","2999K","3999K","4999K","5999K","6999K","7999K","8999K","999TK","999JK","999QK","A2999","A3999","A4999",
      "A5999","A6999","A7999","A8999","A999T","A999J","A999Q","A999K","23TTT","24TTT","34TTT","25TTT","35TTT","45TTT","26TTT","36TTT","46TTT","56TTT","27TTT","37TTT",
      "47TTT","57TTT","67TTT","28TTT","38TTT","48TTT","58TTT","68TTT","78TTT","29TTT","39TTT","49TTT","59TTT","69TTT","79TTT","89TTT","2TTTJ","3TTTJ","4TTTJ","5TTTJ",
      "6TTTJ","7TTTJ","8TTTJ","9TTTJ","2TTTQ","3TTTQ","4TTTQ","5TTTQ","6TTTQ","7TTTQ","8TTTQ","9TTTQ","TTTJQ","2TTTK","3TTTK","4TTTK","5TTTK","6TTTK","7TTTK","8TTTK",
      "9TTTK","TTTJK","TTTQK","A2TTT","A3TTT","A4TTT","A5TTT","A6TTT","A7TTT","A8TTT","A9TTT","ATTTJ","ATTTQ","ATTTK","23JJJ","24JJJ","34JJJ","25JJJ","35JJJ","45JJJ",
      "26JJJ","36JJJ","46JJJ","56JJJ","27JJJ","37JJJ","47JJJ","57JJJ","67JJJ","28JJJ","38JJJ","48JJJ","58JJJ","68JJJ","78JJJ","29JJJ","39JJJ","49JJJ","59JJJ","69JJJ",
      "79JJJ","89JJJ","2TJJJ","3TJJJ","4TJJJ","5TJJJ","6TJJJ","7TJJJ","8TJJJ","9TJJJ","2JJJQ","3JJJQ","4JJJQ","5JJJQ","6JJJQ","7JJJQ","8JJJQ","9JJJQ","TJJJQ","2JJJK",
      "3JJJK","4JJJK","5JJJK","6JJJK","7JJJK","8JJJK","9JJJK","TJJJK","JJJQK","A2JJJ","A3JJJ","A4JJJ","A5JJJ","A6JJJ","A7JJJ","A8JJJ","A9JJJ","ATJJJ","AJJJQ","AJJJK",
      "23QQQ","24QQQ","34QQQ","25QQQ","35QQQ","45QQQ","26QQQ","36QQQ","46QQQ","56QQQ","27QQQ","37QQQ","47QQQ","57QQQ","67QQQ","28QQQ","38QQQ","48QQQ","58QQQ","68QQQ",
      "78QQQ","29QQQ","39QQQ","49QQQ","59QQQ","69QQQ","79QQQ","89QQQ","2TQQQ","3TQQQ","4TQQQ","5TQQQ","6TQQQ","7TQQQ","8TQQQ","9TQQQ","2JQQQ","3JQQQ","4JQQQ","5JQQQ",
      "6JQQQ","7JQQQ","8JQQQ","9JQQQ","TJQQQ","2QQQK","3QQQK","4QQQK","5QQQK","6QQQK","7QQQK","8QQQK","9QQQK","TQQQK","JQQQK","A2QQQ","A3QQQ","A4QQQ","A5QQQ","A6QQQ",
      "A7QQQ","A8QQQ","A9QQQ","ATQQQ","AJQQQ","AQQQK","23KKK","24KKK","34KKK","25KKK","35KKK","45KKK","26KKK","36KKK","46KKK","56KKK","27KKK","37KKK","47KKK","57KKK",
      "67KKK","28KKK","38KKK","48KKK","58KKK","68KKK","78KKK","29KKK","39KKK","49KKK","59KKK","69KKK","79KKK","89KKK","2TKKK","3TKKK","4TKKK","5TKKK","6TKKK","7TKKK",
      "8TKKK","9TKKK","2JKKK","3JKKK","4JKKK","5JKKK","6JKKK","7JKKK","8JKKK","9JKKK","TJKKK","2QKKK","3QKKK","4QKKK","5QKKK","6QKKK","7QKKK","8QKKK","9QKKK","TQKKK",
      "JQKKK","A2KKK","A3KKK","A4KKK","A5KKK","A6KKK","A7KKK","A8KKK","A9KKK","ATKKK","AJKKK","AQKKK","AAA23","AAA24","AAA34","AAA25","AAA35","AAA45","AAA26","AAA36",
      "AAA46","AAA56","AAA27","AAA37","AAA47","AAA57","AAA67","AAA28","AAA38","AAA48","AAA58","AAA68","AAA78","AAA29","AAA39","AAA49","AAA59","AAA69","AAA79","AAA89",
      "AAA2T","AAA3T","AAA4T","AAA5T","AAA6T","AAA7T","AAA8T","AAA9T","AAA2J","AAA3J","AAA4J","AAA5J","AAA6J","AAA7J","AAA8J","AAA9J","AAATJ","AAA2Q","AAA3Q","AAA4Q",
      "AAA5Q","AAA6Q","AAA7Q","AAA8Q","AAA9Q","AAATQ","AAAJQ","AAA2K","AAA3K","AAA4K","AAA5K","AAA6K","AAA7K","AAA8K","AAA9K","AAATK","AAAJK","AAAQK",
    };

    public static List<string> AllStraightSorted { get; } = new List<string>
    {
      "A2345","23456","34567","45678","56789","6789T","789TJ","89TJQ","9TJQK","ATJQK",
    };

    public static List<string> AllFlushSorted { get; } = AllHighCards5Sorted;

    public static List<string> AllFullHouseSorted { get; } = new List<string>
    {
      "22233","22244","22255","22266","22277","22288","22299","222TT","222JJ","222QQ","222KK","AA222","22333","33344","33355","33366","33377","33388","33399","333TT",
      "333JJ","333QQ","333KK","AA333","22444","33444","44455","44466","44477","44488","44499","444TT","444JJ","444QQ","444KK","AA444","22555","33555","44555","55566",
      "55577","55588","55599","555TT","555JJ","555QQ","555KK","AA555","22666","33666","44666","55666","66677","66688","66699","666TT","666JJ","666QQ","666KK","AA666",
      "22777","33777","44777","55777","66777","77788","77799","777TT","777JJ","777QQ","777KK","AA777","22888","33888","44888","55888","66888","77888","88899","888TT",
      "888JJ","888QQ","888KK","AA888","22999","33999","44999","55999","66999","77999","88999","999TT","999JJ","999QQ","999KK","AA999","22TTT","33TTT","44TTT","55TTT",
      "66TTT","77TTT","88TTT","99TTT","TTTJJ","TTTQQ","TTTKK","AATTT","22JJJ","33JJJ","44JJJ","55JJJ","66JJJ","77JJJ","88JJJ","99JJJ","TTJJJ","JJJQQ","JJJKK","AAJJJ",
      "22QQQ","33QQQ","44QQQ","55QQQ","66QQQ","77QQQ","88QQQ","99QQQ","TTQQQ","JJQQQ","QQQKK","AAQQQ","22KKK","33KKK","44KKK","55KKK","66KKK","77KKK","88KKK","99KKK",
      "TTKKK","JJKKK","QQKKK","AAKKK","AAA22","AAA33","AAA44","AAA55","AAA66","AAA77","AAA88","AAA99","AAATT","AAAJJ","AAAQQ","AAAKK",
    };

    public static List<string> AllFourOfAKindSorted { get; } = new List<string>
    {
      "22223","22224","22225","22226","22227","22228","22229","2222T","2222J","2222Q","2222K","A2222","23333","33334","33335","33336","33337","33338","33339","3333T",
      "3333J","3333Q","3333K","A3333","24444","34444","44445","44446","44447","44448","44449","4444T","4444J","4444Q","4444K","A4444","25555","35555","45555","55556",
      "55557","55558","55559","5555T","5555J","5555Q","5555K","A5555","26666","36666","46666","56666","66667","66668","66669","6666T","6666J","6666Q","6666K","A6666",
      "27777","37777","47777","57777","67777","77778","77779","7777T","7777J","7777Q","7777K","A7777","28888","38888","48888","58888","68888","78888","88889","8888T",
      "8888J","8888Q","8888K","A8888","29999","39999","49999","59999","69999","79999","89999","9999T","9999J","9999Q","9999K","A9999","2TTTT","3TTTT","4TTTT","5TTTT",
      "6TTTT","7TTTT","8TTTT","9TTTT","TTTTJ","TTTTQ","TTTTK","ATTTT","2JJJJ","3JJJJ","4JJJJ","5JJJJ","6JJJJ","7JJJJ","8JJJJ","9JJJJ","TJJJJ","JJJJQ","JJJJK","AJJJJ",
      "2QQQQ","3QQQQ","4QQQQ","5QQQQ","6QQQQ","7QQQQ","8QQQQ","9QQQQ","TQQQQ","JQQQQ","QQQQK","AQQQQ","2KKKK","3KKKK","4KKKK","5KKKK","6KKKK","7KKKK","8KKKK","9KKKK",
      "TKKKK","JKKKK","QKKKK","AKKKK","AAAA2","AAAA3","AAAA4","AAAA5","AAAA6","AAAA7","AAAA8","AAAA9","AAAAT","AAAAJ","AAAAQ","AAAAK",
    };

    public static List<string> AllStraightFlushSorted { get; } = AllStraightSorted;

		public static List<string> AllDragon { get; } = new List<string> { "A23456789TJQK" };
		#endregion
	}
}
