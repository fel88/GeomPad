﻿using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace GeomPad.Helpers3D
{
    public class LineHelper : HelperItem, ICommandsContainer
    {
        [EditField]
        public Vector3d Start;
        [EditField]
        public Vector3d End;


        public Line3D Get3DLine()
        {
            return new Line3D() { Start = Start, End = End }; 
        }
        public LineHelper() { }
        public LineHelper(XElement item)
        {
            var pos = item.Attribute("start").Value.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries).Select(z => double.Parse(z.Replace(",", "."), CultureInfo.InvariantCulture)).ToArray();
            Start = new Vector3d(pos[0], pos[1], pos[2]);
            var nrm = item.Attribute("end").Value.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries).Select(z => double.Parse(z.Replace(",", "."), CultureInfo.InvariantCulture)).ToArray();
            End = new Vector3d(nrm[0], nrm[1], nrm[2]);
            if (item.Attribute("drawSize") != null)
                DrawSize = float.Parse(item.Attribute("drawSize").Value.Replace(",", "."));
        }

        public float DrawSize { get; set; } = 2;

        public ICommand[] Commands => new ICommand[] { new Line3DExpandAlongCommand(), new Line3DSwitchStartEndCommand() };
        public Vector3d Dir
        {
            get
            {
                return (End - Start).Normalized();
            }
        }

        public double Length
        {
            get
            {
                return (End - Start).Length;
            }
            set
            {
                End = Start + Dir * value;
            }
        }

        [EditField]
        public bool ShowCrosses { get; set; } = true;

        public override void Draw(IDrawingContext ctx)
        {
            if (!Visible) return;
            GL.Color3(Color.Blue);
            if (Selected)
                GL.Color3(Color.Red);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(Start);
            GL.Vertex3(End);
            GL.End();

            if (ShowCrosses)
            {
                DrawHelpers.DrawCross(Start, DrawSize);
                DrawHelpers.DrawCross(End, DrawSize);
            }

        }

        public override void AppendToXml(StringBuilder sb)
        {
            sb.AppendLine($"<line start=\"{Start.X};{Start.Y};{Start.Z}\" end=\"{End.X};{End.Y};{End.Z}\" drawSize=\"{DrawSize}\"/>");
        }
        public class Line3DExpandAlongCommand : ICommand
        {
            public string Name => "expand along direction";

            public Action<AbstractHelperItem, AbstractHelperItem[], IPadContainer> Process => (z, arr, cc) =>
            {
                var ln = (z as LineHelper);
                ln.Start += -ln.Dir * 100;
                ln.End += ln.Dir * 100;
            };
        }

        public class Line3DSwitchStartEndCommand : ICommand
        {
            public string Name => "switch start and end";

            public Action<AbstractHelperItem, AbstractHelperItem[], IPadContainer> Process => (z, arr, cc) =>
            {
                var ln = (z as LineHelper);
                var t = ln.Start;
                ln.Start = ln.End;
                ln.End = t;
            };
        }
    }

    
}