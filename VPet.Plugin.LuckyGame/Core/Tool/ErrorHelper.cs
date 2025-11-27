using Panuon.WPF.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VPet.Plugin.LuckyGame.Core.Tool
{
    public static class ErrorHelper
    {
        /// <summary>
        /// 显示异常信息（增强版本）
        /// </summary>
        public static void ShowError(Exception ex,
            string userFriendlyMessage = null,
            bool showDetails = true,
            ErrorLevel level = ErrorLevel.Error)
        {
            if (ex == null) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                string message = BuildErrorMessage(ex, userFriendlyMessage, showDetails);
                string caption = GetCaption(level);
                MessageBoxIcon icon = GetMessageBoxImage(level);

                MessageBoxX.Show(message: message,caption: caption,button: MessageBoxButton.OK,icon: icon);
            });
        }

        /// <summary>
        /// 异步显示错误信息（避免UI阻塞）
        /// </summary>
        public static async Task ShowErrorAsync(Exception ex,
            string userFriendlyMessage = null,
            bool showDetails = false)
        {
            await Task.Run(() =>
            {
                ShowError(ex, userFriendlyMessage, showDetails);
            });
        }

        /// <summary>
        /// 安全执行方法并捕获异常
        /// </summary>
        public static bool ExecuteSafely(Action action,
            string userFriendlyMessage = null,
            bool showDetails = false)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                ShowError(ex, userFriendlyMessage, showDetails);
                return false;
            }
        }

        /// <summary>
        /// 安全执行异步方法并捕获异常
        /// </summary>
        public static async Task<bool> ExecuteSafelyAsync(Func<Task> asyncAction,
            string userFriendlyMessage = null,
            bool showDetails = false)
        {
            try
            {
                await asyncAction();
                return true;
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex, userFriendlyMessage, showDetails);
                return false;
            }
        }

        /// <summary>
        /// 安全执行方法并返回结果
        /// </summary>
        public static T ExecuteSafely<T>(Func<T> func,
            T defaultValue = default(T),
            string userFriendlyMessage = null,
            bool showDetails = false)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                ShowError(ex, userFriendlyMessage, showDetails);
                return defaultValue;
            }
        }

        private static string BuildErrorMessage(Exception ex, string userFriendlyMessage, bool showDetails)
        {
            var sb = new StringBuilder();

            // 用户友好信息
            if (!string.IsNullOrEmpty(userFriendlyMessage))
            {
                sb.AppendLine(userFriendlyMessage);
            }
            else
            {
                sb.AppendLine("操作过程中发生错误，请重试或联系管理员。");
            }

            // 详细错误信息
            if (showDetails)
            {
                sb.AppendLine();
                sb.AppendLine("错误详情：");
                sb.AppendLine($"异常类型: {ex.GetType().Name}");
                sb.AppendLine($"错误信息: {ex.Message}");

                if (!string.IsNullOrEmpty(ex.StackTrace))
                {
                    sb.AppendLine();
                    sb.AppendLine("堆栈跟踪：");
                    sb.AppendLine(ex.StackTrace);
                }

                AppendInnerExceptions(ex, sb);
            }

            return sb.ToString();
        }

        private static void AppendInnerExceptions(Exception ex, StringBuilder sb)
        {
            Exception innerEx = ex.InnerException;
            int innerLevel = 1;

            while (innerEx != null && innerLevel <= 3)
            {
                sb.AppendLine();
                sb.AppendLine($"内部异常 #{innerLevel}:");
                sb.AppendLine($"异常类型: {innerEx.GetType().Name}");
                sb.AppendLine($"错误信息: {innerEx.Message}");

                innerEx = innerEx.InnerException;
                innerLevel++;
            }
        }

        private static string GetCaption(ErrorLevel level)
        {
            return level switch
            {
                ErrorLevel.Info => "信息提示",
                ErrorLevel.Warning => "警告",
                ErrorLevel.Error => "错误提示",
                _ => "系统提示"
            };
        }

        private static MessageBoxIcon GetMessageBoxImage(ErrorLevel level)
        {
            return level switch
            {
                ErrorLevel.Info => MessageBoxIcon.Info,
                ErrorLevel.Warning => MessageBoxIcon.Warning,
                ErrorLevel.Error => MessageBoxIcon.Error,
                _ => MessageBoxIcon.Info
            };
        }
    }

    /// <summary>
    /// 错误级别
    /// </summary>
    public enum ErrorLevel
    {
        Info,
        Warning,
        Error
    }
}
