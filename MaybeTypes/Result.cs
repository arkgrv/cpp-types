using System.Collections.Generic;
using System.Text;

namespace MaybeTypes;

/// <summary>
/// Type of error encountered
/// </summary>
public enum ErrorSeverity
{
    /// <summary>
    /// Operation completed successfully
    /// </summary>
    Ok,
    /// <summary>
    /// There might be a problem, but continuation is possible
    /// </summary>
    Warning,
    /// <summary>
    /// Something went wrong, and we cannot continue
    /// </summary>
    Error,
}

/// <summary>
/// C# 10 adaptation of the Rust result struct (with no result attached)
/// </summary>
public class Result
{
    protected readonly List<string> errors;

    protected readonly List<string> warnings;

    public ErrorSeverity Severity { get; init; }

    /// <summary>
    /// Errors descriptions
    /// </summary>
    public IEnumerable<string> Errors { get => errors; }

    /// <summary>
    /// Warnings descriptions
    /// </summary>
    public IEnumerable<string> Warnings { get => warnings; }

    protected Result(ErrorSeverity severity, string? warning = null, string? error = null)
    {
        Severity = severity;
        errors = new List<string>();
        warnings = new List<string>();

        if (!string.IsNullOrEmpty(warning))
        {
            warnings.Add(warning!);
        }

        if (!string.IsNullOrEmpty(error))
        {
            errors.Add(error!);
        }
    }

    protected Result(IEnumerable<string> warnings, IEnumerable<string> errors, ErrorSeverity severity)
    {
        Severity = severity;
        this.errors = new List<string>(errors);
        this.warnings = new List<string>(warnings);
    }

    /// <summary>
    /// Creates an _empty_ result, reporting the status of the operation
    /// </summary>
    /// <param name="severity">Severity of the error</param>
    private Result(ErrorSeverity severity) : this(severity, null, null) { }

    /// <summary>
    /// Creates a result that reports an error
    /// </summary>
    /// <param name="error">Error description</param>
    public static Result Fail(string error) => new(ErrorSeverity.Error, null, error);

    /// <summary>
    /// Creates a result that reports a warning
    /// </summary>
    /// <param name="warning">Warning description</param>
    public static Result Warn(string warning) => new(ErrorSeverity.Warning, warning, null);

    /// <summary>
    /// Creates a result that reports an error with arguments
    /// </summary>
    /// <param name="fmt">Error description and format</param>
    /// <param name="args">Error arguments</param>
    public static Result Fail(string fmt, params object[] args)
    {
        return new(ErrorSeverity.Error, null, string.Format(fmt, args));
    }

    /// <summary>
    /// Creates a result that reports a warning with arguments
    /// </summary>
    /// <param name="fmt">Warning description and format</param>
    /// <param name="args">Warning arguments</param>
    public static Result Warn(string fmt, params object[] args)
    {
        return new(ErrorSeverity.Warning, string.Format(fmt, args), null);
    }

    /// <summary>
    /// Creates a result that reports an error with a list of possible errors
    /// </summary>
    /// <typeparam name="T">Type of wrapped data</typeparam>
    /// <param name="errors">Errors descriptions</param>
    public static Result<T> Fail<T>(IEnumerable<string> errors) =>
        new(default!, ErrorSeverity.Error, new List<string>(), errors);

    /// <summary>
    /// Creates a result that reports a warning with a list of possible warnings
    /// </summary>
    /// <typeparam name="T">Type of wrapped data</typeparam>
    /// <param name="warnings">Warnings descriptions</param>
    public static Result<T> Warn<T>(IEnumerable<string> warnings) =>
        new(default!, ErrorSeverity.Warning, warnings, new List<string>());

    /// <summary>
    /// Create a result that reports an error with arguments
    /// </summary>
    /// <typeparam name="T">Type of wrapped data</typeparam>
    /// <param name="format">Error description and format</param>
    /// <param name="args">Error arguments</param>
    public static Result<T> Fail<T>(string format, params object[] args)
    {
        return new(default!, ErrorSeverity.Error, new List<string>(), new List<string> { string.Format(format, args) });
    }

    /// <summary>
    /// Creates an empty result that reports the operation
    /// has been completed successfully
    /// </summary>
    public static Result Ok() => new(ErrorSeverity.Ok);

    /// <summary>
    /// Creates a result with wrapped data that reports
    /// the operation completed successfully
    /// </summary>
    /// <typeparam name="T">Type of wrapped data</typeparam>
    /// <param name="value">Wrapped value</param>
    public static Result<T> Ok<T>(T value)
    {
        return new(value, ErrorSeverity.Ok);
    }

