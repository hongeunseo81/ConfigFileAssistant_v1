using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConfigFileAssistant_v1
{
    public class MenuManager
    {
        public ContextMenuStrip CreateContextMenu()
        {
            var contextMenuStrip = new ContextMenuStrip();
            var addChildMenuItem = new ToolStripMenuItem("Add Child");
            var addRowMenuItem = new ToolStripMenuItem("Add Row");
            var deleteRowMenuItem = new ToolStripMenuItem("Delete Row");

            contextMenuStrip.Items.AddRange(new ToolStripItem[] { addChildMenuItem, addRowMenuItem, deleteRowMenuItem });

            addChildMenuItem.Click += (sender, e) => AddChildMenuItem_Click();
            addRowMenuItem.Click += (sender, e) => AddRowMenuItem_Click();
            deleteRowMenuItem.Click += (sender, e) => DeleteRowMenuItem_Click();

            return contextMenuStrip;
        }

        private void AddChildMenuItem_Click()
        {
            // Handle add child menu item click
        }

        private void AddRowMenuItem_Click()
        {
            // Handle add row menu item click
        }

        private void DeleteRowMenuItem_Click()
        {
            // Handle delete row menu item click
        }
    }

}
