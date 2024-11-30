using RaWMVC.Enums;

namespace RaWMVC.Models
{
    public class ToastNotificationOptions
    {
        public int DurationInSeconds { get; set; } = 5;
        public bool IsDismissable { get; set; } = true;
        public ToastPositionEnum Position { get; set; } = ToastPositionEnum.TopRight;
    }
}
