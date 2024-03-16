using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LodTransitions.Rendering
{
    public class MainMaterial
    {
        public MainMaterial(Effect effect)
        {
            this.Effect = effect;
        }

        public Effect Effect { get; }

        public Matrix WorldViewProjection
        {
            get
            {
                return this.Effect.Parameters["WorldViewProjection"].GetValueMatrix();
            }
            set
            {
                this.Effect.Parameters["WorldViewProjection"].SetValue(value);
            }
        }

        public Vector3 Albedo
        {
            get
            {
                return this.Effect.Parameters["Albedo"].GetValueVector3();
            }
            set
            {
                this.Effect.Parameters["Albedo"].SetValue(value);
            }
        }

        public Vector3 LightDirection
        {
            get
            {
                return this.Effect.Parameters["LightDirection"].GetValueVector3();
            }
            set
            {
                this.Effect.Parameters["LightDirection"].SetValue(value);
            }
        }

        public float Progress
        {
            get
            {
                return this.Effect.Parameters["Progress"].GetValueSingle();
            }
            set
            {
                this.Effect.Parameters["Progress"].SetValue(value);
            }
        }

        public EffectPass MainPass => this.Effect.CurrentTechnique.Passes["MainPass"];
        public EffectPass AlphaPass => this.Effect.CurrentTechnique.Passes["AlphaPass"];
        public EffectPass NoisePass => this.Effect.CurrentTechnique.Passes["NoisePass"];
        public EffectPass GeomorphPass => this.Effect.CurrentTechnique.Passes["GeomorphPass"];
    }
}