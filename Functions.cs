////////////////////////////////////////////////////////////////////////////////
//
// Class: FUNCTIONS
//  Contains general functions used througout the solution
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
using Microsoft.VisualBasic.FileIO;
using System.IO;
using System.Diagnostics;

namespace CFB_Predictor_v2
{
    public partial class Program
    {
        //
        // Reads in a .csv file with the option to remove the header
        public static string[][] ReadCSV(string pathName, string fileName, bool hasHeader)
        {
            List<string[]> dataList = new List<string[]>();
            using (TextFieldParser parser = new TextFieldParser(pathName + fileName))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                string[] headers;
                if (hasHeader)  // remove headers if they exist
                    headers = parser.ReadFields();
                while (!parser.EndOfData)
                {
                    // Processing row
                    string[] fields = parser.ReadFields();
                    dataList.Add(fields);
                }
            }
            return dataList.ToArray();
        }

        //
        // Converts an array of strings to a double array
        public static double[][] ConvertStringToDouble(string[][] strArray)
        {
            List<double[]> dblArray = new List<double[]>();
            foreach (string[] row in strArray)
            {
                List<double> dblRow = new List<double>();
                foreach (string cell in row)
                {
                    dblRow.Add(Convert.ToDouble(cell));
                }
                dblArray.Add(dblRow.ToArray());
            }
            return dblArray.ToArray();
        }

        //
        // Returns the adjusted rushing yards per attempt
        public static double AdjRushPerAtt(double[] data)
        {
            if (data[RUSH_ATT] == 0)        // no rush attempts
                return 0;
            else
                return (data[RUSH_YARD] + 20 * data[RUSH_TD] + 9 * data[FIRST_DOWN_RUSH]) / data[RUSH_ATT];
        }

        //
        // Returns the adjusted passing yards per attempt
        public static double AdjPassPerAtt(double[] data)
        {
            if (data[PASS_ATT] == 0)        // no pass attempts
                return 0;
            else
                return (data[PASS_YARD] + 20 * data[PASS_TD] - 45 * data[PASS_INT]) / data[PASS_ATT];
        }

        //
        // Returns true if this game is acceptable for use
        public static bool UseGame(Game G)
        {
            return true;
        }
    }
}
