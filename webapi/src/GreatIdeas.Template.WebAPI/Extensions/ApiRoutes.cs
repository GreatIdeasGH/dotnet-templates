namespace GreatIdeas.Template.WebAPI.Extensions;

public static partial class ApiRoutes
{
    public const string ApiServerEndpoint = "ServerAPI";
    public const string AuthEndpoint = "api/auth";
    public const string AccountEndpoint = "api/accounts";
    public const string LevelEndpoint = "api/levels";
    public const string StudentEndpoint = "api/students";
    public const string UnitEndpoint = "api/units";
    public const string UnitCategoryEndpoint = "api/units/{unitId:guid}/categories";
    public const string ResourceEndpoint = "api/contents/{contentId:guid}/resources";
    public const string ResourceResponseEndpoint = "api/resources/{resourceId:guid}/responses";
    public const string ConclusionEndpoint = "api/categories/{categoryId}/conclusions";
    public const string ContentIntroductionEndpoint = "api/contents/{contentId}/introductions";
    public const string CategoryIntroductionEndpoint = "api/categories/{categoryId}/introductions";
    public const string ContentEndpoint = "api/categories/{categoryId}/contents";
    public const string StudentScoreEndpoint = "api/scores";
    public const string StudentProgressEndpoint = "api/progress";
    public const string LeaderboardEndpoint = "api/leaderboards";
    public const string StaffEndpoint = "api/staffs";
    public const string FlashcardEndpoint = "api/flashcards";
    public const string AuditEndpoint = "api/audits";
    public const string DashboardEndpoint = "api/dashboard";
    public const string SpeechLogEndpoint = "api/speechlogs";
    public const string DatabaseEndpoint = "api/database";
}
