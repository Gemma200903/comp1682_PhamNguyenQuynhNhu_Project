using RaWMVC.Enums;
using RaWMVC.Models;

namespace RaWMVC.Services
{
    public class ToastNotificationService
    {
        private readonly ToastNotificationOptions _options;

        public ToastNotificationService(ToastNotificationOptions options)
        {
            _options = options;
        }

        public void ShowToast(string message, int? durationInSeconds = null, bool? isDismissable = null)
        {
            int duration = durationInSeconds ?? _options.DurationInSeconds;
            bool dismissable = isDismissable ?? _options.IsDismissable;
            ToastPositionEnum position = _options.Position;

            // Logic để hiển thị thông báo toast với các cài đặt trên
        }
    }
}
