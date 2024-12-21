using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;

namespace Ljdf.CommandLine
{
	internal sealed class FileInfoConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			try
			{
				return new FileInfo(value as string);
			}
			catch
			{
				throw new InvalidCastException(Resource.CannotConvertFrom.CCFormat(value, typeof(FileInfo)));
			}
		}
	}

	internal sealed class DirectoryInfoConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			try
			{
				return new DirectoryInfo(value as string);
			}
			catch
			{
				throw new InvalidCastException(Resource.CannotConvertFrom.CCFormat(value, typeof(DirectoryInfo)));
			}
		}
	}

	internal sealed class EncodingConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			try
			{
				if (value is string v)
				{
					if (string.Equals(v, "utf-8be", StringComparison.OrdinalIgnoreCase))
					{
						return new UTF8Encoding(true);
					}
					return Encoding.GetEncoding(v);
				}
			}
			catch { }

			return Encoding.UTF8;
		}
	}
}
