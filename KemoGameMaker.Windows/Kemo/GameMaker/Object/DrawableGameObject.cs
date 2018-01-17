using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Kemo.GameMaker.Object
{
    public abstract class DrawableGameObject : GameObject, IDrawable
    {
        public int DrawOrder
        {
            get { return drawOrder; }
            set
            {
                if (drawOrder != value)
                {
                    drawOrder = value;
                    OnDrawOrderChanged(this);
                }
            }
        }

        bool IDrawable.Visible { get { return (Visible || DebugVisible); } }
        public bool Visible {
            get { return visible; }
            set
            {
                if(visible != value)
                {
                    visible = value;
                    OnVisibleChanged(this);
                }
            }
        }
        public bool DebugVisible {
            get { return debugVisible; }
            set
            {
                if (debugVisible != value)
                {
                    debugVisible = value;
                    OnDebugVisibleChanged(this);
                }
            }
        }
        public Vector3 LocalPos3D
        {
            get
            {
                return localPosition;
            }
            set
            {
                if (IsDispose) return;

                localPosition = value;

                if (RootObject != null && RootObject is DrawableGameObject root)
                {
                    worldPosition = root.worldPosition + value;
                }
                else
                {
                    worldPosition = value;
                }
            }
        }
        public Vector3 WorldPos3D
        {
            get
            {
                if (RootObject != null && RootObject is DrawableGameObject root)
                {
                    return root.WorldPos3D + LocalPos3D;
                }
                else
                {
                    return LocalPos3D;
                }
            }
            set
            {
                if (IsDispose) return;

                worldPosition = value;

                if (RootObject != null && RootObject is DrawableGameObject root)
                {
                    localPosition = value - root.worldPosition;
                }
                else
                {
                    localPosition = value;
                }
            }
        }
        public Vector2 LocalPos
        {
            get
            {
                return new Vector2(LocalPos3D.X, LocalPos3D.Y);
            }
            set
            {
                LocalPos3D = new Vector3(value, LocalPos3D.Z);
            }
        }
        public Vector2 WorldPos
        {
            get
            {
                return new Vector2(WorldPos3D.X, WorldPos3D.Y);
            }
            set
            {
                WorldPos3D = new Vector3(value, WorldPos3D.Z);
            }
        }

        public event EventHandler<EventArgs> DrawOrderChanged;
        public event EventHandler<EventArgs> VisibleChanged;
        public event EventHandler<EventArgs> DebugVisibleChanged;

        private Vector3 localPosition;
        private Vector3 worldPosition;
        private int drawOrder;
        private bool visible = true;
        private bool debugVisible = false;


        public DrawableGameObject(Game game, string name, List<Tag> constTag = null, GameObject rootObject = null) : base(game, name, constTag, rootObject)
        {
        }

        /// <summary>
        /// ワールド座標を変えずに、オブジェクトを接続します。
        /// </summary>
        /// <param name="newRootObject"></param>
        public void JoinTo(GameObject newRootObject)
        {
            Vector3 pos = WorldPos3D;
            RootObject = newRootObject;
            WorldPos3D = pos;
        }

        void IDrawable.Draw(GameTime gameTime)
        {
            if (Visible)
            {
                Draw(gameTime);
            }

            if (DebugVisible)
            {
                DebugDraw(gameTime);
            }
        }

        public virtual void Draw(GameTime gameTime)
        {
        }

        public virtual void DebugDraw(GameTime gameTime)
        {
        }

        protected virtual void OnDrawOrderChanged(object sender)
        {
            DrawOrderChanged?.Invoke(sender, EventArgs.Empty);
        }

        protected virtual void OnVisibleChanged(object sender)
        {
            VisibleChanged?.Invoke(sender, EventArgs.Empty);
        }

        protected virtual void OnDebugVisibleChanged(object sender)
        {
            DebugVisibleChanged?.Invoke(sender, EventArgs.Empty);
        }

        public override void Dispose()
        {
            DrawOrderChanged = null;
            VisibleChanged = null;
            DebugVisibleChanged = null;

            localPosition = Vector3.Zero;
            worldPosition = Vector3.Zero;
            visible = false;
            debugVisible = false;

            base.Dispose();
        }
    }
}
