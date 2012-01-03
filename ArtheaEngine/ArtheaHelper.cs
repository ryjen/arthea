namespace ArtheaEngine
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Data;
    using System.Data.Linq;
    using System.Data.Odbc;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;

    using MySql.Data.MySqlClient;

    public static class ArtheaHelper
    {
        #region Fields

        public const short Immortal = 201;
        public const short MaxLevel = 201;

        #endregion Fields

        #region Constructors

        static ArtheaHelper()
        {
            Randomizer = new Random();
        }

        #endregion Constructors

        #region Properties

        public static Random Randomizer
        {
            get; private set;
        }

        #endregion Properties

        #region Methods

        public static void CloneObj(object newObject, object thisObj)
        {
            var fields = newObject.GetType().GetFields();

            var i = 0;

            foreach (var fi in thisObj.GetType().GetFields())
            {
                //We query if the fiels support the ICloneable interface.

                var cloneType = fi.FieldType.
                    GetInterface("ICloneable", true);

                if (cloneType != null)
                {
                    //Getting the ICloneable interface from the object.

                    var clone = (ICloneable) fi.GetValue(thisObj);

                    //We use the clone method to set the new value to the field.

                    fields[i].SetValue(newObject, clone.Clone());
                }
                else
                {
                    // If the field doesn't support the ICloneable

                    // interface then just set it.

                    fields[i].SetValue(newObject, fi.GetValue(thisObj));
                }

                //Now we check if the object support the

                //IEnumerable interface, so if it does

                //we need to enumerate all its items and check if

                //they support the ICloneable interface.

                var enumerableType = fi.FieldType.GetInterface
                    ("IEnumerable", true);
                if (enumerableType != null)
                {
                    //Get the IEnumerable interface from the field.

                    var e = (IEnumerable) fi.GetValue(thisObj);

                    //This version support the IList and the

                    //IDictionary interfaces to iterate on collections.

                    var listType = fields[i].FieldType.GetInterface
                        ("ICollection", true);
                    var dicType = fields[i].FieldType.GetInterface
                        ("IDictionary", true);

                    var j = 0;
                    if (listType != null)
                    {
                        //Getting the IList interface.

                        var list = (IList) fields[i].GetValue(newObject);

                        foreach (var obj in e)
                        {
                            //Checking to see if the current item

                            //support the ICloneable interface.

                            cloneType = obj.GetType().
                                GetInterface("ICloneable", true);

                            if (cloneType != null)
                            {
                                //If it does support the ICloneable interface,

                                //we use it to set the clone of

                                //the object in the list.

                                var clone = (ICloneable) obj;

                                list[j] = clone.Clone();
                            }

                            //NOTE: If the item in the list is not

                            //support the ICloneable interface then in the

                            //cloned list this item will be the same

                            //item as in the original list

                            //(as long as this type is a reference type).

                            j++;
                        }
                    }
                    else if (dicType != null)
                    {
                        //Getting the dictionary interface.

                        var dic = (IDictionary) fields[i].
                                                    GetValue(newObject);
                        j = 0;

                        foreach (DictionaryEntry de in e)
                        {
                            //Checking to see if the item

                            //support the ICloneable interface.

                            cloneType = de.Value.GetType().
                                GetInterface("ICloneable", true);

                            if (cloneType != null)
                            {
                                var clone = (ICloneable) de.Value;

                                dic[de.Key] = clone.Clone();
                            }
                            j++;
                        }
                    }
                }
                i++;
            }
        }

        public static NotifyCollectionChangedEventHandler CollectionChangedHandler<T>(Action<T, bool> action)
        {
            return new NotifyCollectionChangedEventHandler((o, e) =>
            {
            if (e.NewItems != null)
            {
                foreach (T r in e.NewItems)
                {
                    action(r, true);
                }
            }
            if (e.OldItems != null)
            {
                foreach (T r in e.OldItems)
                {
                    action(r, false);
                }
            }
            });
        }

        public static bool IsEmail(string inputEmail)
        {
            inputEmail = inputEmail ?? "";
            const string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                                    @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                                    @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            var re = new Regex(strRegex);
            return re.IsMatch(inputEmail);
        }

        public static IDbConnection NewConnection()
        {
            var conn =  new MySqlConnection(ConfigurationManager.ConnectionStrings["ArtheaMYSQL"].ConnectionString);

            conn.Open();

            return conn;
        }

        #endregion Methods
    }

    public static class PropertyHelper<T>
    {
        #region Methods

        public static PropertyInfo GetProperty<TValue>(
            Expression<Func<T, TValue>> selector)
        {
            Expression body = selector;
            if (body is LambdaExpression)
            {
                body = ((LambdaExpression)body).Body;
            }
            switch (body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return (PropertyInfo)((MemberExpression)body).Member;
                default:
                    throw new InvalidOperationException();
            }
        }

        #endregion Methods
    }
}