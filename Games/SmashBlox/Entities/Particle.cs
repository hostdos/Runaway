using GameAPI;
using GameAPI.BudgetBoy;

namespace SmashBlox.Entities
{
    class Particle
    {
        public Vector2f Position { get; set; }
        public Vector2f Velocity { get; set; }

        public double Lifetime { get; set; }

        public bool ShouldRemove { get { return Lifetime <= 0d; } }

        public void Update(double dt)
        {
            Position += Velocity * (float) dt;

            Lifetime -= dt;
        }
    }
}
