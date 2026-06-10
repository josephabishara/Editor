using System;
using System.Collections.Generic;
using System.Text;

namespace EditorLogicLayer.Helpers
{
    public static class StringExtensions
    {
        /// <summary>Returns null if the string is null or whitespace; otherwise returns the string.</summary>
        public static string? NullIfEmpty(this string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
