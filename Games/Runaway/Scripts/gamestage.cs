using System.Collections;
using System.Collections.Generic;
using GameAPI;
using GameAPI.BudgetBoy;
namespace Games.Runaway
{
    public class GameStage : Stage
    {

        public new Main Game { get {return (Main) base.Game; } }

        List<Bullet> _bullets = new List<Bullet>();
        Player _player;

        float _elapsedTime;
        // called when this stage is created
        public GameStage(Main game) : base(game)
        {
            Debug.Log("GameStage created");
            _player = Add(new Player(), 0);

            _player.Position = new Vector2f(40f, 40f);

        }
        // called when this stage is entered
        protected override void OnEnter()
        {
            Graphics.SetClearColor(Game.Swatches.ClearColor); // set the background color
            _playerDieSound = Audio.GetSound("Resources", "sounds", "player_die");
            StartCoroutine(SpawnBullets);
        }
        // called each tick
        protected override void OnUpdate()
        {
            base.OnUpdate();

            // check collision between player and bullets
            foreach(Bullet bullet in _bullets)
            {
                // check distance from each bullet to the player
                float distSqr = (bullet.Position - _player.Position).LengthSquared;
                if(distSqr < 150.0f)
                {
                    // play death sound
                    Audio.Play(_playerDieSound, 0.0f, 1.0f, 1.0f);
                    // player's dead, restart game
                    Game.SetStage(new TitleStage(Game));
                }
            }
        }
        // called when this stage is rendered
        protected override void OnRender()
        {
            base.OnRender();
        }

        IEnumerator SpawnBullets()
        {
            while(true)
            {
                // wait
                yield return Delay(0.5f);
                // spawn bullet
                float PADDING = 10.0f;
                Vector2f bulletPos = new Vector2f(Mathf.Random(PADDING, Graphics.Width - PADDING), Graphics.Height + PADDING);
                AddBullet(bulletPos, false);
                float delay = Mathf.Ceiling(0.5f - _elapsedTime * 0.01f);

            }
        }
        void AddBullet(Vector2f pos, bool movingDownward)
        {
            Bullet bullet = Add(new Bullet(movingDownward, _elapsedTime), 1);
            bullet.Position = pos;
            // save the bullet to our list so we can keep track of it
            _bullets.Add(bullet);
        }
        public void RemoveBullet(Bullet bullet)
        {
            // remove the bullet from our list (we no longer want to keep track of it)
            _bullets.Remove(bullet);
            // remove the bullet from the stage (this destroys it)
            Remove(bullet);
        }
    }
}
