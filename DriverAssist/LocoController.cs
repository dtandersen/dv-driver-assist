using System;
using System.Collections.Generic;

namespace DriverAssist
{
    public interface LocoController
    {
        float Throttle { get; set; }
        float TrainBrake { get; set; }
        float IndBrake { get; set; }
        float Reverser { get; set; }
        float Speed { get; }
        /// Speed relative to the Reverser (1=Speed, 0=-Speed)
        float RelativeSpeed { get; }
        float Temperature { get; }
        float Torque { get; }
        float Acceleration { get; }
        /// Acceleration relative to the Reverser (1=Speed, 0=-Speed)
        float RelativeAcceleration { get; }
        float Amps { get; }
        float Rpm { get; }
        float AmpsRoc { get; }
        float AverageAmps { get; }
        bool IsElectric { get; }
        bool IsForward { get; }
        bool IsReversing { get; }
    }

    public class LocoType
    {
        public const string DE2 = "LocoShunter";
        public const string DH4 = "LocoDH2";
        public const string DE6 = "LocoDiesel";
        public const string DM3 = "LocoDM1";
        public const string STEAM = "LocoSteamHeavy";
    }

    internal class Integrator
    {
        private List<Node> nodes;
        private int index;
        private int size;

        internal Integrator(int size = 60)
        {
            this.size = size;
            this.nodes = new List<Node>(size);
            this.index = 0;
        }

        internal void Add(float value, float time)
        {
            if (nodes.Count < size)
            {
                nodes.Add(new Node(value, time));
            }
            else
            {
                nodes[index % nodes.Count] = new Node(value, time);
                index++;
            }
        }

        internal float Average()
        {
            float sum = 0;

            foreach (Node node in nodes)
            {
                sum += node.value;
            }

            return sum / nodes.Count;
        }

        internal float Integrate()
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

        internal class Node
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
