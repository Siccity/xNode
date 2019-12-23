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
        public Vector2 openBeforeMousePos;
        private SearchField search;
        private MenuTreeView menuTree;
        public Action onCloseAction;
        public MenuPopupWindow()
        {
            search = new SearchField();
            menuTree = new MenuTreeView();
        }

        private bool isInit;
        
        /// <summary>
        /// Add Item
        /// </summary>
        /// <param name="menuPath">Item Path</param>
        /// <param name="onClick"></param>
        /// <param name="symbol">Path separator</param>
        /// <param name="autoClose">Automatically close window after selecting an item</param>
        public void AddItem(string menuPath, Action onClick, char symbol = '/',bool autoClose = true)
        {
            menuTree.AddItem(menuPath, () =>
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
            menuTree.Reload();
            isInit = true;
        }

        public override void OnOpen()
        {
            search.SetFocus();
        }

        public override void OnClose()
        {
            onCloseAction?.Invoke();
        }

        private string _str;
        public override void OnGUI(Rect rect)
        {
            if (!isInit)
            {
                Init();
            }

            EventAction();
            
            EditorGUI.BeginChangeCheck();
            {
                _str = search.OnGUI(new Rect(rect.position, new Vector2(rect.width, 20)),_str);
            }
            if (EditorGUI.EndChangeCheck())
            {
                menuTree.searchString = _str;
            }
            
            menuTree.OnGUI(new Rect(new Vector2(0,25),rect.size - new Vector2(0,20)));
        }
        
        private void EventAction()
        {
            Event e = Event.current;
            switch (e.type)
            {
                case EventType.KeyDown:
                    
                    if (e.keyCode == KeyCode.DownArrow && !menuTree.HasFocus())
                    {
                        menuTree.SetFocusAndEnsureSelectedItem();
                        e.Use();
                    }
                    break;
            }
        }
    }
}