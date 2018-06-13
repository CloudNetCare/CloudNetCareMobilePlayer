using System;

namespace CloudNetCare.SeleniumWrapper
{
    /// <summary>
    /// Represents a step in a Scenario
    /// </summary>
    public class LocalStep
    {
        public string Target { get; set; }
        public string Value { get; set; }
        public string Command { get; set; }
        public int? Type { get; set; }
        public String Name { get; set; }
        public String Condition { get; set; }
        public String Browser { get; set; }
        public int? TimeBetweenSteps { get; set; }
        public int? TimedOutValue { get; set; }

        public LocalStep(string target, string value)
        {
            Value = value;
            Target = target;
        }

        public LocalStep(string command, string target, string value, int? type, String name, int? TimeBetweenSteps, int? timedOutValue, string browsers, string condition)
        {
            Command = command;
            Value = value;
            Target = target;
            Type = type;
            Name = name;
            TimedOutValue = timedOutValue;
            Browser = browsers;
            Condition = condition;
        }

    }
}
