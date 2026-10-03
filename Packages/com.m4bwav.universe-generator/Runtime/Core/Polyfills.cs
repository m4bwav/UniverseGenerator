#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    /// <summary>Lets records and init accessors compile on netstandard2.0 and in Unity, which lack this type.</summary>
    internal static class IsExternalInit
    {
    }
}
#endif
