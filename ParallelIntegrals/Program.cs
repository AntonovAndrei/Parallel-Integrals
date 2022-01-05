using System;
using System.Collections.Generic;
using System.Threading;

namespace ParallelIntegrals
{
    class Program
    {
        private static double _left = 2;
        private static double _right = 8;
        private static double _precision = 1e-4;
        private static List<double> _areas = new List<double>();
        private static Mutex _mutex = new Mutex();

        //Функция - cos(0.5x) + 3
        private static double _function(double x) => Math.Cos(0.5*x) + 3;
        static void Main(string[] args)
        {
            Function f = new Function()
            {
                Left = _left, Right = _right, LeftHeight = _function(_left), RightHeight = _function(_right),
                TotalArea = _function((_left + _right) / 2) * (_right - _left)
            };
            Thread function = new Thread(IntegralMyFunction);
            function.Start(f);
            function.Join();
            double sum = 0;
            foreach (var s in _areas)
            {
                sum += s;
            }
            Console.WriteLine(sum);
        }
        
        
        static void IntegralMyFunction(object f)
        {
            Console.WriteLine("Поток"+Thread.CurrentThread.ManagedThreadId);
            Function myFunct = (Function) f;
            double middle = (myFunct.Left + myFunct.Right) / 2;
            double middleHeight = _function(middle);
            double leftArea = ((myFunct.LeftHeight + middleHeight) / 2) * (middle - myFunct.Left);
            double rightArea = ((middleHeight + myFunct.RightHeight) / 2) * (myFunct.Right - middle);
            if (Math.Abs(Math.Abs(leftArea + rightArea) - myFunct.TotalArea) > _precision)
            {
                Thread leftThread = new Thread(IntegralMyFunction);
                leftThread.Start(new Function()
                {
                    Left = myFunct.Left, Right = middle, LeftHeight = myFunct.LeftHeight, RightHeight = middleHeight,
                    TotalArea = leftArea
                });
                
                Thread rightThread = new Thread(IntegralMyFunction);
                rightThread.Start(new Function()
                {
                    Left = middle, Right = myFunct.Right, LeftHeight = middleHeight, RightHeight = myFunct.RightHeight,
                    TotalArea = rightArea
                });
                leftThread.Join();
                rightThread.Join();
            }
            else
            {
                _mutex.WaitOne();
                _areas.Add(leftArea);
                _areas.Add(rightArea);
                _mutex.ReleaseMutex();
            }
        }
    }
}