using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markov
{
	public class MarkovNode<T>
	{
		#region Member Variables
		public int _initialProbability = 1;
		public int _stepProbability = 1000;
		#endregion

		#region Constructors and Finalizers
		// RK 10-Mar-2016: [RK TODO] This is already looking ugly. There are in fact three type of nodes:
		// 1. leaf nodes, which have a probability and no children.
		// 2. branch nodes, which have children, and a probability that doesn't matter
		// 2. Parents of Leaf nodes, which have children, and may or may have been smoothed, which means having leaf nodes
		//		created for every known token, but left with the base probabilty.
		// I'm not exactly sure what the solution is. My first impulse is a class hierarch, but I don't think that's right.
		

		// Constuctor for a Leaf Node
		public MarkovNode(int initialProbability, int stepProbability)
		{
			_initialProbability = initialProbability;
			_stepProbability = stepProbability;
		}

		// Constructor for a branch node.
		public MarkovNode()
		{
			Children = new Dictionary<T, MarkovNode<T>>();
		}
		#endregion

		#region Properties
		public bool IsLeaf { get { return Children == null; } }

		#region Leaf Properties
		public int Probability { get; private set; }
		#endregion

		#region Branch Properties
		public Dictionary<T, MarkovNode<T>> Children { get; private set; }
		#endregion
		#endregion

		#region Methods
		public void Increment()
		{
			Probability += _stepProbability;
		}
		#endregion
	}
}
