﻿using System.Collections.Generic;
using ZKWeb.Localize;
using ZKWebStandard.Extensions;
using ZKWebStandard.Ioc;

namespace ZKWeb.Plugins.Finance.Payment.WeChat.src.Components.Translates {
	/// <summary>
	/// 中文翻译
	/// </summary>
	[ExportMany, SingletonReuse]
	public class zh_CN : ITranslateProvider {
		private static HashSet<string> Codes = new HashSet<string>() { "zh-CN" };
		private static Dictionary<string, string> Translates = new Dictionary<string, string>()
		{
			{ "WechatPaymentApi", "微信支付" },
			{ "Support pay transactions by wechat", "支持通过微信支付交易" },
			{ "WechatPay", "微信支付" },
			{ "PublicAccountId", "公众号Id" },
			{ "PartnerId", "商户Id" },
			{ "PartnerKey", "商户密钥" },
			{ "ReturnDomain", "返回域名" },
			{ "keep empty will use the default domain", "留空时使用默认域名" }
		};

		public bool CanTranslate(string code) {
			return Codes.Contains(code);
		}

		public string Translate(string text) {
			return Translates.GetOrDefault(text);
		}
	}
}
