using System;
using System.Windows.Forms;
using ININ.Alliances.HootsuiteClientAddin.model;
using ININ.InteractionClient.AddIn;
using Newtonsoft.Json;

namespace ININ.Alliances.HootsuiteClientAddin.viewModel
{
    public class InteractionViewModel : ViewModelBase
    {
        #region Private Members
        
        private readonly IInteraction _model;
        private readonly object _hootsuiteDataLocker = new object();
        private HootsuiteData _hootsuiteData;
        private HootsuiteDataViewModel _hootsuiteDataViewModel;

        #endregion



        #region Public Members

        public string InteractionId { get { return _model.GetAttribute(InteractionAttributes.InteractionId); } }
        public string Subject { get { return _model.GetAttribute("Hootsuite_Subject"); } }
        public string Priority { get { return _model.GetAttribute("Hootsuite_Priority"); } }
        public string Reason { get { return _model.GetAttribute("Hootsuite_Reason"); } }
        public string Name { get { return CheckHootsuiteData().Post.User.UserName; } }
        public DateTime Date { get { return CheckHootsuiteData().Post.Datetime; } }
        public string Notes { get { return _model.GetAttribute("Hootsuite_Notes"); } }

        public HootsuiteDataViewModel HootsuiteData
        {
            get
            {
                return _hootsuiteDataViewModel ??
                       (_hootsuiteDataViewModel = new HootsuiteDataViewModel(CheckHootsuiteData()));
            }
        }

        #endregion



        public InteractionViewModel(IInteraction interaction)
        {
            _model = interaction;
        }



        #region Private Methods

        private HootsuiteData CheckHootsuiteData()
        {
            try
            {
                lock (_hootsuiteDataLocker)
                {
                    // Release the lock if we have the data already
                    if (_hootsuiteData != null) return _hootsuiteData;

                    // Deserialize from attribute data
                    var data = _model.GetAttribute("Hootsuite_RawData");
                    _hootsuiteData = JsonConvert.DeserializeObject<HootsuiteData>(data);

                    // Return data
                    return _hootsuiteData;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
#if DEBUG
                MessageBox.Show(ex.Message);
#endif

                // Bummer. Create empty data structure and return
                _hootsuiteData = new HootsuiteData();
                return _hootsuiteData;
            }
        }

        #endregion



        #region Public Methods

        public void RaiseChangedNotifications()
        {
            try
            {
                Context.Send(s =>
                {
                    try
                    {
                        /* None of these should change since they're all set in the initial request, 
                         * but leaving this stub here in case it's needed
                         */
                        //OnPropertyChanged("Date");
                        //OnPropertyChanged("HootsuiteData");
                        //OnPropertyChanged("InteractionId");
                        //OnPropertyChanged("Name");
                        //OnPropertyChanged("Notes");
                        //OnPropertyChanged("Priority");
                        //OnPropertyChanged("Reason");
                        //OnPropertyChanged("Subject");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
#if DEBUG
                MessageBox.Show(ex.Message);
#endif
            }
        }

        #endregion
    }
}
