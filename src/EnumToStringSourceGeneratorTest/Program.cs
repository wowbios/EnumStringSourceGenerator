using System;

namespace EnumStringSourceGenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(Print(TestEnum1.First));
        }

        private static string Print(TestEnum1 state) => state.ToStringFromSgen();
    }

    public enum TestEnum1
    {
        First,
        Second
    }    
}
