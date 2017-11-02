using System.Threading.Tasks;
using System.Windows.Controls;
using TLSharp.Core;
using TLSharp.Core.MTProto;

namespace Telesharp
{
    public partial class DialogCard : UserControl
    {
        DialogCardInfo CardInfo;

        public DialogCard(DialogCardInfo cardInfo)
        {
            InitializeComponent();

            CardInfo = cardInfo;
            Loaded += loaded;
        }

        async void loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await ReloadInfo();
        }

        public async Task ReloadInfo()
        {
            dialogName.Text = CardInfo.DialogName;
            topMessage.Text = CardInfo.TopMessage;

            // TODO MIGHT BE UNAVAILABLE TYPE
            var flt = ((TL.FileLocationType)CardInfo.PhotoSmall);
            var image = await Telegram.DownloadImage(
                new TL.InputFileLocationType(flt.VolumeId, flt.LocalId, flt.Secret));

            if (image != null)
                photo.Source = image;
        }
    }
}
