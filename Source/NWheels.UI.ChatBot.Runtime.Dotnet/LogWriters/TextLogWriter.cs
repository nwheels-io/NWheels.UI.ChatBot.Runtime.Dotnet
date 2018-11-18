using System;
using System.IO;
using Newtonsoft.Json;
using NWheels.UI.ChatBot.Runtime.Dotnet.Abstractions;

namespace NWheels.UI.ChatBot.Runtime.Dotnet.LogWriters
{
    public class TextLogWriter : ILogWriter
    {
        private static readonly string IndentUnit = "   ";
        
        private readonly TextWriter _writer;
        private string _indent;

        public TextLogWriter(TextWriter writer)
        {
            _writer = writer;
            _indent = string.Empty;
        }

        public void Event(string name, object input = null, object output = null)
        {
            _writer.WriteLine(_indent + "." + name.ToUpper());

            if (input != null)
            {
                _writer.Write(" <- ");
                _writer.Write(FormatData(input));
            }

            if (output != null)
            {
                _writer.Write(" -> ");
                _writer.Write(FormatData(output));
            }
        }

        public IDisposable EventSpan(string name, object input = null, object output = null)
        {
            Event(name, input, output);

            _indent += IndentUnit;

            return new LogSpan(onDispose: () => {
                _indent = _indent.Substring(0, _indent.Length - IndentUnit.Length);
            });
        }

        private string FormatData(object data)
        {
            if (data == null)
            {
                return "null";
            }
            else if (data.GetType().IsValueType)
            {
                return data.ToString();
            }
            else
            {
                var jsonText = JsonConvert.SerializeObject(data, Formatting.Indented);

                return _indent.Length > 0
                    ? jsonText.Replace("\n", "\n" + _indent)
                    : jsonText;
            }
        }
    }
}