using System.Collections.Generic;

namespace GeomPad.Common
{
    public interface IPad2DDataModel
    {
        IHelperItem SelectedItem { get; }
        IHelperItem[] SelectedItems { get; }
        List<IHelperItem> Items { get; }
    }
}
