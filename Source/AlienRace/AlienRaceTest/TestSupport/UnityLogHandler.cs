namespace AlienRaceTest.TestSupport
{
    using System;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public class UnityLogHandler : ILogHandler
    {
        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            Console.WriteLine($"{logType}, {context}, {format}, {args}");
        }

        public void LogException(Exception exception, Object context)
        {
            Console.WriteLine($"{exception}, {context}");
        }
    }
}