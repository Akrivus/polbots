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
using static AgenticDialogueGenerator;

public class SoccerIntegration : MonoBehaviour
{
    public string GameScene => "_FootballSimulator/_StartingScene";
    private List<string> AddedScenes = new List<string>();

    public bool UnloadScenes = false;

    [SerializeField]
    private ShareScreenUIManager _shareScreenUIManager;

    [SerializeField]
    private TeamEntry[] teams;

    [SerializeField]
    private ChatEventBroker broker;

    [SerializeField]
    private AudioSource _audio;

    [SerializeField, Range(0f, 1f)]
    private float _maxVolume;

    [SerializeField]
    private float _secondsToSilence;

    private float _volume;

    private TeamEntry _homeTeam;
    private TeamEntry _awayTeam;

    private int _homeScore;
    private int _awayScore;
    private float _gameTime;

    private bool _isMatchLoaded;
    private bool _isSceneLoaded;

    private void Awake()
    {
        RegisterEmissionEvents();
    }

    private void Start()
    {
        // ChatManager.Instance.OnChatQueueEmpty += BreakTheSilence;
        ChatManager.Instance.OnIntermission += (chat) => ToggleGame(chat);
    }

    private void Update()
    {
        _volume = Mathf.Lerp(_volume, 0, Time.deltaTime / _secondsToSilence);
        _audio.volume = _isMatchLoaded ? Mathf.Clamp01(_volume) * _maxVolume : 0;

        if (!_isMatchLoaded)
            return;
        _shareScreenUIManager.SetShareScreenInfo(_gameTime,
            _homeTeam.TeamName, _homeScore,
            _awayTeam.TeamName, _awayScore);
    }

    private void BreakTheSilence()
    {
        if (ChatManager.Instance.NowPlaying == null)
            return;
        var names = ChatManager.Instance.NowPlaying.Names;
        var home = names[Random.Range(0, names.Length)];
        var away = names[Random.Range(0, names.Length)];

        while (home == away)
            away = names[Random.Range(0, names.Length)];
        new Idea($"Let's play a casual and fun game of soccer between {home} and {away}!");
    }

    private IEnumerator ToggleGame(Chat chat)
    {
        if (!chat.NewEpisode || chat.IsLocked)
            yield break;
        if (_isMatchLoaded)
        {
            var task = UnloadGame();
            yield return new WaitUntil(() => task.IsCompleted);
        }

        var topic = chat.Topic;
        var homeName = topic.Find("Home").Trim().Replace("*", string.Empty);
        var awayName = topic.Find("Away").Trim().Replace("*", string.Empty);

        var home = Actor.All[homeName] ?? chat.Actors[0].Reference;
        var away = Actor.All[awayName] ?? chat.Actors[1].Reference;

        while (home == away)
            away = chat.Actors[Random.Range(0, chat.Actors.Length)].Reference;
        
        LoadGame(home, away);
    }

    private void LoadGame(Actor home, Actor away)
    {
        if (_isMatchLoaded)
            return;
        _homeTeam = teams[Random.Range(0, teams.Length)];
        _awayTeam = teams[Random.Range(0, teams.Length)];

        while (_homeTeam == _awayTeam)
            _awayTeam = teams[Random.Range(0, teams.Length)];

        RenameTeam(_homeTeam, home);
        RenameTeam(_awayTeam, away);

        _homeScore = 0;
        _awayScore = 0;

        _gameTime = 0;

        if (!_isSceneLoaded)
            LoadGameScene();
    }

    private async Task StartGame()
    {
        var match = new MatchCreateRequest(_homeTeam, _awayTeam);
        await MatchEngineLoader.Current.StartMatchEngine(
            new UpcomingMatchEvent(match), false, true);
        _shareScreenUIManager.ShareScreenOn();
        _isMatchLoaded = true;
    }

    private void AnnounceGameStart()
    {
        broker.Receive($"The game between {_homeTeam.TeamName} and {_awayTeam.TeamName} has started! May the best team win!", false, true);
    }

    private void EndGame()
    {
        broker.Receive($"The game between {_homeTeam.TeamName} and {_awayTeam.TeamName} has ended!\n" +
            $"The final score is {_homeTeam.TeamName} {_homeScore} - {_awayScore} {_awayTeam.TeamName}!\n" +
            $"(It's time to leave. Append `{DialogueAgent.END_TOKEN}` to exit the conversation.)", false, true);
        broker.Close();
        StartCoroutine(CloseGame());
    }

    private IEnumerator CloseGame()
    {
        yield return new WaitForSeconds(60);
        var task = UnloadGame();
        yield return new WaitUntil(() => task.IsCompleted);
    }

