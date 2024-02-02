namespace CompetitiveGamingApp.Controllers;

using System;
using System.Collections.Generic;

public class PlayerComparer : IComparer<Dictionary<string, object>> {
    private readonly List<string> sortKeys;

    public class PlayerComparer(List<string> sortKeys) {
        this.sortKeys = sortKeys;
    }
    public int Compare(Dictionary<string, object> a, Dictionary<string, object> b) {
        bool calculated = false;
        for (var k in sortKeys) {
            if (k == "playerName" || k == "playerId") {
                continue;
            }
            else if ((k == "wins" || k == "loses" || k == "ties") && !calculated) {
                int res = CompareRecords(a, b);
                if (res != 0) {
                    return res;
                }
                calculated = true;
            }
            else if ((k != "wins" && k != "loses" && k != "ties")) {
                var res = a[k].CompareTo(b[k]);
                if (res != 0) {
                    return res;
                }
            }
        }
        return 0;
    }

    public int CompareRecords(Dictionary<string, object> a, Dictionary<string, object> b) {
        double aWinPer = findWinPercentage(a["wins"], a["losses"], a["draws"]);
        double bWinPer = findWinPercentage(b["wins"], b["losses"], b["draws"]);

        return aWinPer.CompareTo(bWinPer);
    }

    public double findWinPercentage(int wins, int losses, int draws) {
        int allGames = wins + losses + draws;
        return allGames == 0 ? 0 : (double) wins / allGames;
    }
}