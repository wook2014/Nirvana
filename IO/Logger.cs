﻿using System;
using System.Text;

namespace IO
{
    public static class Logger
    {
        // can be redirected to any logger
        public static Action<string> WriteLine { get; set; }
        public static Action<string> Write     { get; set; }
        
        public const string Url = "Url";

        static Logger()
        {
            WriteLine = Console.WriteLine;
            Write     = Console.Write;
        }

        public static void SetBold()    => Console.ForegroundColor = ConsoleColor.Yellow;
        public static void ResetColor() => Console.ResetColor();

        public static void Silence()
        {
            WriteLine = s => { };
            Write     = s => { };
        }

        public static void Log(Exception e)
        {
            var sb   = new StringBuilder();
            var line = new string('-', 80);
            sb.AppendLine(line);

            const string vcfLine = "VcfLine";
            const string errorLine = "Line";

            while (e != null)
            {
                sb.AppendLine($"{e.GetType()}: {e.Message}");
                sb.AppendLine($"Stack trace: {e.StackTrace}");
                if (e.Data.Contains(vcfLine)) sb.AppendLine($"VCF line: {e.Data[vcfLine]}");
                if (e.Data.Contains(errorLine)) sb.AppendLine($"Line: {e.Data[errorLine]}");
                if (e.Data.Contains(Url)) sb.AppendLine($"URL: {e.Data[Url]}");

                sb.AppendLine(errorLine);
                e = e.InnerException;
            }

            WriteLine(sb.ToString());
        }
    }
}