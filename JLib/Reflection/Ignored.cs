﻿namespace JLib.Reflection;

/// <summary>
/// used to indicate when a TypeArgument is not used for reflection and can be ignored
/// </summary>
// ReSharper disable once ConvertToStaticClass
public sealed class Ignored
{
    private Ignored()
    {
    }

    public static Ignored Instance { get; } = new();
}