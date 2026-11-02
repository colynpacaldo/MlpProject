using System;

namespace MlpProject
{
    /// <summary>
    /// A fully connected three-layer MLP consisting of an input layer, one hidden
    /// layer, and an output layer. The implementation uses sigmoid activations and
    /// is intentionally independent of external machine-learning libraries.
    /// </summary>
    public sealed class MlpNetwork
    {
        private readonly double[,] _inputToHiddenWeights;
        private readonly double[] _hiddenBiases;
        private readonly double[,] _hiddenToOutputWeights;
        private readonly double[] _outputBiases;
        private readonly double _learningRate;

        /// <summary>Gets the number of input features accepted by the network.</summary>
        public int InputCount { get; }

        /// <summary>Gets the number of neurons in the hidden layer.</summary>
        public int HiddenCount { get; }

        /// <summary>Gets the number of output neurons.</summary>
        public int OutputCount { get; }

        /// <summary>
        /// Creates a deterministic network with small random initial weights.
        /// </summary>
        /// <param name="inputCount">Number of input features.</param>
        /// <param name="hiddenCount">Number of hidden neurons.</param>
        /// <param name="outputCount">Number of output neurons.</param>
        /// <param name="learningRate">Gradient-descent learning rate.</param>
        /// <param name="randomSeed">Seed used for reproducible initialization.</param>
        public MlpNetwork(int inputCount, int hiddenCount, int outputCount, double learningRate, int randomSeed = 42)
        {
            if (inputCount <= 0) throw new ArgumentOutOfRangeException(nameof(inputCount));
            if (hiddenCount <= 0) throw new ArgumentOutOfRangeException(nameof(hiddenCount));
            if (outputCount <= 0) throw new ArgumentOutOfRangeException(nameof(outputCount));
            if (learningRate <= 0.0 || double.IsNaN(learningRate) || double.IsInfinity(learningRate))
            {
                throw new ArgumentOutOfRangeException(nameof(learningRate));
            }

            InputCount = inputCount;
            HiddenCount = hiddenCount;
            OutputCount = outputCount;
            _learningRate = learningRate;
            _inputToHiddenWeights = new double[inputCount, hiddenCount];
            _hiddenBiases = new double[hiddenCount];
            _hiddenToOutputWeights = new double[hiddenCount, outputCount];
            _outputBiases = new double[outputCount];

            var random = new Random(randomSeed);
            InitializeWeights(_inputToHiddenWeights, random, Math.Sqrt(6.0 / (inputCount + hiddenCount)));
            InitializeWeights(_hiddenToOutputWeights, random, Math.Sqrt(6.0 / (hiddenCount + outputCount)));
        }

