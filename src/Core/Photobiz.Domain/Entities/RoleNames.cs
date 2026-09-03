namespace Photobiz.Domain.Entities
{
    public static class RoleNames
    {
        public const string Admin = "Admin";
        public const string Photographer = "Photographer";
        public const string Assistant = "Assistant";

        public static readonly IReadOnlyList<string> All = [Admin, Photographer, Assistant];
    }
}
