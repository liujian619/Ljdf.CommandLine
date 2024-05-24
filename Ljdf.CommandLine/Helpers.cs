using System;
using System.Globalization;

namespace Ljdf.CommandLine
{
	internal static class Helpers
	{
		internal static string CCFormat(this string format, params object[] args)
		{
			return string.Format(CultureInfo.CurrentCulture, format, args);
		}

		internal static StringComparer GetComparer(bool ignoreCase)
		{
			return ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
		}

		internal static bool Eq(string s1, string s2, bool ignoreCase)
		{
			if (ignoreCase)
				return string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase);
			else
				return string.Equals(s1, s2);
		}
	}
}
