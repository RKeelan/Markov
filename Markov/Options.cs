using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace Markov
{
	class Options
	{
		[Option('s', "seed", Required = false, HelpText = "The seed to use. If ommited a seed will be generated from the curren time.")]
		public int? Seed { get; set; }

		[Option('o', "order", Required = false, HelpText = "The order of the markov chains to generate")]
		public int Order { get; set; }

		[Option('n', "names", Required = true, HelpText = "The number of names to generate.")]
		public int NamesToGenerate { get; set; }

		[Option('t', "training", Required = true, HelpText = "Training file, each name on a new line.")]
		public string TrainingFile { get; set; }

		[Option('l', "laplace", Required = false, HelpText = "Use Laplace smoothing. False if omitted.")]
		public bool UseLaplaceSmoothing { get; set; }

		[Option('b', "base", Required = false, HelpText = "The base probability of a token.")]
		public int? BaseProbabilty { get; set; }

		[Option('p', "probability", Required = false, HelpText = "The probability step size when incrementing.")]
		public int? ProbabilityStep { get; set; }

		public Options()
		{
			Order = 3;
		}

		[HelpOption]
		public string GetUsage()
		{
			return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
		}
	}
}
