using System.Collections.Generic;

namespace DriverAssist
{
    public interface LocoController
    {
        float Throttle { get; set; }
        float TrainBrake { get; set; }
        float IndBrake { get; set; }
        float Speed { get; }
        float PositiveSpeed { get; }
        float Temperature { get; }
        float Torque { get; }
        float Reverser { get; set; }
        float Acceleration { get; }
        float Amps { get; }
        float Rpm { get; }
    }

    internal class AccelerationMonitor
    {
        List<Node> nodes;
        int index;
        int size;

        internal AccelerationMonitor()
        {
            size = 60;
            nodes = new List<Node>(size);
            index = 0;
        }

        internal void Add(float speed, float deltaTime)
        {
            if (nodes.Count < size)
            {
                nodes.Add(new Node(speed, deltaTime));
            }
            else
            {
                nodes[index % nodes.Count] = new Node(speed, deltaTime);
                index++;
            }
        }

        internal float Sum()
        {
            float v = 0;
            float t = 0;

            foreach (Node node in nodes)
            {
                v += node.value;
                t += node.time;
            }

            return v * t;
        }

        class Node
        {
            public float value;
            public float time;

            public Node(float value, float time)
            {
                this.value = value;
                this.time = time;
            }
        }
    }
}

