using System.Collections.Generic;
using System.IO;
using System.Linq;

using GameAPI;
using GameAPI.BudgetBoy;

using ResourceLibrary;

using SmashBlox.Stages;

namespace SmashBlox
{
    [GameInfo(
        Title = "Smash Blox",
        AuthorName = "JamesK",
        AuthorContact = "james.king@facepunchstudios.com",
        Description = "Like Breakout but cheaper!",

        UpdateRate = 60
    )]
    [GraphicsInfo(Width = 200, Height = 160)]
    public class Main : Game
    {
        private Demo _demo;
        
        private Image[] _countdownImages;
        private Sprite _countdownSprite;

        public Swatches Swatches { get; private set; }

        protected override void OnLoadResources(ResourceVolume volume)
        {
            base.OnLoadResources(volume);

            _demo = volume.Get<Demo>("Resources", "attract");
        }

        protected override void OnLoadPalette(PaletteBuilder builder)
        {
            Swatches = new Swatches(builder);
        }

        protected override IEnumerable<Highscore> OnSetupInitialScores()
        {
            yield return new Highscore("AAA", 100);
            yield return new Highscore("RLY", 90);
            yield return new Highscore("LAY", 80);
            yield return new Highscore("BUC", 70);
            yield return new Highscore("ROB", 60);
            yield return new Highscore("ZKS", 50);
            yield return new Highscore("IAN", 40);
            yield return new Highscore("GAR", 30);
            yield return new Highscore("JON", 20);
            yield return new Highscore("IVN", 10);
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _countdownImages = Enumerable.Range(1, 10)
                .Select(x => Graphics.GetImage("Resources", "countdown", x.ToString()))
                .ToArray();

            var swatch = Graphics.Palette.FindSwatch(0x000000, 0xffffff, 0x000000);

            _countdownSprite = new Sprite(_countdownImages[0], swatch);
            _countdownSprite.Position = (Graphics.Size - _countdownSprite.Size) / 2;
        }

        protected override void OnReset()
        {
            if (_demo != null) {
                SetStage(new AttractStage(this, _demo));
            } else {
                Start();
            }
        }

        public void Start()
        {
            SetStage(new GameStage(this));
        }

        public void SubmitHighscore(bool completed, int score)
        {
            SetStage(new EnterScoreStage(this, completed, score));
        }

        public void ShowHighscores()
        {
            SetStage(new HighscoreStage(this));
        }

        public void ShowHighscores(Highscore submit)
        {
            SubmitHighscore(submit);

            SetStage(new HighscoreStage(this, submit));
        }

        protected override void OnSaveState(BinaryWriter writer)
        {
            writer.Write(CurrentStage.GetType().Name);
            CurrentStage.SaveState(writer);
        }

        protected override void OnLoadState(BinaryReader reader)
        {
            var stageName = reader.ReadString();

            if (stageName == typeof(AttractStage).Name) {
                SetStage(new AttractStage(this, _demo, reader));
            } else if (stageName == typeof(GameStage).Name) {
                SetStage(new GameStage(this, reader));
            } else if (stageName == typeof(EnterScoreStage).Name) {
                SetStage(new EnterScoreStage(this, reader));
            } else if (stageName == typeof(HighscoreStage).Name) {
                SetStage(new HighscoreStage(this, reader));
            }
        }

        protected override void OnRenderPauseScreen(double timeUntilReset)
        {
            RenderPausedFrame();

            _countdownSprite.Image = _countdownImages[(int) Mathf.Clamp((float) timeUntilReset, 0, 9)];
            _countdownSprite.Render(Graphics);
        }
    }
}