        /// <summary>
        /// Trains the network on one sample and returns the sample's binary cross-entropy loss.
        /// </summary>
        /// <param name="features">Scaled input features.</param>
        /// <param name="targets">Target vector containing values in the range [0, 1].</param>
        public double TrainSample(double[] features, double[] targets)
        {
            ValidateVector(features, InputCount, nameof(features));
            ValidateVector(targets, OutputCount, nameof(targets));

            double[] hidden = new double[HiddenCount];
            double[] output = Forward(features, hidden);
            double[] outputErrors = new double[OutputCount];

            double loss = 0.0;
            for (int outputIndex = 0; outputIndex < OutputCount; outputIndex++)
            {
                double target = targets[outputIndex];
                if (target < 0.0 || target > 1.0)
                {
                    throw new ArgumentOutOfRangeException(nameof(targets), "Targets must be in the range [0, 1].");
                }

                double probability = ClampProbability(output[outputIndex]);
                loss -= target * Math.Log(probability) + (1.0 - target) * Math.Log(1.0 - probability);
                outputErrors[outputIndex] = output[outputIndex] - target;
            }

            var hiddenErrors = new double[HiddenCount];
            for (int hiddenIndex = 0; hiddenIndex < HiddenCount; hiddenIndex++)
            {
                double error = 0.0;
                for (int outputIndex = 0; outputIndex < OutputCount; outputIndex++)
                {
                    error += outputErrors[outputIndex] * _hiddenToOutputWeights[hiddenIndex, outputIndex];
                }

                hiddenErrors[hiddenIndex] = error * hidden[hiddenIndex] * (1.0 - hidden[hiddenIndex]);
            }

            for (int hiddenIndex = 0; hiddenIndex < HiddenCount; hiddenIndex++)
            {
                for (int outputIndex = 0; outputIndex < OutputCount; outputIndex++)
                {
                    _hiddenToOutputWeights[hiddenIndex, outputIndex] -=
                        _learningRate * outputErrors[outputIndex] * hidden[hiddenIndex];
                }
            }

            for (int outputIndex = 0; outputIndex < OutputCount; outputIndex++)
            {
                _outputBiases[outputIndex] -= _learningRate * outputErrors[outputIndex];
            }

            for (int inputIndex = 0; inputIndex < InputCount; inputIndex++)
            {
                for (int hiddenIndex = 0; hiddenIndex < HiddenCount; hiddenIndex++)
                {
                    _inputToHiddenWeights[inputIndex, hiddenIndex] -=
                        _learningRate * hiddenErrors[hiddenIndex] * features[inputIndex];
                }
            }

            for (int hiddenIndex = 0; hiddenIndex < HiddenCount; hiddenIndex++)
            {
                _hiddenBiases[hiddenIndex] -= _learningRate * hiddenErrors[hiddenIndex];
            }

            return loss / OutputCount;
        }

        /// <summary>
        /// Calculates the network output without changing weights.
        /// </summary>
        /// <param name="features">Scaled input features.</param>
        /// <returns>One probability per output neuron.</returns>
        public double[] Predict(double[] features)
        {
            ValidateVector(features, InputCount, nameof(features));
            return Forward(features, new double[HiddenCount]);
        }

        private double[] Forward(double[] features, double[] hidden)
        {
            for (int hiddenIndex = 0; hiddenIndex < HiddenCount; hiddenIndex++)
            {
                double weightedSum = _hiddenBiases[hiddenIndex];
                for (int inputIndex = 0; inputIndex < InputCount; inputIndex++)
                {
                    weightedSum += features[inputIndex] * _inputToHiddenWeights[inputIndex, hiddenIndex];
                }

                hidden[hiddenIndex] = Sigmoid(weightedSum);
            }

            var output = new double[OutputCount];
            for (int outputIndex = 0; outputIndex < OutputCount; outputIndex++)
            {
                double weightedSum = _outputBiases[outputIndex];
                for (int hiddenIndex = 0; hiddenIndex < HiddenCount; hiddenIndex++)
                {
                    weightedSum += hidden[hiddenIndex] * _hiddenToOutputWeights[hiddenIndex, outputIndex];
                }

                output[outputIndex] = Sigmoid(weightedSum);
            }

            return output;
        }

        private static void InitializeWeights(double[,] weights, Random random, double limit)
        {
            for (int row = 0; row < weights.GetLength(0); row++)
            {
                for (int column = 0; column < weights.GetLength(1); column++)
                {
                    weights[row, column] = (random.NextDouble() * 2.0 - 1.0) * limit;
                }
            }
        }

        private static double Sigmoid(double value)
        {
            if (value < -40.0) return 0.0;
            if (value > 40.0) return 1.0;
            return 1.0 / (1.0 + Math.Exp(-value));
        }

        private static double ClampProbability(double value)
        {
            return Math.Max(1e-12, Math.Min(1.0 - 1e-12, value));
        }

        private static void ValidateVector(double[] vector, int expectedLength, string parameterName)
        {
            if (vector == null) throw new ArgumentNullException(parameterName);
            if (vector.Length != expectedLength)
            {
                throw new ArgumentException(
                    $"Expected vector length {expectedLength}, but received {vector.Length}.", parameterName);
            }
        }
    }
}
