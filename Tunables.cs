////////////////////////////////////////////////////////////////////////////////
//
// Class: TUNABLES
//  Contains global constants that act as tunable values, usually for the
//  search or neural network algorithms.
//
// Author:          Dylan Eustice
// Date Created:    9/3/2014
// Last Edited:     9/3/2014
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
        // Activation function
        public const int USE_ACT = LINEAR;

        // Neural network ranges
        public const double MAX_WEIGHT = 1;            // max initial weight of neural network synapse
        public const double MIN_WEIGHT = -1;           // min initial weight of neural network synapse
        public const double RESET_WEIGHT = 0.1;        // factor by which max weights are increased every reset

        // Pythagorean expectaton power
        public const double PY_EXP = 2.37;

        // Activation funciton scales
        public const double HYP_SCALE = 3;
        public const double LINEAR_SCALE = 1;
        public const double EXP_SCALE = 4;

        // Particle swarm parameters
        public const double MAX_MOVEMENT = 0;
        public const double MIN_MOVEMENT = 0.001;
        public const double MOMENTUM = 0.95;
        public const double MOM_GLOBAL = 0.2;
        public const double MOM_PERSONAL = 0.2;
        public const int ITERATIONS = 1000;
    }
}
