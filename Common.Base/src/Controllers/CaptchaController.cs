﻿using DryIoc;
using DryIocAttributes;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ZKWeb.Model;
using ZKWeb.Model.ActionResults;
using ZKWeb.Utils.Extensions;

namespace ZKWeb.Plugins.Common.Base.src.Controllers {
	/// <summary>
	/// 验证码控制器
	/// </summary>
	[ExportMany]
	public class CaptchaController : IController {
		/// <summary>
		/// 获取验证码
		/// </summary>
		/// <returns></returns>
		[Action("captcha")]
		public IActionResult Captcha() {
			var request = HttpContext.Current.Request;
			var response = HttpContext.Current.Response;
			var key = request.GetParam<string>("key");
			var captchaManager = Application.Ioc.Resolve<CaptchaManager>();
			return new ImageResult(captchaManager.Generate(key));
		}
	}
}
