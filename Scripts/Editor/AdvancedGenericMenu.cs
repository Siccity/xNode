#if UNITY_2019_1_OR_NEWER
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using static UnityEditor.GenericMenu;

namespace XNodeEditor
{
    public class AdvancedGenericMenu : AdvancedDropdown
    {
        public static float? DefaultMinWidth = 200f;
        public static float? DefaultMaxWidth = 300f;

        private class AdvancedGenericMenuItem : AdvancedDropdownItem
        {
            private MenuFunction func;

            private MenuFunction2 func2;
            private object userData;

            public AdvancedGenericMenuItem( string name ) : base( name )
            {
            }

            public AdvancedGenericMenuItem( string name, bool enabled, Texture2D icon, MenuFunction func ) : base( name )
            {
                Set( enabled, icon, func );
            }

            public AdvancedGenericMenuItem( string name, bool enabled, Texture2D icon, MenuFunction2 func, object userData ) : base( name )
            {
                Set( enabled, icon, func, userData );
            }

            public void Set( bool enabled, Texture2D icon, MenuFunction func )
            {
                this.enabled = enabled;
                this.icon = icon;
                this.func = func;
            }

            public void Set( bool enabled, Texture2D icon, MenuFunction2 func, object userData )
            {
                this.enabled = enabled;
                this.icon = icon;
                this.func2 = func;
                this.userData = userData;
            }

            public void Run()
            {
                if ( func2 != null )
                    func2( userData );
                else if ( func != null )
                    func();
            }
        }

        private List<AdvancedGenericMenuItem> items = new List<AdvancedGenericMenuItem>();

        private AdvancedGenericMenuItem FindOrCreateItem( string name, AdvancedGenericMenuItem currentRoot = null )
        {
            if ( string.IsNullOrWhiteSpace( name ) )
                return null;

            AdvancedGenericMenuItem item = null;

            string[] paths = name.Split( '/' );
            if ( currentRoot == null )
            {
                item = items.FirstOrDefault( x => x != null && x.name == paths[0] );
                if ( item == null )
                    items.Add( item = new AdvancedGenericMenuItem( paths[0] ) );
            }
            else
            {
                item = currentRoot.children.OfType<AdvancedGenericMenuItem>().FirstOrDefault( x => x.name == paths[0] );
                if ( item == null )
                    currentRoot.AddChild( item = new AdvancedGenericMenuItem( paths[0] ) );
            }

            if ( paths.Length > 1 )
                return FindOrCreateItem( string.Join( "/", paths, 1, paths.Length - 1 ), item );

            return item;
        }

        private AdvancedGenericMenuItem FindParent( string name )
        {
            string[] paths = name.Split( '/' );
            return FindOrCreateItem( string.Join( "/", paths, 0, paths.Length - 1 ) );
        }

        private string Name { get; set; }

        public AdvancedGenericMenu() : base( new AdvancedDropdownState() )
        {
            Name = "";
        }

        public AdvancedGenericMenu( string name, AdvancedDropdownState state ) : base( state )
        {
            Name = name;
        }

        //
        // Summary:
        //     Add a disabled item to the menu.
        //
        // Parameters:
        //   content:
        //     The GUIContent to display as a disabled menu item.
        public void AddDisabledItem( GUIContent content )
        {
            //var parent = FindParent( content.text );
            var item = FindOrCreateItem( content.text );
            item.Set( false, null, null );
        }

        //
        // Summary:
        //     Add a disabled item to the menu.
        //
        // Parameters:
        //   content:
        //     The GUIContent to display as a disabled menu item.
        //
        //   on:
        //     Specifies whether to show that the item is currently activated (i.e. a tick next
        //     to the item in the menu).
        public void AddDisabledItem( GUIContent content, bool on )
        {
        }

        public void AddItem( string name, bool on, MenuFunction func )
        {
            AddItem( new GUIContent( name ), on, func );
        }

        public void AddItem( GUIContent content, bool on, MenuFunction func )
        {
            //var parent = FindParent( content.text );
            var item = FindOrCreateItem( content.text );
            item.Set( true/*on*/, null, func );
        }

        public void AddItem( string name, bool on, MenuFunction2 func, object userData )
        {
            AddItem( new GUIContent( name ), on, func, userData );
        }

        public void AddItem( GUIContent content, bool on, MenuFunction2 func, object userData )
        {
            //var parent = FindParent( content.text );
            var item = FindOrCreateItem( content.text );
            item.Set( true/*on*/, null, func, userData );
        }

        //
        // Summary:
        //     Add a seperator item to the menu.
        //
        // Parameters:
        //   path:
        //     The path to the submenu, if adding a separator to a submenu. When adding a separator
        //     to the top level of a menu, use an empty string as the path.
        public void AddSeparator( string path = null )
        {
            var parent = string.IsNullOrWhiteSpace( path ) ? null : FindParent( path );
            if ( parent == null )
                items.Add( null );
            else
                parent.AddSeparator();
        }

        //
        // Summary:
        //     Show the menu at the given screen rect.
        //
        // Parameters:
        //   position:
        //     The position at which to show the menu.
        public void DropDown( Rect position )
        {
            position.width = Mathf.Clamp( position.width, DefaultMinWidth.HasValue ? DefaultMinWidth.Value : 1f, DefaultMaxWidth.HasValue ? DefaultMaxWidth.Value : Screen.width );

            Show( position );
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem( Name );

            foreach ( var m in items )
            {
                if ( m == null )
                    root.AddSeparator();
                else
                    root.AddChild( m );
            }

            return root;
        }

        protected override void ItemSelected( AdvancedDropdownItem item )
        {
            if ( item is AdvancedGenericMenuItem gmItem )
                gmItem.Run();
        }
    }
}
#endif