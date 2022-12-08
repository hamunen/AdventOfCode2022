using System.Xml.Linq;

namespace AdventOfCode;

[ExcludeFromCodeCoverage]
public class Day07 : BaseDay
{
    private string _input;
    public Day07()
    {
        _input = File.ReadAllText(InputFilePath);
    }

    public override ValueTask<string> Solve_1()
    {
        var terminal = new Terminal();
        terminal.ParseLines(_input);

        var directories = terminal.GetAllDirectories();
        var result = directories
            .Select(d => d.GetDirectorySizeIncludingChildDirectories())
            .Where(size => size <= 100000)
            .Sum();

        return new(result.ToString());
    }

    public override ValueTask<string> Solve_2()
    {
        const int TOTAL_DISK_SPACE = 70000000;
        const int UNUSED_SPACE_NEEDED = 30000000;

        var terminal = new Terminal();
        terminal.ParseLines(_input);

        var totalUsedSpace = terminal.RootDirectory.GetDirectorySizeIncludingChildDirectories();
        var unUsedSpace = TOTAL_DISK_SPACE - totalUsedSpace;
        var additionalSpaceNeeded = UNUSED_SPACE_NEEDED - unUsedSpace;

        var directories = terminal.GetAllDirectories();
        var result = directories
            .Select(d => d.GetDirectorySizeIncludingChildDirectories())
            .Where(size => size >= additionalSpaceNeeded)
            .Min();

        return new(result.ToString());
    }
}

public class Terminal {

    public Directory RootDirectory { get; set; }
    public Terminal()
    {
        RootDirectory = Directory.CreateRoot();
    }

    public void ParseLines(string input)
    {
        var commandEntries = input
            .Split("$", StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));

        var currentDirectory = RootDirectory;

        foreach (var entry in commandEntries)
        {
            currentDirectory = ProcessCommandEntry(entry, currentDirectory);
        }
    }

    private Directory ProcessCommandEntry(string[] commandEntry, Directory currentDirectory)
    {
        var command = commandEntry[0].Split(" ");
        if (command[0] == "cd")
        {
            var newDirectory = command[1];
            currentDirectory = newDirectory switch
            {
                "/" => RootDirectory,
                ".." => currentDirectory.Parent,
                _ => currentDirectory.GetChildDirectory(newDirectory),
            };

            return currentDirectory;
        }

        if (command[0] == "ls")
        {
            var outputs = commandEntry[1..].Select(l => l.Split(" "));
            foreach(var output in outputs)
            {
                if (output[0] == "dir") { }
                else
                {
                    var name = output[1];
                    var size = Int32.Parse(output[0]);
                    currentDirectory.AddFileIfNotExists(name, size);
                }
            }
        }
        return currentDirectory;
    }

    public IEnumerable<Directory> GetAllDirectories() =>
        RootDirectory.FlattenAllChildDirectories();
}

public class Directory
{
    public string Name { get; set; }
    public Directory Parent { get; set; }
    public List<Directory> Children { get; set; }
    public List<FileInfo> Files { get; set; }

    public Directory(string name)
    {
        Name = name;
        Files = new List<FileInfo>();
        Children= new List<Directory>();
    }

    public Directory(string name, Directory parent)
    {
        Name = name;
        Parent = parent;
        Files = new List<FileInfo>();
        Children = new List<Directory>();
    }

    public static Directory CreateRoot()
    {
        var rootDir = new Directory("/");
        rootDir.Parent = rootDir;
        return rootDir;
    }

    public Directory GetChildDirectory(string newDir)
    {
        var directory = Children.FirstOrDefault(d => d.Name == newDir);
        if (directory is not null) return directory;

        var newChildDirectory = new Directory(newDir, this);
        Children.Add(newChildDirectory);
        return newChildDirectory;
    }

    public void AddFileIfNotExists(string name, int size)
    {
        if (!Files.Any(f => f.Name == name))
        {
            Files.Add(new FileInfo(name, size));
        }
    }

    public IEnumerable<Directory> FlattenAllChildDirectories()
    {
        var stack = new Stack<Directory>();
        stack.Push(this);
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            yield return current;

            current.Children.ForEach(stack.Push);
        }
    }

    public int GetDirectorySize() => Files.Sum(f => f.Size);

    public int GetDirectorySizeIncludingChildDirectories()
    {
        var directories = FlattenAllChildDirectories();
        return directories.Sum(d => d.GetDirectorySize());
    }
}

public class FileInfo
{
    public string Name { get; set; }
    public int Size { get; set; }

    public FileInfo(string name, int size) { Name = name; Size = size; }

}