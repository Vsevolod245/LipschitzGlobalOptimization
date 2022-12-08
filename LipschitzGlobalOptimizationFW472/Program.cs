using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
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
                Console.WriteLine("Type Kmax");
                if(!int.TryParse(Console.ReadLine(), out Kmax))
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
                Console.WriteLine("Type r");
                if (!float.TryParse(Console.ReadLine(), out r))
                {
                    Console.Clear();
                    Console.WriteLine("Ошибка ввода. Попробуйте еще раз.");
                    continue;
                }
                break;
            }
            var LGO = new LGO();
            LGO.Optimize(Kmax, a, b, r);
            Console.WriteLine("END");
            Console.ReadKey();
        }
    }
    internal class LGO
    {
        private List<float> zUp = new List<float>();

        private float Function(float x)
        {
            return x * x;
        }
        public void Optimize(int Kmax, float a, float b, float r)
        {
            #region First tests on boarders
            FirstTests(a);
            FirstTests(b);
            #endregion
            int k = 2;

        }
        private float SelectNextValue(int k)
        {

        }
        private void FirstTests(float xValue)
        {
            zUp.Add(Function(xValue));
        }
        private float CalculateR(float xMinus, float x, float m)
        {
            var z = Function(x);
            var zMinus = Function(xMinus);
            var Numerator = z - zMinus;
            Numerator = Numerator * Numerator;
            return m*(x - xMinus)+Numerator/(m*(x-  xMinus))-2f*(m*(z+zMinus));
        }
        private float Calculate_m(float[] xValues, float r)
        {
            var CandidatesM = new float [xValues.Length-1];
            for(int i = 1; i< xValues.Length; i++)
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
            float xUp = 0.5f * (xValue + xMinusValue) - (z - zMinus) / (2f * m);
            return Function(xUp);
        }

    }
}
