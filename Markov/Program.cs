using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommandLine;
using Markov;

namespace NameGenerator
{
	class Program
	{
		static void Main(string[] args)
		{
			var options = new Options();
			if (!Parser.Default.ParseArguments(args, options)) {
				Console.WriteLine(options.GetUsage());
				return;
			}


			int seed = options.Seed ?? DateTime.Now.Millisecond;

			// Setup
			var mm = new MarkovModel<char>(seed, options.Order, options.UseLaplaceSmoothing);
			if (options.BaseProbabilty != null) {
				mm.BaseProbability = options.BaseProbabilty.Value;
			}

			if (options.ProbabilityStep != null) {
				mm.StepProbability = options.ProbabilityStep.Value;
			}

			// Train
			List<string> inputNames = GetTrainingDataFromFile(options.TrainingFile);
			List<List<char>> trainingData = inputNames.Select(n => n.ToCharArray().ToList()).ToList();
			foreach (var trainingInstance in trainingData) {
				mm.Train(trainingInstance);
			}

			// Generate
			int generatedNames = 0;
			while (generatedNames < options.NamesToGenerate) {
				List<char> sequence = mm.GenerateSequence();
				string name = new string(sequence.ToArray());
				if (inputNames.Contains(name)) {
					continue;
				}
				else {
					generatedNames++;
					Console.WriteLine("\t" + name);
				}
			}
		}

		private static List<string> GetTrainingDataFromFile(string trainingFile)
		{
			var trainingData = new List<string>();
			foreach (string line in File.ReadAllLines(trainingFile)) {
				var trimmedLine = line.Trim();
				trainingData.Add(trimmedLine);
			}

			return trainingData;
		}
	}
}
