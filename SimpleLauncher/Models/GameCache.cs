using System;
using System.Collections.Generic;
using MessagePack;

namespace SimpleLauncher.Models;

[MessagePackObject]
public class GameCache
{
    [Key(0)]
    public int FileCount { get; set; }

    [Key(1)]
    public required List<string> FileNames { get; set; }

    [Key(2)]
    public DateTime FolderLastWriteTimeUtc { get; set; }
}