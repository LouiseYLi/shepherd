using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Shepherd.Common.Enums;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

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
        /// <summary>
        /// Entity categories.
        /// </summary>
        public EntityType Type { get; set; }

        /// <summary>
        /// Maximum number of tracked entities.
        /// </summary>
        private const int MaxTrackedEntities = 10;
        
        /// <summary>
        /// ScanEntity() interval.
        /// </summary>
        private const int ScanEntityInterval = 180;
        
        /// <summary>
        /// Stores target entities to scan for as entity IDS.
        /// </summary>
        private Dictionary<EntityType, HashSet<int>> PinnedEntities;

        /// <summary>
        /// Timer for debugging.
        /// </summary>
        private int debug_timer = 0;
        
        /// <summary>
        /// Closest entity to the Main.LocalPlayer.
        /// </summary>
        private Entity closest_entity;

        /// <summary>
        /// Retrieves a boolean on whether there is a currently tracked active entity.
        /// Returns true if <c>closest_entity</c> is not null and is active. False otherwise.
        /// </summary>
        private bool HasActiveClosestEntity => closest_entity?.active == true;

        /// <summary>
        /// EntityTracker constructor, initializes <c>closest_entity</c> as null and an empty <c>PinnedEntities</c> collection.
        /// </summary>
        public EntityTracker()
        {
            closest_entity = null;
            PinnedEntities = new Dictionary<EntityType, HashSet<int>>()
            {
                { EntityType.NPC, new HashSet<int>() }
            };
            // TODO: remove this after debugging
            // should track only zombies
            PinnedEntities[EntityType.NPC].Add(3);
        }

        public override void OnWorldLoad()
        {
            // Clear the tracker when entering a world
            // ClearTracker();
        }

        public override void OnWorldUnload()
        {
            // Clear the tracker when leaving a world to prevent memory leaks
            ClearTracker();
        }

        private void ClearTracker()
        {
            closest_entity = null;
            debug_timer = 0;
            PinnedEntities[EntityType.NPC].Clear();
        }

        /// <summary>
        /// Runs after game updates for each tick.
        /// </summary>
        public override void PostUpdateEverything()
        {
            debug_timer++;
            if (debug_timer > ScanEntityInterval)
            {
                ScanEntity();
                debug_timer = 0;
            }

        }

        /// <summary>
        /// Adds entity <c>e</c> to <c>PinnedEntities</c> dictionary, enabling scans to include entity <c>e</c>.
        /// </summary>
        /// <param name="entity_type">The entity category to attach <c>e</c> in <c>PinnedEntities</c>.</param>
        /// <param name="entity_id">The entity identifier to add to <c>PinnedEntities</c>.</param>
        public void PinEntity(EntityType entity_type, int entity_id)
        {
            PinnedEntities[entity_type].Add(entity_id);
        }

        /// <summary>
        /// Removes entity from <c>PinnedEntities</c> dictionary, causing scans to not include entity <c>e</c>
        /// </summary>
        /// <param name="entity_type">The entity category to remove <c>e</c> from in <c>PinnedEntities</c>.</param>
        /// <param name="entity_id">The entity identifier to remove from <c>PinnedEntities</c>.</param>
        public void UnpinEntity(EntityType entity_type, int entity_id)
        {
            PinnedEntities[entity_type].Remove(entity_id);
        }

        /// <summary>
        /// Checks if <c>PinnedEntities</c> contains <c>entity_id</c>.
        /// </summary>
        /// <param name="entity_type">The entity category to check the <c>entity_id</c> in <c>PinnedEntities</c>.</param>
        /// <param name="entity_id">The entity identifier to check whether or not it is pinned for scanning in <c>PinnedEntities</c>.</param>
        private bool IsPinned(EntityType entity_type, int entity_id) => PinnedEntities[entity_type].Contains(entity_id);

        /// <summary>
        /// Scans active NPCs to find the closest NPC to Main.LocalPlayer.
        /// </summary>
        private void ScanEntity()
        {
            foreach (NPC npc in Main.ActiveNPCs) {
                // npc is closer to the LocalPlayer than closest_entity
                if (IsPinned(EntityType.NPC, npc.type) && IsCloser(npc))
                {
                    SetClosestEntity(npc);
                    Main.NewText($"Reassigned closest_entity to NPC of type: {npc.type}");
                }
            }

            if (HasActiveClosestEntity && closest_entity is NPC closest_NPC)
            {
                Main.NewText($"Closest NPC: {closest_NPC.FullName}");
            } else
            {
                Main.NewText($"No target NPCs found.");
            }
        }

        /// <summary>
        /// Returns whether there is an active entity.
        /// </summary>
        /// <returns>
        /// True if there is an active entity.
        /// False otherwise.
        /// </returns>
        public bool GetHasActiveClosestEntity()
        {
            return HasActiveClosestEntity;
        }


        /// <summary>
        /// Gets the entity closest to the player.
        /// </summary>
        /// <returns>
        /// <c>closest_entity</c>.
        /// </returns>
        public Entity GetClosestEntity()
        {
            return closest_entity;
        }

        /// <summary>
        /// Sets the <c>closest_entity</c> to <paramref name="e"/>.
        /// </summary>
        private void SetClosestEntity(Entity e)
        {
            closest_entity = e;
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
        private bool IsCloser(Entity e)
        {
            // Null check
            if (!HasActiveClosestEntity)
            {
                return true;
            }

            // Returns whether e is closer than closest_entity
            return CalculateDistance(e) < CalculateDistance(closest_entity);
        } 

        /// <summary>
        /// Calculates the Euclidean (shortest) distance between <c>Main.LocalPlayer</c> and <paramref name="e"/>.
        /// </summary>
        /// <param name="e">The entity to calculate the distance to the currently tracked <c>Main.LocalPlayer</c>.</param>
        /// <returns>
        /// Distance of <paramref name="e"/> to <c>Main.LocalPlayer</c>.
        /// </returns>
        private float CalculateDistance(Entity e)
        {
            return (float) Math.Sqrt(
                Math.Pow(Main.LocalPlayer.Center.X - e.Center.X, 2) +
                Math.Pow(Main.LocalPlayer.Center.Y - e.Center.Y, 2)
            );
        }



        /// <summary>
        /// Draws navigational arrows when ready.
        /// </summary>
        public override void PostDrawTiles()
        {
            if (!HasActiveClosestEntity)
                return;

            DrawArrowToEntity();
        }

        /// <summary>
        /// Draws navigational arrow to entity.
        /// </summary>
        private void DrawArrowToEntity()
        {
            var arrowTexture = ModContent.Request<Texture2D>("Shepherd/Assets/arrow");

            // if (arrowTexture == null || !arrowTexture.IsLoaded)
            // {
            //     Main.NewText("arrow text not loaded");
            //     return;
            // }
            // Main.NewText("drawing arrow");

            SpriteBatch spriteBatch = Main.spriteBatch;

            spriteBatch.Begin();

            Vector2 playerPos = Main.LocalPlayer.Center;
            Vector2 closestEntityPos = closest_entity.Center;
            Vector2 direction = closestEntityPos - playerPos;

            float rotation = direction.ToRotation() + MathHelper.PiOver2;
            // float rotation = direction.ToRotation();

            Vector2 arrowPosition = playerPos + Vector2.Normalize(direction) * 50f;

            Vector2 screenPos = arrowPosition - Main.screenPosition;

            spriteBatch.Draw(
                arrowTexture.Value,
                screenPos,
                null,
                Color.White,
                rotation,
                arrowTexture.Value.Size() / 2f,  // Origin (center of sprite)
                1f,  // Scale
                SpriteEffects.None,
                0f
            );

            spriteBatch.End();
        }

        /*
            Runs before entities update
        */
        // public override void PreUpdateEntities()
        // {

        // }
    }
}