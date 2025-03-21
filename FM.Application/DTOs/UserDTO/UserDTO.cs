namespace FM.Application.DTOs.UserDTO
{
    public class UserDTO
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string SpotifyUserId { get; set; }
        public int UserRoleId { get; set; }
    }

    public class UserCreateDTO
    {
        public string DisplayName { get; set; }
        public string SpotifyUserId { get; set; }
        public int UserRoleId { get; set; }
    }

    public class UserUpdateDTO
    {
        public string DisplayName { get; set; }
        public string SpotifyUserId { get; set; }
        public int UserRoleId { get; set; }
    }
}