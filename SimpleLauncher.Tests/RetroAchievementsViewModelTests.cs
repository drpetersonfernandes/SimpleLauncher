using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Moq;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.RetroAchievements;
using SimpleLauncher.Core.Services.SettingsManager;
using SimpleLauncher.Services.RetroAchievements;
using SimpleLauncher.ViewModels;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
///     Tests for <see cref="RetroAchievementsViewModel" /> (WPF) — credentials, success, unauthorized, error, and utility
///     paths.
///     Mirrors the Avalonia suite; WPF service requires two ILogger parameters (both non-null, second overwrites first).
/// </summary>
public class RetroAchievementsViewModelTests
{
    private static IConfiguration Config => new ConfigurationBuilder().Build();

    private static string ProfileJson(string user = "testuser", int permissions = 1, int untracked = 0,
        string rank = "123",
        string motto = "Hello", string richPresence = "Playing", string memberSince = "2020-01-01 00:00:00")
    {
        return JsonSerializer.Serialize(new
        {
            User = user,
            ULID = "ulid123",
            UserPic = "/UserPic/test.png",
            MemberSince = memberSince,
            RichPresenceMsg = richPresence,
            LastGameID = 1,
            ContribCount = 5,
            ContribYield = 100,
            TotalPoints = 1234,
            TotalSoftcorePoints = 100,
            TotalTruePoints = 1500,
            Permissions = permissions,
            Untracked = untracked,
            ID = 42,
            UserWallActive = true,
            Motto = motto,
            Rank = rank
        });
    }

    private static string RecentlyPlayedJson(int gameId = 100, string title = "Super Mario Bros.")
    {
        return JsonSerializer.Serialize(new[]
        {
            new
            {
                GameID = gameId,
                ConsoleID = 7,
                ConsoleName = "NES",
                Title = title,
                ImageIcon = "/Images/000001.png",
                ImageTitle = "/Images/000002.png",
                ImageIngame = "/Images/000003.png",
                ImageBoxArt = "/Images/000004.png",
                LastPlayed = "2024-01-01 12:00:00",
                AchievementsTotal = 10,
                NumPossibleAchievements = 10,
                PossibleScore = 400,
                NumAchieved = 5,
                ScoreAchieved = 200,
                NumAchievedHardcore = 3,
                ScoreAchievedHardcore = 150
            }
        });
    }

    private static string EarnedAchievementsJson(int count = 2)
    {
        var list = Enumerable.Range(1, count).Select(i => new
        {
            Date = "2024-01-15 10:00:00",
            HardcoreMode = 1,
            AchievementID = 1000 + i,
            Title = $"Achievement {i}",
            Description = $"Desc {i}",
            BadgeName = "badge",
            Points = 10 * i,
            TrueRatio = 10 * i,
            Type = "progression",
            Author = "author",
            AuthorULID = "ulid",
            GameTitle = "Game",
            GameIcon = "/img.png",
            GameID = 1,
            ConsoleName = "NES",
            CumulScore = 10 * i,
            BadgeURL = "/Badge/badge.png",
            GameURL = "/game/1"
        }).ToArray();
        return JsonSerializer.Serialize(list);
    }

    private static string CompletionProgressJson(int gameId = 1, string title = "Game")
    {
        return JsonSerializer.Serialize(new
        {
            Count = 1,
            Total = 1,
            Results = new[]
            {
                new
                {
                    GameID = gameId,
                    Title = title,
                    ImageIcon = "/Images/000001.png",
                    ConsoleID = 7,
                    ConsoleName = "NES",
                    MaxPossible = 10,
                    NumAwarded = 5,
                    NumAwardedHardcore = 3,
                    MostRecentAwardedDate = "2024-01-01 12:00:00",
                    HighestAwardKind = "mastered",
                    HighestAwardDate = "2024-01-02 12:00:00"
                }
            }
        });
    }

    private static RetroAchievementsViewModel CreateVm(
        Func<HttpRequestMessage, HttpResponseMessage> responder,
        string? raUsername = "testuser",
        string? raApiKey = "testkey",
        Mock<IResourceProvider>? resourceProvider = null,
        Mock<IMessageBoxLibraryService>? messageBox = null)
    {
        var logger = new Mock<ILogger>();
        var credProtector = new Mock<ICredentialProtector>();
        credProtector.Setup(p => p.Protect(It.IsAny<string>())).Returns<string>(s => s);
        credProtector.Setup(p => p.Unprotect(It.IsAny<string>())).Returns<string>(s => s);
        var mb = messageBox ?? new Mock<IMessageBoxLibraryService>();
        var settings = new SettingsManagerService(Config, logger.Object, credProtector.Object, mb.Object);
        settings.RaUsername = raUsername ?? "";
        settings.RaApiKey = raApiKey ?? "";

        var rp = resourceProvider ?? new Mock<IResourceProvider>();
        rp.Setup(r => r.GetString(It.IsAny<string>(), It.IsAny<string>())).Returns<string, string>((_, fb) => fb);
        // Also setup single-arg overload for safety
        rp.Setup(r => r.GetString(It.IsAny<string>())).Returns<string>(k => k);

        var handler = new FakeHandler(responder);
        var httpClient = new HttpClient(handler);
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
        var manager = new RetroAchievementsManager();
        var raService = new RetroAchievementsService(factory.Object, manager, logger.Object, Config, logger.Object);
        var vm = new RetroAchievementsViewModel(mb.Object, rp.Object, settings, raService, logger.Object);
        return vm;
    }

