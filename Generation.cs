using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

// Deep Copy???
namespace GP
{
	public enum Opertor
	{
		Sum,
		Sub,
		Mul,
		Div,
		Pow,
	}
	public class Generation
	{
		public List<Function> Functions { get; set; } = new List<Function>();
		static public int Size { get; set; } = 250;
		static public double MutationRate { get; set; } = 0.2;

		static public Random random = new Random();
		static int MAX_DEPTH = 5;

		public static Function Crossover(Function parent1, Function parent2)
		{
			BinaryExpression left = parent1.Tree;

			var maxLeftLength = random.Next(2, MAX_DEPTH);
			left = GoDeep(parent1, left, maxLeftLength);

			BinaryExpression rigth = parent2.Tree;
			var maxRightLength = random.Next(2, MAX_DEPTH);
			rigth = GoDeep(parent2, rigth, maxRightLength);

			var rndOperator = random.Next(4);
			Opertor opertor = (Opertor)rndOperator;

			ExpressionType expressionType = ExpressionType.Add;
			switch(opertor)
			{
				case Opertor.Sum:
					expressionType = ExpressionType.Add;
					break;
				case Opertor.Sub:
					expressionType = ExpressionType.Subtract;
					break;
				case Opertor.Mul:
					expressionType = ExpressionType.Multiply;
					break;
				case Opertor.Div:
					expressionType = ExpressionType.Divide;
					break;
				//case Opertor.Pow:
				//	expressionType = ExpressionType.Power;
				//break;
				default:
					break;
			}

			var newTree = Expression.MakeBinary(expressionType, left, rigth);
			return new Function()
			{
				Tree = newTree,
				Fitness = 0,
			};
		}

		private static BinaryExpression GoDeep(Function function, BinaryExpression tree, int maxDepth)
		{
			var height = random.Next(maxDepth);
			for(int i = 0; i < height; i++)
			{
				if((tree.Left == null || tree.Left.NodeType == ExpressionType.Constant || tree.Left.NodeType == ExpressionType.Parameter)
					&& (tree.Right == null || tree.Right.NodeType == ExpressionType.Constant || tree.Right.NodeType == ExpressionType.Parameter))
				{
					break;
				}

				if((tree.Left == null || tree.Left.NodeType == ExpressionType.Constant || tree.Left.NodeType == ExpressionType.Parameter))
				{
					tree = (BinaryExpression)tree.Right;
					continue;
				}
				else if(tree.Right == null || tree.Right.NodeType == ExpressionType.Constant || tree.Right.NodeType == ExpressionType.Parameter)
				{
					tree = (BinaryExpression)tree.Left;
					continue;
				}

				int rnd = random.Next(2);
				if(rnd == 0)
				{
					// go left
					tree = (BinaryExpression)tree.Left;
				}
				else
				{
					// go right
					tree = (BinaryExpression)tree.Right;
				}
			}

			return tree;
		}

		public static void Mutate(Function func)
		{
			var tree = func.Tree;
			var maxDepth = random.Next(MAX_DEPTH);
			tree = GoDeep(func, tree, maxDepth);

			/// Not now: change the constant

			var mutationCase = random.Next(4);
			switch(mutationCase)
			{
				case 0:
					{
						// change some operator in middle
						var rndOperator = random.Next(4);
						Opertor opertor = (Opertor)rndOperator;

						tree = GoDeep(null, tree, 10);

						ExpressionType expressionType = ExpressionType.Add;
						switch(opertor)
						{
							case Opertor.Sum:
								expressionType = ExpressionType.Add;
								break;
							case Opertor.Sub:
								expressionType = ExpressionType.Subtract;
								break;
							case Opertor.Mul:
								expressionType = ExpressionType.Multiply;
								break;
							case Opertor.Div:
								expressionType = ExpressionType.Divide;
								break;
							default:
								break;
						}

						tree = Expression.MakeBinary(expressionType, tree.Left, tree.Right);
					}
					break;

				case 1:
					{
						// cut some of tree
						tree = GoDeep(null, tree, 5);
						double rnd = random.Next(-10, 10);
						tree = Expression.MakeBinary(ExpressionType.Add, tree.Left, Expression.Constant(rnd));

						//int rand = random.Next(3);
						//if(rand == 1 && !(tree.Left == null || tree.Left.NodeType == ExpressionType.Constant || tree.Left.NodeType == ExpressionType.Parameter))
						//{
						//	tree = GoDeep(new Function() { Tree = (BinaryExpression)tree.Left }, tree, 5);
						//}
					}
					break;

				case 2:
					{
						// cut some of tree
						tree = GoDeep(null, tree, 5);
						double rnd = random.Next(-10, 10);
						tree = Expression.MakeBinary(ExpressionType.Add, Expression.Constant(rnd), tree.Right);

						//int rand = random.Next(3);
						//if(rand == 1 && !(tree.Right == null || tree.Right.NodeType == ExpressionType.Constant || tree.Right.NodeType == ExpressionType.Parameter))
						//{
						//	tree = GoDeep(new Function() { Tree = (BinaryExpression)tree.Right }, tree, 5);
						//}
					}
					break;
				case 3:
					{
						// get to power
						double pow = random.Next(1, 4);
						tree = Expression.MakeBinary(ExpressionType.Power, tree, Expression.Constant(pow));
					}
					break;
				default:
					break;
			}
		}

	}
}