    /// <summary>
    /// Creates a result that reports a warning with a wrapped value
    /// and a warning description
    /// </summary>
    /// <typeparam name="T">Type of wrapped value</typeparam>
    /// <param name="value">Wrapped value</param>
    /// <param name="warning">Warning description</param>
    public static Result<T> Warn<T>(T value, string warning)
    {
        return new(value, ErrorSeverity.Warning, new List<string> { warning }, new List<string>());
    }

    /// <summary>
    /// Creates a result that reports a warning with a wrapped value,
    /// a warning description and a list of arguments
    /// </summary>
    /// <typeparam name="T">Type of wrapped value</typeparam>
    /// <param name="value">Wrapped value</param>
    /// <param name="fmt">Warning description and format</param>
    /// <param name="args">Warning arguments</param>
    /// <returns></returns>
    public static Result<T> Warn<T>(T value, string fmt, params object[] args)
    {
        return Warn<T>(value, string.Format(fmt, args));
    }

    /// <summary>
    /// Returns true if the result is Ok or Warning (valid result)
    /// </summary>
    public bool IsOk => Severity == ErrorSeverity.Ok || Severity == ErrorSeverity.Warning;

    /// <summary>
    /// Returns true if the result is Error (invalid result)
    /// </summary>
    public bool IsError => Severity == ErrorSeverity.Error;

    /// <summary>
    /// Returns true if the result is Warning (valid result, with something to warn about)
    /// </summary>
    public bool IsWarning => Severity == ErrorSeverity.Warning;

    /// <summary>
    /// Returns the list of error or warning messages
    /// </summary>
    public string Info
    {
        get
        {
            var sb = new StringBuilder();
            if (IsError)
            {
                foreach (var error in Errors)
                {
                    sb.Append($"{error}, ");
                }
            }

            if (IsWarning)
            {
                foreach (var warning in Warnings)
                {
                    sb.Append($"{warning}, ");
                }
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Concatenates a generic error with its details
    /// </summary>
    /// <param name="error">Generic error description</param>
    /// <param name="details">Error details</param>
    public static string AppendErrors(string error, IEnumerable<string>? details)
    {
        if (details is null)
        {
            return error;
        }

        return error + string.Join(", ", details);
    }

    /// <summary>
    /// Combines two result together
    /// </summary>
    public static Result operator +(Result lhs, Result rhs)
    {
        return new Result<bool>(lhs.IsOk, lhs.Severity, lhs.Warnings, lhs.Errors) +
               new Result<bool>(rhs.IsOk, rhs.Severity, rhs.Warnings, rhs.Errors);
    }
}

/// <summary>
/// C# 10 adaptation of the Rust result class, with a wrapped
/// attached value
/// </summary>
/// <typeparam name="T">Type of wrapped value</typeparam>
public sealed class Result<T> : Result
{
    public T Value { get; init; }

    public Result(T value, ErrorSeverity severity)
        : this(value, severity, new List<string>(), new List<string>())
    {

    }

    public Result(T value, ErrorSeverity severity, IEnumerable<string> warnings, IEnumerable<string> errors)
        : base(warnings, errors, severity)
    {
        Value = value;
    }

    /// <summary>
    /// Append errors and warnings to of both results
    /// </summary>
    /// <returns>A new result with the worst severity of any of the two
    /// errors and their warning and errors list combined</returns>
    public static Result<T> operator +(Result<T> lhs, Result<T> rhs)
    {
        var severity = ErrorSeverity.Ok;

        // Find worst severity
        if (lhs.Severity == ErrorSeverity.Error || rhs.Severity == ErrorSeverity.Error)
        {
            severity = ErrorSeverity.Error;
        }
        else
        {
            if (lhs.Severity == ErrorSeverity.Warning || rhs.Severity == ErrorSeverity.Warning)
            {
                severity = ErrorSeverity.Warning;
            }
        }

        var result = new Result<T>(lhs.Value, severity, lhs.Warnings, rhs.Warnings);
        if (lhs.Value is not null && !lhs.Value.Equals(rhs.Value))
        {
            Console.Error.WriteLine("Warning: results have different values: '{0}' and '{1}'", lhs.Value!, rhs.Value!);
        }

        result.warnings.AddRange(rhs.Warnings);
        result.errors.AddRange(rhs.Errors);

        return result;
    }

    /// <summary>
    /// Returns <paramref name="result" /> if it is Ok, otherwise wrap
    /// the error in the <paramref name="check" />
    /// </summary>
    public static Result<T> Combine(Result<T> result, Result check)
    {
        if (check.IsError)
        {
            return Result.Fail<T>(check.Info);
        }

        return result;
    }
}
