using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Graphs
{
    public readonly struct Airport
    {
        public readonly int X;
        public readonly int Y;
        // Size of the marker in the map
        public readonly Rectangle Area;
        public readonly string Name;

        public Airport(int x, int y, Rectangle area, string name)
        {
            this.X = x;
            this.Y = y;
            this.Area = area;
            this.Name = name;
        }
    }

    public readonly struct Flight
    {
        public readonly Airport Start;
        public readonly Airport End;
        public readonly int Price;
        public readonly List<Flight> Intersections;

        public Flight(Airport start, Airport end, int distance)
        {
            this.Start = start;
            this.End = end;
            this.Price = distance;
            this.Intersections = new List<Flight>();
        }
    }

    public static class GraphUtils
    {
        public static Dictionary<string, Airport> Airports;

        // Indexes to search for the flight
        public static Dictionary<string, int> AirportIndex;
        public static Dictionary<int, string> ReverseAirportIndex;

        public static Flight[] Flights;

        public static int[,] DistanceGraph;
        public static int[,] PriceGraph;

        public static int StartAirport;
        public static int EndAirport;
       
        public static int[,] UsedGraph;

        public static List<(int, int)> DesiredPath;

        public static Dictionary<Flight, int> Heights;

        /// <summary>
        /// Finds the shortest path
        /// </summary>
        /// <param name="graph">The adjancency matrix to use</param>
        /// <param name="start">Index of the starting node</param>
        /// <param name="target">Index of the final node</param>
        /// <returns></returns>
        public static List<(int, int)> GetPath(int[,] graph, int start, int target)
        {
            // Start the timer
            var timer = new Stopwatch();
            timer.Start();
            
            int current = target;
            int[] allPaths;

            try
            {
                // Run djikstra
                allPaths = Djikstra.RunDijkstra(graph, start, target);
            }
            catch
            {
                // If it can't find the shortest path, returns the last one
                return DesiredPath;
            }

            List<(int, int)> result = new();

            // Run until reach the end
            while (current != -1)
            {
                // Add the flight between the current and the next
                result.Add((current, allPaths[current]));
                // Gets the next node
                current = allPaths[current];
            }
            
            // Stops the stopwatch and prints the time to the console
            timer.Stop();
            Console.WriteLine($"Dijkstra : {timer.Elapsed.ToString(@"ss\.ffffff")}s");

            // Return a list of nodes with the shortest path
            return result;
        }

        /// <summary>
        /// Read the airports files
        /// </summary>
        /// <param name="path">The file path</param>
        /// <param name="mapTexture">The map texture</param>
        public static void ReadAirports(string path, Texture2D mapTexture)
        {
            using var file = new StreamReader(path);

            // Reads the airport count
            int airportCount = int.Parse(file.ReadLine() ?? "0");

            Airports = new Dictionary<string, Airport>();
            AirportIndex = new Dictionary<string, int>();
            ReverseAirportIndex = new Dictionary<int, string>();

            for (var i = 0; i < airportCount; i++)
            {
                string line = file.ReadLine();

                // If the line is null or empty, go the the next one
                if (string.IsNullOrWhiteSpace(line))
                {
                    // Goes back one in the counter to keep track of the nodes correctly
                    i--;
                    continue;
                }

                string[] infos = line.Split(' ');

                // Converts the coordinates to pixels
                double x = mapTexture.Width * (double.Parse(infos[2]) + 180.0) / 360.0;
                double y = mapTexture.Height * (-double.Parse(infos[1]) + 90.0) / 180.0;

                // Add the airport to the hashtable
                Airports.TryAdd(infos[0],
                    new Airport((int) x, (int) y, new Rectangle((int) x - 10, (int) y - 10, 20, 20), infos[0]));
            }

            // Add each the aiport to the index
            for (int i = 0; i < Airports.Count; i++)
            {
                AirportIndex.Add(Airports.ElementAt(i).Key, i);
                ReverseAirportIndex.Add(i, Airports.ElementAt(i).Key);
            }

            // Init the adjacency matrix
            DistanceGraph = new int[AirportIndex.Count, AirportIndex.Count];
            PriceGraph = new int[AirportIndex.Count, AirportIndex.Count];

            file.Close();
        }

        /// <summary>
        /// Read the flights file
        /// </summary>
        /// <param name="path">The file path</param>
        public static void ReadFlights(string path)
        {
            using var file = new StreamReader(path);

            // Get the flight amount
            int flightCount = int.Parse(file.ReadLine() ?? "0");

            Flights = new Flight[flightCount];

            for (var i = 0; i < flightCount; i++)
            {
                string line = file.ReadLine();

                // If the line is null or empty, go the the next one
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] infos = line.Split(' ');

                Flights[i] = new Flight(Airports[infos[0]], Airports[infos[1]], int.Parse(infos[2]));

                // Calculate the distnace using the pythagoras theorem
                var distance = (int) Math.Sqrt(Math.Pow(Airports[infos[1]].X - Airports[infos[0]].X, 2) +
                                               Math.Pow(Airports[infos[1]].Y - Airports[infos[0]].Y, 2));
                
                // Add the flight to the adjacency matrix
                DistanceGraph[AirportIndex[infos[0]], AirportIndex[infos[1]]] = distance;
                DistanceGraph[AirportIndex[infos[1]], AirportIndex[infos[0]]] = distance;

                PriceGraph[AirportIndex[infos[0]], AirportIndex[infos[1]]] = Flights[i].Price;
                PriceGraph[AirportIndex[infos[1]], AirportIndex[infos[0]]] = Flights[i].Price;
            }
        }
        
        // Calculates the orientation using geometry
        private static int Orientation(Point p, Point q, Point r)
        {
            int val = (q.Y - p.Y) * (r.X - q.X) -
                      (q.X - p.X) * (r.Y - q.Y);

            if (val == 0) return 0; // Collinear 

            return (val > 0) ? 1 : 2; // Clock or counterclockwise 
        }
        
        // Verify if one line inteersects with other using geometry
        private static bool DoIntersect(Point p1, Point q1, Point p2, Point q2)
        {
            // Find the four orientations needed for general and 
            // special cases 
            int o1 = Orientation(p1, q1, p2);
            int o2 = Orientation(p1, q1, q2);
            int o3 = Orientation(p2, q2, p1);
            int o4 = Orientation(p2, q2, q1);

            // General case 
            return o1 != o2 && o3 != o4;
        }

        // Finds all intersections
        public static void ComputeIntersections()
        {
            // Statr the stopwatch
            var timer = new Stopwatch();
            timer.Start();
            
            Heights = new Dictionary<Flight, int>();

            for (var i = 0; i < Flights.Length; i++)
            {
                for (var j = 0; j < Flights.Length; j++)
                {
                    // If the flight is the same, jump to the next one
                    if (i == j) continue;
                    if (Flights[i].Start.X == Flights[j].Start.X && Flights[i].Start.Y == Flights[j].Start.Y) continue;
                    if (Flights[i].End.X == Flights[j].End.X && Flights[i].End.Y == Flights[j].End.Y) continue;

                    if (Flights[i].Start.X == Flights[j].End.X && Flights[i].Start.Y == Flights[j].End.Y) continue;
                    if (Flights[i].End.X == Flights[j].Start.X && Flights[i].End.Y == Flights[j].Start.Y) continue;

                    // If the flights doesn't intersect, go to the next one
                    if (Flights[i].Intersections.Contains(Flights[j]) ||
                        Flights[j].Intersections.Contains(Flights[i]) || !DoIntersect(
                            new Point(Flights[i].Start.X, Flights[i].Start.Y),
                            new Point(Flights[i].End.X, Flights[i].End.Y),
                            new Point(Flights[j].Start.X, Flights[j].Start.Y),
                            new Point(Flights[j].End.X, Flights[j].End.Y))) continue;

                    // If they do, add them to the lists
                    Heights.TryAdd(Flights[i], 0);
                    Heights.TryAdd(Flights[j], 0);
                    Flights[i].Intersections.Add(Flights[j]);
                    Flights[j].Intersections.Add(Flights[i]);
                }
            }

            // Order the flights by number of intersections ascending
            Flights = Flights.OrderBy(value => value.Intersections.Count).ToArray();

            for (var i = 0; i < Flights.Length; i++)
            {
                // If the flight doesn't have intersections, go to the next one
                if (Flights[i].Intersections.Count == 0) continue;
                if (!Heights.ContainsKey(Flights[i])) continue;

                // If the flight intersects with another of the same amount of intersections and is on the initial height
                bool needToChange = Flights[i].Intersections.Any(intersection =>
                    Heights[intersection] == 0 && intersection.Intersections.Count <= Flights[i].Intersections.Count);

                // If it needs to change, put in one altitude higher than the highest intersecting flight
                if (needToChange)
                    Heights[Flights[i]] =
                        Heights[Flights[i].Intersections.OrderByDescending(value => value.Intersections.Count).First()] + 1;
            }
            
            // Stop the stopwatch and prints the elapsed time
            timer.Stop();
            Console.WriteLine($"Correct altitudes: {timer.Elapsed.ToString(@"ss\.ffffff")}s");
        }

        public static void PrintTable(int[,] graph)
        {
            Console.Write("    ");
            for (var i = 0; i < ReverseAirportIndex.Count; i++)
            {
                Console.Write($"{ReverseAirportIndex[i]} ");
            }

            Console.WriteLine();

            for (var i = 0; i < graph.GetLength(1); i++)
            {
                Console.Write($"{ReverseAirportIndex[i]} ");
                for (var j = 0; j < graph.GetLength(1); j++)
                {
                    Console.Write($"{graph[i, j]:000} ");
                }

                Console.WriteLine();
            }
        }
    }
}