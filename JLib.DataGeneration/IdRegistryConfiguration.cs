namespace JLib.DataGeneration;

public class IdRegistryConfiguration
{
    /// <summary>
    /// this namespace will be removed from the idGroupName via string replace.
    /// </summary>
    public string? DefaultNamespace { get; init; }

    internal const string DefaultNamespaceReplacement = "~.";
    internal string ApplyDefaultNamespace(string inputName)
        => DefaultNamespace is null ? inputName : inputName.Replace(DefaultNamespace, DefaultNamespaceReplacement);
}