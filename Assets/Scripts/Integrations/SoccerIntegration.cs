using FStudio.Database;
using FStudio.Events;
using FStudio.MatchEngine;
using FStudio.MatchEngine.Events;
using FStudio.UI.MatchThemes.MatchEvents;
using Shared.Responses;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Diagnostics;

public class SoccerIntegration : MonoBehaviour
{
    public static SoccerIntegration Instance => _instance ?? (_instance = FindAnyObjectByType<SoccerIntegration>());
    private static SoccerIntegration _instance;

    public event System.Action<string> OnEmit;

    public bool ForceSoccerGame = false;

    private const string GameScene = "3rdParty/FootballSimulator/_StartingScene";
    private List<string> addedScenes = new List<string>();
    private Dictionary<string, string[]> lines = new Dictionary<string, string[]>();

    [SerializeField]
    private ChatGenerator banterGenerator;

    [SerializeField]
    private ShareScreenUIManager shareScreenUiManager;

    [SerializeField]
    private TeamEntry[] teams;

    [SerializeField]
    private AudioSource teamAudio;

    [SerializeField, Range(0f, 1f)]
    private float maxVolume;

    [SerializeField]
    private float fadeOutDuration;

    [SerializeField]
    private int waitTimeBetweenMatches = 80;

    private Actor homeActor;
    private Actor awayActor;
    private TeamEntry homeTeam;
    private TeamEntry awayTeam;
    private float volume;
    private bool isMatchLoaded;
    private bool isSceneLoaded;
    private Stopwatch matchTimer = new Stopwatch();
    private string gameEventLog;
    private int TimeSinceLastMatch => matchTimer.Elapsed.Minutes;

    private void Awake()
    {
        _instance = this;
        ConfigManager.Instance.RegisterConfig(typeof(SoccerConfigs), "soccer", config => Configure((SoccerConfigs) config));
        RegisterEmissionEvents();

        ForceSoccerGame = true;
    }

    private void Update()
    {
        volume = Mathf.Lerp(volume, 0, Time.deltaTime / fadeOutDuration);
        teamAudio.volume = isMatchLoaded ? Mathf.Clamp01(volume) * maxVolume : 0;
    }

    public void Configure(SoccerConfigs config)
    {
        lines = config.Lines;
        waitTimeBetweenMatches = config.WaitTimeBetweenMatches;
        matchTimer.Start();
        ChatManager.Instance.OnChatQueueEmpty += BreakTheSilence;
        ChatManager.Instance.AfterIntermission += ToggleGame;
    }

    private void BreakTheSilence()
    {
        if (isMatchLoaded || TimeSinceLastMatch < waitTimeBetweenMatches && !ForceSoccerGame)
            return;
        var names = Actor.All.List.Select(a => a.Name).ToArray();
        var home = names.Random();
        var away = names.Except(new[] { home }).Random();

        homeActor = Actor.All[home];
        awayActor = Actor.All[away];

        Emit($"# {Names} challenge each other to a game of soccer!");
        ForceSoccerGame = false;
    }

    private void ToggleGame(Chat chat)
    {
        if (isMatchLoaded || !chat.NewEpisode)
            return;

        var homeName = chat.Topic.Find("Home");
        var awayName = chat.Topic.Find("Away");

        homeActor = Actor.All[homeName];
        awayActor = Actor.All[awayName];

        if (homeActor == null || awayActor == null)
            StartCoroutine(CloseGame());
        else
            LoadGame();
    }

    private void LoadGame()
    {
        if (isMatchLoaded)
            return;
        homeTeam = teams.Random();
        awayTeam = teams.Except(new[] { homeTeam }).Random();

        RenameTeam(homeTeam, homeActor);
        RenameTeam(awayTeam, awayActor);

        if (!isSceneLoaded)
            LoadGameScene();
    }

    private async Task StartGame()
    {
        var match = new MatchCreateRequest(homeTeam, awayTeam);
        await MatchEngineLoader.CreateMatch(match);
        await MatchEngineLoader.Current.StartMatchEngine(new UpcomingMatchEvent(match), false, true);

        shareScreenUiManager.ShareScreenOn();
        isMatchLoaded = true;
    }

    private IEnumerator CloseGame()
    {
        yield return new WaitForSeconds(60);
        yield return UnloadGame();
    }

    private async Task UnloadGame()
    {
        if (!isMatchLoaded)
            return;
        shareScreenUiManager.ShareScreenOff();
        await MatchEngineLoader.Current.UnloadMatch();

        isMatchLoaded = false;
        matchTimer.Restart();
        UnloadGameScene();
    }

