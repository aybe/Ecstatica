using System.IO;

// ReSharper disable InconsistentNaming

namespace Ecstatica.Tests;

public sealed class FilePath
{
    private readonly string? _path;

    public FilePath(string? path)
    {
        _path = path;
    }

    public string? DirectoryName => Path.GetDirectoryName(_path);

    public bool Exists => Directory.Exists(_path) || File.Exists(_path);

    public string? Extension => Path.GetExtension(_path);

    public string? FileName => Path.GetFileName(_path);

    public string? FileNameWithoutExtension => Path.GetFileNameWithoutExtension(_path);

    public string? FullPath => _path != null ? Path.GetFullPath(_path) : null;

    public FilePath AppendToFileName(string text, string? separator = null)
    {
        return Combine(DirectoryName ?? string.Empty, string.Concat(FileNameWithoutExtension, separator, text, Extension));
    }

    public string PrependToFileName(string text, string? separator = null)
    {
        return Combine(DirectoryName ?? string.Empty, string.Concat(text, separator, FileNameWithoutExtension, Extension));
    }

    public FilePath ChangeExtension(string? extension)
    {
        return new FilePath(Path.ChangeExtension(_path, extension));
    }

    public FilePath Combine(params string[] paths)
    {
        return Path.Combine(paths);
    }

    public override string ToString()
    {
        return _path ?? string.Empty;
    }

    public static implicit operator string(FilePath path)
    {
        return path.ToString();
    }

    public static implicit operator FilePath(string path)
    {
        return new FilePath(path);
    }
}