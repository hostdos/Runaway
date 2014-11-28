using System;
using System.Collections;
using System.IO;

using GameAPI;
using GameAPI.BudgetBoy;

using SmashBlox.Entities;

namespace SmashBlox.Stages
{
    class GameStage : SmashBloxStage
    {
        private enum State : byte
        {
            PreInit,
            NewBall,
            Serving,
            Playing,
            LostBall,
            NextLifeWait,
            GameOver
        }

        private BlockGrid _blocks;
        private Paddle _paddle;
        private Ball _ball;

        private Text _scoreText;
        private Text _livesText;

        private int _score;
        private int _combo;
        private int _lives;

        private Random _rand;

        private State _curState;

        public GameStage(Main game)
            : base(game)
        {
            Setup();
            StartCoroutine(MainCoroutine);
        }

        public GameStage(Main game, BinaryReader reader)
            : base(game)
        {
            Setup();
            LoadState(reader);
            StartCoroutine(MainCoroutine);
        }

        private void Setup()
        {
            Graphics.SetClearColor(SwatchIndex.Black);

            _paddle = Add(new Paddle {
                X = Graphics.Width / 2,
                Y = 8
            }, 0);

            _ball = Add(new Ball(), 1);

            _blocks = Add(new BlockGrid(12, 8) {
                X = 4,
                Y = Graphics.Height - 68 - 12
            }, 0);

            var font = Graphics.GetImage("Resources", "font");
            var white = Graphics.Palette.FindSwatch(0xffffff, 0xffffff, 0xffffff);

            _scoreText = Add(new Text(font, white) {
                X = 4, Y = Graphics.Height - 12
            }, 0);

            _livesText = Add(new Text(font, white) {
                Y = Graphics.Height - 12
            }, 0);

            _blocks.BlockHit += (sender, e) => {
                AddScore(++_combo);
            };

            _ball.PaddleHit += (sender, e) => {
                _combo = 0;
            };

            _rand = new Random();

            _curState = State.PreInit;

            StartCoroutine(ParticleSpamCoroutine);
        }

        protected override void OnSaveState(BinaryWriter writer)
        {
            writer.Write((byte) _curState);

            writer.Write(_score);
            writer.Write(_lives);

            writer.Write(_combo);

            _ball.SaveState(writer);
            _paddle.SaveState(writer);
            _blocks.SaveState(writer);
        }

        protected override void OnLoadState(BinaryReader reader)
        {
            _curState = (State) reader.ReadByte();

            SetScore(reader.ReadInt32());
            SetLives(reader.ReadInt32());

            _combo = reader.ReadInt32();

            _ball.LoadState(reader);
            _paddle.LoadState(reader);
            _blocks.LoadState(reader);
        }

        private void AddScore(int val)
        {
            SetScore(_score + val);
        }

        private void SetScore(int val)
        {
            _score = val;
            _scoreText.Value = String.Format("SCORE {0}", val);
        }

        private void SetLives(int val)
        {
            _lives = val;
            _livesText.Value = String.Format("{0} LIVES", val);
            _livesText.X = Graphics.Width - _livesText.Width - 4;
        }

        private void ChangeState(State nextState)
        {
            _curState = nextState;
        }

        private bool ChangeState(State nextState, ref bool skip)
        {
            if (skip && nextState != _curState) return false;

            ChangeState(nextState);

            skip = false;

            return true;
        }

        private IEnumerator MainCoroutine()
        {
            var skip = true;

            if (ChangeState(State.PreInit, ref skip)) {
                SetScore(0);
                SetLives(3);

                for (int y = 0; y < _blocks.Rows; ++y) {
                    for (int x = 0; x < _blocks.Columns; ++x) {
                        if ((x + (y / 2) * 3 + 1) % 6 <= 1) continue;

                        _blocks[x, y] = 1 + (y % _blocks.Phases);
                    }
                }

                _ball.Velocity = new Vector2f(0f, 0f);
                _ball.X = _paddle.NextX;
                _ball.Y = _paddle.Y + 8;
                _ball.IsVisible = false;

                yield return FadeInCoroutine(FadeDuration);
            }

            _ball.IsVisible = true;

            while (_lives >= 0) {
                if (ChangeState(State.NewBall, ref skip)) {
                    _ball.Velocity = new Vector2f(0f, 0f);
                    _ball.Y = _paddle.Y + 8;

                    _combo = 0;

                    StartCoroutine(BallFlashCoroutine);
                }

                if (ChangeState(State.Serving, ref skip)) {
                    do {
                        _ball.X = _paddle.NextX;
                        yield return null;
                    } while (!Controls.A.JustPressed);
                    
                    _ball.Velocity = new Vector2f(_paddle.NextX > _paddle.X ? 1f : -1f, 1.5f) * 96f;
                }

                if (ChangeState(State.Playing, ref skip)) {
                    while (_ball.Y > 4f) {
                        yield return null;
                    }
                }

                if (ChangeState(State.LostBall, ref skip)) {
                    SetLives(_lives - 1);
                    Audio.Play(Audio.GetSound("Resources", "miss"), 0f, 1, 1);
                }

                if (ChangeState(State.NextLifeWait, ref skip)) {
                    yield return Delay(0.5d);
                }
            }

            ChangeState(State.GameOver);

            _ball.IsVisible = false;

            yield return FadeOutCoroutine(FadeDuration);

            Game.SubmitHighscore(false, _score);
        }

        private IEnumerator ParticleSpamCoroutine()
        {
            while (true) {
                yield return Delay(1d / 30d);
                if (_ball.IsVisible) {
                    AddParticle(_ball.Position, new Vector2f(
                        (float) _rand.NextDouble() * 16f - 8f,
                        (float) _rand.NextDouble() * 16f - 8f),
                        _rand.NextDouble() * 0.25 + 0.25);
                }
            }
        }

        private IEnumerator BallFlashCoroutine()
        {
            for (int i = 0; i < 4; ++i) {
                _ball.IsVisible = false;
                yield return Delay(1d / 15d);
                _ball.IsVisible = true;
                yield return Delay(1d / 15d);
            }
        }
    }
}
