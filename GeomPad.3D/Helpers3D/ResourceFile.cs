using System.IO;
using System.Linq;
using System.Reflection;

namespace GeomPad.Helpers3D
{
    public static class ResourceFile
    {
        public static string GetFileText(string name, Assembly assembly = null)
        {

            if (assembly == null)
            {
                assembly = Assembly.GetEntryAssembly();
            }

            //var asm = Assembly.GetAssembly(typeof(SdfShader));

            var nms = assembly.GetManifestResourceNames();
            string ret = "";
            var nfr = nms.First(z => z.ToLower().Contains(name.ToLower()));
            //resourceName = "FxEngine.Graphics.Shaders.sdf.vs";
            name = nfr;
            using (Stream stream = assembly.GetManifestResourceStream(name))
            using (StreamReader reader = new StreamReader(stream))
            {
                ret = reader.ReadToEnd();
            }
            return ret;
        }
        public static byte[] GetFile(string name, Assembly assembly = null)
        {

            if (assembly == null)
            {
                assembly = Assembly.GetEntryAssembly();
            }

            //var asm = Assembly.GetAssembly(typeof(SdfShader));

            var nms = assembly.GetManifestResourceNames();
            string ret = "";
            var nfr = nms.First(z => z.ToLower().Contains(name.ToLower()));
            //resourceName = "FxEngine.Graphics.Shaders.sdf.vs";
            name = nfr;
            MemoryStream ms = new MemoryStream();
            using (Stream stream = assembly.GetManifestResourceStream(name))
            {
                stream.CopyTo(ms);

            }
            return ms.ToArray();
        }
    }
}
