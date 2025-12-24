using System.Windows;

namespace Irvuewin.Helpers
{
    /// <summary>
    /// A helper class to enable data binding in disconnected visual trees, 
    /// such as ContextMenus or Popups, where the visual tree is not automatically inherited.
    /// This allows binding to the DataContext of the owning control (e.g., a Window) 
    /// from within a ContextMenu by using this proxy as a StaticResource.
    /// </summary>
    public class BindingProxy : Freezable
    {
        /// <summary>
        /// Creates a new instance of the BindingProxy class.
        /// Required by the Freezable base class.
        /// </summary>
        /// <returns>A new instance of BindingProxy.</returns>
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        /// <summary>
        /// Gets or sets the data object to be proxied.
        /// This is typically bound to the DataContext of the parent control.
        /// </summary>
        public object Data
        {
            get => (object)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="Data"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(object), typeof(BindingProxy), new PropertyMetadata(null));
    }
}
