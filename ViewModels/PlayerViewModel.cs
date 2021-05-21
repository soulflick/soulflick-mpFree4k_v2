using Extensions;
using Controls;

namespace ViewModels
{
    public class PlayerViewModel : Notify
    {

        private Models.PlaylistInfo _current;
        public  Models.PlaylistInfo  current
        {
            get => _current;
            set => Update(ref _current, value, nameof(current));
        }

        public PlayerViewModel() => Player.Instance.ViewModelChanged += (s, e) => OnDisplay(e);

        private void OnDisplay(ViewModelChangedEventArgs e) => current = (Models.PlaylistInfo)e.value;
    }
}
