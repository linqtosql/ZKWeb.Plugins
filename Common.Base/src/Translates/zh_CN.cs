﻿using DryIocAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZKWeb.Model;
using ZKWeb.Utils.Extensions;

namespace ZKWeb.Plugins.Common.Base.src.Translates {
	/// <summary>
	/// 中文翻译
	/// </summary>
	[ExportMany(ContractKey = "zh-CN"), SingletonReuse]
	public class zh_CN : ITranslateProvider {
		private static Dictionary<string, string> Translates = new Dictionary<string, string>()
		{
			{ "abc", "测试翻译" },
			{ "Admin Login", "管理员登陆" },
			{ "ZKWeb Default Website", "ZKWeb默认站点" }
		};

		public string Translate(string text) {
			return Translates.GetOrDefault(text);
		}
	}
}