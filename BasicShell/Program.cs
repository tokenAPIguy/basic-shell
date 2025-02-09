using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

while (true) {
    Console.Write("$ ");
    string input = Console.ReadLine()!;

    var parsed = CommandParser.Parse(input);
    var cmd = parsed.cmd;
    var parts = parsed.parts != null ? string.Join(" ", parsed.parts) : string.Empty;

    if (string.IsNullOrWhiteSpace(cmd)) {
        continue;
    }

    if (cmd == "exit") {
        if (Builtin.Exit(parts)) {
            break;
        }
    }

    if (Helpers.IsExecutable(cmd, out _)) {
        if (parsed.parts != null) {
            Helpers.RunExecutable(cmd, parsed.parts);
        }
        else {
            Helpers.RunExecutable(cmd);
        }
        continue;
    }
    
    switch (cmd) {
        case "cd":
            Builtin.Cd(parts);
            continue;
        case "echo":
            Console.WriteLine(Builtin.Echo(parts));
            continue; 
        case "pwd":
            Console.WriteLine(Builtin.Pwd());
            continue;
        case "type":
            Console.WriteLine(Builtin.Type(parts));
            continue;
        default:
            Console.WriteLine($"{input}: command not found");
            continue;
    }
}

internal static class Builtin {
    public static void Cd(string input) {
        PlatformID OS = Environment.OSVersion.Platform;
        if (string.IsNullOrEmpty(input) || input == "~") {
            string? home = OS.ToString().Substring(0, 3).Contains("win", StringComparison.CurrentCultureIgnoreCase)
                ? Environment.GetEnvironmentVariable("USERPROFILE")
                : Environment.GetEnvironmentVariable("HOME");
        
            if (home != null) {
                try {
                    Directory.SetCurrentDirectory(home);
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    throw;
                }
                return;
            }
            Console.WriteLine("Home directory is not set in environment variables.");
            return;
        }

        // Handle relative paths
        if (input.Contains('.')) {
            string workingDirectory = Directory.GetCurrentDirectory();
            string fullPath = Path.Join(workingDirectory, input);

            if (File.Exists(fullPath)) {
                Directory.SetCurrentDirectory(fullPath);
            }
            return;
        }

        Directory.SetCurrentDirectory(input);
    }
    
    public static string Echo(string input) {
        return input;
    }

    public static bool Exit(string input) {
        return input == "0";
    }
    
    public static string Pwd() {
        return Directory.GetCurrentDirectory();
    }

    public static string Type(string input) {
        if (Helpers.IsBuiltin(input)) {
            return $"{input} is a shell builtin";
        }
        
        return Helpers.IsExecutable(input, out var fullPath)
            ? $"{input} is {fullPath}"
            : $"{input} not found";
    }
}
internal static class CommandParser {
    public static (string? cmd, List<string>? parts) Parse(string input) {
        if (string.IsNullOrEmpty(input)) {
            return (string.Empty, null);
        }
        
        List<string> chunks = input.ToLower().Trim().Split(" ").ToList();
        
        string cmd = chunks[0];
        List<string> parts = chunks[1..];
        
        return (cmd, parts);
    }
}
internal static class Helpers {
    public static bool IsBuiltin(string cmd) {
        return Enum.TryParse(cmd, true, out Builtins result);
    }

    public static bool IsExecutable(string cmd, out string fullPath) {
        string? args = Environment.GetEnvironmentVariable("PATH");
        if (args != null) {
            PlatformID OS = Environment.OSVersion.Platform; 
            string[] paths = OS.ToString().Substring(0, 3).Contains("win", StringComparison.CurrentCultureIgnoreCase) ? args.Split(";") : args.Split(":"); 

            foreach (string path in paths) {
                fullPath = Path.Join(path, cmd);
                if (File.Exists(fullPath)) {
                    return (true);
                }
            }
        }

        fullPath = string.Empty;
        return (false);
    }

    public static void RunExecutable(string executable) {
        Process.Start(executable);
    }
    
    public static void RunExecutable(string executable, List<string> args) {
        Process.Start(executable, args);
    }
}
internal enum Builtins {
    Cd,
    Echo,
    Pwd,    
    Type,
    Exit
}