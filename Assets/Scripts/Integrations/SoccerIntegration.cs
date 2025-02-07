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

    public string GameScene => "3rdParty/FootballSimulator/_StartingScene";

    private List<string> AddedScenes = new List<string>();

    public bool UnloadScenes = false;

    [SerializeField]
    private ChatGenerator banterGenerator;

    [SerializeField]
    private ShareScreenUIManager shareScreenUiManager;

    [SerializeField]
    private TeamEntry[] teams;

    [SerializeField]
    private AudioSource teamAudio;

    private Actor homeActor;
    private Actor awayActor;

    private TeamEntry homeTeam;
    private TeamEntry awayTeam;

    [SerializeField, Range(0f, 1f)]
    private float maxVolume;

    private float volume;

    [SerializeField]
    private float fadeOutDuration;

    private string gameEventLog;
    private bool isMatchLoaded;
    private bool isSceneLoaded;

    [SerializeField]
    private int waitTimeBetweenMatches = 80;

    private Dictionary<string, string[]> lines = new Dictionary<string, string[]>();

    private Stopwatch matchTimer = new Stopwatch();

    private int timeSinceLastMatch => matchTimer.Elapsed.Minutes;

    public void Configure(SoccerConfigs c)
    {
        lines = c.Lines;
        waitTimeBetweenMatches = c.WaitTimeBetweenMatches;
        matchTimer.Start();

        ChatManager.Instance.OnChatQueueEmpty += BreakTheSilence;
        ChatManager.Instance.OnIntermission += (chat) => ToggleGame(chat);
    }

    private void Awake()
    {
        _instance = this;
        ConfigManager.Instance.RegisterConfig(typeof(SoccerConfigs), "soccer", (config) => Configure((SoccerConfigs) config));
        RegisterEmissionEvents();
    }

    private void Update()
    {
        volume = Mathf.Lerp(volume, 0, Time.deltaTime / fadeOutDuration);
        teamAudio.volume = isMatchLoaded ? Mathf.Clamp01(volume) * maxVolume : 0;
    }

    private void BreakTheSilence()
    {
        if (isMatchLoaded || timeSinceLastMatch < waitTimeBetweenMatches)
            return;

        var names = Actor.All.List.Select(a => a.Name).ToArray();
        var home = names[Random.Range(0, names.Length)];
        var away = names[Random.Range(0, names.Length)];

        while (home == away)
            away = names[Random.Range(0, names.Length)];
        Emit($"{home} and {away} challenge each other to a game of soccer!");
    }

    private IEnumerator ToggleGame(Chat chat)
    {
        if (isMatchLoaded || !chat.NewEpisode)
            yield break;

        var topic = chat.Topic;
        var homeName = topic.Find("Home").Trim().Replace("*", string.Empty);
        var awayName = topic.Find("Away").Trim().Replace("*", string.Empty);

        if (string.IsNullOrEmpty(homeName) || string.IsNullOrEmpty(awayName))
        {
            yield return CloseGame();
        }
        else
        {
            homeActor = Actor.All[homeName] ?? chat.Actors[0].Reference;
            awayActor = Actor.All[awayName] ?? chat.Actors[1].Reference;

            while (homeActor == awayActor)
                awayActor = chat.Actors[Random.Range(0, chat.Actors.Length)].Reference;

            LoadGame(homeActor, awayActor);
        }
    }

    private void LoadGame(Actor home, Actor away)
    {
        if (isMatchLoaded)
            return;
        homeTeam = teams[Random.Range(0, teams.Length)];
        awayTeam = teams[Random.Range(0, teams.Length)];

        while (homeTeam == awayTeam)
            awayTeam = teams[Random.Range(0, teams.Length)];

        RenameTeam(homeTeam, home);
        RenameTeam(awayTeam, away);

        if (!isSceneLoaded)
            LoadGameScene();
    }

    private async Task StartGame()
    {
        var match = new MatchCreateRequest(homeTeam, awayTeam);

        await MatchEngineLoader.CreateMatch(match);
        await MatchEngineLoader.Current.StartMatchEngine(
            new UpcomingMatchEvent(match), false, true);
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

        if (UnloadScenes)
            UnloadGameScene();
    }

    private void LoadGameScene()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadSceneAsync(GameScene, LoadSceneMode.Additive);
    }

    private async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AddedScenes.Add(scene.name);
        if (!GameScene.Contains(scene.name))
            return;
        await StartGame();
        isSceneLoaded = true;
    }

    private void UnloadGameScene()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        foreach (var scene in AddedScenes)
            SceneManager.UnloadSceneAsync(scene);
        isSceneLoaded = false;
    }

    private void RegisterEmissionEvents()
    {
        EventManager.Subscribe<GoalScoredEvent>((e) =>
        {
            Emit($"# **{Mathf.CeilToInt(MatchManager.Current.minutes)}'**: :{homeActor.Costume}: **{homeScore} - {awayScore}** :{awayActor.Costume}");
            volume += 1.0f;
        });
        EventManager.Subscribe<FinalWhistleEvent>((e) =>
        {
            Emit($"The game has ended! Final score: {homeScore} - {awayScore}!");
            StartCoroutine(CloseGame());
        });

        RegisterMessageEvents();
    }

    private void RegisterMessageEvents()
    {
        EventManager.Subscribe<RefereeLongWhistleEvent>((e) => Emit(e, "Referee"));
        EventManager.Subscribe<RefereeShortWhistleEvent>((e) => Emit(e, "Referee"));
        EventManager.Subscribe<RefereeLastWhistleEvent>((e) => Emit(e, "Referee"));
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

    private void Emit(IBaseEvent e, string name = null, bool push = false)
    {
        var lineFormat = lines[e.GetType().Name].Random();
        var line = string.Format(lineFormat, name);

        Emit(line, push);
    }

    private void Emit(string log, bool push = true)
    {
        gameEventLog += $"{log}\n";
        volume += 0.1f;

        if (push)
            Push();
        OnEmit?.Invoke(log);
    }

    private void Push()
    {
        banterGenerator.AddIdeaToQueue(new Idea(gameEventLog));
        gameEventLog = string.Empty;
    }

    private string GetName(PlayerEntry e)
    {
        return $"{e.Name} ({e.team.TeamName})";
    }

    private string GetName(AbstractPlayerEvent e)
    {
        return GetName(e.Player.MatchPlayer.Player);
    }

    private void RenameTeam(TeamEntry team, Actor actor)
    {
        for (var i = 0; i < team.Players.Length; ++i)
            team.Players[i].Name = actor.Players[i];

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
}