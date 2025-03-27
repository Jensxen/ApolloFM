namespace FM.Application.QueryDTO.SubForumDTO
{
    public class SubForumQueryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class SubForumCreateDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class SubForumUpdateDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}