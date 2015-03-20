using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using ININ.Alliances.HootsuiteClientAddin.Annotations;

namespace ININ.Alliances.HootsuiteClientAddin.viewModel
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        #region Private Members

        protected SynchronizationContext Context { get; private set; }

        #endregion



        #region Public Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion



        public ViewModelBase()
        {
            Context = SynchronizationContext.Current;
#if DEBUG
            if (Context == null)
                MessageBox.Show(GetType() + " was not created on the UI thread!");
#endif
        }



        #region Private Methods

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            Context.Send(s =>
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }, null);
        }

        #endregion



        #region Public Methods



        #endregion
    }
}
