using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Ljdf.CommandLine
{
	/// <summary>
	/// 解析器配置。
	/// </summary>
	public sealed class ParserConfig
	{		
		/// <summary>
		/// 获取或设置一个值，指示在解析过程中是否忽略大小写。
		/// </summary>
		public bool IgnoreCase { get; set; }

		/// <summary>
		/// 获取或设置一个值，指示是否禁用默认的帮助处理程序。
		/// </summary>
		public bool DisableHelpHandler { get; set; }

		/// <summary>
		/// 获取或设置一个值，指示是否禁用默认的版本号处理程序。
		/// </summary>
		public bool DisableVersionHandler { get; set; }

		/// <summary>
		/// 获取或设置一个值，指示阻止默认的帮助（选项为“-?”时)）。
		/// </summary>
		public bool PreventDefaultHelp { get; set; }

		/// <summary>
		/// 获取或设置一个值，指示是否禁用完成提示。
		/// </summary>
		public bool DisableDoneTip { get; set; }

		/// <summary>
		/// 获取或设置默认的资源类的类型。
		/// </summary>
		public Type DefaultResourceType { get; set; }
	}
}
