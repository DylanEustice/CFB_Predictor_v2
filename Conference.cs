////////////////////////////////////////////////////////////////////////////////
//
// Class: CONFERENCE
//  Contains all data for a conference from a season of college football
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
    public class Conference
    {
        public int Code;
        public string Name;
        public string Division;
        public List<Team> Teams = new List<Team>();

        //
        // Blank constructor
        public Conference()
        {
        }

        //
        // Constructor
        public Conference(int code, string name, string div)
        {
            Code = code;
            Name = name;
            Division = div;
        }

        //
        // Returns true if a game is out of conference
        public bool IsOOC(Game G)
        {
            if (G.Home.Conference != this && G.Visitor.Conference != this)
                Console.WriteLine("WARNING: Neither team is in this conference!\n");
            if (G.Home.Conference != this || G.Visitor.Conference != this)
                return true;
            else
                return false;
        }
    }
}
