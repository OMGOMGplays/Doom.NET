﻿using System;
using System.Text.Json;

using DoomNET.Entities;

namespace DoomNET;

public class Program
{
    public static readonly JsonSerializerOptions serializerOptions = new()
    {
        WriteIndented = true,
        AllowTrailingCommas = true,
        Converters = { new EntityConverter() }
    };

    [STAThread]
    public static void Main()
    {
        Game game = new Game();
        game.Initialize();
    }
}
