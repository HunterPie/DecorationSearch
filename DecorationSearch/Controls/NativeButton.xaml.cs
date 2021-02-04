using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DecorationSearch.Controls
{
    /// <summary>
    /// Interaction logic for NativeButton.xaml
    /// </summary>
    public partial class NativeButton : UserControl
    {

        public bool IsHovering
        {
            get { return (bool)GetValue(IsHoveringProperty); }
            set { SetValue(IsHoveringProperty, value); }
        }
        public static readonly DependencyProperty IsHoveringProperty =
            DependencyProperty.Register("IsHovering", typeof(bool), typeof(NativeButton), new PropertyMetadata(true));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(NativeButton));


        public NativeButton()
        {
            InitializeComponent();
        }

        private void OnMouseStateChanged(object sender, MouseEventArgs e)
        {
            IsHovering = ((UserControl) sender).IsMouseOver;
        }

        private void OnFocusChanged(object sender, RoutedEventArgs e)
        {
            IsHovering = ((UserControl) sender).IsFocused;
        }
    }
}
