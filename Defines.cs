////////////////////////////////////////////////////////////////////////////////
//
// Class: DEFINES
//  Contains global constants, usually used for data indexing
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
    public partial class Program
    {
        // CFB team-game-statistics.csv constants
        public const int TEAM_CODE = 0;
        public const int GAME_CODE = 1;
        public const int RUSH_ATT = 2;
        public const int RUSH_YARD = 3;
        public const int RUSH_TD = 4;
        public const int PASS_ATT = 5;
        public const int PASS_COMP = 6;
        public const int PASS_YARD = 7;
        public const int PASS_TD = 8;
        public const int PASS_INT = 9;
        public const int PASS_CONV = 10;
        public const int KICKOFF_RET = 11;
        public const int KICKOFF_RET_YARD = 12;
        public const int KICKOFF_RET_TD = 13;
        public const int PUNT_RET = 14;
        public const int PUNT_RET_YARD = 15;
        public const int PUNT_RET_TD = 16;
        public const int FUM_RET = 17;
        public const int FUM_RET_YARD = 18;
        public const int FUM_RET_TD = 19;
        public const int INT_RET = 20;
        public const int INT_RET_YARD = 21;
        public const int INT_RET_TD = 22;
        public const int MISC_RET = 23;
        public const int MISC_RET_YARD = 24;
        public const int MISC_RET_TD = 25;
        public const int FIELD_GOAL_ATT = 26;
        public const int FIELD_GOAL_MADE = 27;
        public const int OFF_XP_KICK_ATT = 28;
        public const int OFF_XP_KICK_MADE = 29;
        public const int OFF_2XP_ATT = 30;
        public const int OFF_2XP_MADE = 31;
        public const int DEF_2XP_ATT = 32;
        public const int DEF_2XP_MADE = 33;
        public const int SAFETY = 34;
        public const int POINTS = 35;
        public const int PUNT = 36;
        public const int PUNT_YARD = 37;
        public const int KICKOFF = 38;
        public const int KICKOFF_YARD = 39;
        public const int KICKOFF_TOUCHBACK = 40;
        public const int KICKOFF_OB = 41;
        public const int KICKOFF_ONSIDES = 42;
        public const int FUMBLE = 43;
        public const int FUMBLE_LOST = 44;
        public const int TACKLE_SOLO = 45;
        public const int TACKLE_AST = 46;
        public const int TACKLE_FOR_LOSS = 47;
        public const int TACKLE_FOR_LOSS_YARD = 48;
        public const int SACK = 49;
        public const int SACK_YARD = 50;
        public const int QB_HURRY = 51;
        public const int FUMBLE_FORCED = 52;
        public const int PASS_BROKEN_UP = 53;
        public const int KICK_PUNT_BLOCKED = 54;
        public const int FIRST_DOWN_RUSH = 55;
        public const int FIRST_DOWN_PASS = 56;
        public const int FIRST_DOWN_PENALTY = 57;
        public const int TIME_OF_POS = 58;
        public const int PENALTY = 59;
        public const int PENALTY_YARD = 60;
        public const int THIRD_DOWN_ATT = 61;
        public const int THIRD_DOWN_CONV = 62;
        public const int FOURTH_DOWN_ATT = 63;
        public const int FOURTH_DOWN_CONV = 64;
        public const int RED_ZONE_ATT = 65;
        public const int RED_ZONE_TD = 66;
        public const int RED_ZONE_FG = 67;

        public const int TEAM_GAME_PTS = 68;
        
        // Advanced game stats
        public const int IS_HOME = 68;          // 1 for home team
        public const int TOTAL_YARDS = 69;      // rush + pass yards
        public const int TO_LOST = 70;          // turnovers lost
        public const int TO_GAIN = 71;          // turnovers gained
        public const int TO_NET = 72;           // turnovers gained - turnovers lost
        public const int RZ_TD_PER = 73;        // Red zone td %
        public const int RZ_SCORE_PER = 74;     // Red zone score %
        public const int ADJ_RUSH_AVG = 75;     // adjusted yards per rushing attempt
        public const int ADJ_PASS_AVG = 76;     // adjusted yards per passing attempt
        public const int TOTAL_ATT = 77;        // pass att + rush att
        public const int INT_PER_ATT = 78;      // interception / pass att
        public const int FUM_PER_ATT = 79;      // fumble / rush att
        public const int TD_PER_ATT = 80;       // td / total att
        public const int FIRST_PER_ATT = 81;    // first down per att
        public const int COMP_PER = 82;         // passing completion %
        public const int PASS_BKN_PER = 83;     // % of passes broken up
        public const int YARD_PER_RUSH = 84;    // yards / rush att
        public const int YARD_PER_PASS = 85;    // yards / rush att

        public const int XTRA_DATA_PTS = 18;
        public const int N_DATA_PTS = TEAM_GAME_PTS + XTRA_DATA_PTS;

        // Team metrics only
        public const int OOC_PYTHAG = N_DATA_PTS;           // conference OOC pythagorean winning expectation
        public const int PYTHAG_EXPECT = N_DATA_PTS + 1;    // team's pythagorean winning expectation

        public const int XTRA_METRICS = 2;
        public const int METRIC_PTS = N_DATA_PTS + XTRA_METRICS;
    }
}
