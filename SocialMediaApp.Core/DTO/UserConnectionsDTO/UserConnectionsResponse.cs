namespace SocialMediaApp.Core.DTO.UserConnectionsDTO
{
    public class UserConnectionsResponse
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Bio { get; set; }
        public string ProfileImg { get; set; }
        public bool IsFollowed { get; set; } = false;
    }
}
