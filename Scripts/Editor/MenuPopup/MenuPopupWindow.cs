using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace XNodeEditor
{
    /// <summary>
    /// Menu Popup Window
    /// </summary>
    public class MenuPopupWindow : PopupWindowContent
    {
        public Vector2 OpenBeforeMousePos;
        private SearchField _search;
        private MenuTreeView _menuTree;
        public Action OnCloseA;
        public MenuPopupWindow()
        {
            _search = new SearchField();
            _menuTree = new MenuTreeView();
        }

        private bool _isInit;
        
        /// <summary>
        /// Add Item
        /// </summary>
        /// <param name="menuPath">Item Path</param>
        /// <param name="onClick"></param>
        /// <param name="symbol">Path separator</param>
        /// <param name="autoClose">Automatically close window after selecting an item</param>
        public void AddItem(string menuPath, Action onClick, char symbol = '/',bool autoClose = true)
        {
            _menuTree.AddItem(menuPath, () =>
            {
                onClick?.Invoke();
                if (autoClose)
                {
                    editorWindow.Close();
                }
            },symbol);
        }

        /// <summary>
        /// Init or Reload Tree
        /// </summary>
        public void Init()
        {
            _menuTree.Reload();
            _isInit = true;
        }

        public override void OnOpen()
        {
            _search.SetFocus();
        }

        public override void OnClose()
        {
            OnCloseA?.Invoke();
        }

        private string _str;
        public override void OnGUI(Rect rect)
        {
            if (!_isInit)
            {
                Init();
            }

            _action();
            
            EditorGUI.BeginChangeCheck();
            {
                _str = _search.OnGUI(new Rect(rect.position, new Vector2(rect.width, 20)),_str);
            }
            if (EditorGUI.EndChangeCheck())
            {
                _menuTree.searchString = _str;
            }
            
            _menuTree.OnGUI(new Rect(new Vector2(0,25),rect.size - new Vector2(0,20)));
        }
        
        private void _action()
        {
            Event e = Event.current;
            switch (e.type)
            {
                case EventType.KeyDown:
                    
                    if (e.keyCode == KeyCode.DownArrow && !_menuTree.HasFocus())
                    {
                        _menuTree.SetFocusAndEnsureSelectedItem();
                        e.Use();
                    }
                    break;
            }
        }
    }
}