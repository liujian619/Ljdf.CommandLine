namespace Ljdf.CommandLine
{
	/// <summary>
	/// 独参选项特性。
	/// </summary>
	public class SingleValueOptionAttribute : OptionAttribute
	{
		/// <summary>
		/// 构造 <see cref="SingleValueOptionAttribute"/> 对象。
		/// </summary>
		/// <param name="shortName">短名称。</param>
		/// <param name="description">描述文本。</param>
		public SingleValueOptionAttribute(char shortName, string description)
			: base(shortName, null, description)
		{
		}

		/// <summary>
		/// 构造 <see cref="SingleValueOptionAttribute"/> 对象。
		/// </summary>
		/// <param name="longName">长名称。</param>
		/// <param name="description">描述文本。</param>
		public SingleValueOptionAttribute(string longName, string description)
			: base(null, longName, description)
		{
		}

		/// <summary>
		/// 构造 <see cref="SingleValueOptionAttribute"/> 对象。
		/// </summary>
		/// <param name="shortName">短名称。</param>
		/// <param name="longName">长名称。</param>
		/// <param name="description">描述文本。</param>
		public SingleValueOptionAttribute(char shortName, string longName, string description) 
			: base(shortName, longName, description)
		{
		}
	}
}
