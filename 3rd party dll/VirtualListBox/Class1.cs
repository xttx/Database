using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.ComponentModel;
using System.Collections;
using System.Data;

namespace VirtualListBox
{
    public class VListBox : ListBox
    {
        /*
        * Listbox Styles
        */
        private const int LBS_NOTIFY = 0x0001;
        private const int LBS_SORT = 0x0002;
        private const int LBS_NOREDRAW = 0x0004;
        private const int LBS_MULTIPLESEL = 0x0008;
        private const int LBS_OWNERDRAWFIXED = 0x0010;
        private const int LBS_OWNERDRAWVARIABLE = 0x0020;
        private const int LBS_HASSTRINGS = 0x0040;
        private const int LBS_USETABSTOPS = 0x0080;
        private const int LBS_NOINTEGRALHEIGHT = 0x0100;
        private const int LBS_MULTICOLUMN = 0x0200;
        private const int LBS_WANTKEYBOARDINPUT = 0x0400;
        private const int LBS_EXTENDEDSEL = 0x0800;
        private const int LBS_DISABLENOSCROLL = 0x1000;
        private const int LBS_NODATA = 0x2000;

        private const int LB_GETCOUNT = 0x018B;
        private const int LB_SETCOUNT = 0x01A7;

        private const int LB_SETSEL = 0x0185;
        private const int LB_SETCURSEL = 0x0186;
        private const int LB_GETSEL = 0x0187;
        private const int LB_GETCURSEL = 0x0188;
        private const int LB_GETSELCOUNT = 0x0190;
        private const int LB_GETSELITEMS = 0x0191;
        private const int LB_SETITEMHEIGHT = 0x01A0;

        public bool DontChangeSelectionIndexOnDataSourceChange = false;

        [DllImport("user32", CharSet = CharSet.Auto)]
        private extern static int SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

        #region Member Variables
        private int selectedIndex = -1;
        private VListBox.SelectedIndexCollection selectedIndices = null;
        private object _DataSource = null;
        private string _DisplayMember = "";
        private string _ValueMember = "";
        #endregion


        /// <summary>
        /// Constructs a new instance of this class.
        /// </summary>
        public VListBox() : base()
        {
            selectedIndices = new VListBox.SelectedIndexCollection(this);
        }

