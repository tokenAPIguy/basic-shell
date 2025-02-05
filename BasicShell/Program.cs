using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

while (true) {
    Console.Write("$ ");
    string command = Console.ReadLine()!;

    if (string.IsNullOrEmpty(command)) {
        continue;
    }
    
    if (command == "exit 0") {
        break;
    }
    
    if (command.StartsWith("echo", StringComparison.OrdinalIgnoreCase)) {
        command = command.Substring(4).Trim();
        Console.WriteLine(Builtin.Echo(command));
        continue;
    }

    if (command.StartsWith("type", StringComparison.OrdinalIgnoreCase)) {
        command = command.Substring(4).Trim();
        Console.WriteLine(Builtin.Type(command));
        continue;
    }

    if (command.StartsWith("pwd", StringComparison.OrdinalIgnoreCase)) {
        command = command.Substring(3).Trim();
        Console.WriteLine(Builtin.Pwd(command));
        continue;
    }

    string[] parts = command.Split(" ");
    
    if (Helpers.IsExecutable(parts[0]) != string.Empty) {
        Helpers.RunExecutable(parts[0], parts[1..]);
        continue;
    }
    
    Console.WriteLine($"{command}: command not found");
}

internal static class Builtin {
    public static string Echo(string command) {
        return command;
    }

    public static string Pwd(string command) {
        return Directory.GetCurrentDirectory();
    }

    public static string Type(string command) {
        if (Enum.TryParse(command, true, out Builtins result)) {
            return $"{command} is a shell builtin";
        }

        string path = Helpers.IsExecutable(command);
        return path != string.Empty 
            ? $"{command} is {path}" 
            : $"{command}: not found";
    }
}

internal static class Helpers {
    public static string IsExecutable(string command) {
        string args = Environment.GetEnvironmentVariable("PATH")!;
        string[] paths = args.Split(":"); // on windows this needs to be a semicolon 
        
        foreach (string path in paths) {
            string fullPath = Path.Combine(path, command);
            if (File.Exists(fullPath)) {
                return fullPath;
            }
        }
        return string.Empty;
    }

    public static void RunExecutable(string executable, string[] args) {
        Process.Start(executable, args);
    }
}

internal enum Builtins {
    Echo,
    Pwd,    
    Type,
    Exit
}

