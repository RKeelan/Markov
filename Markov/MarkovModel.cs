using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markov
{
	public class MarkovModel<T>
	{
		#region Enums
		#endregion

		#region Constants
		private const int REP_MULTIPLIER = 10;
		private const char TERMINATOR = '\0';
		private static readonly char[] DEFAULT_CHARS = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
								'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', TERMINATOR };

		#endregion

		#region Member Variables
		private int _order;
		private Random _random;
		private bool _useLaplaceSmoothing;
		private Dictionary<T, MarkovNode<T>> _model;
		private SortedSet<T> _tokenSpace;
		#endregion

		#region Constructors and Finalizers
		public MarkovModel(int seed, int order, bool useLaplaceSmoothing)
		{
			_order = order;
			_random = new Random(seed);
			_useLaplaceSmoothing = useLaplaceSmoothing;
			_model = new Dictionary<T, MarkovNode<T>>();
			_tokenSpace = new SortedSet<T>();

			BaseProbability = 0;
			StepProbability = 1;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Base Probabilty is the probability of a chain that's never been seen before. Either it's not a good idea, or I
		/// implemented Laplace Smoothing too clumsily. In Any case, it's a good idea to leave these at the default values.
		/// </summary>
		public int BaseProbability { get; set; }
		public int StepProbability { get; set; }
		#endregion

		#region Methods
		#region Model Training
		// RK 10-Mar-2016: [RK TODO] Change this to List<T> and add a terminator to the end.
		//public void Train(T[] input)
		public void Train(List<T> input)
		{
			if (input == null || input.Count == 0) {
				return;
			}

			if (!input[input.Count-1].Equals(default(T))) {
				// Add the implicit terminator
				input.Add(default(T));
			}

			// First Order means the next element is a function of the current.
			// Second Order means the next element is a function of the current and previous.
			// And so on...
			// So the chain has to be of size _order + 1, because the nth element is a function of the o previous
			// elements
			int chainLength = _order + 1;
			// Console.WriteLine("Input Length: "+input.Length); // DBG

			for (int i = 0; i < input.Count; i++) {
				if(!_tokenSpace.Contains(input[i])) {
					_tokenSpace.Add(input[i]);
				}

				// Add tokens to the chain in reverse order, so that the top of the stack is the first toekn in the chain
				var chain = new Stack<T>();
				//Console.Write((i - j)+": "); // DBG
				for (int j = 0; j < chainLength; j++) {
					if (i - j < 0) {
						//Console.WriteLine("Default"); // DBG
						chain.Push(default(T));
					}
					else {
						//Console.WriteLine(input[i - j + 1]); // DBG
						chain.Push(input[i - j]);
					}
				}

				//Console.WriteLine("Chain: "+  ChainToString(chain)); // DBG
				UpdateModel(chain, _model);
			}

			//WriteModel(Console.Out, new List<T>(), _model); // DBG
		}

		private void UpdateModel(Stack<T> chain, Dictionary<T, MarkovNode<T>> model)
		{
			T token = chain.Pop();

			if (chain.Count == 0) {
				// We've reached a leaf node
				if (!model.ContainsKey(token)) {
					// This token has never been seen, so add it
					model.Add(token, new MarkovNode<T>(BaseProbability, StepProbability));
				}

				// This token has already been seen, so just increment it
				model[token].Increment();
			}
			else {
				// Otherwise, we're still traversing the tree
				if (!model.ContainsKey(token)) {
					// This token has never been seen, so add it, then continue traversal
					model.Add(token, new MarkovNode<T>());
				}

				// Recurse to the next level
				UpdateModel(chain, model[token].Children);
			}
		}
		#endregion

		#region Serialization
		private void WriteModel(TextWriter writer, List<T> chain, Dictionary<T, MarkovNode<T>> model)
		{
			foreach (T key in model.Keys) {
				MarkovNode<T> node = model[key];
				List<T> newChain = chain.ToList();
				newChain.Add(key);

				if (node.IsLeaf) {
					writer.WriteLine(String.Format("{0}: {1}", ChainToString(newChain), node.Probability));
				}
				else {
					WriteModel(writer, newChain, node.Children);
				}
			}
		}
		#endregion

		#region Sequence Generation
		public List<T> GenerateSequence()
		{
			List<T> sequence = new List<T>();

			for(int i = 0;;i++) {
				var chain = new List<T>();
				for (int j = 0; j < _order; j++) {
					//Console.Write((i + j)+": "); // DBG
					if (i + j < _order) {
						//Console.WriteLine("Default"); // DBG
						chain.Add(default(T));
					}
					else {
						//Console.WriteLine(sequence[i + j - _order]); // DBG
						chain.Add(sequence[i + j - _order]);
					}
				}

				//Console.WriteLine("Chain: "+  ChainToString(chain)); // DBG
				T token = GenerateToken(chain);
				if (token.Equals(default(T))) {
					break;
				}
				else {
					sequence.Add(token);
				}
			}

			return sequence;
		}

		private T GenerateToken(List<T> chain)
		{
			if (chain.Count != _order) {
				throw new ArgumentException(String.Format("Expected a chain of {0} elements, not {1}", _order, chain.Count));
			}

			Dictionary<T, MarkovNode<T>> model = _model;
			foreach (T token in chain) {
				// This for loop traverses the tree from root to node.
				if (model.ContainsKey(token)) {
					model = model[token].Children;
				}
				else {
					if (_useLaplaceSmoothing) {
						// In the case of Laplace Smoothing, every chain has an implicit small probability, so add it.
						model.Add(token, new MarkovNode<T>());
						model = model[token].Children;
					}
					else {
						// If we find a token that has never been found to follow the preceeding token, return the default
						// token, which acts as terminator.
						return default(T);
					}
				}
			}

			int[] probabilityDistribution = _tokenSpace.Select(k =>
			{
				if (model.ContainsKey(k)) {
					if (!model[k].IsLeaf) {
						throw new InvalidOperationException("Expected a leaf node: " + ChainToString(chain));
					}
					return model[k].Probability;
				}
				else {
					if (_useLaplaceSmoothing) {
						return BaseProbability;
					}
					else {
						return 0;
					}
				}
				
			}).ToArray();

			int total = probabilityDistribution.Sum();
			int point = _random.Next(total);
			int subtotal = 0;
			int index = 0;

			for (index = 0; index < _tokenSpace.Count; index++) {
				if (subtotal > point) {
					break;
				}
				else {
					subtotal += probabilityDistribution[index];
				}
			}

			return _tokenSpace.ElementAt(index-1);
		}
		#endregion

		#region Helpers
		private String ChainToString(IEnumerable<T> chain)
		{
			return String.Join("->", chain);
		}
		#endregion
		#endregion
	}
}
