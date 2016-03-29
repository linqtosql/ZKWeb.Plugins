﻿using DryIoc;
using DryIocAttributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using ZKWeb.Plugins.Common.Base.src.Model;
using ZKWeb.Plugins.Shopping.Product.src.FormFieldAttributes;
using ZKWeb.Plugins.Shopping.Product.src.Managers;
using ZKWeb.Plugins.Shopping.Product.src.Model;
using ZKWeb.Templating;
using ZKWeb.Utils.Extensions;

namespace ZKWeb.Plugins.Shopping.Product.src.FormFieldHandlers {
	/// <summary>
	/// 商品相册上传器
	/// </summary>
	[ExportMany(ContractKey = typeof(ProductAlbumUploaderAttribute)), SingletonReuse]
	public class ProductAlbumUploader : IFormFieldHandler {
		/// <summary>
		/// 获取表单字段的html
		/// </summary>
		public string Build(FormField field, Dictionary<string, string> htmlAttributes) {
			var attribute = (ProductAlbumUploaderAttribute)field.Attribute;
			var html = new StringBuilder();
			var value = (ProductAlbumUploadData)field.Value;
			var templateManger = Application.Ioc.Resolve<TemplateManager>();
			var albumManager = Application.Ioc.Resolve<ProductAlbumManager>();
			for (int x = 1; x <= ProductAlbumUploadData.MaxImageCount; ++x) {
				html.Append(templateManger.RenderTemplate("shopping.product/tmpl.album_uploader.html", new {
					prefix = attribute.Name,
					image_url = value.ImageUrls[x - 1],
					index = x,
					is_main_image = value.MainImageIndex == x
				}));
			}
			return html.ToString();
		}

		/// <summary>
		/// 解析提交的字段的值
		/// </summary>
		public object Parse(FormField field, string value) {
			var data = new ProductAlbumUploadData();
			var attribute = (ProductAlbumUploaderAttribute)field.Attribute;
			var request = HttpContext.Current.Request;
			data.MainImageIndex = request.GetParam<long>(attribute.Name + "_MainImageIndex");
			for (int x = 1; x <= ProductAlbumUploadData.MaxImageCount; ++x) {
				var image = request.Files[attribute.Name + "_Image_" + x];
				var deleteImage = request.GetParam<bool>(attribute.Name + "_DeleteImage_" + x);
				data.UploadedImages.Add(image == null ? null : new HttpPostedFileWrapper(image));
				data.DeleteImages.Add(deleteImage);
			}
			return data;
		}
	}
}