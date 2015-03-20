using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using ININ.Alliances.HootsuiteClientAddin.viewModel;
using MessageBox = System.Windows.Forms.MessageBox;

namespace ININ.Alliances.HootsuiteClientAddin.view
{
    /// <summary>
    /// Interaction logic for InteractionView.xaml
    /// </summary>
    public partial class InteractionView : UserControl
    {
        private InteractionViewModel Interaction { get { return DataContext as InteractionViewModel; } }

        public InteractionView()
        {
            InitializeComponent();
        }

        private void OpenMedia_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(Interaction.HootsuiteData.Href);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
#if DEBUG
                MessageBox.Show(ex.Message);
#endif
            }
        }
    }
}
