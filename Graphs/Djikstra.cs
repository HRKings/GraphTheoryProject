namespace Graphs
{
    // Djikstra implementation using adjacency matrix
    public class Djikstra
    {
        private static readonly int NO_PARENT = -1;
        
        public static int[] RunDijkstra(int[,] adjacencyMatrix, int startVertex, int target)
        {
            int nVertices = adjacencyMatrix.GetLength(0);
            
            int[] shortestDistances = new int[nVertices];
            bool[] added = new bool[nVertices];
            
            for (int vertexIndex = 0; vertexIndex < nVertices; vertexIndex++)
            {
                shortestDistances[vertexIndex] = int.MaxValue;
                added[vertexIndex] = false;
            }

            shortestDistances[startVertex] = 0;

            int[] parents = new int[nVertices];

            parents[startVertex] = NO_PARENT;

            for (int i = 1; i < nVertices; i++)
            {
                int nearestVertex = -1;
                int shortestDistance = int.MaxValue;
                for (int vertexIndex = 0; vertexIndex < nVertices; vertexIndex++)
                {
                    if (!added[vertexIndex] && shortestDistances[vertexIndex] < shortestDistance)
                    {
                        nearestVertex = vertexIndex;
                        shortestDistance = shortestDistances[vertexIndex];
                    }
                }
                
                added[nearestVertex] = true;
                
                for (int vertexIndex = 0; vertexIndex < nVertices; vertexIndex++)
                {
                    int edgeDistance = adjacencyMatrix[nearestVertex, vertexIndex];

                    if (edgeDistance > 0 && ((shortestDistance + edgeDistance) < shortestDistances[vertexIndex]))
                    {
                        parents[vertexIndex] = nearestVertex;
                        shortestDistances[vertexIndex] = shortestDistance + edgeDistance;
                    }
                }
                
                if(nearestVertex == target) break;
            }

            return parents;
        }
    }
}