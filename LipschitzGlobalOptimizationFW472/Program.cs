using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LipschitzGlobalOptimizationFW472
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int Kmax = new int();
            float a = new float();
            float b = new float();
            float r = new float();
            while (true)
            {
                Kmax = 0;
                a = 0;
                b = 0;
                r = 0;
                Console.WriteLine("Type Kmax > 2");
                if (!int.TryParse(Console.ReadLine(), out Kmax))
                {
                    Console.Clear();
                    Console.WriteLine("Ошибка ввода. Попробуйте еще раз.");
                    continue;
                }
                Console.WriteLine("Type a");
                if (!float.TryParse(Console.ReadLine(), out a))
                {
                    Console.Clear();
                    Console.WriteLine("Ошибка ввода. Попробуйте еще раз.");
                    continue;
                }
                Console.WriteLine("Type b");
                if (!float.TryParse(Console.ReadLine(), out b))
                {
                    Console.Clear();
                    Console.WriteLine("Ошибка ввода. Попробуйте еще раз.");
                    continue;
                }
                Console.WriteLine("Type r > 1");
                if (!float.TryParse(Console.ReadLine(), out r))
                {
                    Console.Clear();
                    Console.WriteLine("Ошибка ввода. Попробуйте еще раз.");
                    continue;
                }
                break;
            }
            long BestTime = long.MaxValue;
            //for (int i = 0; i < 100; i++)
            //{
                var stopwatch = new System.Diagnostics.Stopwatch();
                var LGO = new LGO();
                stopwatch.Start();
                LGO.Optimize(Kmax, a, b, r);
                stopwatch.Stop();
                //if (stopwatch.ElapsedMilliseconds<BestTime) BestTime = stopwatch.ElapsedMilliseconds;
            //}
            Console.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");
            //Console.WriteLine($"Execution Time: {BestTime} ms");
            Console.WriteLine("END");
            Console.ReadKey();
        }
    }
    internal class LGO
    {
        private List<float> zUp = new List<float>();
        private List<float> xUp = new List<float>();

        private float Function(float x)
        {
            return x * x;
        }
        public void Optimize(int Kmax, float a, float b, float r)
        {
            FirstTests(a);
            FirstTests(b);
            int k = 2;
            var Delta = b - a;
            while (Delta > 0.0001f && k < Kmax)
            {
                Delta = SelectNextValues(r, Delta);
                k++;
            }
            int minIndex = 0;
            for (int i = 1; i < xUp.Count; i++)
            {
                if (zUp[i] < zUp[i - 1]) minIndex = i;
            }
            Console.WriteLine("Minimum is in x = " + xUp[minIndex] + " f(x) = " + zUp[minIndex]);
        }
        private float SelectNextValues(float r, float Delta)
        {
            var xDown = new List<float>(xUp);
            xDown.Sort();
            var m = Calculate_m(xDown.ToArray(), r);
            var Rs = new (float, int)[xDown.Count - 1];
            for (int i = 1; i < xDown.Count; i++)
            {
                Rs[i - 1] = (CalculateR(xDown[i - 1], xDown[i], m), i);
            }
            bool unsorted = true;
            while (unsorted)
            {
                unsorted = false;
                for (int i = 1; i < Rs.Length; i++)
                {
                    if (Rs[i].Item1 <= Rs[i - 1].Item1) continue;
                    (float, int) buffer1 = (Rs[i].Item1, Rs[i].Item2);
                    (float, int) buffer2 = (Rs[i - 1].Item1, Rs[i - 1].Item2);
                    Rs[i] = buffer2;
                    Rs[i - 1] = buffer1;
                    unsorted = true;
                }
            }

            var p = Environment.ProcessorCount - 1;

            var ParallelOptions = new ParallelOptions();

            var TestSubjects = new List<int>();
            if (xUp.Count - 1 < p)
            {
                for (int i = 0; i < Rs.Length; i++)
                {
                    TestSubjects.Add(Rs[i].Item2);
                    var DeltaT = xDown[i + 1] - xDown[i];
                    if (Delta > DeltaT) Delta = DeltaT;
                }
                ParallelOptions.MaxDegreeOfParallelism = xUp.Count - 1;
            }
            else
            {
                for (int i = 0; i < p; i++)
                {
                    TestSubjects.Add(Rs[i].Item2);
                }
                ParallelOptions.MaxDegreeOfParallelism = p;
            }

            var xMinus = new float[TestSubjects.Count];
            var x = new float[TestSubjects.Count];
            for (int i = 0; i < TestSubjects.Count; i++)
            {
                xMinus[i] = xDown[TestSubjects[i] - 1];
                x[i] = xDown[TestSubjects[i]];
                var DeltaT = x[i] - xMinus[i];
                if (Delta > DeltaT) Delta = DeltaT;
            }
            var xUpNew = new float[TestSubjects.Count];

            //for (int i = 0; i < TestSubjects.Count; i++)
            //{
            //    xUpNew[i] = TestValue(xMinus[i], x[i], m);
            //}
            Parallel.For(0, TestSubjects.Count, ParallelOptions, i =>
            {
                xUpNew[i] = TestValue(xMinus[i], x[i], m);
            });
            for (int i = 0; i < xUpNew.Length; i++)
            {
                if (xUp.Contains(xUpNew[i])) continue;
                xUp.Add(xUpNew[i]);
                zUp.Add(Function(xUpNew[i]));
            }
            return Delta;
        }
        private void FirstTests(float xValue)
        {
            zUp.Add(Function(xValue));
            xUp.Add(xValue);
        }
        private float CalculateR(float xMinus, float x, float m)
        {
            var z = Function(x);
            var zMinus = Function(xMinus);
            var Numerator = z - zMinus;
            Numerator = Numerator * Numerator;
            return m * (x - xMinus) + Numerator / (m * (x - xMinus)) - 2f * (m * (z + zMinus));
        }
        private float Calculate_m(float[] xValues, float r)
        {
            var CandidatesM = new float[xValues.Length - 1];
            for (int i = 1; i < xValues.Length; i++)
            {
                CandidatesM[i - 1] = Math.Abs((Function(xValues[i]) - Function(xValues[i - 1])) / (xValues[i] - xValues[i - 1]));
            }
            var M = CandidatesM.Max();
            if (M > 0)
            {
                return r * M;
            }
            else
            {
                return 1f;
            }
        }
        private float TestValue(float xMinusValue, float xValue, float m)
        {
            var zMinus = Function(xMinusValue);
            var z = Function(xValue);
            return 0.5f * (xValue + xMinusValue) - (z - zMinus) / (2f * m);
        }

    }
}
