using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utils.StaticUtils
{
    public class AlertHelp
    {
        /// <summary>
        /// 信息提醒
        /// </summary>
        /// <param name="alertText">文本</param>
        public static DialogResult AlertInformation(string alertText)
        {
            return MessageBox.Show(alertText, "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 错误提醒
        /// </summary>
        /// <param name="alertText">文本</param>
        public static DialogResult AlertError(string alertText)
        {
            return MessageBox.Show(alertText, "提醒", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}