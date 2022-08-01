using System.Collections;

namespace Utilities;

/// <summary>
/// C# remake of C++ std::optional-like optional type.
/// </summary>
/// <typeparam name="T">Type parameter for optionalization</typeparam>
public class Optional<T> : IEnumerable<T>
{
    private readonly T[] data;
    
    private Optional(T[] data)
    {
        this.data = data;
    }

    /// <summary>
    /// Create a new valid optional with a contained value
    /// </summary>
    /// <param name="value"></param>
    /// <returns>new Optional<typeparamref name="T"/> with value</returns>
    public static Optional<T> Create(T value)
    {
        return new Optional<T>(new[] { value });
    }

    /// <summary>
    /// Create a new empty optional
    /// </summary>
    /// <returns>empty Optional<typeparamref name="T"/> without value</returns>
    public static Optional<T> Empty()
    {
        return new Optional<T>(Array.Empty<T>());
    }

    /// <summary>
    /// Override of IEnumerable's GetEnumerator
    /// </summary>
    public IEnumerator<T> GetEnumerator()
    {
        return ((IEnumerable<T>)data).GetEnumerator();
    }

    /// <summary>
    /// Customary GetEnumerator() for enumerable type
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}