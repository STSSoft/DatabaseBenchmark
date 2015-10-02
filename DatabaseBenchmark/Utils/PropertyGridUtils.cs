using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBenchmark.Utils
{
    public sealed class PropertyGridUtils : ICustomTypeDescriptor, INotifyPropertyChanged
    {
        private Type type;
        private AttributeCollection attributes;
        private TypeConverter typeConverter;
        private Dictionary<Type, object> editors;
        private EventDescriptor defaultEvent;
        private PropertyDescriptor defaultProperty;
        private EventDescriptorCollection events;

        public event PropertyChangedEventHandler PropertyChanged;

        private PropertyGridUtils()
        {
        }

        public PropertyGridUtils(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            this.type = type;
            typeConverter = TypeDescriptor.GetConverter(type);
            defaultEvent = TypeDescriptor.GetDefaultEvent(type);
            defaultProperty = TypeDescriptor.GetDefaultProperty(type);
            events = TypeDescriptor.GetEvents(type);

            List<PropertyDescriptor> normalProperties = new List<PropertyDescriptor>();
            OriginalProperties = TypeDescriptor.GetProperties(type);

            foreach (PropertyDescriptor property in OriginalProperties)
            {
                if (!property.IsBrowsable)
                    continue;

                normalProperties.Add(property);
            }

            Properties = new PropertyDescriptorCollection(normalProperties.ToArray());
            attributes = TypeDescriptor.GetAttributes(type);
            editors = new Dictionary<Type, object>();

            object editor = TypeDescriptor.GetEditor(type, typeof(UITypeEditor));

            if (editor != null)
                editors.Add(typeof(UITypeEditor), editor);

            editor = TypeDescriptor.GetEditor(type, typeof(ComponentEditor));

            if (editor != null)
                editors.Add(typeof(ComponentEditor), editor);

            editor = TypeDescriptor.GetEditor(type, typeof(InstanceCreationEditor));

            if (editor != null)
                editors.Add(typeof(InstanceCreationEditor), editor);
        }

        public T GetPropertyValue<T>(string name, T defaultValue)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            foreach (PropertyDescriptor pd in Properties)
            {
                if (pd.Name == name)
                {
                    try
                    {
                        return (T)Convert.ChangeType(pd.GetValue(Component), typeof(T));
                    }
                    catch
                    {
                        return defaultValue;
                    }
                }
            }
            return defaultValue;
        }

        public void SetPropertyValue(string name, object value)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            foreach (PropertyDescriptor pd in Properties)
            {
                if (pd.Name == name)
                {
                    pd.SetValue(Component, value);
                    break;
                }
            }
        }

        internal void OnValueChanged(PropertyDescriptor prop)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(prop.Name));
            }
        }

        internal static T GetAttribute<T>(AttributeCollection attributes) where T : Attribute
        {
            if (attributes == null)
                return null;

            foreach (Attribute att in attributes)
            {
                if (typeof(T).IsAssignableFrom(att.GetType()))
                    return (T)att;
            }
            return null;
        }

        public sealed class DynamicProperty : PropertyDescriptor, INotifyPropertyChanged
        {
            private readonly Type type;
            private readonly bool hasDefaultValue;
            private readonly object defaultValue;
            private readonly PropertyDescriptor existing;
            private readonly PropertyGridUtils descriptor;
            private Dictionary<Type, object> editors;
            private bool? readOnly;
            private bool? browsable;
            private string displayName;
            private string description;
            private string category;
            private List<Attribute> attributes = new List<Attribute>();

            public event PropertyChangedEventHandler PropertyChanged;

            internal DynamicProperty(PropertyGridUtils descriptor, Type type, object value, string name, Attribute[] attrs)
                : base(name, attrs)
            {
                this.descriptor = descriptor;
                this.type = type;
                Value = value;

                DefaultValueAttribute def = PropertyGridUtils.GetAttribute<DefaultValueAttribute>(Attributes);

                if (def == null)
                    hasDefaultValue = false;
                else
                {
                    hasDefaultValue = true;
                    defaultValue = def.Value;
                }

                if (attrs != null)
                {
                    foreach (Attribute att in attrs)
                        attributes.Add(att);
                }
            }

            internal static Attribute[] GetAttributes(PropertyDescriptor existing)
            {
                List<Attribute> atts = new List<Attribute>();
                foreach (Attribute a in existing.Attributes)
                    atts.Add(a);

                return atts.ToArray();
            }

            internal DynamicProperty(PropertyGridUtils descriptor, PropertyDescriptor existing, object component)
                : this(descriptor, existing.PropertyType, existing.GetValue(component), existing.Name, GetAttributes(existing))
            {
                this.existing = existing;
            }

            public void RemoveAttributesOfType<T>() where T : Attribute
            {
                List<Attribute> remove = new List<Attribute>();

                foreach (Attribute att in attributes)
                {
                    if (typeof(T).IsAssignableFrom(att.GetType()))
                        remove.Add(att);
                }

                foreach (Attribute att in remove)
                    attributes.Remove(att);
            }

            public IList<Attribute> AttributesList
            {
                get
                {
                    return attributes;
                }
            }

            public override AttributeCollection Attributes
            {
                get
                {
                    return new AttributeCollection(attributes.ToArray());
                }
            }

            public object Value { get; set; }

            public override bool CanResetValue(object component)
            {
                if (existing != null)
                    return existing.CanResetValue(component);

                return hasDefaultValue;
            }

            public override Type ComponentType
            {
                get
                {
                    if (existing != null)
                        return existing.ComponentType;

                    return typeof(object);
                }
            }

            public override object GetValue(object component)
            {
                if (existing != null)
                    return existing.GetValue(component);

                return Value;
            }

            public override string Category
            {
                get
                {
                    if (category != null)
                        return category;

                    return base.Category;
                }
            }

            public void SetCategory(string category)
            {
                this.category = category;
            }

            public override string Description
            {
                get
                {
                    if (description != null)
                        return description;

                    return base.Description;
                }
            }

            public void SetDescription(string description)
            {
                this.description = description;
            }

            public override string DisplayName
            {
                get
                {
                    if (displayName != null)
                        return displayName;

                    if (existing != null)
                        return existing.DisplayName;

                    return base.DisplayName;
                }
            }

            public void SetDisplayName(string displayName)
            {
                this.displayName = displayName;
            }

            public override bool IsBrowsable
            {
                get
                {
                    if (browsable.HasValue)
                        return browsable.Value;

                    return base.IsBrowsable;
                }
            }

            public void SetBrowsable(bool browsable)
            {
                this.browsable = browsable;
            }

            public override bool IsReadOnly
            {
                get
                {
                    if (readOnly.HasValue)
                        return readOnly.Value;

                    if (existing != null)
                        return existing.IsReadOnly;

                    ReadOnlyAttribute att = PropertyGridUtils.GetAttribute<ReadOnlyAttribute>(Attributes);
                    if (att == null)
                        return false;

                    return att.IsReadOnly;
                }
            }

            public void SetIsReadOnly(bool readOnly)
            {
                this.readOnly = readOnly;
            }

            public override Type PropertyType
            {
                get
                {
                    if (existing != null)
                        return existing.PropertyType;

                    return type;
                }
            }

            public override void ResetValue(object component)
            {
                if (existing != null)
                {
                    existing.ResetValue(component);
                    PropertyChangedEventHandler handler = PropertyChanged;
                    if (handler != null)
                        handler(this, new PropertyChangedEventArgs(Name));
         
                    descriptor.OnValueChanged(this);
                    return;
                }

                if (CanResetValue(component))
                {
                    Value = defaultValue;
                    descriptor.OnValueChanged(this);
                }
            }

            public override void SetValue(object component, object value)
            {
                if (existing != null)
                {
                    existing.SetValue(component, value);
                    PropertyChangedEventHandler handler = PropertyChanged;
                    if (handler != null)
                        handler(this, new PropertyChangedEventArgs(Name));

                    descriptor.OnValueChanged(this);
                    return;
                }

                Value = value;
                descriptor.OnValueChanged(this);
            }

            public override bool ShouldSerializeValue(object component)
            {
                if (existing != null)
                    return existing.ShouldSerializeValue(component);

                return false;
            }

            public override object GetEditor(Type editorBaseType)
            {
                if (editorBaseType == null)
                    throw new ArgumentNullException("editorBaseType");

                if (editors != null)
                {
                    object type;
                    if ((editors.TryGetValue(editorBaseType, out type)) && (type != null))
                        return type;
                }
                return base.GetEditor(editorBaseType);
            }

            public void SetEditor(Type editorBaseType, object obj)
            {
                if (editorBaseType == null)
                    throw new ArgumentNullException("editorBaseType");

                if (editors == null)
                {
                    if (obj == null)
                        return;

                    editors = new Dictionary<Type, object>();
                }
                if (obj == null)
                    editors.Remove(editorBaseType);
                else
                    editors[editorBaseType] = obj;
            }
        }

        public PropertyDescriptor AddProperty(Type type, string name, object value, string displayName, string description, string category, bool hasDefaultValue, object defaultValue, bool readOnly)
        {
            return AddProperty(type, name, value, displayName, description, category, hasDefaultValue, defaultValue, readOnly, null);
        }

        public PropertyDescriptor AddProperty(
            Type type,
            string name,
            object value,
            string displayName,
            string description,
            string category,
            bool hasDefaultValue,
            object defaultValue,
            bool readOnly,
            Type uiTypeEditor)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (name == null)
                throw new ArgumentNullException("name");

            List<Attribute> atts = new List<Attribute>();
            if (!string.IsNullOrEmpty(displayName))
                atts.Add(new DisplayNameAttribute(displayName));

            if (!string.IsNullOrEmpty(description))
                atts.Add(new DescriptionAttribute(description));

            if (!string.IsNullOrEmpty(category))
                atts.Add(new CategoryAttribute(category));

            if (hasDefaultValue)
                atts.Add(new DefaultValueAttribute(defaultValue));

            if (uiTypeEditor != null)
                atts.Add(new EditorAttribute(uiTypeEditor, typeof(UITypeEditor)));

            if (readOnly)
                atts.Add(new ReadOnlyAttribute(true));

            DynamicProperty property = new DynamicProperty(this, type, value, name, atts.ToArray());
            AddProperty(property);
            return property;
        }

        public void RemoveProperty(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            List<PropertyDescriptor> remove = new List<PropertyDescriptor>();
            foreach (PropertyDescriptor pd in Properties)
            {
                if (pd.Name == name)
                    remove.Add(pd);
            }

            foreach (PropertyDescriptor pd in remove)
            {
                Properties.Remove(pd);
            }
        }

        public void AddProperty(PropertyDescriptor property)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            Properties.Add(property);
        }

        public override string ToString()
        {
            return base.ToString() + " (" + Component + ")";
        }

        public PropertyDescriptorCollection OriginalProperties { get; private set; }
        public PropertyDescriptorCollection Properties { get; private set; }

        public PropertyGridUtils FromComponent(object component)
        {
            if (component == null)
                throw new ArgumentNullException("component");

            if (!type.IsAssignableFrom(component.GetType()))
                throw new ArgumentException(null, "component");

            PropertyGridUtils desc = new PropertyGridUtils();
            desc.type = type;
            desc.Component = component;

            // shallow copy on purpose
            desc.typeConverter = typeConverter;
            desc.editors = editors;
            desc.defaultEvent = defaultEvent;
            desc.defaultProperty = defaultProperty;
            desc.attributes = attributes;
            desc.events = events;
            desc.OriginalProperties = OriginalProperties;

            // track values
            List<PropertyDescriptor> properties = new List<PropertyDescriptor>();
            foreach (PropertyDescriptor pd in Properties)
            {
                DynamicProperty ap = new DynamicProperty(desc, pd, component);
                properties.Add(ap);
            }

            desc.Properties = new PropertyDescriptorCollection(properties.ToArray());
            return desc;
        }

        public object Component { get; private set; }
        public string ClassName { get; set; }
        public string ComponentName { get; set; }

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return attributes;
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            if (ClassName != null)
                return ClassName;

            if (Component != null)
                return Component.GetType().Name;

            if (type != null)
                return type.Name;

            return null;
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            if (ComponentName != null)
                return ComponentName;

            return Component != null ? Component.ToString() : null;
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return typeConverter;
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return defaultEvent;
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return defaultProperty;
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            object editor;
            if (editors.TryGetValue(editorBaseType, out editor))
                return editor;

            return null;
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return events;
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return events;
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            return Properties;
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return Properties;
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return Component;
        }
    }
}
