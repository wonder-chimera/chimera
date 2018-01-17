using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kemo.GameMaker.Object
{
    public abstract class GameObject : GameComponent
    {
        public string Name { get; private set; }
        public List<Tag> TagList { get; set; }
        public List<Tag> ConstTagList { get; private set; }
        public Tag[] AllTag { get { return ConstTagList.Concat(TagList).ToArray(); } }
        public GameObject RootObject {

            get { return rootObject; }
            set
            {
                if(rootObject != value)
                {
                    if (rootObject != null)
                    {
                        rootObject.SubObjects.Remove(this);
                    }

                    if (value != null)
                    {
                        value.SubObjects.Add(this);
                    }

                    rootObject = value;
                    OnRootObjectChanged(this);
                }
            }
        }
        public List<GameObject> SubObjects { get; private set; }
        public bool IsDispose { get; protected set; }

        public event EventHandler<EventArgs> RootObjectChanged;

        private GameObject rootObject;

        public GameObject(Game game, string name, List<Tag> constTag = null, GameObject rootObject = null) : base(game)
        {
            Name = name;
            ConstTagList = constTag ?? new List<Tag>();
            TagList = new List<Tag>();
            RootObject = rootObject;
            SubObjects = new List<GameObject>();
            IsDispose = false;

        }

        public void Add(GameObject newObject)
        {
            newObject.RootObject = this;
        }

        public void AddRange(IEnumerable<GameObject> newObjects)
        {
            foreach (var newObject in newObjects)
            {
                newObject.RootObject = this;
            }
        }

        public List<GameObject> GetAllSubObject()
        {
            List<GameObject> ret = new List<GameObject>();

            foreach (GameObject sub in SubObjects)
            {
                ret.Add(sub);
                ret.AddRange(sub.GetAllSubObject());
            }

            return ret;
        }

        protected virtual void OnRootObjectChanged(object sender)
        {
            RootObjectChanged?.Invoke(sender, EventArgs.Empty);
        }

        public virtual new void Dispose()
        {
            Name = null;
            ConstTagList = null;
            TagList = null;
            rootObject = null;
            SubObjects = null;
            IsDispose = true;

            base.Dispose();
        }
    }
}