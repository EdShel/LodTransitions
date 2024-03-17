using System;

namespace LodTransitions.Utilities
{
    public class FpsCounter
    {
        private int counter;
        private double secondsCounter;

        public int Fps { get; set; }

        public void Tick(TimeSpan timeSinceLastFrame)
        {
            this.counter++;
            this.secondsCounter += timeSinceLastFrame.TotalSeconds;

            if (this.secondsCounter > 1.0)
            {
                this.secondsCounter = 0;
                this.Fps = this.counter;
                this.counter = 0;
            }
        }
    }
}
