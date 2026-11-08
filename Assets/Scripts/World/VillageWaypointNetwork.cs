using System;
using System.Collections.Generic;
using UnityEngine;

namespace FarmGame2D.World
{
    public sealed class VillageWaypointNetwork
    {
        private readonly Vector2[] points;
        private readonly int[][] links;

        public VillageWaypointNetwork(Vector2[] points, int[][] links)
        {
            this.points = points ?? throw new ArgumentNullException(nameof(points));
            this.links = links ?? throw new ArgumentNullException(nameof(links));
            if (points.Length == 0 || points.Length != links.Length)
            {
                throw new ArgumentException("Waypoint points and links must have the same non-zero length.");
            }
        }

        public int Count => points.Length;

        public Vector2 GetPoint(int index) => points[index];

        public int FindClosestNode(Vector2 position)
        {
            var bestIndex = 0;
            var bestDistance = float.MaxValue;
            for (var index = 0; index < points.Length; index++)
            {
                var distance = Vector2.SqrMagnitude(points[index] - position);
                if (distance >= bestDistance)
                {
                    continue;
                }

                bestDistance = distance;
                bestIndex = index;
            }

            return bestIndex;
        }

        public int GetRandomNeighbor(int node, int previousNode, System.Random random)
        {
            var neighbors = links[node];
            if (neighbors.Length == 0)
            {
                return node;
            }

            if (neighbors.Length == 1)
            {
                return neighbors[0];
            }

            var choice = neighbors[random.Next(neighbors.Length)];
            return choice == previousNode
                ? neighbors[(Array.IndexOf(neighbors, choice) + 1) % neighbors.Length]
                : choice;
        }

        public int GetNextNodeToward(int start, int goal)
        {
            if (start == goal)
            {
                return goal;
            }

            var previous = new int[points.Length];
            Array.Fill(previous, -1);
            var queue = new Queue<int>();
            queue.Enqueue(start);
            previous[start] = start;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var neighbor in links[current])
                {
                    if (previous[neighbor] >= 0)
                    {
                        continue;
                    }

                    previous[neighbor] = current;
                    if (neighbor == goal)
                    {
                        return TraceFirstStep(previous, start, goal);
                    }

                    queue.Enqueue(neighbor);
                }
            }

            return start;
        }

        private static int TraceFirstStep(IReadOnlyList<int> previous, int start, int goal)
        {
            var current = goal;
            while (previous[current] != start)
            {
                current = previous[current];
            }

            return current;
        }
    }
}
