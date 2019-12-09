using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace XNodeEditor {
    public class MenuPopupWindow : PopupWindowContent
    {
        private SearchField _search;
        private MenuTreeView _menuTree;
        public Action OnCloseA;
        public MenuPopupWindow()
        {
            _search = new SearchField();
            _menuTree = new MenuTreeView();
        }

        private bool _isInit;
        
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
            Root = new TreeViewItem(_id++,-1,nameof(Root));
        }

        private int _id = -1;

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
                    
                    var temp = new TreeViewItem(_id++,depth++,path);
                    
                    last.AddChild(temp);
                    
                    last = temp;
                    
                    end: ;
                }
            }
            
            last.AddChild(new MenuItem(_id++,depth,paths.Last(),onClick));
        }
        
        protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
        {
            if (item.parent != null && item.parent.displayName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
            
            return base.DoesItemMatchSearch(item, search);
        }
        
        List<int> _ids = new List<int>();
        protected override void DoubleClickedItem(int id)
        {
            var item = FindItem(id,Root);
            if (item.hasChildren)
            {
                if (hasSearch)
                {
                    searchString = "";

                    _ids.Clear();

                    while (item != null)
                    {
                        _ids.Add(item.id);
                        item = item.parent;
                    }
                    
                    SetExpanded(_ids);
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
    
    /// <summary> Base class to derive custom Node Graph editors from. Use this to override how graphs are drawn in the editor. </summary>
    [CustomNodeGraphEditor(typeof(XNode.NodeGraph))]
    public class NodeGraphEditor : XNodeEditor.Internal.NodeEditorBase<NodeGraphEditor, NodeGraphEditor.CustomNodeGraphEditorAttribute, XNode.NodeGraph> {
        [Obsolete("Use window.position instead")]
        public Rect position { get { return window.position; } set { window.position = value; } }
        /// <summary> Are we currently renaming a node? </summary>
        protected bool isRenaming;

        public virtual void OnGUI() { }

        /// <summary> Called when opened by NodeEditorWindow </summary>
        public virtual void OnOpen() { }

        public virtual Texture2D GetGridTexture() {
            return NodeEditorPreferences.GetSettings().gridTexture;
        }

        public virtual Texture2D GetSecondaryGridTexture() {
            return NodeEditorPreferences.GetSettings().crossTexture;
        }

        /// <summary> Return default settings for this graph type. This is the settings the user will load if no previous settings have been saved. </summary>
        public virtual NodeEditorPreferences.Settings GetDefaultPreferences() {
            return new NodeEditorPreferences.Settings();
        }

        /// <summary> Returns context node menu path. Null or empty strings for hidden nodes. </summary>
        public virtual string GetNodeMenuName(Type type) {
            //Check if type has the CreateNodeMenuAttribute
            XNode.Node.CreateNodeMenuAttribute attrib;
            if (NodeEditorUtilities.GetAttrib(type, out attrib)) // Return custom path
                return attrib.menuName;
            else // Return generated path
                return NodeEditorUtilities.NodeDefaultPath(type);
        }

        /// <summary> Add items for the context menu when right-clicking this node. Override to add custom menu items. </summary>
        public virtual void AddContextMenuItems(MenuPopupWindow menu) {
            Vector2 pos = NodeEditorWindow.current.WindowToGridPosition(Event.current.mousePosition);
            for (int i = 0; i < NodeEditorReflection.nodeTypes.Length; i++) {
                Type type = NodeEditorReflection.nodeTypes[i];

                //Get node context menu path
                string path = GetNodeMenuName(type);
                if (string.IsNullOrEmpty(path)) continue;

                menu.AddItem(path, () => {
                    XNode.Node node = CreateNode(type, pos);
                    NodeEditorWindow.current.AutoConnect(node);
                });
            }
//            menu.AddSeparator("");
            if (NodeEditorWindow.copyBuffer != null && NodeEditorWindow.copyBuffer.Length > 0) 
                menu.AddItem("Paste", () => NodeEditorWindow.current.PasteNodes(pos));
//            else menu.AddDisabledItem(new GUIContent("Paste"));
            menu.AddItem("Preferences", () => NodeEditorReflection.OpenPreferences());
            menu.AddItem("创建所有的节点 ---> 测试用", () =>
            {
                if (!EditorUtility.DisplayDialog("warning","Are you sure you want to create all the nodes?","ok","no"))
                {
                    return;
                }

                for (int i = 0; i < NodeEditorReflection.nodeTypes.Length; i++)
                {
                    Type type = NodeEditorReflection.nodeTypes[i];

                    //Get node context menu path
                    string path = GetNodeMenuName(type);
                    //当前Group 不支持该节点跳过
                    if (string.IsNullOrEmpty(path))
                    {
                        continue;
                    }

                    XNode.Node node = CreateNode(type, pos);
                    NodeEditorWindow.current.AutoConnect(node);
                }
            });
            menu.AddCustomContextMenuItems(target);
        }

        /// <summary> Returned gradient is used to color noodles </summary>
        /// <param name="output"> The output this noodle comes from. Never null. </param>
        /// <param name="input"> The output this noodle comes from. Can be null if we are dragging the noodle. </param>
        public virtual Gradient GetNoodleGradient(XNode.NodePort output, XNode.NodePort input) {
            Gradient grad = new Gradient();

            // If dragging the noodle, draw solid, slightly transparent
            if (input == null) {
                Color a = GetTypeColor(output.ValueType);
                grad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(a, 0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(0.6f, 0f) }
                );
            }
            // If normal, draw gradient fading from one input color to the other
            else {
                Color a = GetTypeColor(output.ValueType);
                Color b = GetTypeColor(input.ValueType);
                // If any port is hovered, tint white
                if (window.hoveredPort == output || window.hoveredPort == input) {
                    a = Color.Lerp(a, Color.white, 0.8f);
                    b = Color.Lerp(b, Color.white, 0.8f);
                }
                grad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(a, 0f), new GradientColorKey(b, 1f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
                );
            }
            return grad;
        }

        /// <summary> Returned float is used for noodle thickness </summary>
        /// <param name="output"> The output this noodle comes from. Never null. </param>
        /// <param name="input"> The output this noodle comes from. Can be null if we are dragging the noodle. </param>
        public virtual float GetNoodleThickness(XNode.NodePort output, XNode.NodePort input) {
            return 5f;
        }

        public virtual NoodlePath GetNoodlePath(XNode.NodePort output, XNode.NodePort input) {
            return NodeEditorPreferences.GetSettings().noodlePath;
        }

        public virtual NoodleStroke GetNoodleStroke(XNode.NodePort output, XNode.NodePort input) {
            return NodeEditorPreferences.GetSettings().noodleStroke;
        }

        /// <summary> Returned color is used to color ports </summary>
        public virtual Color GetPortColor(XNode.NodePort port) {
            return GetTypeColor(port.ValueType);
        }

        /// <summary> Returns generated color for a type. This color is editable in preferences </summary>
        public virtual Color GetTypeColor(Type type) {
            return NodeEditorPreferences.GetTypeColor(type);
        }

        /// <summary> Override to display custom tooltips </summary>
        public virtual string GetPortTooltip(XNode.NodePort port) {
            Type portType = port.ValueType;
            string tooltip = "";
            tooltip = portType.PrettyName();
            if (port.IsOutput) {
                object obj = port.node.GetValue(port);
                tooltip += " = " + (obj != null ? obj.ToString() : "null");
            }
            return tooltip;
        }

        /// <summary> Deal with objects dropped into the graph through DragAndDrop </summary>
        public virtual void OnDropObjects(UnityEngine.Object[] objects) {
            Debug.Log("No OnDropObjects override defined for " + GetType());
        }

        /// <summary> Create a node and save it in the graph asset </summary>
        public virtual XNode.Node CreateNode(Type type, Vector2 position) {
            Undo.RecordObject(target, "Create Node");
            XNode.Node node = target.AddNode(type);
            Undo.RegisterCreatedObjectUndo(node, "Create Node");
            node.position = position;
            if (node.name == null || node.name.Trim() == "") node.name = NodeEditorUtilities.NodeDefaultName(type);
            AssetDatabase.AddObjectToAsset(node, target);
            if (NodeEditorPreferences.GetSettings().autoSave) AssetDatabase.SaveAssets();
            NodeEditorWindow.RepaintAll();
            return node;
        }

        /// <summary> Creates a copy of the original node in the graph </summary>
        public XNode.Node CopyNode(XNode.Node original) {
            Undo.RecordObject(target, "Duplicate Node");
            XNode.Node node = target.CopyNode(original);
            Undo.RegisterCreatedObjectUndo(node, "Duplicate Node");
            node.name = original.name;
            AssetDatabase.AddObjectToAsset(node, target);
            if (NodeEditorPreferences.GetSettings().autoSave) AssetDatabase.SaveAssets();
            return node;
        }

        /// <summary> Safely remove a node and all its connections. </summary>
        public virtual void RemoveNode(XNode.Node node) {
            Undo.RecordObject(node, "Delete Node");
            Undo.RecordObject(target, "Delete Node");
            foreach (var port in node.Ports)
                foreach (var conn in port.GetConnections())
                    Undo.RecordObject(conn.node, "Delete Node");
            target.RemoveNode(node);
            Undo.DestroyObjectImmediate(node);
            if (NodeEditorPreferences.GetSettings().autoSave) AssetDatabase.SaveAssets();
        }

        [AttributeUsage(AttributeTargets.Class)]
        public class CustomNodeGraphEditorAttribute : Attribute,
        XNodeEditor.Internal.NodeEditorBase<NodeGraphEditor, NodeGraphEditor.CustomNodeGraphEditorAttribute, XNode.NodeGraph>.INodeEditorAttrib {
            private Type inspectedType;
            public string editorPrefsKey;
            /// <summary> Tells a NodeGraphEditor which Graph type it is an editor for </summary>
            /// <param name="inspectedType">Type that this editor can edit</param>
            /// <param name="editorPrefsKey">Define unique key for unique layout settings instance</param>
            public CustomNodeGraphEditorAttribute(Type inspectedType, string editorPrefsKey = "xNode.Settings") {
                this.inspectedType = inspectedType;
                this.editorPrefsKey = editorPrefsKey;
            }

            public Type GetInspectedType() {
                return inspectedType;
            }
        }
    }
}