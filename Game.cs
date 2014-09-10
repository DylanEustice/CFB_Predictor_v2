////////////////////////////////////////////////////////////////////////////////
//
// Class: GAME
//  Contains all data for a particular college football game
//
// Author:          Dylan Eustice
// Date Created:    8/27/2014
// Last Edited:     8/27/2014
//
////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFB_Predictor_v2
{
    public class Game
    {
        // Game data
        public long Code;
        public int Date;
        public Season Season;
        // Visitor data
        public Team Visitor;
        public double[] VisitorData = new double[Program.N_DATA_PTS];
        public double[] VisitorMetrics = new double[Program.METRIC_PTS];
        public double[] VisitorOppMetrics = new double[Program.METRIC_PTS];
        public bool NoVisitorMetrics = false;
        // Home data
        public Team Home;
        public double[] HomeData = new double[Program.N_DATA_PTS];
        public double[] HomeMetrics = new double[Program.METRIC_PTS];
        public double[] HomeOppMetrics = new double[Program.METRIC_PTS];
        public bool NoHomeMetrics = false;
        public bool MetricsDone = false;
        // Who won?
        public bool HomeWin = false;
        public bool VisitorWin = false;
        public bool Tie = false;

        //
        // Constructor
        public Game(double[] stats, Season season)
        {
            // Get code and season
            Code = (long)stats[Program.GAME_CODE];
            Season = season;

            // Use code to get home and visitor codes and date
            int visitorCode = (int)Math.Floor(Code / Math.Pow(10, 12));
            int homeCode = (int)(Math.Floor(Code / Math.Pow(10, 8)) % Math.Pow(10, 4));
            Date = (int)(Code % Math.Pow(10, 8));

            // Find visitor and home teams, then get their game data
            FindTeams(visitorCode, homeCode);
            FindTeamGameStats();

            // Determine winner
            if (HomeData[Program.POINTS] > VisitorData[Program.POINTS])
                HomeWin = true;
            else if (VisitorData[Program.POINTS] > HomeData[Program.POINTS])
                VisitorWin = true;
            else
                Tie = true;

            // Add this game to each team's array
            Home.InsertGame(this);
            Visitor.InsertGame(this);
        }

        //
        // Finds the home and visitor teams within this season
        public void FindTeams(int visitorCode, int homeCode)
        {
            bool foundVisitor = false;
            bool foundHome = false;
            int i = 0;
            while (!foundVisitor || !foundHome)
            {
                if (Season.Teams[i].Code == visitorCode)
                {
                    foundVisitor = true;
                    Visitor = Season.Teams[i];
                }
                else if (Season.Teams[i].Code == homeCode)
                {
                    foundHome = true;
                    Home = Season.Teams[i];
                }
                i++;

                // Warn if teams not found
                if (i >= Season.Teams.Count)
                {
                    if (!foundVisitor)
                        Console.WriteLine("WARNING: Visitor team ({0}) not found in game {1}!", visitorCode, Code);
                    if (!foundHome)
                        Console.WriteLine("WARNING: Home team ({0}) not found in game {1}!", homeCode, Code);
                    break;
                }
            }
        }

        #region Point Predictions
        //
        // Finds appropriate metrics and runs them on the inputted ANN
        public double PredictTeamPoints(Neural_Network predictor, bool getHome)
        {
            // Find input metrics
            double[] inputs = new double[predictor.InputStats.Length];
            for (int i = 0; i < predictor.InputStats.Length; i++)
                inputs[i] = GetMetric(predictor.InputStats[i], predictor.UseOpponent[i], predictor.UseOffense[i], getHome);
            double[] output = predictor.Think(inputs);  // return outputs
            return output[0];
        }

        //
        // Returns a team's metric
        public double GetMetric(int type, bool useOpp, bool useOff, bool getHome)
        {
            if ((getHome && !useOpp) || (!getHome && useOpp))
                return useOff ? HomeMetrics[type] : HomeOppMetrics[type];       // Get home stats
            else
                return useOff ? VisitorMetrics[type] : VisitorOppMetrics[type]; // Get visitor stats
        }
        #endregion

        #region Get this team-game-stats array
        //
        // Gets the teams' data from the team-game-stats
        public void FindTeamGameStats()
        {
            // Get visitor full game data
            int row = 0;
            while ((int)Season.TeamGameStats[row][Program.TEAM_CODE] != Visitor.Code)   // increment to team
                row++;
            while ((long)Season.TeamGameStats[row][Program.GAME_CODE] != Code)          // then find this game
                row++;
            for (int i = 0; i < Program.TEAM_GAME_PTS; i++)                             // then read in statistics
                VisitorData[i] = Season.TeamGameStats[row][i];

            // Put in advanced stats
            VisitorData[Program.IS_HOME] = 0;
            CalculateAdvancedStats(ref VisitorData);

            // Get home full game data
            row = 0;
            while ((int)Season.TeamGameStats[row][Program.TEAM_CODE] != Home.Code)      // increment to team
                row++;
            while ((long)Season.TeamGameStats[row][Program.GAME_CODE] != Code)          // then find this game
                row++;
            for (int i = 0; i < Program.TEAM_GAME_PTS; i++)                             // then read in statistics
                HomeData[i] = Season.TeamGameStats[row][i];

            // Put in advanced stats
            HomeData[Program.IS_HOME] = 1;
            CalculateAdvancedStats(ref HomeData);
        }

        //
        // Calculates the advanced stats for this data set
        public void CalculateAdvancedStats(ref double[] dataSet)
        {
            // Simple stats
            dataSet[Program.TOTAL_YARDS] = dataSet[Program.PASS_YARD] + dataSet[Program.RUSH_YARD];
            dataSet[Program.TO_LOST] = dataSet[Program.FUMBLE_LOST] + dataSet[Program.PASS_INT];
            dataSet[Program.TO_GAIN] = dataSet[Program.FUM_RET] + dataSet[Program.INT_RET];
            dataSet[Program.TO_NET] = dataSet[Program.TO_GAIN] - dataSet[Program.TO_LOST];
            dataSet[Program.TOTAL_ATT] = dataSet[Program.PASS_ATT] + dataSet[Program.RUSH_ATT];

            // Advanced stats
            dataSet[Program.ADJ_RUSH_AVG] = Program.AdjRushPerAtt(dataSet);
            dataSet[Program.ADJ_PASS_AVG] = Program.AdjPassPerAtt(dataSet);
            dataSet[Program.TD_PER_ATT] = (dataSet[Program.PASS_TD] + dataSet[Program.RUSH_TD]) / dataSet[Program.TOTAL_ATT];
            dataSet[Program.FIRST_PER_ATT] = (dataSet[Program.FIRST_DOWN_PASS] + dataSet[Program.FIRST_DOWN_RUSH]) / dataSet[Program.TOTAL_ATT];

            // Handle 0 pass attempts
            if (dataSet[Program.PASS_ATT] == 0)
            {
                dataSet[Program.PASS_BKN_PER] = 0;
                dataSet[Program.COMP_PER] = 0;
                dataSet[Program.INT_PER_ATT] = 0;
                dataSet[Program.YARD_PER_PASS] = 0;
            }
            else
            {
                dataSet[Program.COMP_PER] = dataSet[Program.PASS_COMP] / dataSet[Program.PASS_ATT];
                dataSet[Program.PASS_BKN_PER] = dataSet[Program.PASS_BROKEN_UP] / dataSet[Program.PASS_ATT];
                dataSet[Program.INT_PER_ATT] = dataSet[Program.PASS_INT] / dataSet[Program.PASS_ATT];
                dataSet[Program.YARD_PER_PASS] = dataSet[Program.PASS_YARD] / dataSet[Program.PASS_ATT];
            }

            // Handle 0 rush attempts
            if (dataSet[Program.RUSH_ATT] == 0)
            {
                dataSet[Program.FUM_PER_ATT] = 0;
                dataSet[Program.YARD_PER_RUSH] = 0;
            }
            else
            {
                dataSet[Program.FUM_PER_ATT] = dataSet[Program.FUMBLE_LOST] / dataSet[Program.RUSH_ATT];
                dataSet[Program.YARD_PER_RUSH] = dataSet[Program.RUSH_YARD] / dataSet[Program.RUSH_ATT];
            }

            // Handle no red zone attempts
            if (dataSet[Program.RED_ZONE_ATT] == 0)
            {
                dataSet[Program.RZ_TD_PER] = 0;
                dataSet[Program.RZ_SCORE_PER] = 0;
            }
            else
            {
                dataSet[Program.RZ_TD_PER] = dataSet[Program.RED_ZONE_TD] / dataSet[Program.RED_ZONE_ATT];
                dataSet[Program.RZ_SCORE_PER] = (dataSet[Program.RED_ZONE_TD] + dataSet[Program.RED_ZONE_FG]) / dataSet[Program.RED_ZONE_ATT];
            }
        }
        #endregion

        #region Get these team's metrics
        //
        // Gets the metrics used to predict this game
        public void GetTeamMetrics()
        {
            // Make sure all metrics for prior games have been calculated
            foreach (Team T in Home.Conference.Teams)
                foreach (Game G in T.Games)
                    if (G.Date < Date && !G.MetricsDone)
                        G.GetTeamMetrics();
            foreach (Team T in Visitor.Conference.Teams)
                foreach (Game G in T.Games)
                    if (G.Date < Date && !G.MetricsDone)
                        G.GetTeamMetrics();

            // Get team-game-stats averages from last season
            double[] homePrevSimpleAvg = new double[Program.SIMPLE_PTS];
            double[] homePrevOppSimpleAvg = new double[Program.SIMPLE_PTS];
            double[] visitorPrevSimpleAvg = new double[Program.SIMPLE_PTS];
            double[] visitorPrevOppSimpleAvg = new double[Program.SIMPLE_PTS];
            if (Season.PastSeasons.Count > 0)   // If 0 index of the array is not 1, the values were not set (do not use)
            {
                homePrevSimpleAvg = GetPrevSimpleAverages(Home);
                homePrevOppSimpleAvg = GetPrevOppSimpleAverages(Home);
                visitorPrevSimpleAvg = GetPrevSimpleAverages(Visitor);
                visitorPrevOppSimpleAvg = GetPrevOppSimpleAverages(Visitor);
            }
            bool usePrevHome = homePrevSimpleAvg[0] == 1 ? true : false;           // determine if this is either team's 1st year
            bool usePrevVisitor = visitorPrevSimpleAvg[0] == 1 ? true : false;

            // Get simple stats lists for this season
            List<double[]> homeSimpleLists = Home.ReturnSimpleLists(this);
            List<double[]> homeOppSimpleLists = Home.ReturnOppSimpleLists(this);
            List<double[]> visitorSimpleLists = Visitor.ReturnSimpleLists(this);
            List<double[]> visitorOppSimpleLists = Visitor.ReturnOppSimpleLists(this);

            // Find simple averages for the metrics
            double[] homeMetricTotals = new double[Program.METRIC_PTS];
            double[] homeOppMetricTotals = new double[Program.METRIC_PTS];
            double[] visitorMetricTotals = new double[Program.METRIC_PTS];
            double[] visitorOppMetricTotals = new double[Program.METRIC_PTS];
            InitMetricTotals(usePrevHome, ref homeMetricTotals, ref homeOppMetricTotals, homePrevSimpleAvg, homePrevOppSimpleAvg);
            InitMetricTotals(usePrevVisitor, ref visitorMetricTotals, ref visitorOppMetricTotals, visitorPrevSimpleAvg, visitorPrevOppSimpleAvg);

            // Get home averages
            AddSeasonMetrics(ref homeMetricTotals, ref homeOppMetricTotals, homeSimpleLists, homeOppSimpleLists);
            double nHomeGames = homeSimpleLists.Count + (usePrevHome ? 1 : 0);
            for (int i = 2; i < homeMetricTotals.Length; i++)
            {
                HomeMetrics[i] = homeMetricTotals[i] / nHomeGames;
                HomeOppMetrics[i] = homeOppMetricTotals[i] / nHomeGames;
            }
            if (nHomeGames == 0)
                NoHomeMetrics = true;
            HomeMetrics[Program.IS_HOME] = 1;

            // Get visitor averages
            AddSeasonMetrics(ref visitorMetricTotals, ref visitorOppMetricTotals, visitorSimpleLists, visitorOppSimpleLists);
            double nVisitorGames = visitorSimpleLists.Count + (usePrevVisitor ? 1 : 0);
            for (int i = 2; i < visitorMetricTotals.Length; i++)
            {
                VisitorMetrics[i] = visitorMetricTotals[i] / nVisitorGames;
                VisitorOppMetrics[i] = visitorOppMetricTotals[i] / nVisitorGames;
            }
            if (nVisitorGames == 0)
                NoVisitorMetrics = true;

            // Get advanced metrics
            GetAdvancedMetrics();
            MetricsDone = true;
        }

        //
        // Initializes the totals array
        public void InitMetricTotals(bool usePrev, ref double[] metricTotals, ref double[] oppMetricTotals, double[] prevSimple, double[] prevOppSimple)
        {
            if (usePrev)    // initialize with previous stats if they're available
            {
                for (int i = 2; i < prevSimple.Length; i++)
                {
                    metricTotals[i] = prevSimple[i];
                    oppMetricTotals[i] = prevOppSimple[i];
                }
            }
        }

        //
        // Adds this season's stats to the metrics array
        public void AddSeasonMetrics(ref double[] metricTotals, ref double[] oppMetricTotals, List<double[]> simpleLists, List<double[]> oppSimpleLists)
        {
            for (int i = 0; i < simpleLists.Count; i++)
            {
                double[] simpleStats = simpleLists[i];
                double[] simpleOppStats = oppSimpleLists[i];
                for (int j = 2; j < simpleStats.Length; j++)
                {
                    metricTotals[j] += simpleStats[j];
                    oppMetricTotals[j] += simpleOppStats[j];
                }
            }
        }

        //
        // Calculates advanced metrics for this game
        public void GetAdvancedMetrics()
        {
            // Get all passing metrics
            GetPassMetrics(ref HomeMetrics);
            GetPassMetrics(ref HomeOppMetrics);
            GetPassMetrics(ref VisitorMetrics);
            GetPassMetrics(ref VisitorOppMetrics);

            // Get all rushing metrics
            GetRushMetrics(ref HomeMetrics);
            GetRushMetrics(ref HomeOppMetrics);
            GetRushMetrics(ref VisitorMetrics);
            GetRushMetrics(ref VisitorOppMetrics);

            // Get all red zone metrics
            GetRedZoneMetrics(ref HomeMetrics);
            GetRedZoneMetrics(ref HomeOppMetrics);
            GetRedZoneMetrics(ref VisitorMetrics);
            GetRedZoneMetrics(ref VisitorOppMetrics);

            // Get all red zone metrics
            GetMiscMetrics(ref HomeMetrics);
            GetMiscMetrics(ref HomeOppMetrics);
            GetMiscMetrics(ref VisitorMetrics);
            GetMiscMetrics(ref VisitorOppMetrics);

            // Get pythagorean expectations
            GetPythagExpMetrics();
            GetOOCPythagExpMetrics(Home, ref HomeMetrics);
            GetOOCPythagExpMetrics(Visitor, ref VisitorMetrics);
        }
        #endregion

        #region Calculate advanced metrics
        //
        // Gets the previous season averages for complex passing stats
        public void GetPassMetrics(ref double[] metrics)
        {
            if (metrics[Program.PASS_ATT] == 0)
            {
                metrics[Program.COMP_PER] = 0;
                metrics[Program.PASS_BKN_PER] = 0;
                metrics[Program.INT_PER_ATT] = 0;
                metrics[Program.YARD_PER_PASS] = 0;
                metrics[Program.ADJ_PASS_AVG] = 0;
            }
            else
            {
                metrics[Program.COMP_PER] = metrics[Program.PASS_COMP] / metrics [Program.PASS_ATT];
                metrics[Program.PASS_BKN_PER] = metrics[Program.PASS_BROKEN_UP] / metrics[Program.PASS_ATT];
                metrics[Program.INT_PER_ATT] = metrics[Program.PASS_INT] / metrics[Program.PASS_ATT];
                metrics[Program.YARD_PER_PASS] = metrics[Program.PASS_YARD] / metrics[Program.PASS_ATT];
                metrics[Program.ADJ_PASS_AVG] = (metrics[Program.PASS_YARD] + 20 * metrics[Program.PASS_TD] - 45 * metrics[Program.PASS_INT]) / metrics[Program.PASS_ATT];
            }
        }

        //
        // Gets the previous season averages for complex rushing stats
        public void GetRushMetrics(ref double[] metrics)
        {
            if (metrics[Program.RUSH_ATT] == 0)
            {
                metrics[Program.FUM_PER_ATT] = 0;
                metrics[Program.YARD_PER_RUSH] = 0;
                metrics[Program.ADJ_RUSH_AVG] = 0;
            }
            else
            {
                metrics[Program.FUM_PER_ATT] = metrics[Program.FUMBLE_LOST] / metrics[Program.RUSH_ATT];
                metrics[Program.YARD_PER_RUSH] = metrics[Program.RUSH_YARD] / metrics[Program.RUSH_ATT];
                metrics[Program.ADJ_RUSH_AVG] = (metrics[Program.RUSH_YARD] + 20 * metrics[Program.RUSH_TD] + 9 * metrics[Program.FIRST_DOWN_RUSH]) / metrics[Program.RUSH_ATT];
            }
        }
        
        //
        // Gets the previous season averages for complex red zone stats
        public void GetRedZoneMetrics(ref double[] metrics)
        {
            if (metrics[Program.RED_ZONE_ATT] == 0)
            {
                metrics[Program.RZ_TD_PER] = 0;
                metrics[Program.RZ_SCORE_PER] = 0;
            }
            else
            {
                metrics[Program.RZ_TD_PER] = metrics[Program.RED_ZONE_TD] / metrics[Program.RED_ZONE_ATT];
                metrics[Program.RZ_SCORE_PER] = (metrics[Program.RED_ZONE_TD] + metrics[Program.RED_ZONE_FG]) / metrics[Program.RED_ZONE_ATT];
            }
        }

        //
        // Gets the rest of the advanced metrics
        public void GetMiscMetrics(ref double[] metrics)
        {
            metrics[Program.TD_PER_ATT] = (metrics[Program.RUSH_TD] + metrics[Program.PASS_TD]) / metrics[Program.TOTAL_ATT];
            metrics[Program.TD_PER_ATT] = (metrics[Program.FIRST_DOWN_RUSH] + metrics[Program.FIRST_DOWN_PASS]) / metrics[Program.TOTAL_ATT];
        }

        //
        // Gets the pythagorean expectations for the metrics
        public void GetPythagExpMetrics()
        {
            // Get home and visitor pythagorean expectations
            if (Double.IsNaN(HomeMetrics[Program.POINTS]) || Double.IsNaN(HomeOppMetrics[Program.POINTS]))
                HomeMetrics[Program.PYTHAG_EXPECT] = 0.5;
            else
                HomeMetrics[Program.PYTHAG_EXPECT] = Program.GetPythagExp(HomeMetrics[Program.POINTS], HomeOppMetrics[Program.POINTS]);

            if (Double.IsNaN(VisitorMetrics[Program.POINTS]) || Double.IsNaN(VisitorMetrics[Program.POINTS]))
                VisitorMetrics[Program.PYTHAG_EXPECT] = 0.5;
            else
                VisitorMetrics[Program.PYTHAG_EXPECT] = Program.GetPythagExp(VisitorMetrics[Program.POINTS], VisitorOppMetrics[Program.POINTS]);

            // Get home and visitor previous opponents' average pythagorean expectation
            double homeOppTotal = 0;        // home
            double homeGames = 0;
            foreach (Game G in Home.Games)
            {
                if (G.Date < Date && Program.UseGame(G))
                {
                    homeOppTotal += (G.Home == Home) ? G.VisitorMetrics[Program.PYTHAG_EXPECT] : G.HomeMetrics[Program.PYTHAG_EXPECT];
                    homeGames++;
                }
            }
            HomeOppMetrics[Program.PYTHAG_EXPECT] = (homeGames > 0) ? homeOppTotal / homeGames : 0.5;
            double visitorOppTotal = 0;     // visitor
            double visitorGames = 0;
            foreach (Game G in Visitor.Games)
            {
                if (G.Date < Date && Program.UseGame(G))
                {
                    visitorOppTotal += (G.Visitor == Visitor) ? G.HomeMetrics[Program.PYTHAG_EXPECT] : G.VisitorMetrics[Program.PYTHAG_EXPECT];
                    visitorGames++;
                }
            }
            VisitorOppMetrics[Program.PYTHAG_EXPECT] = (visitorGames > 0) ? visitorOppTotal / visitorGames : 0.5;
        }

        //
        // Gets the teams' conferences OOC pythagorean expectation
        public void GetOOCPythagExpMetrics(Team team, ref double[] metrics)
        {
            // Add all team's conference OOC games to a list (from this season and last)
            List<Game> OOCGames = new List<Game>();
            Conference prevConference = new Conference();
            if (Season.PastSeasons.Count > 0)   // add last season's games
                if (FindPreviousConference(team.Conference, ref prevConference))
                    foreach (Team T in prevConference.Teams)
                        foreach (Game G in T.Games)
                            if (Program.UseGame(G) && prevConference.IsOOC(G))
                                OOCGames.Add(G);
            foreach (Team T in team.Conference.Teams)   // add this season's games
                foreach (Game G in T.Games)
                    if (G.Date < Date && Program.UseGame(G) && team.Conference.IsOOC(G))
                        OOCGames.Add(G);
            if (OOCGames.Count == 0)
                metrics[Program.OOC_PYTHAG] = 0.5;
            else
            {
                double RS = 0, RA = 0;
                foreach (Game G in OOCGames)
                {
                    RS += (G.Home.Conference == Home.Conference) ? G.HomeData[Program.POINTS] : G.VisitorData[Program.POINTS];
                    RA += (G.Home.Conference == Home.Conference) ? G.VisitorData[Program.POINTS] : G.HomeData[Program.POINTS];
                }
                metrics[Program.OOC_PYTHAG] = Program.GetPythagExp(RS, RA);
            }
        }
        #endregion

        #region Get previous season averages
        //
        // Gets this team's last season average for simple averages data
        public double[] GetPrevSimpleAverages(Team team)
        {
            double[] teamGameAverages = new double[Program.SIMPLE_PTS];

            // Find last season's team
            Team prevTeam = new Team();
            if (!FindPreviousTeam(team, ref prevTeam))
                return teamGameAverages;

            // Get averages
            for (int i = 2; i < teamGameAverages.Length; i++)
                teamGameAverages[i] = prevTeam.GetSeasonAverage(i);

            teamGameAverages[0] = 1;    // values set flag
            return teamGameAverages;
        }

        //
        // Gets this team's opponents' last season averages for simple averages data
        public double[] GetPrevOppSimpleAverages(Team team)
        {
            double[] teamGameAverages = new double[Program.SIMPLE_PTS];

            // Find last season's team
            Team prevTeam = new Team();
            if (!FindPreviousTeam(team, ref prevTeam))
                return teamGameAverages;

            // Get averages
            for (int i = 2; i < teamGameAverages.Length; i++)
                teamGameAverages[i] = prevTeam.GetOppSeasonAverage(i);

            teamGameAverages[0] = 1;    // values set flag
            return teamGameAverages;
        }
        #endregion

        #region Find previous objects
        //
        // Gets reference to the previous season's conference. Returns false if that conference did not have a previous season.
        public bool FindPreviousConference(Conference conference, ref Conference prevConference)
        {
            int confNum = 0;
            while (Season.PastSeasons[Season.PastSeasons.Count - 1].Conferences[confNum].Code != Home.Conference.Code)
            {
                confNum++;
                if (confNum >= Season.PastSeasons[Season.PastSeasons.Count - 1].Conferences.Count)
                    return false;
            }
            prevConference = Season.PastSeasons[Season.PastSeasons.Count - 1].Conferences[confNum];
            return true;
        }

        //
        // Gets reference to the previous season's team. Returns false if that team did not have a previous season.
        public bool FindPreviousTeam(Team team, ref Team prevTeam)
        {
            int teamNum = 0;
            while (Season.PastSeasons[Season.PastSeasons.Count - 1].Teams[teamNum].Code != team.Code)
            {
                teamNum++;
                if (Season.PastSeasons[Season.PastSeasons.Count - 1].Teams.Count <= teamNum)    // 1st year in D1 for this team
                    return false;
            }
            prevTeam = Season.PastSeasons[Season.PastSeasons.Count - 1].Teams[teamNum];
            return true;
        }
        #endregion
    }
}
