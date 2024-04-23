using System.Collections.Generic;
using UnityEngine;

namespace Tools.Animations
{
    public class ParticlesAnimation : OvAnimation
    {
        #region Members

        protected GameObject m_Particles;
      
        #endregion


        #region Animation

        public void Initialize(string id = "", float duration = -1f, string particlesName = "", float size = 1f, float speed = 1f, string layer = "Default")
        {
            var animationParticles = AssetLoader.Load<GameObject>(particlesName, AssetLoader.c_AnimationParticlesPath);
            if (animationParticles == null)
                return;

            m_Particles = Instantiate(animationParticles, gameObject.transform);

            SetUpPos();
            SetUpScale(size);
            SetUpSpeed(speed);
            SetUpRendering(layer);

            base.Initialize(id, duration);
        }

        public override void Deactivate()
        {
            // remove particles
            Destroy(m_Particles);
        }

        #endregion


        #region GUI Manipulators

        public void SetUpPos()
        {
            var initY = -gameObject.GetComponent<RectTransform>().rect.height / 2;
            m_Particles.transform.localPosition += new Vector3(0, initY, 0);
        }

        public void SetUpScale(float size)
        {
            m_Particles.transform.localScale *= size;
        }

        public void SetUpSpeed(float speed)
        {
            var particles = Finder.FindComponents<ParticleSystem>(m_Particles);
            foreach (var particle in particles)
            {
                var mainModule = particle.main;
                mainModule.startSpeedMultiplier = speed;
            }
        }

        public void SetUpRendering(string layer)
        {
            // Get the Renderer component of the particle system
            List<Renderer> renderers = Finder.FindComponents<Renderer>(m_Particles);

            // Adjust the sorting order to render the particles on top of other UI elements
            foreach (Renderer particlesRenderer in renderers)
            {
                particlesRenderer.sortingLayerName = layer; 
            }
        }

        #endregion
    }
}