using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace Graphs
{
    public static class WorldTravel
    {
        public static int[] Heuristic(int[,] adjacencyMatrix, int totalVertices, int startVertex)
        {
            // Iniciazaliza e comeca o timer
            var timer = new Stopwatch();
            timer.Start();
            
            // Declara e incializa as variavies
            List<int> result = new();
            List<int> visited = new();
            
            // Adiciona o vertice inicial na resposta e marca ele como visitado
            result.Add(startVertex);
            visited.Add(startVertex);
            
            // Declara o vertice atual como o incial
            int currentNode = startVertex;
            
            // Inicializa o algotimo o quadrado de vezes totais, para garantir que todos serao visitados
            for(int i = 0; i < totalVertices*totalVertices; i++)
            {
                // Inicializa o menor caminho com o valor maximo de uma int
                var shortestPath = Int32.MaxValue;

                // Declara o proximo vertice como -1
                var neighbourNode = -1;

                // Para cada outro vertice na matriz
                for (var j = 0; j < adjacencyMatrix.GetLength(1); j++)
                {
                    // Se o proximo ja foi visitado, se o caminho nao existe ou se o caminho for maior, passe para o proximo
                    if (visited.Contains(j) || adjacencyMatrix[currentNode, j] == 0 || adjacencyMatrix[currentNode, j] >= shortestPath) continue;

                    // Se ele nao for visitado ou e se o caminho for igual, entao declare ele como o vizinho mais proximo
                    neighbourNode = j;
                    shortestPath = adjacencyMatrix[currentNode, j];
                }

                // Se foi achado um vizinho mais proximo
                if (neighbourNode != -1)
                {
                    // Adiciona o vizinho na resposta e marca como visitado
                    result.Add(neighbourNode);
                    visited.Add(neighbourNode);
                    
                    // Passa para o proximo vertice
                    currentNode = neighbourNode;
                }
                else
                {
                    // Se nao achar o vizinho mais proximo, quebre o loop
                    break;
                }
            }
            
            // Para o timer e imprime no terminal o tempo
            timer.Stop();
            Console.WriteLine($"Volta ao mundo, passando por {result.Count} vertices : {timer.Elapsed.ToString(@"ss\.ffffff")}s");
            
            // Retorna o resultado em forma de array
            return result.ToArray();
        }

        // Volta ao mundo por forca bruta
        public static (int, List<Flight>) BruteForce(int[,] adjacencyMatrix, int totalVertices)
        {
            // Iniciazaliza e comeca o timer
            var timer = new Stopwatch();
            timer.Start();
            
            // Inicizaliza as tabelas hash
            Dictionary<int, int[]> possiblePaths = new();
            Dictionary<int, List<Flight>> possibleResults = new();

            // Para cada vertice da matriz de adjacencia
            for (int i = 0; i < totalVertices; i++)
            {
                // Inicializa o preco do caminho atual como 0
                var currentPrice = 0;
                // Calcula o vizinho mais proximo a partir do vertice atual
                int[] currentPath = Heuristic(adjacencyMatrix, totalVertices, i);
                
                // Inicializa a lista de voos
                var currentFlights = new List<Flight>();
                
                // Passa por cada vertice do caminho
                for (var j = 0; j < currentPath.Length; j++)
                {
                    // Pega o aeropoto do vertice atual
                    var currentAirport = GraphUtils.Airports[GraphUtils.ReverseAirportIndex[currentPath[j]]];

                    // Se nao existir o proximo aeroporto, continue
                    if (j + 1 >= currentPath.Length) continue;
                    
                    // Pega o proximo aeropoto
                    var nextAirport = GraphUtils.Airports[GraphUtils.ReverseAirportIndex[currentPath[j + 1]]];
                    
                    // Procura o voo que conceta o aeroporto atual com o proximo
                    var flight = GraphUtils.Flights.First(value =>
                        (value.Start.Name == currentAirport.Name && value.End.Name == nextAirport.Name) ||
                        value.End.Name == currentAirport.Name && value.Start.Name == nextAirport.Name);

                    // Calcula o valor e adiciona na lista de voos
                    currentPrice += flight.Price;
                    currentFlights.Add(flight);
                }

                // Se o caminho visitou todos os vertice, adicione nos caminhos possiveis
                if (currentPath.Length == totalVertices)
                {
                    possiblePaths.TryAdd(currentPrice, currentPath);
                    possibleResults.TryAdd(currentPrice, currentFlights);
                }
                    
            }

            // Pega o caminho com o menor preco
            int minPrice = possiblePaths.Keys.Min();
            
            // Para o timer e imprime no terminal o tempo
            timer.Stop();
            Console.WriteLine($"{totalVertices} voltas ao mundo : {timer.Elapsed.ToString(@"ss\.ffffff")}s");
            
            // Retorna o preco e o caminho
            return (minPrice, possibleResults[minPrice]);
        }
    }
}