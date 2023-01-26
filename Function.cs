using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GP;

#region Handy Tree
//public enum Opertor
//{
//	Sum,
//	Sub,
//	Mul,
//	Div,
//	Pow,
//}
//public class Node
//{
//	public Opertor? Operator { get; set; } = null;
//	public double Value { get; set; } = 0;

//	public Node? Right { get; set; } = null;
//	public Node? Left { get; set; } = null;
//}
#endregion

public class Function
{
	public BinaryExpression Tree { get; set; }
	public Expression<Func<double, double>>? le = null;
	public double Fitness { get; set; } = -1;
	public double CrossoverRate { get; set; } = 0.7;

	public double MutationRate { get; set; } = 0.1;

	public double CalculateFitness(ParameterExpression parameter, double[] inputs, double[] outputs)
	{
		// calculate fitness function
		double diff = 0;

		for(int i = 0; i < inputs.Length; i++)
		{
			var functionOutput = CalculateTree(Tree, parameter, inputs[i]);

			if(functionOutput == double.NaN)
			{
				this.Fitness = double.MaxValue;
				return double.MaxValue;
			}
			diff += Math.Pow(outputs[i] - functionOutput, 2);
		}

		var fitness = diff / inputs.Length;

		Fitness = fitness;

		// return fitness
		return fitness;
	}

	public double CalculateTree(BinaryExpression tree, ParameterExpression parameter, double input)
	{
		if(le == null)
			le = Expression.Lambda<Func<double, double>>(tree, parameter);
		
		var result = le.Compile()(input);
		return result;
	}


	//private static void DFS(Node node, bool[] visit, StringBuilder stringBuilder)
	//{
	//	if(node.Operator == null)
	//	{
	//		stringBuilder.Append(node.Operator);
	//	}
	//	else
	//	{
	//		stringBuilder.Append(node.Value);
	//	}

	//	if(node.Left != null)
	//	{

	//	}
	//	else if(node.Right != null)
	//	{
	//		DFS(node.Right, visit, stringBuilder);
	//	}
	//}
}
