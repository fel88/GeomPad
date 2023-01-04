using GeomPad.Common;
using GeomPad.Controls._2d;
using System;
using System.CodeDom.Compiler;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace GeomPad
{
    public partial class ScriptEditor2D : Form
    {
        public ScriptEditor2D()
        {
            InitializeComponent();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;

            richTextBox1.Text = File.ReadAllText(ofd.FileName);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() != DialogResult.OK) return;

            File.WriteAllText(sfd.FileName, richTextBox1.Text);
        }

        void SetStatus(string text, StatusTypeEnum type)
        {
            Stuff.SetStatus(toolStripStatusLabel1, text, type);
        }
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            listView5.Items.Clear();
            var res = CsharpCompiler.CompileCodeInMem(richTextBox1.Text);

            var ll = res.Errors.Cast<CompilerError>().ToList();
            foreach (var compilerError in ll)
            {
                var lvi = new ListViewItem(new string[] { compilerError.Line + "", compilerError.ErrorText }) { BackColor = Color.Yellow, ForeColor = Color.Blue, Tag = compilerError };
                if (!compilerError.IsWarning)
                {
                    lvi.BackColor = Color.LightPink;
                    lvi.ForeColor = Color.DarkBlue;
                }
                listView5.Items.Add(lvi);
            }

            if (ll.Any())
            {
                SetStatus("compile complete with " + ll.Count + " errors.", StatusTypeEnum.Error);
            }
            else
            {
                SetStatus("compile compete successfull!", StatusTypeEnum.Success);
                AssemblyName assemblyName = new AssemblyName();
                assemblyName.CodeBase = res.PathToAssembly;

                var tps = res.CompiledAssembly.GetTypes();
                if (tps.Length == 0)
                {
                    SetStatus("Empty code", StatusTypeEnum.Warning);
                    return;
                }
                var type = tps.First();
                var script = Activator.CreateInstance(type) as IScript;
                if (script == null)
                {
                    SetStatus("IScript not detected", StatusTypeEnum.Error);
                    return;
                }
                lastCompiledScript = script;
            }
        }

        IScript lastCompiledScript;
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (lastCompiledScript == null)
            {
                SetStatus("compile script first", StatusTypeEnum.Warning);
                return;
            }
            string result = null;
            if (lastCompiledScript.GetType().GetCustomAttribute(typeof(ScriptAttribute)) != null)
            {
                var sat = lastCompiledScript.GetType().GetCustomAttribute(typeof(ScriptAttribute)) as ScriptAttribute;
                result = sat.Name;
            }
            else
            {
                result = DialogHelpers.StringDialog("Enter name");
                if (string.IsNullOrEmpty(result))
                    return;
            }

            if (Stuff.Scripts.Any(z => z.Name == result))
            {
                if (GuiHelpers.Question($"Script with such name ({result}) already exists. Do you want to replace?", Text))
                {
                    Stuff.Scripts.First(z => z.Name == result).Script = lastCompiledScript;
                }
            }
            else
                Stuff.Scripts.Add(new ScriptRunInfo() { Script = lastCompiledScript, Name = result });
        }

        private void sample1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!GuiHelpers.Question("Are you sure to load sample1?", Text))
                return;

            richTextBox1.Lines = new[] { "using GeomPad.Common;" ,
                "[Script(Name=\"sampleScript1\")]",
                "class SampleScript1:IScript{" ,
                "public void Run(IPad2DDataModel model, IPadContainer pad){",
                "",
                "}",
                "}" };
        }

        IPad2DDataModel Model;
        internal void Init(Pad2DDataModel dataModel)
        {
            Model = dataModel;
        }
    }
}