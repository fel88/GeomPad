using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GeomPad
{
    public static class Stuff
    {

        public static void Invoke(Control ctrl, Action act)
        {
            if (ctrl.InvokeRequired)
            {
                ctrl.Invoke(act);
            }
            else
            {
                act();
            }
        }

        public static List<ScriptRunInfo> Scripts = new List<ScriptRunInfo>();

        public static void SetStatus(ToolStripLabel ctrl, string text, StatusTypeEnum status)
        {

            Invoke(ctrl.Owner, () =>
            {

                ctrl.Text = DateTime.Now.ToLongTimeString() + ":" + text;
                switch (status)
                {
                    case StatusTypeEnum.Error:
                        ctrl.BackColor = Color.Red;
                        ctrl.ForeColor = Color.White;
                        break;
                    case StatusTypeEnum.Success:
                        ctrl.BackColor = Color.Green;
                        ctrl.ForeColor = Color.White;
                        break;
                    case StatusTypeEnum.Info:
                        ctrl.BackColor = Color.LightGray;
                        ctrl.ForeColor = Color.Black;
                        break;
                    case StatusTypeEnum.Warning:
                        ctrl.BackColor = Color.Yellow;
                        ctrl.ForeColor = Color.Blue;
                        break;
                }
            });

        }

    }
}