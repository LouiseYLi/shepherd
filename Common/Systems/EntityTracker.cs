using System;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

/// <summary>
/// Contains systems responsible for entity tracking.
/// </summary>
namespace Shepherd.Common.Systems
{
    /// <summary>
    /// Tracks the entity closest to the local player.
    /// </summary>
    public class EntityTracker : ModSystem
    {
        // Timer for debugging
        private int debug_timer = 0;
        
        // Closest entity to the LocalPlayer
        private Entity closest_entity;
        private float closest_entity_distance;

        /// <summary>
        /// EntityTracker constructor.
        /// </summary>
        public EntityTracker()
        {
            closest_entity = null;
        }

        /// <summary>
        /// Runs after game updates for each tick.
        /// </summary>
        public override void PostUpdateEverything()
        {
            debug_timer++;
            if (debug_timer > 180)
            {
                Main.NewText($"Player position: {Main.LocalPlayer.Center}");
                debug_timer = 0;

                foreach (NPC npc in Main.ActiveNPCs) {
                    // npc is closer to the LocalPlayer than closest_entity
                    if (IsCloser(npc))
                    {
                        SetClosestEntity(npc);
                        
                    }
                }

                if (closest_entity != null && closest_entity.active && closest_entity is NPC closest_NPC)
                {
                    Main.NewText($"Closest NPC: {closest_NPC.FullName}");
                } else
                {
                    Main.NewText($"No NPCs found.");
                }
            }

        }

        /// <summary>
        /// Sets the <c>closest_entity</c> to <paramref name="e"/>.
        /// </summary>
        public void SetClosestEntity(Entity e)
        {
            closest_entity = e;
            closest_entity_distance = CalculateDistance(e);
        }
        
        /// <summary>
        /// Determines if the entity <paramref name="e"/> is closer to the LocalPlayer than the currently
        /// tracked <c>closest_entity</c>. If no entity is being tracked, or if the entity is inactive, sets
        /// <c>closest_entity</c> to <paramref name="e"/>.
        /// </summary>
        /// <param name="e">The entity to check against the currently tracked <c>closest_entity</c>.</param>
        /// <returns>
        /// True if <paramref name="e"/> is closer than the currently tracked <c>closest_entity</c>
        /// or if there is no <c>closest_entity</c> active. False otherwise.
        /// </returns>
        public bool IsCloser(Entity e)
        {
            // Null check
            if (closest_entity == null || !closest_entity.active)
            {
                return true;
            }

            // Returns whether e is closer than closest_entity
            return CalculateDistance(e) < closest_entity_distance;
        } 

        /// <summary>
        /// Calculates the Euclidean (shortest) distance between Main.LocalPlayer and <paramref name="e"/>.
        /// </summary>
        /// <param name="e">The entity to calculate the distance to the currently tracked <c>Main.LocalPlayer</c>.</param>
        /// <returns>
        /// Distance of <paramref name="e"/> to <c>Main.LocalPlayer</c>.
        /// </returns>
        public float CalculateDistance(Entity e)
        {
            return (float) Math.Sqrt(
                Math.Pow(Main.LocalPlayer.Center.X - e.Center.X, 2) +
                Math.Pow(Main.LocalPlayer.Center.Y - e.Center.Y, 2)
            );
        }





        /*
            Runs before entities update
        */
        // public override void PreUpdateEntities()
        // {

        // }
    }
}