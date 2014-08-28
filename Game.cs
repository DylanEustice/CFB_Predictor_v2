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
        // Home data
        public Team Home;
        public double[] HomeData = new double[Program.N_DATA_PTS];
        public double[] HomeMetrics = new double[Program.METRIC_PTS];
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

            FindTeams(visitorCode, homeCode);   // Find visitor and home teams
            FindTeamGameStats();                // Get visitor and home data

            // Determine winner
            if (HomeData[Program.POINTS] > VisitorData[Program.POINTS])
                HomeWin = true;
            else if (VisitorData[Program.POINTS] > HomeData[Program.POINTS])
                VisitorWin = true;
            else
                Tie = true;

            // Get team's metrics going into this game
            if (Season.PastSeasons.Count > 0)
                GetTeamMetrics();
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
            for (int i = 0; i < Program.N_DATA_PTS - Program.XTRA_DATA_PTS; i++)        // then read in statistics
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
            for (int i = 0; i < Program.N_DATA_PTS - Program.XTRA_DATA_PTS; i++)        // then read in statistics
                HomeData[i] = Season.TeamGameStats[row][i];

            // Put in advanced stats
            HomeData[Program.IS_HOME] = 1;
            CalculateAdvancedStats(ref HomeData);
        }

        //
        // Calculates the advanced stats for this data set
        public void CalculateAdvancedStats(ref double[] dataSet)
        {
            dataSet[Program.TOTAL_YARDS] = dataSet[Program.PASS_YARD] + dataSet[Program.RUSH_YARD];
            dataSet[Program.TO_LOST] = dataSet[Program.FUMBLE_LOST] + dataSet[Program.PASS_INT];
            dataSet[Program.TO_GAIN] = dataSet[Program.FUM_RET] + dataSet[Program.INT_RET];
            dataSet[Program.TO_NET] = dataSet[Program.TO_GAIN] - dataSet[Program.TO_LOST];
            dataSet[Program.ADJ_RUSH_AVG] = Program.AdjRushPerAtt(dataSet);
            dataSet[Program.ADJ_PASS_AVG] = Program.AdjPassPerAtt(dataSet);
            dataSet[Program.TOTAL_ATT] = dataSet[Program.PASS_ATT] + dataSet[Program.RUSH_ATT];
            dataSet[Program.TD_PER_ATT] = (dataSet[Program.PASS_TD] + dataSet[Program.RUSH_TD]) / dataSet[Program.TOTAL_ATT];
            dataSet[Program.FIRST_PER_ATT] = (dataSet[Program.FIRST_DOWN_PASS] + dataSet[Program.FIRST_DOWN_RUSH]) / dataSet[Program.TOTAL_ATT];

            // Handle 0 pass attempts
            if (dataSet[Program.PASS_ATT] == 0)
            {
                dataSet[Program.PASS_BKN_PER] = 0;
                dataSet[Program.COMP_PER] = 0;
                dataSet[Program.INT_PER_ATT] = 0;
            }
            else
            {
                dataSet[Program.COMP_PER] = dataSet[Program.PASS_COMP] / dataSet[Program.PASS_ATT];
                dataSet[Program.PASS_BKN_PER] = dataSet[Program.PASS_BROKEN_UP] / dataSet[Program.PASS_ATT];
                dataSet[Program.INT_PER_ATT] = dataSet[Program.PASS_INT] / dataSet[Program.PASS_ATT];
            }

            // Handle 0 rush attempts
            if (dataSet[Program.RUSH_ATT] == 0)
                dataSet[Program.FUM_PER_ATT] = 0;
            else
                dataSet[Program.FUM_PER_ATT] = dataSet[Program.FUMBLE_LOST] / dataSet[Program.RUSH_ATT];

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

        //
        // Gets the metrics used to predict this game
        public void GetTeamMetrics()
        {
            // Get team-game-stats averages from last season
            double[] homeTGSAvg = GetPreviousTGSAverages(Home);
            double[] visitorTGSAvg = GetPreviousTGSAverages(Visitor);
        }

        //
        // Gets this team's last season average for team-game-stats data
        public double[] GetPreviousTGSAverages(Team team)
        {
            double[] teamGameAverages = new double[Program.TEAM_GAME_PTS];
            int teamNum = 0;   // find the team from last season
            while (Season.PastSeasons[Season.PastSeasons.Count - 1].Teams[teamNum].Code != team.Code)
                teamNum++;
            for (int i = 2; i < Program.TEAM_GAME_PTS; i++)
                teamGameAverages[i] = Season.PastSeasons[Season.PastSeasons.Count - 1].Teams[teamNum].GetSeasonAverage(i);
            return teamGameAverages;
        }
    }
}
