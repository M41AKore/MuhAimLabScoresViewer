using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuhAimLabScoresViewer
{
    internal class ProgramArguments
    {
        public string DisplayName { get; set; }
        public string OutputFolder { get; set; }
        public string OutputFile { get; set; }
        public int Bitrate { get; set; }
        public int FPS { get; set; }

        public int BufferSeconds { get; set; }

        public ProgramArguments(IReadOnlyList<string> args)
        {
            for (var i = 0; i < args.Count; ++i)
            {
                string GetNextArgument()
                {
                    if (i + 1 > args.Count - 1)
                        throw new ArgumentNullException(args[i], "Argument required.");

                    return args[++i];
                }

                switch (args[i])
                {
                    case "-d":
                        DisplayName = GetNextArgument();
                        break;
                    case "-o":
                        OutputFolder = GetNextArgument();
                        break;
                    case "-r":
                        int.TryParse(GetNextArgument(), out int mbps);
                        Bitrate = mbps;
                        break;
                    case "-f":
                        int.TryParse(GetNextArgument(), out int rate);
                        FPS = rate;
                        break;
                    case "-file":
                        OutputFile = GetNextArgument();
                        break;
                    case "-s":
                        int.TryParse(GetNextArgument(), out int seconds);
                        BufferSeconds = seconds;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(args[i], "Unknown argument.");
                }
            }

            if (OutputFolder == null)
            {
                throw new ArgumentNullException("--output", "An output path must be specified using --output");
            }
        }
    }
}
