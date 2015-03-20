using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using ININ.InteractionClient.AddIn;

namespace ININ.Alliances.HootsuiteClientAddin.viewModel
{
    public class AddinViewModel :ViewModelBase
    {
        #region Private Members

        private ObservableCollection<InteractionViewModel> _interactions = new ObservableCollection<InteractionViewModel>();
        private readonly IServiceProvider _serviceProvider;
        private IQueue _queue;
        private IInteractionSelector _interactionSelector;
        private InteractionViewModel _selectedInteraction;

        #endregion



        #region Public Members

        public ObservableCollection<InteractionViewModel> Interactions
        {
            get { return _interactions; }
            set { _interactions = value; }
        }

        public InteractionViewModel SelectedInteraction
        {
            get { return _selectedInteraction; }
            set
            {
                _selectedInteraction = value; 
                OnPropertyChanged();
            }
        }

        #endregion



        public AddinViewModel(IServiceProvider provider)
        {
            _serviceProvider = provider;
            Initialize();
        }



        #region Private Methods

        private void Initialize()
        {
            try
            {
                // Set up queue watch
                var queueService = _serviceProvider.GetService(typeof (IQueueService)) as IQueueService;
                _queue = queueService.GetMyInteractions(new[]
                {
                    "Eic_InteractionId",
                    "Hootsuite_Subject",
                    "Hootsuite_Priority",
                    "Hootsuite_Reason",
                    "Hootsuite_Notes",
                    "Hootsuite_RawData",
                    InteractionAttributes.State,
                    InteractionAttributes.StateDisplay
                });
                _queue.InteractionAdded += Queue_OnInteractionAdded;
                _queue.InteractionChanged += Queue_OnInteractionChanged;
                _queue.InteractionRemoved += Queue_OnInteractionRemoved;

                // Set up context changed event
                _interactionSelector = _serviceProvider.GetService(typeof(IInteractionSelector)) as IInteractionSelector;
                if (_interactionSelector != null)
                {
                    _interactionSelector.SelectedInteractionChanged += InteractionSelector_OnSelectedInteractionChanged;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
#if DEBUG
                MessageBox.Show(ex.Message);
#endif
            }
        }

        private void Queue_OnInteractionAdded(object sender, InteractionEventArgs e)
        {
            try
            {
                Context.Send(s =>
                {
                    try
                    {
                        // Make sure it's not already disconnected
                        var state = e.Interaction.GetAttribute(InteractionAttributes.State);
                        if (state.Equals(InteractionAttributeValues.State.ExternalDisconnect, StringComparison.InvariantCultureIgnoreCase) ||
                            state.Equals(InteractionAttributeValues.State.InternalDisconnect, StringComparison.InvariantCultureIgnoreCase))
                            return;

                        // Verify that this interaction has hootsuite data
                        if (string.IsNullOrEmpty(e.Interaction.GetAttribute("Hootsuite_RawData"))) return;

                        // Add it to the list
                        Interactions.Add(new InteractionViewModel(e.Interaction));

                        // Select it if we don't have anything else
                        if (SelectedInteraction == null)
                            SelectedInteraction = Interactions[Interactions.Count - 1];
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
#if DEBUG
                        MessageBox.Show(ex.Message);
#endif
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

        private void Queue_OnInteractionChanged(object sender, InteractionEventArgs e)
        {
            try
            {
                Context.Send(s =>
                {
                    try
                    {
                        // Find the interaction
                        var interaction =
                            Interactions.FirstOrDefault(i => i.InteractionId.Equals(e.Interaction.InteractionId));
                        if (interaction == null) return;

                        // The addin doesn't tell us what attributes changed, so we have to do a blanket update
                        interaction.RaiseChangedNotifications();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
#if DEBUG
                        MessageBox.Show(ex.Message);
#endif
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

        private void Queue_OnInteractionRemoved(object sender, InteractionEventArgs e)
        {
            try
            {
                Context.Send(s =>
                {
                    try
                    {
                        // Find the interaction
                        var interaction =
                            Interactions.FirstOrDefault(i => i.InteractionId.Equals(e.Interaction.InteractionId));
                        if (interaction == null) return;

                        // Remove from our list
                        Interactions.Remove(interaction);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
#if DEBUG
                        MessageBox.Show(ex.Message);
#endif
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

        private void InteractionSelector_OnSelectedInteractionChanged(object sender, EventArgs e)
        {
            try
            {
                Context.Send(s =>
                {
                    try
                    {
                        // If it's nothing, skip
                        if (_interactionSelector.SelectedInteraction == null) return;

                        // Find the interaction
                        var interaction =
                            Interactions.FirstOrDefault(
                                i => i.InteractionId.Equals(_interactionSelector.SelectedInteraction.InteractionId));

                        // Skip if we didn't find the interaction (it was probably a non-hootsuite interaction like a call or chat)
                        if (interaction == null) return;

                        // Set it as the selected interaction
                        SelectedInteraction = interaction;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
#if DEBUG
                        MessageBox.Show(ex.Message);
#endif
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



        #region Public Methods



        #endregion
    }
}
