using System.Collections;

namespace Utilities;

/// <summary>
/// Rust-like Option type for optional values
/// </summary>
/// <typeparam name="T">type of value</typeparam>
public abstract class Option<T>
{
    /// <summary>
    /// Implicit conversion of value to Option<typeparamref name="T"/>
    /// </summary>
    /// <param name="value">value to insert</param>
    public static implicit operator Option<T>(T value) => new Some<T>(value);
    
    /// <summary>
    /// Implicit conversion of none to Option<typeparamref name="T"/>
    /// </summary>
    /// <param name="none">None value</param>
    public static implicit operator Option<T>(None _) => new None<T>();

    /// <summary>
    /// Allows mapping an optional into another type
    /// </summary>
    /// <typeparam name="TResult">the type to map into</typeparam>
    /// <param name="map">mapping function to <typeparamref name="TResult"/></param>
    /// <returns>new mapped type, if none it will map to None.Value</returns>
    public abstract Option<TResult> Map<TResult>(Func<T, TResult> map);

    /// <summary>
    /// Allows to map an optional to another optional type
    /// </summary>
    /// <typeparam name="TResult">The type to map into as optional</typeparam>
    /// <param name="map">mapping function to Optional<typeparamref name="TResult"/></param>
    /// <returns>new Optional<typeparamref name="TResult"/>, if None it will map to None.Value</returns>
    public abstract Option<TResult> MapOptional<TResult>(Func<T, Option<TResult>> map);

    /// <summary>
    /// Reduces the option to its contained type, and to a custom
    /// specified value if the provided value is none
    /// </summary>
    /// <param name="whenNone">custom type to return if none</param>
    /// <returns>the value if Some, <paramref name="whenNone"/> if none</returns>
    public abstract T Reduce(T whenNone);

    /// <summary>
    /// Reduces the option to its contained type, and to a custom
    /// specified value if the provided value is none
    /// </summary>
    /// <param name="whenNone">function to call if none</param>
    /// <returns>the value if Some<typeparamref name="T"/>, the result of the function call to <paramref name="whenNone"/>
    /// </returns>
    public abstract T Reduce(Func<T> whenNone);

    public Option<TNew> OfType<TNew>() where TNew : class
    {
        return this is Some<T> some && typeof(TNew).IsAssignableFrom(typeof(T))
               && some.Value is not null
               ? new Some<TNew>((some.Value as TNew)!)
               : None.Value;
    }
}

/// <summary>
/// Optional type with some value
/// </summary>
/// <typeparam name="T">type of value</typeparam>
public sealed class Some<T> : Option<T>, IEquatable<Some<T>>
{
    /// <summary>
    /// Effective contained value
    /// </summary>
    public T Value { get; set; }
    
    public Some(T value)
    {
        Value = value;
    }

    /// <summary>
    /// Implicitly converts the optional into its contained type
    /// </summary>
    /// <param name="some">Optional Some<typeparamref name="T"/> value</param>
    public static implicit operator T(Some<T> some)
    {
        return some.Value;
    }

    public override Option<TResult> Map<TResult>(Func<T, TResult> map)
    {
        return map(Value);
    }

    public override Option<TResult> MapOptional<TResult>(Func<T, Option<TResult>> map)
    {
        return map(Value);
    }

    public override T Reduce(T whenNone)
    {
        return Value;
    }

    public override T Reduce(Func<T> whenNone)
    {
        return Value;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }
        
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        return obj is Some<T> some && Equals(some);
    }

    public bool Equals(Some<T>? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }
        
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return EqualityComparer<T>.Default.Equals(Value, other.Value);
    }

    public override int GetHashCode()
    {
        return EqualityComparer<T>.Default.GetHashCode(Value!);
    }

    public static bool operator ==(Some<T> lhs, Some<T> rhs)
    {
        return (lhs is null && rhs is null) || (lhs is not null && lhs.Equals(rhs));
    }

    public static bool operator !=(Some<T> lhs, Some<T> rhs)
    {
        return !(lhs == rhs);
    }
}

/// <summary>
/// Optional type without value
/// </summary>
/// <typeparam name="T">type of value</typeparam>
public sealed class None<T> : Option<T>, IEquatable<None<T>>
{
    public override Option<TResult> Map<TResult>(Func<T, TResult> map)
    {
        return None.Value;
    }
    
    public override Option<TResult> MapOptional<TResult>(Func<T, Option<TResult>> map)
    {
        return None.Value;
    }

    public override T Reduce(T whenNone)
    {
        return whenNone;
    }

    public override T Reduce(Func<T> whenNone)
    {
        return whenNone();
    }

    public override bool Equals(object? obj)
    {
        return obj is not null && ((obj is None<T>) || (obj is None));
    }

    public bool Equals(None<T>? _)
    {
        return true;
    }

    public bool Equals(None? _)
    {
        return true;
    }

    public override int GetHashCode()
    {
        return 0;
    }

    public static bool operator ==(None<T> lhs, None<T> rhs)
    {
        return (lhs is null && rhs is null) || (lhs is not null && lhs.Equals(rhs));
    }

    public static bool operator !=(None<T> lhs, None<T> rhs)
    {
        return !(lhs == rhs);
    }
}

/// <summary>
/// Represents a non-value to store into Option<T>
/// </summary>
public sealed class None : IEquatable<None>
{
    public static None Value { get; } = new None();

    private None() { }

    public override bool Equals(object? obj)
    {
        return (obj is not null) && ((obj is None) || IsGenericNone(obj.GetType()));
    }

    public bool Equals(None? _)
    {
        return true;
    }

    public override int GetHashCode()
    {
        return 0;
    }

    private static bool IsGenericNone(Type type)
    {
        return (type.GenericTypeArguments.Length == 1 &&
                typeof(None<>).MakeGenericType(type.GenericTypeArguments[0]) == type);
    }
}