    private void LoadGameScene()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadSceneAsync(GameScene, LoadSceneMode.Additive);
    }

    private async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        addedScenes.Add(scene.name);
        if (!GameScene.Contains(scene.name))
            return;
        await StartGame();
        isSceneLoaded = true;
    }

    private void UnloadGameScene()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        foreach (var scene in addedScenes) SceneManager.UnloadSceneAsync(scene);
        isSceneLoaded = false;
    }

    private void RegisterEmissionEvents()
    {
        EventManager.Subscribe<FinalWhistleEvent>(e => StartCoroutine(CloseGame()));
        EventManager.Subscribe<GoalScoredEvent>(e => EmitToChat(e));
        RegisterMessageEvents();
    }

    private void RegisterMessageEvents()
    {
        EventManager.Subscribe<RefereeLongWhistleEvent>((e) => Emit(e));
        EventManager.Subscribe<RefereeShortWhistleEvent>((e) => Emit(e));
        EventManager.Subscribe<RefereeLastWhistleEvent>((e) => Emit(e));
        EventManager.Subscribe<FirstWhistleEvent>((e) => Emit(e));
        EventManager.Subscribe<FinalWhistleEvent>((e) => Emit(e));
        EventManager.Subscribe<BallHitTheWoodWorkEvent>((e) => Emit(e));
        EventManager.Subscribe<KickOffEvent>((e) => Emit(e));
        EventManager.Subscribe<OutEvent>((e) => Emit(e));
        EventManager.Subscribe<ShootWentOutEvent>((e) => Emit(e));
        EventManager.Subscribe<KeeperHitTheBallButCouldNotControlEvent>((e) => Emit(e, GetName(e)));
        EventManager.Subscribe<KeeperSavesTheBallEvent>((e) => Emit(e, GetName(e)));
        EventManager.Subscribe<PlayerDisbalancedEvent>((e) => Emit(e, GetName(e)));
        EventManager.Subscribe<PlayerSlideTackleEvent>((e) => Emit(e, GetName(e)));
        EventManager.Subscribe<PlayerTackledEvent>((e) => Emit(e, GetName(e)));
        EventManager.Subscribe<PlayerPassEvent>((e) => Emit(e, GetName(e)));
        EventManager.Subscribe<PlayerControlBallEvent>((e) => Emit(e, GetName(e)));
        EventManager.Subscribe<PlayerWinTheBallEvent>((e) => Emit(e, GetName(e)));
        EventManager.Subscribe<PlayerLossTheBallEvent>((e) => Emit(e, GetName(e)));
        EventManager.Subscribe<PlayerChipShootEvent>((e) => Emit(e, GetName(e)));
        EventManager.Subscribe<PlayerShootEvent>((e) => Emit(e, GetName(e)));
        EventManager.Subscribe<PlayerThrowInEvent>((e) => Emit(e, GetName(e)));
    }

    private void EmitToChat(GoalScoredEvent e)
    {
        Emit($"# :soccer: GOAL! {Score} ({e.Scorer.Name}, {Minutes})");
        volume += 1.0f;
    }

    private void Emit(IBaseEvent e, string name = null, bool push = false) {
        var line = lines[e.GetType().Name].Random();
        var log = string.Format(line,
            name,
            Score,
            Minutes);
        Emit(log, push);
    }

    private void Emit(string log, bool push = true)
    {
        gameEventLog += log + "\n";
        volume += 0.1f;
        if (push) Push();
        OnEmit?.Invoke(log);
    }

    private void Push()
    {
        if (banterGenerator.Count > 0)
            return;
        banterGenerator.AddIdeaToQueue(new Idea(gameEventLog));
        gameEventLog = string.Empty;
    }

    private void RenameTeam(TeamEntry team, Actor actor)
    {
        team.Players.Zip(actor.Players, (p, n) => p.Name = n).ToList();
        team.TeamLogo.TeamLogoColor1 = actor.Color1;
        team.TeamLogo.TeamLogoColor2 = actor.Color2;
        team.TeamName = actor.Name;

        team.AwayKit.Color1 = actor.Color2;
        team.AwayKit.Color2 = actor.Color1;
        team.AwayKit.GKColor1 = actor.Color3;
        team.AwayKit.GKColor2 = actor.Color3;

        team.HomeKit.Color1 = actor.Color1;
        team.HomeKit.Color2 = actor.Color2;
        team.HomeKit.GKColor1 = actor.Color3;
        team.HomeKit.GKColor2 = actor.Color3;
    }

    private string GetName(AbstractPlayerEvent e) => e.Player.MatchPlayer.Player.Name;

    public string Names => $"**{homeActor.Name}** {homeActor.Costume} and **{awayActor.Name}** {awayActor.Costume}";
    public string Score => $"{homeActor.Costume} **{MatchManager.Current.homeTeamScore} - {MatchManager.Current.awayTeamScore}** {awayActor.Costume}";
    public string Minutes => $"**{Mathf.CeilToInt(MatchManager.Current.minutes / 60)}’**";
}
