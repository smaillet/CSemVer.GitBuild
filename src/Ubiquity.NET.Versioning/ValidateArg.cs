﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Ubiquity.NET.Versioning
{

    // TODO: With C#14 extensions it should be possible to make these a static method extension to the exception types
    //       so that it appears as a static method to that class. Allowing syntax like:
    //       ArgumentException.ThrowIfNotMatch(inputStr, myRegEx);
    //       Have to wait and see how that goes and if it all makes it in. (Been fooled by that more than once!)

    internal static class ValidateArg
    {
        public static string ThrowIfNotMatch( [NotNull] this string self, Regex regex, [CallerArgumentExpression( nameof( self ) )] string? exp = null )
        {
            self.ThrowIfNull(exp);
            return regex.IsMatch( self ) ? self : throw new ArgumentException( $"Input '{self}' does not match expected pattern '{regex}'", exp );
        }

        public static T ThrowIfOutOfRange<T>( [NotNull] this T? self, T min, T max, [CallerArgumentExpression( nameof( self ) )] string? exp = null )
            where T : IComparable<T>
        {
            self.ThrowIfNull(exp);
            if(self.CompareTo( min ) < 0 || self.CompareTo( max ) > 0)
            {
                throw new ArgumentOutOfRangeException( exp );
            }

            return self;
        }

        public static T ThrowIfNull<T>([NotNull] this T? self, [CallerArgumentExpression( nameof( self ) )] string? exp = null)
        {
            return self is not null ? self : throw new ArgumentException(exp);
        }

        public static string ThrowIfNullOrWhiteSpace( [NotNull] this string? self, [CallerArgumentExpression( nameof( self ) )] string? exp = null )
        {
            self.ThrowIfNull();
            return !string.IsNullOrWhiteSpace(self) ? self : throw new ArgumentException(exp);
        }

        public static string ThrowIfNullOrWhitespaceOrLongerThan([NotNull] this string? self, int length, [CallerArgumentExpression( nameof( self ) )] string? exp = null)
        {
            self.ThrowIfNullOrWhiteSpace(exp);
            return self.Length <= length ? self : throw new ArgumentException($"Length of {self.Length} exceeds limit {length}", exp);
        }
    }
}
