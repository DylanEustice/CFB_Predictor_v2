////////////////////////////////////////////////////////////////////////////////
//
// Class: PARTICLE
//  One particle in a swarm used to find optimal neural network
//
// Author:          Dylan Eustice
// Date Created:    9/24/2014
// Last Edited:     9/24/2014
//
////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFB_Predictor_v2
{
    public class Particle
    {
        public double ParticleFitness;
        public double PersonalBest = double.MaxValue;
        public Neural_Network bestNetwork;
        public Neural_Network currNetwork;
        public Neural_Network stepNetwork;
        public double AverageMovement = double.MaxValue;

        //
        // Constructor
        public Particle(int[] layerInfo, int[] inputStats, int[] outputStats, bool[] inUseOpponent, bool[] inUseOffense, int actFunction, Random random)
        {
            currNetwork = new Neural_Network(layerInfo, inputStats, outputStats, inUseOpponent, inUseOffense, actFunction, random, 0);
            bestNetwork = currNetwork.SetWeights();
            stepNetwork = new Neural_Network(layerInfo, inputStats, outputStats, inUseOpponent, inUseOffense, actFunction, 0);
        }

        //
        // Adjusts the weights of the particle
        public void MoveParticle(double momentum, double globalWeight, double personalWeight, Neural_Network globalBestNetwork)
        {
            // Apply momentum to the last step
            Neural_Network step;
            step = stepNetwork.MultiplyConstant(momentum);

            // Add difference between current weights and group best
            Neural_Network tmpG = globalBestNetwork.SubtractNetworks(currNetwork);
            tmpG.MultiplyConstant(globalWeight);
            step.AddNetwork(tmpG);

            // Add difference between current weights and personal best
            Neural_Network tmpP = bestNetwork.SubtractNetworks(currNetwork);
            tmpP.MultiplyConstant(globalWeight);
            step.AddNetwork(tmpP);

            // Keep movement small
            if (Program.MAX_MOVEMENT > 0)
                while (step.AverageWeights() > Program.MAX_MOVEMENT)
                    step = step.MultiplyConstant(0.5);

            // Add to the current weights and set as last step
            currNetwork.AddNetwork(step);
            stepNetwork = step.SetWeights();

            // Find average movement in this step
            AverageMovement = stepNetwork.AverageWeights();
        }

        //
        // Find fitness of this particle for all training data
        //  - This is the summed squared error
        public double GetFitness(Game[] Games)
        {
            // Get error
            double error = 0;
            foreach (Game G in Games)
            {
                double homePts = G.PredictTeamPoints(currNetwork, true);
                double visitPts = G.PredictTeamPoints(currNetwork, false);
                if (double.IsNaN(homePts))
                {
                    Console.WriteLine("STOP");
                    G.PredictTeamPoints(currNetwork, true);
                }
                if (double.IsNaN(visitPts))
                {
                    Console.WriteLine("STOP");
                    G.PredictTeamPoints(currNetwork, false);
                }
                error += Math.Abs((homePts - G.HomeData[Program.POINTS]) * (homePts - G.HomeData[Program.POINTS]));
                error += Math.Abs((visitPts - G.VisitorData[Program.POINTS]) * (visitPts - G.VisitorData[Program.POINTS]));
            }
            ParticleFitness = error;

            // Check for personal best
            if (ParticleFitness < PersonalBest)
            {
                PersonalBest = ParticleFitness;
                bestNetwork = currNetwork.SetWeights();
            }

            return ParticleFitness;
        }
    }
}
