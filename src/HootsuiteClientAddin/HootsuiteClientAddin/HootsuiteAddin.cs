using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using ININ.Alliances.HootsuiteClientAddin.view;
using ININ.Alliances.HootsuiteClientAddin.viewModel;
using ININ.InteractionClient.AddIn;

namespace ININ.Alliances.HootsuiteClientAddin
{
    public class HootsuiteAddin : AddInWindow
    {
        private ElementHost _content;
        private IServiceProvider _serviceProvider;

        protected override string Id
        {
            get { return "HOOTSUITE_ADDIN"; }
        }

        protected override string DisplayName
        {
            get { return "Hootsuite Addin"; }
        }

        protected override string CategoryId
        {
            get { return "HOOTSUITE"; }
        }

        protected override string CategoryDisplayName
        {
            get { return "Hootsuite"; }
        }

        public override string Title
        {
            get { return "Hootsuite"; }
        }

        public override object Content
        {
            get
            {
                return _content ??
                       (_content =
                           new ElementHost
                           {
                               Child = new HootsuiteInteractions { DataContext = new AddinViewModel(_serviceProvider) },
                               Dock = DockStyle.Fill
                           });
            }
        }

        protected override void OnLoad(IServiceProvider serviceProvider)
        {
            base.OnLoad(serviceProvider);

            _serviceProvider = serviceProvider;
        }

        protected override void OnUnload()
        {
            base.OnUnload();
        }
    }
}
