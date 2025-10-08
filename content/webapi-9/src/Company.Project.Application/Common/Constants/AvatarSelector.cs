namespace Company.Project.Application.Common.Constants;

public static class AvatarSelector
{
    private static readonly string[] avatars =
    [
        "imgs/avatar1.png",
        "imgs/avatar3.png",
        "imgs/avatar4.png",
        "imgs/avatar5.png",
        "imgs/avatar6.png",
        "imgs/avatar7.png",
        "imgs/avatar8.png",
    ];

    private static readonly Random random = Random.Shared;

    public static string GetRandomAvatar()
    {
        int index = random.Next(avatars.Length);
        return avatars[index];
    }
}
