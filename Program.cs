// See https://aka.ms/new-console-template for more information
using GP;
using System.Diagnostics;
using System.Linq.Expressions;

#region Temp
//ParameterExpression parameter = Expression.Parameter(typeof(int), "Input1");
//BinaryExpression binaryExpression = Expression.Multiply(parameter, Expression.Constant(10));

//binaryExpression = binaryExpression.Update(binaryExpression.Left, null, Expression.Subtract(Expression.Constant(2), binaryExpression.Right));

//// output the function in formatted way
//Console.WriteLine(binaryExpression.ToString());

//Expression<Func<int, int>> le = Expression.Lambda<Func<int, int>>(binaryExpression);

//// Compile the lambda expression.  
//// Execute the lambda expression.  
//var result = le.Compile().Invoke(3);

//Console.WriteLine(result);
#endregion

int n = int.Parse(Console.ReadLine());
double[] inputs = new double[n];
double[] outputs = new double[n];

for(int i = 0; i < n; i++)
{
	var input = Console.ReadLine().Split(' ');

	inputs[i] = double.Parse(input[0]);
	outputs[i] = double.Parse(input[1]);
}

// calculate time
var timer = new Stopwatch();
timer.Start();

List<Generation> generations = new List<Generation>();
ParameterExpression parameter = Expression.Parameter(typeof(double), "Input1");

// constants
double answerRate = 0.01;
double crossoverCount = 150;
double mutationCount = 100;

// make first generation
var firsGeneration = MakeFirstGeneration(parameter);
for(int i = 0; i < firsGeneration.Functions.Count; i++)
{
	firsGeneration.Functions[i].CalculateFitness(parameter, inputs, outputs);
}
generations.Add(firsGeneration);


Function ans = null;

int generationCount = 0;
for(; generationCount < 150; generationCount++)
{
	generations[generationCount].Functions = generations[generationCount].Functions
		.OrderBy(f => f.Fitness)
		.ToList();

	Console.WriteLine($"Generation No : {generationCount}");
	Console.WriteLine($"Best Fitness : {generations[generationCount].Functions[0].Fitness}");
	
	
	if(generations[generationCount].Functions[0].Fitness <= answerRate)
	{
		timer.Stop();
		ans = generations[generationCount].Functions[0];
		break;
	}

	var generatedGeneration = MakeNewGeneration(generations.Last());

	var newGeneration = new Generation();
	// calculate fitness for new generation
	for(int i = 0; i < generatedGeneration.Functions.Count; i++)
	{
		generatedGeneration.Functions[i].CalculateFitness(parameter, inputs, outputs);

		if(generatedGeneration.Functions[i].Fitness != double.NaN
		&& generatedGeneration.Functions[i].Fitness != double.NegativeInfinity
		&& generatedGeneration.Functions[i].Fitness != double.PositiveInfinity
		&& generatedGeneration.Functions[i].Fitness < 100000 * n
		&& generatedGeneration.Functions[i].Fitness >= 0)
		{
			newGeneration.Functions.Add(generatedGeneration.Functions[i]);
		}
	}
	generations.Add(newGeneration);
}


Console.WriteLine("=======================");
Console.WriteLine($"Generation No : {generationCount}");
Console.WriteLine($"Fitness : {ans.Fitness}");
Console.WriteLine($"Time elapsed : {timer.Elapsed.TotalMilliseconds} ms");
Console.WriteLine(ans.Tree.ToString());


Generation MakeNewGeneration(Generation generation)
{
	Generation newGeneration = new Generation();
	Random random = new Random();

	// Add functions form previous generation
	var fromPreviousGeneration = generation.Functions.Take(generation.Functions.Count / 10).ToList();
	newGeneration.Functions.AddRange(fromPreviousGeneration);

	// make new generation
	for(int i = 0; i < crossoverCount; i++)
	{
		// select parents
		int parent1Index = random.Next(generation.Functions.Count);
		int parent2Index = random.Next(generation.Functions.Count);
		var parent1 = generation.Functions[parent1Index];
		var parent2 = generation.Functions[parent2Index];

		// use created childs
		int rand = random.Next(2);
		if(newGeneration.Functions.Count != 0 && rand % 2 == 0)
		{
			rand = random.Next(newGeneration.Functions.Count);
			parent2 = newGeneration.Functions[rand];
		}

		// crossover
		var child = Generation.Crossover(parent1, parent2);

		// mutate
		double rnd = random.NextDouble();
		if(rnd < child.MutationRate)
		{
			Generation.Mutate(child);
		}

		// calculate fitness of child
		child.CalculateFitness(parameter, inputs, outputs);

		// add to new generation
		newGeneration.Functions.Add(child);
	}

	for(int i = 0; i < mutationCount; i++)
	{
		int parent1Index = random.Next(generation.Functions.Count);
		var parent1 = generation.Functions[parent1Index];

		int rand = random.Next(2);
		if(newGeneration.Functions.Count != 0 && rand % 2 == 0)
		{
			rand = random.Next(newGeneration.Functions.Count);
			parent1 = newGeneration.Functions[rand];
		}

		Generation.Mutate(parent1);
		parent1.CalculateFitness(parameter, inputs, outputs);
		newGeneration.Functions.Add(parent1);
	}

	return newGeneration;
}

Generation MakeFirstGeneration(ParameterExpression parameter)
{
	Random random = new Random();

	Generation generation = new Generation();

	List<Function> functions = new List<Function>();

	// constants c
	for(double i = -20; i < 20; i++)
	{
		functions.Add(new Function()
		{
			Tree = Expression.Add(Expression.Constant(i), Expression.Constant(0.0))
		});
	}

	// 1/c
	for(double i = -20; i < 20; i++)
	{
		if(i == 0)
		{
			continue;
		}

		functions.Add(new Function()
		{
			Tree = Expression.Add(Expression.Constant(1 / i), Expression.Constant(0.0))
		});
	}

	// ax + b
	for(int i = 0; i < 50; i++)
	{
		double a = random.Next(-20, 20);
		double b = random.Next(-20, 20);

		functions.Add(new Function()
		{
			Tree = Expression.Add(Expression.Multiply(Expression.Constant(a), parameter), Expression.Constant(b))
		});
	}


	// a*x^n + b
	for(double i = 1; i < 3; i++)
	{
		double a = random.Next(-20, 20);
		double b = random.Next(-20, 20);

		for(int j = 0; j < 30; j++)
		{
			functions.Add(new Function()
			{
				Tree = Expression.Add(Expression.Multiply(Expression.Power(parameter, Expression.Constant(i)), Expression.Constant(a)), Expression.Constant(b))
			});
		}
	}

	// x^n + x^n-1 + ...
	for(double pow = 1; pow < 3; pow++)
	{
		BinaryExpression binaryExpression = Expression.Power(parameter, Expression.Constant(pow));
		for(double j = 1; j < pow; j++)
		{
			binaryExpression.Update(binaryExpression, null, Expression.Add(binaryExpression, Expression.Power(parameter, Expression.Constant(pow - j))));
		}
	}

	// 1/ a*x^n
	for(double i = 1; i < 3; i++)
	{
		for(int j = 0; j < 40; j++)
		{
			double a = random.Next(-20, 20);
			functions.Add(new Function()
			{
				Tree = Expression.Divide(Expression.Constant(a), Expression.Power(parameter, Expression.Constant(i)))
			});
		}
	}

	generation.Functions.AddRange(functions);
	return generation;
}