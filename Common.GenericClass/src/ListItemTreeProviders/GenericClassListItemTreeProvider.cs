﻿using DryIoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZKWeb.Localize;
using ZKWeb.Plugins.Common.Base.src.Model;
using ZKWeb.Plugins.Common.Base.src.Repositories;
using ZKWeb.Plugins.Common.GenericClass.src.Manager;
using ZKWeb.Plugins.Common.GenericClass.src.Scaffolding;
using ZKWeb.Utils.Extensions;
using ZKWeb.Utils.Functions;

namespace ZKWeb.Plugins.Common.GenericClass.src.ListItemProviders {
	/// <summary>
	/// 通用分类列表树的提供器
	/// </summary>
	public class GenericClassListItemTreeProvider<TClass> : IListItemTreeProvider
		where TClass : GenericClassBuilder, new() {
		/// <summary>
		/// 获取选项列表树
		/// </summary>
		/// <returns></returns>
		public ITreeNode<ListItem> GetTree() {
			var type = new TClass().Type;
			var manager = Application.Ioc.Resolve<GenericClassManager>();
			var classes = manager.GetClasses(type);
			var classMap = classes.ToDictionary(c => c.Id);
			var tree = TreeUtils.CreateTree(classes,
				c => new ListItem(c.Name, c.Id.ToString()),
				c => c.Parent == null ? null : classMap.GetOrDefault(c.Parent.Id));
			return tree;
		}
	}
}