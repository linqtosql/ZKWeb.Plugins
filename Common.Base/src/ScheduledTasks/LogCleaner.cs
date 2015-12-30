﻿using DryIoc;
using DryIocAttributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZKWeb.Core;
using ZKWeb.Model;
using ZKWeb.Plugins.Common.Base.src.Model;

namespace ZKWeb.Plugins.Common.Base.src.ScheduledTasks {
	/// <summary>
	/// 日志清理器
	/// 每小时删除一次旧的日志
	/// 日志保留时间
	///		Debug 3天
	///		Info 3天
	///		Error 30天
	///		Transaction 30天
	/// </summary>
	[ExportMany]
	public class LogCleaner : IScheduledTaskExecutor {
		/// <summary>
		/// 任务键名
		/// </summary>
		public string Key { get { return "Common.Base.LogCleaner"; } }

		/// <summary>
		/// 每小时执行一次
		/// </summary>
		/// <param name="lastExecuted"></param>
		/// <returns></returns>
		public bool ShouldExecuteNow(DateTime lastExecuted) {
			return ((DateTime.UtcNow - lastExecuted).TotalHours > 1.0);
		}

		/// <summary>
		/// 删除过期的日志
		/// </summary>
		public void Execute() {
			var now = DateTime.UtcNow.ToLocalTime();
			var count = 0;
			var logsDirectory = PathConfig.LogsDirectory;
			if (!Directory.Exists(logsDirectory)) {
				return;
			}
			foreach (var path in Directory.EnumerateFiles(logsDirectory)) {
				// 从文件名获取日志等级和记录日期
				var filename = Path.GetFileName(path);
				var parts = filename.Split('.');
				if (parts.Length != 3 || parts[2] != "log") {
					continue;
				}
				DateTime createTime;
				if (!DateTime.TryParseExact(parts[1],
					"yyyyMMdd", null, DateTimeStyles.None, out createTime)) {
					continue;
				}
				// 判断保留时间，超过时删除
				var timeSpan = now - createTime;
				if (((parts[0] == "Debug" || parts[0] == "Info") && timeSpan.TotalDays > 3) ||
					((parts[0] == "Error" || parts[0] == "Transaction") && timeSpan.TotalDays > 30)) {
					File.Delete(path);
					++count;
				}
			}
			var logManager = Application.Ioc.Resolve<LogManager>();
			logManager.LogInfo(string.Format(
				"LogCleaner executed, {0} files removed", count));
		}
	}
}