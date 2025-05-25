using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithmExample
{
    class Program
    {
        static int PopulationSize = 9;
        static int ChromosomesPerParameter = 5;
        static int Iterations = 20;
        static double MutationRate = 0.1;
        static int TournamentSize = 2;

        const double MinValue = 0;
        const double MaxValue = 100;

        static Random random = new Random();

        class IndividualFitness
        {
            public int[] Individual { get; set; }
            public double Fitness { get; set; }
        }

        static void Main(string[] args)
        {
            ShowMenu();

            var population = InitializePopulation();

            for (int iteration = 0; iteration < Iterations; iteration++)
            {
                var evaluatedPopulation = population.Select(ind => new IndividualFitness
                {
                    Individual = ind,
                    Fitness = EvaluateFitness(ind)
                }).ToList();

                var bestFitness = evaluatedPopulation.Max(ind => ind.Fitness);
                var averageFitness = evaluatedPopulation.Average(ind => ind.Fitness);
                Console.WriteLine($"Iteracja {iteration + 1}: Najlepsze przystosowanie = {bestFitness}, Średnie przystosowanie = {averageFitness}");

                var newPopulation = new int[PopulationSize][];
                for (int i = 0; i < PopulationSize - 1; i++)
                {
                    var selectedIndividual = TournamentSelection(evaluatedPopulation);
                    newPopulation[i] = Mutate(selectedIndividual);
                }

                var bestIndividual = evaluatedPopulation.OrderByDescending(ind => ind.Fitness).First().Individual;
                newPopulation[PopulationSize - 1] = bestIndividual;

                population = newPopulation;
            }

            var finalBestIndividual = population.OrderByDescending(ind => EvaluateFitness(ind)).First();
            var (x1, x2) = DecodeIndividual(finalBestIndividual);
            Console.WriteLine($"Najlepsze rozwiązanie: x1 = {x1}, x2 = {x2}, Wartość funkcji = {EvaluateFitness(finalBestIndividual)}");
        }

        static void ShowMenu()
        {
            Console.WriteLine("--- Konfiguracja algorytmu genetycznego ---");

            PopulationSize = ReadInt("Liczba osobników w populacji (nieparzysta)", 9);
            if (PopulationSize % 2 == 0) PopulationSize++;

            ChromosomesPerParameter = ReadInt("Liczba chromosomów na parametr (>= 3)", 5, 3);
            Iterations = ReadInt("Liczba iteracji", 20);
            MutationRate = ReadDouble("Prawdopodobieństwo mutacji (0.0 - 1.0)", 0.1, 0.0, 1.0);
            TournamentSize = ReadInt("Rozmiar turnieju (2 - 20% populacji)", 2, 2, Math.Max(2, PopulationSize / 5));
        }

        static int ReadInt(string prompt, int defaultValue, int min = int.MinValue, int max = int.MaxValue)
        {
            Console.Write($"{prompt} [domyślnie: {defaultValue}]: ");
            string input = Console.ReadLine();
            if (int.TryParse(input, out int value) && value >= min && value <= max)
                return value;
            return defaultValue;
        }

        static double ReadDouble(string prompt, double defaultValue, double min = double.MinValue, double max = double.MaxValue)
        {
            Console.Write($"{prompt} [domyślnie: {defaultValue}]: ");
            string input = Console.ReadLine();
            if (double.TryParse(input, out double value) && value >= min && value <= max)
                return value;
            return defaultValue;
        }

        static int[][] InitializePopulation()
        {
            var population = new int[PopulationSize][];
            for (int i = 0; i < PopulationSize; i++)
            {
                population[i] = new int[ChromosomesPerParameter * 2];
                for (int j = 0; j < ChromosomesPerParameter * 2; j++)
                {
                    population[i][j] = random.Next(2);
                }
            }
            return population;
        }

        static double EvaluateFitness(int[] individual)
        {
            var (x1, x2) = DecodeIndividual(individual);
            return Math.Sin(x1 * 0.05) + Math.Sin(x2 * 0.05) + 0.4 * Math.Sin(x1 * 0.15) * Math.Sin(x2 * 0.15);
        }

        static (double x1, double x2) DecodeIndividual(int[] individual)
        {
            double x1 = DecodeChromosome(individual.Take(ChromosomesPerParameter).ToArray());
            double x2 = DecodeChromosome(individual.Skip(ChromosomesPerParameter).ToArray());
            return (x1, x2);
        }

        static double DecodeChromosome(int[] chromosome)
        {
            int value = 0;
            for (int i = 0; i < chromosome.Length; i++)
            {
                value += chromosome[i] * (int)Math.Pow(2, i);
            }
            return MinValue + (MaxValue - MinValue) * value / (Math.Pow(2, chromosome.Length) - 1);
        }

        static int[] TournamentSelection(List<IndividualFitness> evaluatedPopulation)
        {
            var tournament = evaluatedPopulation.OrderBy(x => random.Next()).Take(TournamentSize).ToList();
            return tournament.OrderByDescending(ind => ind.Fitness).First().Individual;
        }

        static int[] Mutate(int[] individual)
        {
            var mutatedIndividual = individual.ToArray();
            if (random.NextDouble() < MutationRate)
            {
                int mutationPoint = random.Next(mutatedIndividual.Length);
                mutatedIndividual[mutationPoint] = 1 - mutatedIndividual[mutationPoint];
            }
            return mutatedIndividual;
        }
    }
}
