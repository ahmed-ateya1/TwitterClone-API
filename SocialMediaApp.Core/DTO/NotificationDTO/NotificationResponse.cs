namespace SocialMediaApp.Core.DTO.NotificationDTO
{
    public class NotificationResponse
    {
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string NotificationType { get; set; }
        public Guid ProfileID { get; set; }
        public string UserName { get; set; }
        public string ProfilePicture { get; set; }
        public string? ReferenceURL { get; set; }
        //public int TotalUnreadNotification { get; set; }
    }
}
