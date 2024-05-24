using System;
using System.Linq;
using System.Reflection;

namespace Ljdf.CommandLine
{
	/// <summary>
	/// 用于参数选项的特性。
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public abstract class OptionAttribute : BaseAttribute
	{
		/// <summary>
		/// 构造 <see cref="OptionAttribute"/> 对象。
		/// </summary>
		/// <param name="shortName">短名称。</param>
		/// <param name="longName">长名称。</param>
		/// <param name="description">描述文本。</param>
		protected OptionAttribute(char? shortName, string longName, string description)
			: base(description)
		{
			if (!shortName.HasValue && longName is null)
			{
				throw new InvalidOperationException(Resource.ShortNameAndLongNameCannotBeNull);
			}

			if (shortName.HasValue)
			{
				CheckShortName(shortName.Value);
				ShortName = shortName;
			}

			if (longName != null)
			{
				CheckLongName(longName);
				LongName = longName;
			}
		}


		/// <summary>
		/// 获取短名称。
		/// </summary>
		public char? ShortName { get; }

		/// <summary>
		/// 获取长名称。
		/// </summary>
		public string LongName { get; }

		/// <summary>
		/// 获取或设置是否可选。默认为 <see langword="false"/>。
		/// </summary>
		public bool Optional { get; set; }

		/// <summary>
		/// 获取或设置可选时的默认值。
		/// <para><b>注意：</b>该值只是用于辅助生成帮助文档，不会实际赋值给选项。</para>
		/// </summary>
		public virtual object OptionalValue { get; set; }

		/// <summary>
		/// 获取或设置一个值，指示是否是默认选项。
		/// </summary>
		public virtual bool IsDefault { get; set; }

		/// <summary>
		/// 获取特性所作用的属性。
		/// </summary>
		internal PropertyInfo PropertyInfo { get; set; }


		internal static bool ValidateShortName(char shortName)
		{
			// 短名称： 字母
			return char.IsLetter(shortName);
		}

		internal static bool ValidateLongName(string longName)
		{
			// 长名称：长度大于等于2  必须是字母、数字、连接符(-)，同时第一个字符必须是字母
			return longName.Length >= 2 && char.IsLetter(longName[0])
				&& longName.All(c => char.IsNumber(c) || char.IsLetter(c) || c == '-');
		}


		private static void CheckShortName(char shortName)
		{
			if (!ValidateShortName(shortName))
			{
				throw new FormatException(Resource.InvalidShortName.CCFormat(shortName));
			}
		}

		private static void CheckLongName(string longName)
		{
			if (!ValidateLongName(longName))
			{
				throw new FormatException(Resource.InvalidLongName.CCFormat(longName));
			}
		}
	}
}
