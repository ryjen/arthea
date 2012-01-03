namespace ArtheaServer.Creation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text;

    using ArtheaEngine;
    using ArtheaEngine.Model;

    public abstract class Editor : IWritable
    {
        #region Fields

        protected TelnetConnection _connection;

        #endregion Fields

        #region Constructors

        public Editor(TelnetConnection conn)
        {
            _connection = conn;
        }

        #endregion Constructors

        #region Delegates

        public delegate void ActionHandler();

        #endregion Delegates

        #region Events

        public event ActionHandler OnBack;

        #endregion Events

        #region Properties

        public abstract string Title
        {
            get;
        }

        protected Editor Next
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        public void Back()
        {
            if (OnBack != null)
            {
                OnBack();
            }
            _connection.Editing = Next;
            if (Next != null)
            {
                Next.Show();
            }
            else
            {
                _connection.WriteLine("Done editing.");
            }
        }

        public virtual bool Execute(Argument argument)
        {
            if (!argument || argument == "show")
            {
                Show();
                return true;
            }

            if (argument == "q" || argument == "quit")
            {
                Back();
                return true;
            }

            if (argument == "save")
            {
                Save();
                return true;
            }
            return false;
        }

        public void Forward(Editor editor)
        {
            editor.Next = this;
            _connection.Editing = editor;
            editor.Show();
        }

        public void Page(string buf)
        {
            _connection.Page(buf);
        }

        public virtual void Prompt()
        {
            if (this is TextEditor)
            {
                _connection.Write("~CType \\? for help or \\q to {0}:~x ", Next != null ? "go back" : "quit");
            }
            else
            {
                _connection.Write("~CSelect an option or Q to {0}:~x ", Next != null ? "go back" : "quit");
            }
        }

        public abstract void Save();

        public abstract void Show();

        public void Write(string format, params object[] args)
        {
            _connection.Write(format, args);
        }

        public void Write(byte[] value)
        {
            _connection.Write(value);
        }

        public void Write(object obj)
        {
            _connection.Write(obj);
        }

        public void WriteLine()
        {
            _connection.WriteLine();
        }

        public void WriteLine(object value)
        {
            _connection.WriteLine(value);
        }

        public void WriteLine(string format, params object[] args)
        {
            _connection.WriteLine(format, args);
        }

        protected void Reset()
        {
            _connection.ClearScreen();

            _connection.SetCursor();

            _connection.Header(Title);
        }

        protected void ShowValidEnumValues<T>()
        {
            WriteLine("~CValid values are:~W");
            var cols = new ColumnList(5);
            foreach(var n in Enum.GetNames(typeof(T)))
            {
                cols.Add(n);
            }

            cols.Show(_connection);
        }

        #endregion Methods
    }
}