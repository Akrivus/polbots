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

    private TeamEntry homeTeam;
    private int homeScore;
    private TeamEntry awayTeam;
    private int awayScore;

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

    private Stopwatch matchTimer = new Stopwatch();
    private Stopwatch sceneTimer = new Stopwatch();

    private int timeSinceLastMatch => matchTimer.Elapsed.Minutes;
    private float gameTime => sceneTimer.Elapsed.Minutes;

    public void Configure(SoccerConfigs c)
    {
        waitTimeBetweenMatches = c.WaitTimeBetweenMatches;
        matchTimer.Start();

        ChatManager.Instance.OnChatQueueEmpty += BreakTheSilence;
        ChatManager.Instance.OnIntermission += (chat) => ToggleGame(chat);
    }

    private void Awake()
    {
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
        Emit($"{home} and {away} challenge each other to a game of soccer!", true);
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
            var home = Actor.All[homeName] ?? chat.Actors[0].Reference;
            var away = Actor.All[awayName] ?? chat.Actors[1].Reference;

            while (home == away)
                away = chat.Actors[Random.Range(0, chat.Actors.Length)].Reference;

            LoadGame(home, away);
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

        homeScore = 0;
        awayScore = 0;

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

    private void EndGame()
    {
        Emit($"The game between {homeTeam.TeamName} and {awayTeam.TeamName} has ended in {gameTime} minutes!\n" +
            $"The final score is {homeTeam.TeamName} {homeScore} - {awayScore} {awayTeam.TeamName}!",
            true);
        StartCoroutine(CloseGame());
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
        sceneTimer.Restart();
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
        EventManager.Subscribe<FirstWhistleEvent>((e) => Emit($"The game between {homeTeam.TeamName} and {awayTeam.TeamName} has started!" +
            $"May the best team win!", true));
        EventManager.Subscribe<FinalWhistleEvent>((e) => EndGame());

        EventManager.Subscribe<GoalScoredEvent>((e) =>
        {
            if (e.Scorer.team.TeamName == homeTeam.TeamName)
                homeScore++;
            else
                awayScore++;
            Emit($"{GetName(e.Scorer)} scores a goal for {e.Scorer.team.TeamName} at {gameTime} minutes!\n" +
                $"The score is now {homeTeam.TeamName} {homeScore} - {awayScore} {awayTeam.TeamName}!",
                true);
            volume += 1.0f;
        });

        RegisterMessageEvents();
    }

    private void RegisterMessageEvents()
    {
        EventManager.Subscribe<RefereeLongWhistleEvent>((e) => Emit("**Referee:** That�s the long whistle � time to take a breather!"));
        EventManager.Subscribe<RefereeShortWhistleEvent>((e) => Emit("**Referee:** Quick whistle! What�s the call?"));
        EventManager.Subscribe<RefereeLastWhistleEvent>((e) => Emit("**Referee:** That�s the final whistle! Game over, folks!"));
        EventManager.Subscribe<BallHitTheWoodWorkEvent>((e) => Emit($"SO CLOSE! The ball smacks the woodwork and stays out � the goalposts say 'not today!'"));
        EventManager.Subscribe<KickOffEvent>((e) => Emit("Kick-off! The game is underway!"));
        EventManager.Subscribe<OutEvent>((e) => Emit("Out of bounds! The ball's gone rogue."));
        EventManager.Subscribe<ShootWentOutEvent>((e) => Emit("Wild shot! That ball�s heading for the stands!"));
        EventManager.Subscribe<KeeperHitTheBallButCouldNotControlEvent>((e) => Emit($"{GetName(e)} gets a touch but can�t keep hold of it! Danger still lurks!"));
        EventManager.Subscribe<KeeperSavesTheBallEvent>((e) => Emit($"{GetName(e)} with a clutch save! The goal is still protected!"));
        EventManager.Subscribe<PlayerDisbalancedEvent>((e) => Emit($"{GetName(e)} takes a tumble! Not the smoothest move."));
        EventManager.Subscribe<PlayerSlideTackleEvent>((e) => Emit($"{GetName(e)} goes in for a slide tackle! Clean or dirty?"));
        EventManager.Subscribe<PlayerTackledEvent>((e) => Emit($"{GetName(e)} gets taken down! That�s gotta sting."));
        EventManager.Subscribe<PlayerPassEvent>((e) => Emit($"{GetName(e)} sends a crisp pass! Eyes on the prize."));
        EventManager.Subscribe<PlayerControlBallEvent>((e) => Emit($"{GetName(e)} takes control and drives forward!"));
        EventManager.Subscribe<PlayerWinTheBallEvent>((e) => Emit($"{GetName(e)} snatches the ball back! Possession regained."));
        EventManager.Subscribe<PlayerLossTheBallEvent>((e) => Emit($"{GetName(e)} loses the ball! That didn't go as planned."));
        EventManager.Subscribe<PlayerChipShootEvent>((e) => Emit($"{GetName(e)} tries a cheeky chip! Will it float in?"));
        EventManager.Subscribe<PlayerShootEvent>((e) => Emit($"{GetName(e)} takes a shot! Boom, that�s power!"));
        EventManager.Subscribe<PlayerThrowInEvent>((e) => Emit($"{GetName(e)} launches a throw-in! Let�s see where this lands."));
    }

    private void Emit(string message, bool forcePush = false)
    {
        gameEventLog += message + "\n";
        volume += 0.1f;

        if (forcePush)
            Push();
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
        var names = actor.Players.Shuffle().Select(s => s.Split(' ')[0]).ToArray();
        for (var i = 0; i < team.Players.Length; ++i)
            team.Players[i].Name = names[i];

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
