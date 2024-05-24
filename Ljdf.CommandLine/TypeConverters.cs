using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;

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
}
