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
        // Finds maximums in each metric type for a given set of seasons
        public static double[] FindMetricMaxes(List<Season> seasons)
        {
            double[] maximums = new double[METRIC_PTS];
            foreach (Season S in seasons)
            {
                foreach (Game G in S.Games)
                {
                    if (!UseGame(G))
                        continue;
                    // Check home metrics
                    if (!G.NoHomeMetrics)
                    {
                        for (int i = 0; i < G.HomeMetrics.Length; i++)
                        {
                            if (G.HomeMetrics[i] > maximums[i])
                                maximums[i] = G.HomeMetrics[i];
                            if (G.HomeOppMetrics[i] > maximums[i])
                                maximums[i] = G.HomeOppMetrics[i];
                        }
                    }
                    // Check visitor metrics
                    if (!G.NoVisitorMetrics)
                    {
                        for (int i = 0; i < G.VisitorMetrics.Length; i++)
                        {
                            if (G.VisitorMetrics[i] > maximums[i])
                                maximums[i] = G.VisitorMetrics[i];
                            if (G.VisitorOppMetrics[i] > maximums[i])
                                maximums[i] = G.VisitorOppMetrics[i];
                        }
                    }
                }
            }
            return maximums;
        }

        //
        // Normalizes a set of seasons' metrics by the maximum metrics found
        public static void NormalizeSeasonMetrics(ref List<Season> seasons)
        {
            double[] maximums = FindMetricMaxes(seasons);
            foreach (Season S in seasons)
            {
                foreach (Game G in S.Games)
                {
                    // Normalize home metrics
                    for (int i = 0; i < G.HomeData.Length; i++)
                        G.HomeData[i] /= maximums[i];
                    if (!G.NoHomeMetrics)
                    {
                        for (int i = 0; i < G.HomeMetrics.Length; i++)
                        {
                            if (maximums[i] > 0)
                            {
                                G.HomeMetrics[i] /= maximums[i];
                                G.HomeOppMetrics[i] /= maximums[i];
                            }
                        }
                    }
                    // Normalize visitor metrics
                    for (int i = 0; i < G.VisitorData.Length; i++)
                        G.VisitorData[i] /= maximums[i];
                    if (!G.NoVisitorMetrics)
                    {
                        for (int i = 0; i < G.VisitorMetrics.Length; i++)
                        {
                            if (maximums[i] > 0)
                            {
                                G.VisitorMetrics[i] /= maximums[i];
                                G.VisitorOppMetrics[i] /= maximums[i];
                            }
                        }
                    }
                }
            }
        }

        //
        // Sets up and trains a neural network
        public static Neural_Network TrainNetwork(List<Season> seasons)
        {
            // Stat types to use
            int[] inputStat = {      TOTAL_YARDS,        TOTAL_YARDS,
                                     ADJ_PASS_AVG,       ADJ_PASS_AVG,
                                     ADJ_RUSH_AVG,       ADJ_RUSH_AVG,
                                     POINTS,             POINTS };
            // Use this team's or opponent's stat
            bool[] inUseOpponent = { false, true,     // TOTAL_YARDS
                                     false, true,     // ADJ_PASS_AVG
                                     false, true,     // ADJ_RUSH_AVG
                                     false, true };   // POINTS
            // Use team's offense or defensive metrics
            bool[] inUseOffense = {  true, false,      // TOTAL_YARDS
                                     true, false,      // ADJ_PASS_AVG
                                     true, false,      // ADJ_RUSH_AVG
                                     true, false };    // POINTS

            int[] LayerSizes = { inputStat.Length, 6, 3, 1 };
            int[] outputStat = { POINTS };
            Random random = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);

            List<Game> trainGames = new List<Game>();
            List<Game> testGames = new List<Game>();
            for (int i = 5; i < seasons.Count; i++)
            {
                for (int j = 0; j < seasons[i].Games.Count; j++ )
                {
                    if (!UseGame(seasons[i].Games[j]))
                        continue;
                    if (j % 50 == 0)
                        trainGames.Add(seasons[i].Games[j]);
                    else
                        testGames.Add(seasons[i].Games[j]);
                }
            }
            Console.WriteLine("Train games: {0}", trainGames.Count);

            Neural_Network network = new Neural_Network(LayerSizes, inputStat, outputStat, inUseOpponent, inUseOffense, USE_ACT, random, 0);
            Swarm swarm = new Swarm(250, MOMENTUM, MOM_GLOBAL, MOM_PERSONAL, LayerSizes, inputStat, outputStat, inUseOpponent, inUseOffense, trainGames.ToArray(), testGames.ToArray());
            RunParticleSwarm(ref swarm);
            network = swarm.bestNetwork;
            WriteOutputData(network, swarm);

            return network;
        }

        //
        // Writes outputs from search to file
        public static void WriteOutputData(Neural_Network network, Swarm swarm)
        {
            // Output parameters
            string outputFileName = "../../Output/AllStats_Output.csv";
            StreamWriter outputFileStream = new StreamWriter(outputFileName);

            // Write neural network and/or swarm info
            string[] algorithmInfo = new string[3 + network.LayerSizes[network.LayerSizes.Length - 1]];
            algorithmInfo[0] = network.LayerSizes[0].ToString();
            algorithmInfo[1] = network.LayerSizes[network.LayerSizes.Length - 1].ToString();
            int maxCounter = 2;
            for (int i = 0; i < network.LayerSizes[network.LayerSizes.Length - 1]; i++)
            {
                int max = network.OutputStats[i];
                algorithmInfo[maxCounter] = max.ToString();
                maxCounter++;
            }
            algorithmInfo[maxCounter] = "1";
            for (int i = 0; i < algorithmInfo.Length; i++)
            {
                outputFileStream.Write(algorithmInfo[i]);
                if (i + 1 != algorithmInfo.Length)
                    outputFileStream.Write(",");
            }
            outputFileStream.WriteLine();

            // Write movement data
            string[] movementOut = new string[swarm.AverageMovement.Count];
            for (int i = 0; i < swarm.AverageMovement.Count; i++)
            {
                movementOut[i] = swarm.AverageMovement[i].ToString();
                outputFileStream.Write(movementOut[i]);
                if (i + 1 != swarm.AverageMovement.Count)
                    outputFileStream.Write(",");
            }
            outputFileStream.WriteLine();

            // Write global update value data
            string[] globalValOut = new string[swarm.GlobalBestHistory.Count];
            for (int i = 0; i < swarm.GlobalBestHistory.Count; i++)
            {
                globalValOut[i] = swarm.GlobalBestHistory[i].ToString();
                outputFileStream.Write(globalValOut[i]);
                if (i + 1 != swarm.GlobalBestHistory.Count)
                    outputFileStream.Write(",");
            }
            outputFileStream.WriteLine();

            // Write global update iteration data
            string[] globalUpOut = new string[swarm.GlobalBestUpdateIteration.Count];
            for (int i = 0; i < swarm.GlobalBestUpdateIteration.Count; i++)
            {
                globalUpOut[i] = swarm.GlobalBestUpdateIteration[i].ToString();
                outputFileStream.Write(globalUpOut[i]);
                if (i + 1 != swarm.GlobalBestUpdateIteration.Count)
                    outputFileStream.Write(",");
            }
            outputFileStream.WriteLine();

            // Write real output data
            /*string[] outRealString = new string[testOutputData.Length];
            for (int i = 0; i < testOutputData.Length; i++)
            {
                outRealString[i] = "";
                for (int j = 0; j < testOutputData[i].Length; j++)
                {
                    outRealString[i] += testOutputData[i][j].ToString();
                    if (j + 1 != testOutputData[i].Length)
                        outRealString[i] += ",";
                }
                outputFileStream.Write(outRealString[i]);
                if (i + 1 != testOutputData.Length)
                    outputFileStream.Write(",");
            }
            outputFileStream.WriteLine();

            // Write neural network output data
            string[] outNNString = new string[testOutputData.Length];
            for (int i = 0; i < testOutputData.Length; i++)
            {
                double[] thisInput = testInputData[i];                      // get input for this output
                double[] thisOutput = swarm.bestNetwork.Think(thisInput);   // get network output
                outNNString[i] = "";
                for (int j = 0; j < thisOutput.Length; j++)     // put network outputs in outRealString[i]
                {
                    outNNString[i] += thisOutput[j].ToString();
                    if (j + 1 != thisOutput.Length)
                        outNNString[i] += ",";
                }
                outputFileStream.Write(outNNString[i]);
                if (i + 1 != testOutputData.Length)
                    outputFileStream.Write(",");
            }*/
            outputFileStream.WriteLine();
            outputFileStream.Close();   // close file
        }

        //
        // Trains a neural network via particle swarm
        public static void RunParticleSwarm(ref Swarm swarm)
        {
            Stopwatch stopwatch = new Stopwatch(); ;
            Console.Write("Training neural network via particle swarm");
            for (int i = 1; i <= ITERATIONS; i++)
            {
                // Get start time
                if (i == 1)
                    stopwatch.Start();

                // Write progress
                if (i % (ITERATIONS / 10) == 0)
                {
                    Console.Write(100 * i / ITERATIONS);
                    if (i != ITERATIONS)
                        Console.Write(", ");
                    else
                        Console.WriteLine();
                }
                // Iterate
                swarm.GetParticleFitnesses(i);
                swarm.MoveParticles();
                if (swarm.AverageMovement.Last() < MIN_MOVEMENT)
                    swarm.ResetParticles();

                // Get stop time and estimate total runtime
                if (i == 5)
                {
                    stopwatch.Stop();
                    Console.WriteLine(", expected runtime: {0} s", ITERATIONS * stopwatch.ElapsedMilliseconds / 5000);
                }
            }
            Console.WriteLine();
        }

        //
        // Reads in a .csv file with the option to remove the header
        public static string[][] ReadCSV(string fileName, bool hasHeader)
        {
            List<string[]> dataList = new List<string[]>();
            using (TextFieldParser parser = new TextFieldParser(fileName))
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
        // Reads in a network saved to file
        public static Neural_Network RememberNetwork(string fileName)
        {
            string[][] allData = ReadCSV(fileName, false);     // read in data

            // Set Layer Sizes
            int[] LayerSizes = new int[allData[0].Length];
            for (int i = 0; i < allData[0].Length; i++)
            {
                string[] thisLayerSize = allData[0];
                LayerSizes[i] = (int)Convert.ToDouble(thisLayerSize[i]);
            }
            // Set Opponent Data Info
            bool[] UseOpponent = new bool[allData[1].Length];
            for (int i = 0; i < allData[1].Length; i++)
            {
                string[] thisUseOpponent = allData[1];
                UseOpponent[i] = (thisUseOpponent[i] == "True") ? true : false;
            }
            // Set Offense Data Info
            bool[] UseOffense = new bool[allData[2].Length];
            for (int i = 0; i < allData[2].Length; i++)
            {
                string[] thisUseOffense = allData[2];
                UseOffense[i] = (thisUseOffense[i] == "True") ? true : false;
            }
            // Set Input Info
            int[] InputStats = new int[allData[3].Length];
            for (int i = 0; i < allData[3].Length; i++)
            {
                string[] thisInputStats = allData[3];
                InputStats[i] = (int)Convert.ToDouble(thisInputStats[i]);
            }
            // Set Output Info
            int[] OutputStats = new int[allData[4].Length];
            for (int i = 0; i < allData[4].Length; i++)
            {
                string[] thisOutputStats = allData[4];
                OutputStats[i] = (int)Convert.ToDouble(thisOutputStats[i]);
            }

            // Set network activation function and initialize network to 0
            int actFunction = (int)Convert.ToDouble(allData[5][0]);
            Neural_Network outputNetwork = new Neural_Network(LayerSizes, InputStats, OutputStats, UseOpponent, UseOffense, actFunction, 0);

            // Use data to set weights within network
            int currLayer = 0, currNode = 0;
            for (int i = 5; i < allData.Length; i++)             // the number of lines
            {
                for (int j = 0; j < allData[i].Length; j++)      // the number of weights in this node
                {
                    string[] weights = allData[i];
                    double weight = Convert.ToDouble(weights[j]);
                    outputNetwork.Layers[currLayer].Nodes[currNode].Weights[j] = weight;    // set weight
                }
                currNode++;
                if (currNode == outputNetwork.Layers[currLayer].Nodes.Length)
                {
                    currNode = 0;
                    currLayer++;
                }
            }

            return outputNetwork;
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
        // Returns the pythagorean expectation given RS and RA
        public static double GetPythagExp(double RS, double RA)
        {
            return Math.Pow(RS, PY_EXP) / (Math.Pow(RA, PY_EXP) + Math.Pow(RS, PY_EXP));
        }

        //
        // Returns true if this game is acceptable for use
        public static bool UseGame(Game G)
        {
            if (G.Home.Conference.Division == "FCS" || G.Visitor.Conference.Division == "FCS")
                return false;
            return true;
        }
    }
}
