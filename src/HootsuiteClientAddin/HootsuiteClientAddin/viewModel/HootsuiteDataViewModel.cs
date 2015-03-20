using System;
using ININ.Alliances.HootsuiteClientAddin.model;

namespace ININ.Alliances.HootsuiteClientAddin.viewModel
{
    public class HootsuiteDataViewModel : ViewModelBase
    {
        #region Private Members

        private HootsuiteData _model;

        #endregion



        #region Public Members

        public string Version { get { return _model.Version; } }
        public string Href { get { return _model.Post.Href; } }
        public string Id { get { return _model.Post.Id; } }
        public DateTime Datetime { get { return _model.Post.Datetime; } }
        public string Source { get { return _model.Post.Source; } }

        public HootsuiteNetwork Network
        {
            get
            {
                switch (_model.Post.Network.ToLower())
                {
                    case "facebook":
                        return HootsuiteNetwork.Facebook;
                    case "twitter":
                        return HootsuiteNetwork.Twitter;
                    default:
                        return HootsuiteNetwork.Unknown;
                }
            }
        }

        public string BodyHmtl { get { return _model.Post.Content.BodyHtml; } }
        public string Body { get { return _model.Post.Content.Body; } }
        public string UserId { get { return _model.Post.User.UserId; } }
        public string UserName { get { return _model.Post.User.UserName; } }

        //TODO: Create view models for HootsuitePostConversation and HootsuitePostAttachment
        //public ObservableCollection<HootsuitePostConversationViewModel> Conversations { get; }
        //public ObservableCollection<HootsuitePostAttachmentViewModel> Attachments { get; }  

        #endregion



        public HootsuiteDataViewModel(HootsuiteData data)
        {
            _model = data;
        }



        #region Private Methods



        #endregion



        #region Public Methods



        #endregion
    }
}