    private static HttpResponseMessage JsonResponse(string json, HttpStatusCode status = HttpStatusCode.OK)
    {
        return new HttpResponseMessage(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
    }

    // ---- LoadUserProfileAsync ----

    [Fact]
    public async Task LoadUserProfile_CredentialsNotSet_ShowsNoProfile()
    {
        var vm = CreateVm(_ => JsonResponse("[]"), "", "");
        await vm.LoadUserProfileAsync();
        Assert.True(vm.NoProfileVisible);
        Assert.False(vm.IsLoading);
        Assert.Null(vm.RecentlyPlayedGames);
        Assert.Contains("not set", vm.NoProfileMainMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadUserProfile_Success_PopulatesProfileAndRecentlyPlayed()
    {
        var vm = CreateVm(req =>
        {
            var uri = req.RequestUri!.ToString();
            if (uri.Contains("API_GetUserProfile.php", StringComparison.Ordinal)) return JsonResponse(ProfileJson());
            if (uri.Contains("API_GetUserRecentlyPlayedGames.php", StringComparison.Ordinal))
                return JsonResponse(RecentlyPlayedJson());
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        await vm.LoadUserProfileAsync();

        Assert.False(vm.NoProfileVisible);
        Assert.False(vm.IsLoading);
        Assert.Equal("testuser", vm.ProfileUser);
        Assert.Equal("Hello", vm.ProfileMotto);
        Assert.Equal("https://retroachievements.org/UserPic/test.png", vm.ProfileImageUrl);
        Assert.Equal("1,234", vm.ProfilePoints);
        Assert.Equal("1,500", vm.ProfileTruePoints);
        Assert.Equal("42", vm.ProfileId);
        Assert.NotNull(vm.RecentlyPlayedGames);
        Assert.Single(vm.RecentlyPlayedGames!);
    }

    [Fact]
    public async Task LoadUserProfile_NullProfile_ShowsFailedMessage()
    {
        var vm = CreateVm(req =>
        {
            var uri = req.RequestUri!.ToString();
            if (uri.Contains("API_GetUserProfile.php", StringComparison.Ordinal)) return JsonResponse("null");
            if (uri.Contains("API_GetUserRecentlyPlayedGames.php", StringComparison.Ordinal)) return JsonResponse("[]");
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        await vm.LoadUserProfileAsync();

        Assert.True(vm.NoProfileVisible);
        Assert.Contains("Failed to load", vm.NoProfileMainMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LoadUserProfile_Unauthorized_ShowsUnauthorizedMessage()
    {
        var vm = CreateVm(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized));
        await vm.LoadUserProfileAsync();
        Assert.True(vm.NoProfileVisible);
        Assert.Contains("invalid", vm.NoProfileMainMessage, StringComparison.OrdinalIgnoreCase);
        Assert.False(vm.IsLoading);
    }

    [Fact]
    public async Task LoadUserProfile_NetworkException_ShowsErrorMessage()
    {
        var vm = CreateVm(_ => throw new HttpRequestException("network down"));
        await vm.LoadUserProfileAsync();
        Assert.True(vm.NoProfileVisible);
        Assert.Contains("Failed to load", vm.NoProfileMainMessage, StringComparison.Ordinal);
        Assert.False(vm.IsLoading);
    }

    [Fact]
    public async Task LoadUserProfile_EmptyMotto_UsesFallback()
    {
        var vm = CreateVm(req =>
        {
            var uri = req.RequestUri!.ToString();
            if (uri.Contains("API_GetUserProfile.php", StringComparison.Ordinal))
                return JsonResponse(ProfileJson(motto: ""));
            if (uri.Contains("API_GetUserRecentlyPlayedGames.php", StringComparison.Ordinal)) return JsonResponse("[]");
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        await vm.LoadUserProfileAsync();

        Assert.Equal("No motto set", vm.ProfileMotto);
    }

    // ---- LoadUnlocksByDateAsync ----

    [Fact]
    public async Task LoadUnlocks_CredentialsNotSet_ShowsNoUnlocks()
    {
        var vm = CreateVm(_ => JsonResponse("[]"), "", "");
        await vm.LoadUnlocksByDateAsync();
        Assert.True(vm.NoUnlocksVisible);
        Assert.Null(vm.Unlocks);
        Assert.True(vm.FetchUnlocksEnabled);
        Assert.False(vm.IsLoading);
    }

    [Fact]
    public async Task LoadUnlocks_Success_PopulatesUnlocksAndTotals()
    {
        var vm = CreateVm(_ => JsonResponse(EarnedAchievementsJson()));
        await vm.LoadUnlocksByDateAsync();
        Assert.False(vm.NoUnlocksVisible);
        Assert.NotNull(vm.Unlocks);
        Assert.Equal(2, vm.Unlocks!.Count);
        Assert.Equal("2", vm.TotalUnlocksInRange);
        Assert.Equal("30", vm.TotalPointsEarnedInRange);
        Assert.True(vm.FetchUnlocksEnabled);
    }

    [Fact]
    public async Task LoadUnlocks_EmptyList_ShowsNoUnlocksFound()
    {
        var vm = CreateVm(_ => JsonResponse("[]"));
        await vm.LoadUnlocksByDateAsync();
        Assert.True(vm.NoUnlocksVisible);
        Assert.Null(vm.Unlocks);
        Assert.Equal("0", vm.TotalUnlocksInRange);
        Assert.Contains("No unlocks", vm.NoUnlocksMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LoadUnlocks_NullResult_ShowsFailedMessage()
    {
        var vm = CreateVm(_ => JsonResponse("null"));
        await vm.LoadUnlocksByDateAsync();
        Assert.True(vm.NoUnlocksVisible);
        Assert.Contains("Failed to load", vm.NoUnlocksMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LoadUnlocks_Unauthorized_ShowsUnauthorized()
    {
        var vm = CreateVm(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized));
        await vm.LoadUnlocksByDateAsync();
        Assert.True(vm.NoUnlocksVisible);
        Assert.Contains("invalid", vm.NoUnlocksMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadUnlocks_NetworkException_ShowsError()
    {
        var vm = CreateVm(_ => throw new HttpRequestException("down"));
        await vm.LoadUnlocksByDateAsync();
        Assert.True(vm.NoUnlocksVisible);
        Assert.Null(vm.Unlocks);
        Assert.Contains("Failed to load", vm.NoUnlocksMessage, StringComparison.Ordinal);
        Assert.True(vm.FetchUnlocksEnabled);
    }

    // ---- LoadUserProgressAsync ----

    [Fact]
    public async Task LoadUserProgress_CredentialsNotSet_ShowsNoProgress()
    {
        var vm = CreateVm(_ => JsonResponse("{}"), "", "");
        await vm.LoadUserProgressAsync();
        Assert.True(vm.NoUserProgressVisible);
        Assert.Null(vm.UserProgress);
        Assert.False(vm.IsLoading);
    }

    [Fact]
    public async Task LoadUserProgress_Success_Populates()
    {
        var vm = CreateVm(_ => JsonResponse(CompletionProgressJson()));
        await vm.LoadUserProgressAsync();
        Assert.False(vm.NoUserProgressVisible);
        Assert.NotNull(vm.UserProgress);
        Assert.Single(vm.UserProgress!);
    }

    [Fact]
    public async Task LoadUserProgress_EmptyList_ShowsNoProgressFound()
    {
        var empty = JsonSerializer.Serialize(new { Count = 0, Total = 0, Results = Array.Empty<object>() });
        var vm = CreateVm(_ => JsonResponse(empty));
        await vm.LoadUserProgressAsync();
        Assert.True(vm.NoUserProgressVisible);
        Assert.Contains("No user completion", vm.NoUserProgressMainMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LoadUserProgress_Null_ShowsFailedMessage()
    {
        var vm = CreateVm(_ => JsonResponse("null"));
        await vm.LoadUserProgressAsync();
        Assert.True(vm.NoUserProgressVisible);
        Assert.Contains("Failed to load", vm.NoUserProgressMainMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LoadUserProgress_Unauthorized_ShowsUnauthorized()
    {
        var vm = CreateVm(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized));
        await vm.LoadUserProgressAsync();
        Assert.True(vm.NoUserProgressVisible);
        Assert.Contains("invalid", vm.NoUserProgressMainMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadUserProgress_NetworkException_ShowsError()
    {
        var vm = CreateVm(_ => throw new HttpRequestException("down"));
        await vm.LoadUserProgressAsync();
        Assert.True(vm.NoUserProgressVisible);
        Assert.Contains("Failed to load", vm.NoUserProgressMainMessage, StringComparison.Ordinal);
        Assert.False(vm.IsLoading);
    }

    // ---- GetProfileUrl ----

    [Theory]
    [InlineData("user", "https://retroachievements.org/user/user")]
    [InlineData("user name", "https://retroachievements.org/user/user%20name")]
    [InlineData("a/b", "https://retroachievements.org/user/a%2Fb")]
    public void GetProfileUrl_EncodesUsername(string username, string expected)
    {
        var vm = CreateVm(_ => JsonResponse("{}"), username);
        Assert.Equal(expected, vm.GetProfileUrl());
    }

    [Fact]
    public void Ctor_SetsDefaultDates()
    {
        var vm = CreateVm(_ => JsonResponse("{}"));
        Assert.Equal(DateTime.Today.AddMonths(-1).Date, vm.FromDate!.Value.Date);
        Assert.Equal(DateTime.Today.Date, vm.ToDate!.Value.Date);
    }

    private sealed class FakeHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_responder(request));
        }
    }
}