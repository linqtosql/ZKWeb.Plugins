﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZKWeb.Plugins.Shopping.Product.src.Model {
	/// <summary>
	/// 商品类型的接口
	/// </summary>
	public interface IProductType {
		/// <summary>
		/// 商品类型
		/// </summary>
		string Type { get; }
	}
}