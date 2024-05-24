using System;
using System.ComponentModel;

namespace Ljdf.CommandLine
{
	/// <summary>
	/// 无参选项特性。
	/// </summary>
	public class NoneValueOptionAttribute : OptionAttribute
	{
		/// <summary>
		/// 构造 <see cref="NoneValueOptionAttribute"/> 对象。
		/// </summary>
		/// <param name="shortName">短名称。</param>
		/// <param name="description">描述文本。</param>
		public NoneValueOptionAttribute(char shortName, string description)
			: base(shortName, null, description)
		{
		}

		/// <summary>
		/// 构造 <see cref="NoneValueOptionAttribute"/> 对象。
		/// </summary>
		/// <param name="longName">长名称。</param>
		/// <param name="description">描述文本。</param>
		public NoneValueOptionAttribute(string longName, string description)
			: base(null, longName, description)
		{
		}

		/// <summary>
		/// 构造 <see cref="NoneValueOptionAttribute"/> 对象。
		/// </summary>
		/// <param name="shortName">短名称。</param>
		/// <param name="longName">长名称。</param>
		/// <param name="description">描述文本。</param>
		public NoneValueOptionAttribute(char shortName, string longName, string description) 
			: base(shortName, longName, description)
		{
		}

		/// <inheritdoc/>
		/// <remarks><b>在 <see cref="NoneValueOptionAttribute"/> 中，该属性永远返回
		/// <see langword="false"/>，并且设置该属性的值时会抛出
		/// <see cref="NotSupportedException"/> 异常。</b>
		/// </remarks>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override bool IsDefault 
		{
			get => false;
			set => throw new NotSupportedException();	
		}

		/// <inheritdoc/>
		/// <remarks><b>在 <see cref="NoneValueOptionAttribute"/> 中，该属性永远返回
		/// <see langword="false"/>，并且设置该属性的值时会抛出
		/// <see cref="NotSupportedException"/> 异常。</b>
		/// </remarks>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override object OptionalValue
		{
			get => false;
			set => throw new NotSupportedException();
		}
	}
}
