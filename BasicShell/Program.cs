using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

while (true) {
    Console.Write("$ ");
    string input = Console.ReadLine()!;

    if (string.IsNullOrEmpty(input)) {
        continue;
    }
    
    if (input == "exit 0") {
        break;
    }

    if (input.StartsWith("cd", StringComparison.OrdinalIgnoreCase)) {
        input = input.Substring(2).Trim();
        try {
            Builtin.Cd(input);
        }
        catch (DirectoryNotFoundException e) {
            Console.WriteLine($"cd: {input}: No such file or directory");
        }
        continue;
    } 
    
    if (input.StartsWith("echo", StringComparison.OrdinalIgnoreCase)) {
        input = input.Substring(4).Trim();
        Console.WriteLine(Builtin.Echo(input));
        continue;
    }

    if (input.StartsWith("type", StringComparison.OrdinalIgnoreCase)) {
        input = input.Substring(4).Trim();
        Console.WriteLine(Builtin.Type(input));
        continue;
    }

    if (input.StartsWith("pwd", StringComparison.OrdinalIgnoreCase)) {
        input = input.Substring(3).Trim();
        Console.WriteLine(Builtin.Pwd(input));
        continue;
    }

    string[] parts = input.Split(" ");
    
    if (Helpers.IsExecutable(parts[0]) != string.Empty) {
        Helpers.RunExecutable(parts[0], parts[1..]);
        continue;
    }
    
    Console.WriteLine($"{input}: command not found");
}

internal static class Builtin {
    public static string Echo(string input) {
        return input;
    }

    public static void Cd(string input) {
        if (input.Contains('.')) {
            string workingDirectory = Directory.GetCurrentDirectory();
            string fullPath = Path.Join(workingDirectory, input);
            
            if (File.Exists(fullPath)) {
                Directory.SetCurrentDirectory(fullPath); 
            }
        }
        Directory.SetCurrentDirectory(input);
    }

    public static string Pwd(string input) {
        return Directory.GetCurrentDirectory();
    }

    public static string Type(string input) {
        if (Enum.TryParse(input, true, out Builtins result)) {
            return $"{input} is a shell builtin";
        }

        string path = Helpers.IsExecutable(input);
        return path != string.Empty 
            ? $"{input} is {path}" 
            : $"{input}: not found";
    }
}

internal static class Helpers {
    public static string IsExecutable(string input) {
        string args = Environment.GetEnvironmentVariable("PATH")!;
        string[] paths = args.Split(";"); // on windows this needs to be a semicolon 
        
        foreach (string path in paths) {
            string fullPath = Path.Combine(path, input);
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
    Cd,
    Echo,
    Pwd,    
    Type,
    Exit
}