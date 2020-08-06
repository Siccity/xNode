using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace XNodeEditor
{
    public class MenuTreeView:TreeView
    {
        class MenuItem:TreeViewItem
        {
            public readonly Action OnClick;

            public MenuItem(int id, int depth, string displayName, Action onClick) : base(id, depth, displayName)
            {
                OnClick = onClick;
            }
        }

        public TreeViewItem Root { get; }

        public MenuTreeView():this(new TreeViewState())
        {
        }
        
        public MenuTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader = null) : base(state, multiColumnHeader)
        {
            Root = new TreeViewItem(id++,-1,nameof(Root));
        }

        private int id = -1;

        private Dictionary<int, List<string>> _menuCache = new Dictionary<int, List<string>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="menuPath"></param>
        /// <param name="onClick"></param>
        /// <param name="symbol"></param>
        public void AddItem(string menuPath,Action onClick,char symbol = '/')
        {
            var paths = menuPath.Split(symbol);
            
            int depth = 0;

            TreeViewItem last = Root;

            if (paths.Length > 1)
            {
                for (var i = 0; i < paths.Length - 1; i++)
                {
                    var path = paths[i];
                    
                    if (!_menuCache.TryGetValue(depth, out var caches))
                    {
                        caches = new List<string>();
                        _menuCache.Add(depth, caches);
                    }

                    while (true)
                    {
                        if (last.hasChildren)
                        {
                            foreach (var item in last.children)
                            {
                                if (item.displayName == path)
                                {
                                    last = item;
                                    depth++;
                                    goto end;
                                }                                    
                            }
                        }

                        break;
                    }

                    if (last.hasChildren)
                    {
                        foreach (var child in last.children)
                        {
                            if (child.displayName == path)
                            {
                                return;
                            }
                        }
                    }
                    
                    TreeViewItem temp = new TreeViewItem(id++,depth++,path);
                    
                    last.AddChild(temp);
                    
                    last = temp;
                    
                    end: ;
                }
            }
            
            last.AddChild(new MenuItem(id++,depth,paths.Last(),onClick));
        }
        
        protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
        {
            if (item.parent != null && item.parent.displayName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
            
            return base.DoesItemMatchSearch(item, search);
        }
        
        List<int> ids = new List<int>();
        protected override void DoubleClickedItem(int id)
        {
            TreeViewItem item = FindItem(id,Root);
            if (item.hasChildren)
            {
                if (hasSearch)
                {
                    searchString = "";

                    ids.Clear();

                    while (item != null)
                    {
                        ids.Add(item.id);
                        item = item.parent;
                    }
                    
                    SetExpanded(ids);
                }
                else
                {
                    SetExpanded(id, !IsExpanded(id));
                }
            }
            else
            {
                if (item is MenuItem menuItem)
                {
                    menuItem.OnClick?.Invoke();
                }
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            return Root;
        }

        protected override void KeyEvent()
        {
            Event e = Event.current;
            switch (e.type)
            {
                case EventType.KeyDown:
                    
                    if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                    {
                        DoubleClickedItem(GetSelection()[0]);
                        e.Use();
                    }
                    break;
            }
        }
    }
}