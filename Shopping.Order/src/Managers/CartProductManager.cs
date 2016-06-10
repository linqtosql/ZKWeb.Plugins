﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ZKWeb.Cache;
using ZKWeb.Localize;
using ZKWeb.Plugins.Common.Admin.src.Extensions;
using ZKWeb.Plugins.Common.Base.src.Database;
using ZKWeb.Plugins.Common.Base.src.Managers;
using ZKWeb.Plugins.Common.Base.src.Repositories;
using ZKWeb.Plugins.Shopping.Order.src.Config;
using ZKWeb.Plugins.Shopping.Order.src.Database;
using ZKWeb.Plugins.Shopping.Order.src.Extensions;
using ZKWeb.Plugins.Shopping.Order.src.Model;
using ZKWeb.Plugins.Shopping.Order.src.Repositories;
using ZKWeb.Server;
using ZKWeb.Utils.Extensions;
using ZKWeb.Utils.IocContainer;

namespace ZKWeb.Plugins.Shopping.Order.src.Managers {
	/// <summary>
	/// 购物车商品管理器
	/// </summary>
	[ExportMany]
	public class CartProductManager {
		/// <summary>
		/// 购物车商品的总数量的缓存时间
		/// 默认是3秒，可通过网站配置指定
		/// </summary>
		public TimeSpan CartProductTotalCountCacheTime { get; protected set; }
		/// <summary>
		/// 购物车商品的总数量的缓存
		/// </summary>
		public IsolatedMemoryCache<CartProductType, long?> CartProductTotalCountCache { get; protected set; }
		/// <summary>
		/// 非会员添加购物车商品时，保留会话的天数
		/// 默认是1天，可通过网站配置指定
		/// </summary>
		public TimeSpan SessionExpireDaysForNonUserPurcharse { get; protected set; }

		/// <summary>
		/// 初始化
		/// </summary>
		public CartProductManager() {
			var configManager = Application.Ioc.Resolve<ConfigManager>();
			CartProductTotalCountCacheTime = TimeSpan.FromSeconds(
				configManager.WebsiteConfig.Extra.GetOrDefault(ExtraConfigKeys.CartProductTotalCountCacheTime, 3));
			CartProductTotalCountCache = new IsolatedMemoryCache<CartProductType, long?>("Ident");
			SessionExpireDaysForNonUserPurcharse = TimeSpan.FromDays(
				configManager.WebsiteConfig.Extra.GetOrDefault(
				ExtraConfigKeys.SessionExpireDaysForNonUserPurcharse, 1));
		}

		/// <summary>
		/// 添加购物车商品到当前会话
		/// 如果商品已在购物车则增加里面的数量
		/// </summary>
		/// <param name="productId">商品Id</param>
		/// <param name="type">购物车商品类型</param>
		/// <param name="parameters">匹配参数</param>
		public virtual void AddCartProduct(
			long productId, CartProductType type, IDictionary<string, object> parameters) {
			// 检查是否允许非会员下单
			var configManager = Application.Ioc.Resolve<GenericConfigManager>();
			var settings = configManager.GetData<OrderSettings>();
			var sessionManager = Application.Ioc.Resolve<SessionManager>();
			var session = sessionManager.GetSession();
			var user = session.GetUser();
			if (user == null && !settings.AllowAnonymousVisitorCreateOrder) {
				throw new HttpException(403, new T("Create order require user logged in"));
			}
			// 调用仓储添加购物车商品
			UnitOfWork.WriteRepository<CartProductRepository>(
				r => r.AddOrIncrease(session, productId, type, parameters));
			// 非会员登录时，在购物车商品添加成功后需要延长会话时间
			if (user == null) {
				session.SetExpiresAtLeast(SessionExpireDaysForNonUserPurcharse);
				sessionManager.SaveSession();
			}
			// 删除相关的缓存
			CartProductTotalCountCache.Remove(type);
		}

		/// <summary>
		/// 获取当前会话下的购物车商品列表
		/// </summary>
		/// <param name="type">购物车商品类型</param>
		/// <returns></returns>
		public virtual IList<CartProduct> GetCartProducts(CartProductType type) {
			var sessionManager = Application.Ioc.Resolve<SessionManager>();
			return UnitOfWork.ReadRepository<CartProductRepository, IList<CartProduct>>(
				r => r.GetManyBySession(sessionManager.GetSession(), type).ToList());
		}

		/// <summary>
		/// 获取购物车商品的总数量(商品 * 订购数量)
		/// 结果会按当前用户Id和购物车商品类型缓存一定时间
		/// </summary>
		/// <param name="type">购物车商品类型</param>
		/// <returns></returns>
		public virtual long GetCartProductTotalCount(CartProductType type) {
			// 从缓存获取
			var count = CartProductTotalCountCache.GetOrDefault(type);
			if (count != null) {
				return count.Value;
			}
			// 从数据库获取
			var sessionManager = Application.Ioc.Resolve<SessionManager>();
			count = UnitOfWork.ReadRepository<CartProductRepository, long>(r =>
				r.GetManyBySession(sessionManager.GetSession(), type)
				.Select(p => (long?)p.Count).Sum() ?? 0);
			// 保存到缓存并返回
			CartProductTotalCountCache.Put(type, count, CartProductTotalCountCacheTime);
			return count.Value;
		}

		/// <summary>
		/// 获取购物车商品的总价
		/// 返回 { 货币: 价格 }
		/// </summary>
		/// <param name="cartProducts">购物车商品列表</param>
		/// <returns></returns>
		public virtual IDictionary<string, decimal> GetCartProductTotalPrice(
			IList<CartProduct> cartProducts) {
			var orderManager = Application.Ioc.Resolve<OrderManager>();
			var sessionManager = Application.Ioc.Resolve<SessionManager>();
			var user = sessionManager.GetSession().GetUser();
			var userId = user == null ? null : (long?)user.Id;
			var result = new Dictionary<string, decimal>();
			foreach (var cartProduct in cartProducts) {
				var parameters = cartProduct.ToCreateOrderProductParameters();
				var price = orderManager.CalculateOrderProductUnitPrice(userId, parameters);
				var totalPrice = result.GetOrCreate(price.Currency, () => 0);
				totalPrice = checked(totalPrice + price.Parts.Sum() * cartProduct.Count);
				result[price.Currency] = totalPrice;
			}
			return result;
		}
	}
}