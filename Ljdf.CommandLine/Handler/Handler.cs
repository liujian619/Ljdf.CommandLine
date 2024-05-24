namespace Ljdf.CommandLine
{
	/// <summary>
	/// 用于定义一个命令或选项的参数信息与处理程序。
	/// </summary>
	/// <remarks>
	/// 使用时创建一个继承自该类的类：
	/// 对于顶级命令，应用 <see cref="CommandAttribute()"/> 特性；
	/// 对于子命令，应用 <see cref="CommandAttribute(string, string)"/> 特性。
	/// 类中的属性，根据具体情况分别应用
	/// <see cref="NoneValueOptionAttribute"/> 特性或
	/// <see cref="SingleValueOptionAttribute"/> 特性或
	/// <see cref="MultipleValueOptionAttribute"/> 特性。
	/// </remarks>
	public abstract class Handler
	{
		/// <summary>
		/// 解析成功后的处理程序。
		/// </summary>
		/// <param name="context">上下文对象。</param>
		public abstract void Handle(HandleContext context);
	}
}
