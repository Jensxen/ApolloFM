namespace FM.Domain.Entities
{
    public static class Permissions
    {
        public const string CreatePost = "CreatePost";
        public const string EditPost = "EditPost";
        public const string DeletePost = "DeletePost";
        public const string CreateComment = "CreateComment";
        public const string EditComment = "EditComment";
        public const string DeleteComment = "DeleteComment";
        public const string CreateSubForum = "CreateSubForum";
        public const string EditSubForum = "EditSubForum";
        public const string DeleteSubForum = "DeleteSubForum";
        public const string BanUserFromSubForum = "BanUserFromSubForum";
        public const string UnbanUserFromSubForum = "UnbanUserFromSubForum";
        public const string BanUserFromForum = "BanUserFromForum";
        public const string UnbanUserFromForum = "UnbanUserFromForum";
        public const string PromoteToSubForumModerator = "PromoteToSubForumModerator";
        public const string PromoteToForumAdmin = "PromoteToForumAdmin";
        public const string DemoteToUser = "DemoteToUser";
    }
}

