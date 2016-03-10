using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markov
{
	class MarkovGenerator
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
		private int[, ,] _probabilities;
		private List<char> _characters;
		private Dictionary<String, String> _input;
		private HashSet<String> _results;
		private Random _random;
		#endregion

		#region Constructors and Finalizers
		public MarkovGenerator(int seed)
		{
			_probabilities = new int[DEFAULT_CHARS.Length, DEFAULT_CHARS.Length, DEFAULT_CHARS.Length];

			_characters = new List<char>();
			_characters.AddRange(DEFAULT_CHARS);

			_results = new HashSet<String>();
			_input = new Dictionary<String, String>();

			_random = new Random(seed);
		}

		// NameGenerator created from save file
		public MarkovGenerator(FileInfo sourceFile)
		{
			// TODO write constructor to load from save file
		}
		#endregion

		#region Properties
		#endregion

		#region Methods
		// generates the probabilities matrix from $source
		public void GenerateProbabilityMatrix(string filePath)
		{
			String[] sourceNames = File.ReadAllLines(filePath);
			foreach (String name in sourceNames) {
				AddStringToProbability(name);
			}
		}

		public List<String> GenerateNames(int namesToGenerate)
		{
			int maxIterations = namesToGenerate * REP_MULTIPLIER;
			int iterations = 0;
			_results.Clear();
			while (iterations++ < maxIterations && namesToGenerate != 0) {
				String name = GenerateName();

				//Console.WriteLine("DBG: " + name);
				if (_input.ContainsKey(name) || _results.Contains(name)) {
					continue;
				}

				_results.Add(name);
				namesToGenerate--;
			}

			var names = _results.ToList();
			names.Sort();
			return names;
		}

		// writes the probability matrix to the output directory
		private void SaveProbabilities()
		{
			// TODO write save method
		}
		#region Helpers
		public String GenerateName()
		{
			var result = new StringBuilder();
			bool firstChar = true;
			char last1 = TERMINATOR, last2 = TERMINATOR;
			do {
				char temp = NextCharByLast(last1, last2);
				last1 = last2;
				last2 = temp;
				if (last2 != TERMINATOR) {
					if (firstChar) {
						result.Append(Char.ToUpper(last2));
						firstChar = false;
					}
					else {
						result.Append(last2);
					}
				}
			} while (last2 != TERMINATOR);

			return result.ToString();
		}

		// generates names until one that is not already in $results is found
		//private String GenerateNextUnique()
		//{
		//	String name = GenerateName();
		//	while (_results.ContainsKey(name))
		//		name = GenerateName();
		//	_results.Add(name, name);
		//	return name;
		//}

		// generates names until one that is not already in $results is found
		// stops once
		//private String GenerateNextUnique(int maxAttempts)
		//{
		//	String name = GenerateName();
		//	int count = 0;

		//	while (_results.ContainsKey(name) && count++ < maxAttempts) {
		//		name = GenerateName();
		//	}

		//	_results.Add(name, name);
		//	if (count < maxAttempts) {
		//		return name;
		//	}

		//	return null;
		//}

		// adds $c to the list of recognized characters
		// automatically re-generates the probability matrix
		// precondition: $c is not already in $characters
		private void AddCharacter(char c)
		{
			_characters.Add(c);
			ResetProbabilities();
		}

		// removes $c from the list of recognized characters
		// automatically re-generates the probability matrix
		// precondition: $c is in $characters
		private void RemoveCharacter(char c)
		{
			int index = -1;
			for (int i = 0; i < _characters.Count; i++)
				if (_characters[i] == c) {
					index = i;
				}

			_characters.RemoveAt(index);

			ResetProbabilities();
		}

		// returns true if the name generator recognizes $c
		private bool RecognizesCharacter(char c)
		{
			foreach (var character in _characters)
				if (character == c) {
					return true;
				}
			return false;
		}

		private void ResetProbabilities()
		{
			_probabilities = new int[_characters.Count, _characters.Count, _characters.Count];
			_results = new HashSet<String>();
		}

		private void AddStringToProbability(String name)
		{
			name = name.ToLower();
			name = name.Trim();

			_input.Add(name, name);
			char last1 = TERMINATOR;
			char last2 = TERMINATOR;
			int index = 0;

			while (index < name.Length) {
				if (_characters.IndexOf(name[index]) != -1) {
					char current = name[index];
					_probabilities[_characters.IndexOf(last1), _characters.IndexOf(last2), _characters.IndexOf(current)]++;
					last1 = last2;
					last2 = current;
					index++;
				}
				else {
					index++;
				}
			}

			_probabilities[_characters.IndexOf(last1), _characters.IndexOf(last2), _characters.IndexOf(TERMINATOR)]++;
		}

		// chooses a character from the probability matrix based on the previous two chars
		// precondition: $last1 and $last2 are recognized characters that have previously appeared in that order
		private char NextCharByLast(char last1, char last2)
		{
			int total = 0;
			for (int i = 0; i < _probabilities.GetLength(2); i++) {
				total += _probabilities[_characters.IndexOf(last1), _characters.IndexOf(last2), i];
			}


			total = _random.Next(total);
			int index = 0, subTotal = 0;
			do {
				subTotal += _probabilities[_characters.IndexOf(last1), _characters.IndexOf(last2), index++];
			} while (subTotal <= total);

			return (_characters[index - 1]);
		}
		#endregion
		#endregion
	}
}
