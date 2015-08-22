﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CefSharp.WinForms;
using HCDU.API;

namespace HCDU.Windows
{
    public class WinFormsPlatformAdapter : IPlatformAdapter
    {
        public string OpenFolderBrowserDialog(WindowHandle parent, bool allowCreateFolder)
        {
            Form parentForm = (Form) parent.NativeWindow;

            if (parentForm.InvokeRequired)
            {
                return (string) parentForm.Invoke(new Func<WindowHandle, bool, string>(OpenFolderBrowserDialog), parent, allowCreateFolder);
            }

            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.ShowNewFolderButton = allowCreateFolder;

            if (dlg.ShowDialog(parentForm) != DialogResult.OK)
            {
                return null;
            }

            return dlg.SelectedPath;
        }

        public WindowHandle CreateWindow(WindowPrototype prototype)
        {
            return ConstructDialog(prototype);
        }

        public WindowHandle ShowDialog(WindowHandle parent, WindowPrototype prototype)
        {
            Form parentForm = (Form) parent.NativeWindow;

            if (parentForm.InvokeRequired)
            {
                return (WindowHandle) parentForm.Invoke(new Func<WindowHandle, WindowPrototype, WindowHandle>(ShowDialog), parent, prototype);
            }

            WindowHandle handle = ConstructDialog(prototype);
            Form window = (Form) handle.NativeWindow;
            window.Closed += (sender, args) => prototype.OnClose(handle);

            //todo: use ShowDialog when CefSharp 43 is released (now it freezes the application)
            window.Show(parentForm);

            //todo: this would conflict with window.ShowDialog
            return handle;
        }

        public void CloseDialog(WindowHandle win)
        {
            Form form = (Form) win.NativeWindow;

            if (form.InvokeRequired)
            {
                form.Invoke(new Action<WindowHandle>(CloseDialog), win);
                return;
            }

            form.Close();
        }

        public void NavigateTo(WindowHandle window, string url)
        {
            ChromiumWebBrowser browser = (ChromiumWebBrowser) window.NativeBrowser;
            browser.Load(url);
        }

        public void ReloadPage(WindowHandle window)
        {
            ChromiumWebBrowser browser = (ChromiumWebBrowser) window.NativeBrowser;
            browser.Reload(true);
        }

        public void ShowDevTools(WindowHandle window)
        {
            ChromiumWebBrowser browser = (ChromiumWebBrowser) window.NativeBrowser;
            browser.ShowDevTools();
        }

        private WindowHandle ConstructDialog(WindowPrototype prototype)
        {
            Form form = new Form();
            form.Size = new Size(prototype.Width, prototype.Height);

            ChromiumWebBrowser webBrowser = new ChromiumWebBrowser("about:blank");

            //todo: usage of WindowHandle is a little bit cumbersome
            WindowHandle handle = new WindowHandle(form, webBrowser);

            //todo: is SuspendLayout/ResumeLayout required?
            form.SuspendLayout();

            int occupiedHeight = 0;
            if (prototype.Menu != null && prototype.Menu.Any())
            {
                MenuStrip menu = CreateMenu(handle, prototype.Menu);

                const int menuHeight = 24;
                menu.Name = "menu";
                menu.Size = new Size(form.ClientSize.Width, menuHeight);
                menu.Location = new Point(0, 0);
                menu.TabIndex = 0;
                form.Controls.Add(menu);
                occupiedHeight += menuHeight;
            }

            form.Controls.Add(webBrowser);

            webBrowser.Name = "webBrowser";
            webBrowser.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            webBrowser.Location = new Point(0, occupiedHeight);
            webBrowser.Size = new Size(form.ClientSize.Width, form.ClientSize.Height - occupiedHeight);
            webBrowser.TabIndex = 1;

            form.Controls.Add(webBrowser);

            //todo: is SuspendLayout/ResumeLayout required?
            form.ResumeLayout();

            webBrowser.TitleChanged += (sender, args) =>
                                       {
                                           if (form.InvokeRequired)
                                           {
                                               form.Invoke(new Action<string>(title => { form.Text = title; }), webBrowser.Title);
                                           }
                                       };

            webBrowser.Load(prototype.Url);

            return handle;
        }

        private MenuStrip CreateMenu(WindowHandle window, List<MenuPrototype> menuItems)
        {
            MenuStrip menu = new MenuStrip();

            foreach (MenuPrototype menuItem in menuItems)
            {
                ToolStripMenuItem menuI = new ToolStripMenuItem();
                menu.Items.Add(menuI);

                //todo: is null allowed here?
                menuI.Name = null;
                menuI.Size = new Size(0, 20);
                menuI.Text = menuItem.Text;

                ToolStripItem[] children = menuItem.Items.Select(si => CreateDropDownItems(window, si)).ToArray();
                menuI.DropDownItems.AddRange(children);
            }

            return menu;
        }

        private ToolStripItem CreateDropDownItems(WindowHandle window, MenuPrototype menuItemProt)
        {
            ToolStripItem menuItem = new ToolStripMenuItem();

            //todo: is null allowed here?
            menuItem.Name = null;
            menuItem.Size = new Size(0, 22);
            menuItem.Text = menuItemProt.Text;
            menuItem.Click += (sender, args) => menuItemProt.OnAction(window);

            return menuItem;
        }
    }
}