        /// <summary>
        /// Sets up the <see cref="CreateParams"/> object to tell Windows
        /// how the ListBox control should be created.  In this instance
        /// the default configuration is modified to remove <c>LBS_HASSTRINGS</c>
        /// and <c>LBS_SORT</c> styles and to add <c>LBS_NODATA</c>
        /// and <c>LBS_OWNERDRAWFIXED</c> styles. This converts the ListBox
        /// into a Virtual ListBox.
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams defParams = base.CreateParams;
                //Console.WriteLine("In Param style: {0:X8}", defParams.Style);
                defParams.Style = defParams.Style & ~LBS_HASSTRINGS;
                defParams.Style = defParams.Style & ~LBS_SORT;
                defParams.Style = defParams.Style | LBS_OWNERDRAWFIXED | LBS_NODATA;
                //Console.WriteLine("Out Param style: {0:X8}", defParams.Style);
                return defParams;
            }
        }

        public void DefaultDrawItem(DrawItemEventArgs e, String text)
        {
            bool selected = ((e.State & DrawItemState.Selected) == DrawItemState.Selected);

            if (selected) { e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds); }
            else { e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds); }

            //e.Graphics.DrawString(text, this.Font, selected ? SystemBrushes.HighlightText : SystemBrushes.WindowText, new RectangleF(e.Bounds.X + 1, e.Bounds.Y + 1, e.Bounds.Width - 2, e.Bounds.Height - 2));
            //e.Graphics.DrawString(text, e.Font, selected ? SystemBrushes.HighlightText : SystemBrushes.WindowText, e.Bounds, StringFormat.GenericDefault);
            e.DrawBackground();
            TextRenderer.DrawText(e.Graphics, text, e.Font, new Point(e.Bounds.X, e.Bounds.Y), e.ForeColor);
        }

        /// <summary>
        /// Gets or sets the number of virtual items in the ListBox.
        /// </summary>
        public int Count
        {
            get { return SendMessage(this.Handle, LB_GETCOUNT, 0, IntPtr.Zero); }
            set {
                int ind = SelectedIndex;
                SendMessage(this.Handle, LB_SETCOUNT, value, IntPtr.Zero);
                SendMessage(this.Handle, LB_SETITEMHEIGHT, 0, new IntPtr(base.Font.Height));
                if (ind != SelectedIndex) OnSelectedIndexChanged(new EventArgs());
            }
        }

        /// <summary>
        /// Throws an exception.  All the items for a Virtual ListBox are externally managed.
        /// </summary>
        [BrowsableAttribute(false)]
        public new IEnumerable<object> Items //ObjectCollection Items
        {
            get { 
                if (_DataSource == null)
				{
                    throw new InvalidOperationException("A Virtual ListBox does not have an Items collection.");
                }
                else
				{
                    //return new ObjectCollection(new ListBox(), ((DataTable)_DataSource).DefaultView.Cast<DataRowView>().Cast<object>().ToArray());
                    return ((DataTable)_DataSource).DefaultView.Cast<DataRowView>().Cast<object>();
                }
            }
        }

        public new object SelectedItem
        {
            get {
                if (_DataSource == null)
				{
                    throw new InvalidOperationException("A Virtual ListBox does not have a SelectedObject collection");
                } else
				{
                    if (base.SelectedIndex < 0) return null;
                    else return ((DataTable)_DataSource).DefaultView[base.SelectedIndex];
                }
            }
        }

        /// <summary>
        /// Throws an exception.  All the items for a Virtual ListBox are externally managed.
        /// </summary>
        /// <remarks>The selected index can be obtained using the <see cref="SelectedIndex"/> and
        /// <see cref="SelectedIndices"/> properties.
        /// </remarks>
        [BrowsableAttribute(false)]
        public new SelectedObjectCollection SelectedItems
        {
            get { throw new InvalidOperationException("A Virtual ListBox does not have a SelectedObject collection"); }
        }

        /// <summary>
        /// Returns the selected index in the control.  If the control has the multi-select
        /// style, then the first selected item is returned.
        /// </summary>
        public new int SelectedIndex
        {
            get
            {
                int selIndex = -1;
                if (SelectionMode == SelectionMode.One)
                {
                    selIndex = SendMessage(this.Handle, LB_GETCURSEL, 0, IntPtr.Zero);
                }
                else if ((SelectionMode == SelectionMode.MultiExtended) || (SelectionMode == SelectionMode.MultiSimple))
                {
                    int selCount = SendMessage(this.Handle, LB_GETSELCOUNT, 0, IntPtr.Zero);
                    if (selCount > 0)
                    {
                        IntPtr buf = Marshal.AllocCoTaskMem(4);
                        SendMessage(this.Handle, LB_GETSELITEMS, 1, buf);
                        selIndex = Marshal.ReadInt32(buf);
                        Marshal.FreeCoTaskMem(buf);
                    }
                }
                return selIndex;
            }
            set
            {
                int old_sel_ind = SelectedIndex;
                if (SelectionMode == SelectionMode.One)
                {
                    SendMessage(this.Handle, LB_SETCURSEL, value, IntPtr.Zero);
                }
                else if ((SelectionMode == SelectionMode.MultiExtended) || (SelectionMode == SelectionMode.MultiSimple))
                {
                    Console.WriteLine("Working on it");
                }
                if (old_sel_ind != SelectedIndex) OnSelectedIndexChanged(new EventArgs());
            }
        }

        /// <summary>
        ///  todo
        /// </summary>
        public new SelectedIndexCollection SelectedIndices
        {
            get
            {
                return selectedIndices;
            }
        }

        /// <summary>
        /// Gets the selection state for an item.
        /// </summary>
        /// <param name="index">Index of the item.</param>
        /// <returns><c>true</c> if selected, <c>false</c> otherwise.</returns>
        public bool ItemSelected(int index)
        {
            bool state = false;
            if (SelectionMode == SelectionMode.One)
            {
                state = (SelectedIndex == index);
            }
            else if ((SelectionMode == SelectionMode.MultiExtended) || (SelectionMode == SelectionMode.MultiSimple))
            {
                state = (SendMessage(this.Handle, LB_GETSEL, index, IntPtr.Zero) != 0);
            }
            return state;
        }
        /// <summary>
        /// Sets the selection state for an item.
        /// </summary>
        /// <param name="index">Index of the item.</param>
        /// <param name="state">New selection state for the item.</param>
        public void ItemSelected(int index, bool state)
        {
            if (SelectionMode == SelectionMode.One)
            {
                if (state)
                {
                    SelectedIndex = index;
                }
            }
            else if ((SelectionMode == SelectionMode.MultiExtended) || (SelectionMode == SelectionMode.MultiSimple))
            {
                SendMessage(this.Handle, LB_SETSEL, (state ? 1 : 0), (IntPtr)index);
            }
        }

        public new object SelectedValue
        {
            get
			{
                if (_DataSource == null || SelectedIndex < 0)
				{
                    return null;
				} else {
                    if (string.IsNullOrEmpty(_ValueMember))
					{
                        return ((DataTable)_DataSource).DefaultView[SelectedIndex][0];
                    } else {
                        return ((DataTable)_DataSource).DefaultView[SelectedIndex][_ValueMember];
                    }
				}
			}
        }


        public new object DataSource
		{
			get { return _DataSource; }
			set { 
                _DataSource = value;
                if (_DataSource == null)
                {
                    Count = 0;
                }
                else
                {
                    var dt = (DataTable)_DataSource;
                    Count = dt.Rows.Count;
                    
                    dt.DefaultView.ListChanged += (o, e) => {
                        Count = dt.DefaultView.Count;
                        if (DontChangeSelectionIndexOnDataSourceChange) return;

                        int ind = SelectedIndex;
                        if (ind < 0 && Count > 0)
                        {
                            SelectedIndex = 0;
                        } else {
                            if (Count > ind) { SelectedIndex = ind; } else { SelectedIndex = Count - 1; }
                        }
                    };

                    if (Count > 0) SelectedIndex = 0;
                    //base.OnSelectedIndexChanged(new EventArgs());
                }
            }
		}
        public new string DisplayMember { 
            get { return _DisplayMember; } 
            set { _DisplayMember = value; } 
        }
        public new string ValueMember { 
            get { return _ValueMember; } 
            set { _ValueMember = value; } 
        }
        public new Font Font
		{
            get
			{
                return base.Font;
			}
            set
			{
                int old_count = Count;
                base.Font = value;
                Count = old_count;
            }
		}

        /// <summary>
        /// Called when an item in the control needs to be drawn, and raises the 
        /// <see cref="DrawItem"/> event.
        /// </summary>
        /// <param name="e">Details about the item that is to be drawn.</param>
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) { selectedIndex = e.Index; }

            if (_DataSource != null && e.Index >= 0)
            {
                string str = "";
                if (string.IsNullOrWhiteSpace(DisplayMember))
                { str = ((DataTable)_DataSource).DefaultView[e.Index][0].ToString(); }
                else
                { str = ((DataTable)_DataSource).DefaultView[e.Index][DisplayMember].ToString(); }
                DefaultDrawItem(e, str);
            }

            base.OnDrawItem(e);
        }

        /// <summary>
        /// Hides the Sorted property of the ListBox control.  Any attempt to set this property
        /// to true will result in an exception.
        /// </summary>
        [BrowsableAttribute(false)]
        public new bool Sorted
        {
            get { return false; }
            set
            {
                if (value) { throw new InvalidOperationException("A Virtual ListBox cannot be sorted."); }
            }
        }

        /// <summary>
        /// Gets/sets the DrawMode of the ListBox.  The DrawMode must always
        /// be set to <see cref="System.Windows.Forms.DrawMode.OwnerDrawFixed"/>.
        /// </summary>
        [Browsable(false)]
        public new System.Windows.Forms.DrawMode DrawMode
        {
            get { return System.Windows.Forms.DrawMode.OwnerDrawFixed; }
            set
            {
                if (value != System.Windows.Forms.DrawMode.OwnerDrawFixed) { throw new ArgumentException("DrawMode must be set to OwnerDrawFixed in a Virtual ListBox"); }
            }
        }

        /// <summary>
        /// Called when the ListBox handle is destroyed.  
        /// </summary>
        /// <param name="e">Not used</param>
        protected override void OnHandleDestroyed(EventArgs e)
        {
            // Nasty.  The problem is with the call to NativeUpdateSelection,
            // which calls the EnsureUpToDate on the SelectedObjectCollection method, 
            // and that is broken.
            try
            {
                base.OnHandleDestroyed(e);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Implements a read-only collection of selected items in the
        /// VListBox.
        /// </summary>
        public new class SelectedIndexCollection : ICollection, IEnumerable
        {
            private VListBox owner = null;

            /// <summary>
            /// Creates a new instance of this class
            /// </summary>
            /// <param name="owner">The VListBox which owns the collection</param>
            public SelectedIndexCollection(VListBox owner)
            {
                this.owner = owner;
            }

            /// <summary>
            /// Returns an enumerator which allows iteration through the selected items
            /// collection.
            /// </summary>
            /// <returns></returns>
            public IEnumerator GetEnumerator()
            {
                return new SelectedIndexCollectionEnumerator(this.owner);
            }
 
            /// <summary>
            /// Not implemented. Throws an exception.
            /// </summary>
            /// <param name="dest">Array to copy items to</param>
            /// <param name="startIndex">First index in array to put items in.</param>
            public void CopyTo(Array dest, int startIndex)
            {
                throw new InvalidOperationException("Not implemented");
            }

            /// <summary>
            /// Returns the number of items in the collection.
            /// </summary>
            public int Count
            {
                get
                {
                    return SendMessage(owner.Handle, LB_GETSELCOUNT, 0, IntPtr.Zero);
                }
            }

            /// <summary>
            /// Returns the selected item with the specified 0-based index in the collection
            /// of selected items.  
            /// </summary>
            /// <remarks>
            /// Do not use this method to enumerate through all selected
            /// items as it gets the collection of selected items each item it 
            /// is called.  The <c>foreach</c> enumerator only gets the collection
            /// of items once when it is constructed and is therefore quicker.
            /// </remarks>
            public int this[int index]
            {
                get
                {
                    int selIndex = -1;
                    int selCount = SendMessage(owner.Handle, LB_GETSELCOUNT, 0,
                     IntPtr.Zero);
                    if ((index < selCount) && (index > 0))
                    {
                        IntPtr buf = Marshal.AllocCoTaskMem(4 * (index + 1));
                        SendMessage(owner.Handle, LB_GETSELITEMS, selCount, buf);
                        selIndex = Marshal.ReadInt32(buf, index * 4);
                        Marshal.FreeCoTaskMem(buf);
                    }
                    else
                    {
                        throw new ArgumentException("Index out of bounds", "index");
                    }
                    return selIndex;

                }
            }

            /// <summary>
            /// Returns <c>false</c>.  This collection is not synchronized for
            /// concurrent access from multiple threads.
            /// </summary>
            public bool IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            /// <summary>
            /// Not implemented. Throws an exception.
            /// </summary>
            public object SyncRoot
            {
                get
                {
                    throw new InvalidOperationException("Synchronization not supported.");
                }
            }


        }

        /// <summary>
        /// Implements the <see cref="IEnumerator"/> interface for the selected indexes
        /// within a <see cref="VListBox"/> control.
        /// </summary>
        public class SelectedIndexCollectionEnumerator : IEnumerator, IDisposable
        {
            private IntPtr buf = IntPtr.Zero;
            private int size = 0;
            private int offset = 0;

            /// <summary>
            /// Constructs a new instance of this class.
            /// </summary>
            /// <param name="owner">The <see cref="VListBox"/> which owns the collection.</param>
            public SelectedIndexCollectionEnumerator(VListBox owner)
            {
                int selCount = SendMessage(owner.Handle, LB_GETSELCOUNT, 0,
                 IntPtr.Zero);
                if (selCount > 0)
                {
                    buf = Marshal.AllocCoTaskMem(4 * selCount);
                    SendMessage(owner.Handle, LB_GETSELITEMS, selCount, buf);
                }
            }

            /// <summary>
            /// Clears up any resources associated with this enumerator.
            /// </summary>
            public void Dispose()
            {
                if (!buf.Equals(IntPtr.Zero))
                {
                    Marshal.FreeCoTaskMem(buf);
                    buf = IntPtr.Zero;
                }
            }

            /// <summary>
            /// Resets the enumerator to the start of the list.
            /// </summary>
            public void Reset()
            {
                offset = 0;
            }

            /// <summary>
            /// Returns the current object.
            /// </summary>
            public object Current
            {
                get
                {
                    if (offset >= size)
                    {
                        throw new Exception("Collection is exhausted.");
                    }
                    else
                    {
                        int index = Marshal.ReadInt32(buf, offset * 4);
                        return (object)index;
                    }

                }
            }

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns><c>true</c> if the enumerator was successfully advanced to the next element;
            /// <c>false</c> if the enumerator has passed the end of the collection.</returns>
            public bool MoveNext()
            {
                bool success = false;
                offset++;
                if (offset < size)
                {
                    success = true;
                }
                return success;
            }
        }
    }
}
