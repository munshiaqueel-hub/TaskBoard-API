using TaskBoard.Api.Models;

public static class GuidHelper
{
    public static Guid GetParsedGuidFromString(string id, out Guid returnGuid)
    {
        if (Guid.TryParse(id, out Guid guid))
        {
            returnGuid = guid;
        }
        else
        {
            returnGuid = Guid.Empty;
        }
        return returnGuid;
    }
}