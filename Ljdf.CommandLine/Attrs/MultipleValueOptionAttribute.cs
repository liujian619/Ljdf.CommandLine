namespace Ljdf.CommandLine
{
	/// <summary>
	/// 多参选项特性。
	/// </summary>
	public class MultipleValueOptionAttribute : OptionAttribute
	{
		/// <summary>
		/// 构造 <see cref="NoneValueOptionAttribute"/> 对象。
		/// </summary>
		/// <param name="shortName">短名称。</param>
		/// <param name="description">描述文本。</param>
		public MultipleValueOptionAttribute(char shortName, string description)
			: base(shortName, null, description)
		{
		}

		/// <summary>
		/// 构造 <see cref="MultipleValueOptionAttribute"/> 对象。
		/// </summary>
		/// <param name="longName">长名称。</param>
		/// <param name="description">描述文本。</param>
		public MultipleValueOptionAttribute(string longName, string description)
			: base(null, longName, description)
		{
		}

		/// <summary>
		/// 构造 <see cref="MultipleValueOptionAttribute"/> 对象。
		/// </summary>
		/// <param name="shortName">短名称。</param>
		/// <param name="longName">长名称。</param>
		/// <param name="description">描述文本。</param>
		public MultipleValueOptionAttribute(char shortName, string longName, string description) 
			: base(shortName, longName, description)
		{
		}


		/// <summary>
		/// 获取或设置参数值最多个数。
		/// </summary>
		public int MaxCount { get; set; } = int.MaxValue;

		/// <summary>
		/// 获取或设置参数值最少个数。
		/// </summary>
		public int MinCount { get; set; } = 1;
	}
}
