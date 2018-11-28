using System;

namespace Rpkg
{
    class Write
    {
        public static void Default()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.DarkBlue;
        }
        public static void Info(string msgFormat)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(msgFormat);
            Default();
        }
        public static void Info(string msgFormat, params object[] lst)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(String.Format(msgFormat, lst));
            Default();
        }
        public static void Error(string msgFormat)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msgFormat);
            Default();
        }
        public static void Error(string msgFormat, params object[] lst)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(String.Format(msgFormat, lst));
            Default();
        }
        public static void Title(string msgFormat)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(msgFormat);
            Default();
        }
        public static void Title(string msgFormat, params object[] lst)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(String.Format(msgFormat, lst));
            Default();
        }

        public static void Label(string label, string value)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write(" " + label + ": ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(value);
            Default();
        }

        public static string Question(string question)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(question+":");
            var rc = Console.ReadLine();
            Default();
            return rc; 
        }
        public static bool QuestionSN(string question)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(question + " [s/n]:");
            var rc = Console.ReadKey();
            Default();
            Console.WriteLine();
            if (rc.KeyChar == 's' || rc.KeyChar == 'S')
            {
                return true;
            }
            return false;
        }
        public static string QuestionMulti(string question, string values)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(question + " [" + values + "]:");
            var rc = Console.ReadKey();
            Default();
            Console.WriteLine();
            return rc.KeyChar.ToString().ToLower(); 
        }
    }
}
