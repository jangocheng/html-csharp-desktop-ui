using System;
using System.Collections.Generic;
using System.Linq;

namespace HCDU.API
{
    //todo: rename?
    public class WindowPrototype : ICloneable
    {
        public string Url { get; set; }
        public List<MenuPrototype> Menu { get; private set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public Action<WindowHandle> OnClose { get; set; }

        public WindowPrototype()
        {
            Menu = new List<MenuPrototype>();
            Width = 1280;
            Height = 800;
        }

        public virtual object Clone()
        {
            WindowPrototype prot = (WindowPrototype) MemberwiseClone();
            prot.Menu = Menu.Select(mi => (MenuPrototype) mi.Clone()).ToList();
            return prot;
        }
    }

    //todo: rename?
    public class MenuPrototype : ICloneable
    {
        public string Text { get; set; }
        //todo: add method to check disabled state
        public Action<WindowHandle> OnAction { get; set; }

        public List<MenuPrototype> Items { get; private set; }

        public MenuPrototype()
        {
            Items = new List<MenuPrototype>();
        }

        public virtual object Clone()
        {
            MenuPrototype prot = (MenuPrototype) MemberwiseClone();
            prot.Items = Items.Select(mi => (MenuPrototype) mi.Clone()).ToList();
            return prot;
        }
    }
}