    private async Task UnloadGame()
    {
        if (!_isMatchLoaded)
            return;
        _shareScreenUIManager.ShareScreenOff();

        await MatchEngineLoader.Current.UnloadMatch();
        _isMatchLoaded = false;

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
        _isSceneLoaded = true;
    }

    private void UnloadGameScene()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        foreach (var scene in AddedScenes)
            SceneManager.UnloadSceneAsync(scene);
        _isSceneLoaded = false;
    }

    private void RegisterEmissionEvents()
    {
        EventManager.Subscribe<FirstWhistleEvent>((e) => AnnounceGameStart());
        EventManager.Subscribe<FinalWhistleEvent>((e) => EndGame());

        EventManager.Subscribe<GameTimeEvent>((e) => _gameTime = Mathf.Max(_gameTime, e.GameTime));
        EventManager.Subscribe<GoalScoredEvent>((e) =>
        {
            if (e.Scorer.team.TeamName == _homeTeam.TeamName)
                _homeScore++;
            else
                _awayScore++;
            broker.Receive($"{GetName(e.Scorer)} scores a goal for {e.Scorer.team.TeamName}!\n" +
                $"The score is now {_homeTeam.TeamName} {_homeScore} - {_awayScore} {_awayTeam.TeamName}!", false, true);
            _volume += 1.0f;
        });

        RegisterMessageEvents();
    }

    private void RegisterMessageEvents()
    {
        EventManager.Subscribe<RefereeLongWhistleEvent>((e) => Emit("**Referee:** That’s the long whistle — time to take a breather!"));
        EventManager.Subscribe<RefereeShortWhistleEvent>((e) => Emit("**Referee:** Quick whistle! What’s the call?"));
        EventManager.Subscribe<RefereeLastWhistleEvent>((e) => Emit("**Referee:** That’s the final whistle! Game over, folks!"));
        EventManager.Subscribe<BallHitTheWoodWorkEvent>((e) => Emit($"SO CLOSE! The ball smacks the woodwork and stays out — the goalposts say 'not today!'"));
        EventManager.Subscribe<KickOffEvent>((e) => Emit("Kick-off! The game is underway!"));
        EventManager.Subscribe<OutEvent>((e) => Emit("Out of bounds! The ball’s gone rogue."));
        EventManager.Subscribe<ShootWentOutEvent>((e) => Emit("Wild shot! That ball’s heading for the stands!"));
        EventManager.Subscribe<KeeperHitTheBallButCouldNotControlEvent>((e) => Emit($"{GetName(e)} gets a touch but can’t keep hold of it! Danger still lurks!"));
        EventManager.Subscribe<KeeperSavesTheBallEvent>((e) => Emit($"{GetName(e)} with a clutch save! The goal is still protected!"));
        EventManager.Subscribe<PlayerDisbalancedEvent>((e) => Emit($"{GetName(e)} takes a tumble! Not the smoothest move."));
        EventManager.Subscribe<PlayerSlideTackleEvent>((e) => Emit($"{GetName(e)} goes in for a slide tackle! Clean or dirty?"));
        EventManager.Subscribe<PlayerTackledEvent>((e) => Emit($"{GetName(e)} gets taken down! That’s gotta sting."));
        EventManager.Subscribe<PlayerPassEvent>((e) => Emit($"{GetName(e)} sends a crisp pass! Eyes on the prize.", true));
        EventManager.Subscribe<PlayerControlBallEvent>((e) => Emit($"{GetName(e)} takes control and drives forward!", true));
        EventManager.Subscribe<PlayerWinTheBallEvent>((e) => Emit($"{GetName(e)} snatches the ball back! Possession regained."));
        EventManager.Subscribe<PlayerLossTheBallEvent>((e) => Emit($"{GetName(e)} loses the ball! That didn’t go as planned."));
        EventManager.Subscribe<PlayerChipShootEvent>((e) => Emit($"{GetName(e)} tries a cheeky chip! Will it float in?"));
        EventManager.Subscribe<PlayerShootEvent>((e) => Emit($"{GetName(e)} takes a shot! Boom, that’s power!"));
        EventManager.Subscribe<PlayerThrowInEvent>((e) => Emit($"{GetName(e)} launches a throw-in! Let’s see where this lands."));
    }

    private void Emit(string message, bool initialOnly = false)
    {
        broker.Receive(message, initialOnly, !initialOnly);
        _volume += 0.1f;
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
        var names = actor.Players.Shuffle().ToArray();
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
