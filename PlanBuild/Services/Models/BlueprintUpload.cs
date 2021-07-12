using PlanBuild.Blueprints;

namespace PlanBuild.Services.Models
{
    class BlueprintUpload
    {
        public string Id;
        public string Name;
        public string Creator;
        public string Description;
        public string Blob;

        public static BlueprintUpload Convert(Blueprint blueprint)
        {
            return new BlueprintUpload
            {
                Id = blueprint.ID,
                Name = blueprint.Name,
                Creator = blueprint.Creator,
                Description = blueprint.Description,
                Blob = System.Convert.ToBase64String(blueprint.ToBlob())
            };
        }

        public static Blueprint Convert(BlueprintUpload blueprintUpload)
        {
            return Blueprint.FromBlob(blueprintUpload.Id, System.Convert.FromBase64String(blueprintUpload.Blob));
        }
    }
}
