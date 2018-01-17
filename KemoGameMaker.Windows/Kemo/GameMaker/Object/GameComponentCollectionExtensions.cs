using Microsoft.Xna.Framework;
using System.Linq;

namespace Kemo.GameMaker.Object
{
    public static class GameComponentCollectionExtensions
    {
        public static void AddGameObject( this GameComponentCollection componentCollection, GameObject gameObject)
        {
            componentCollection.Add(gameObject);

            foreach (var obj in gameObject.GetAllSubObject())
            {
                componentCollection.Add(obj);
            }
        }

        public static void RemoveGameObject( this GameComponentCollection componentCollection, ref GameObject gameObject, bool removeSubObject = false, bool deth = true)
        {
            if (removeSubObject)
            {
                foreach (var obj in gameObject.GetAllSubObject())
                {
                    componentCollection.Remove(obj) ;
                }
            }
            else
            {
                while (gameObject.SubObjects.Count > 0)
                {
                    gameObject.SubObjects[0].RootObject = null;
                }
            }

            componentCollection.Remove(gameObject);

            if (deth)
            {
                gameObject.Dispose();
            }
        }

        public static void RemoveGameObjectAtTag(this GameComponentCollection componentCollection, Tag tag, bool removeSubObject = false, bool deth = true)
        {
            while (componentCollection.Any( obj => obj is GameObject go && go.AllTag.Any( t => t.TagName == tag.TagName ) ))
            {
                GameObject removeGameObj = (GameObject)(componentCollection.First(obj => obj is GameObject go && go.AllTag.Any(t => t.TagName == tag.TagName)));

                componentCollection.RemoveGameObject(ref removeGameObj,removeSubObject,deth);
            }
        }

        public static void RemoveGameObjectAtName(this GameComponentCollection componentCollection, string name, bool removeSubObject = false, bool deth = true)
        {
            while (componentCollection.Any(obj => obj is GameObject go && go.Name == name))
            {
                GameObject removeGameObj = (GameObject)(componentCollection.First(obj => obj is GameObject go && go.Name == name));

                componentCollection.RemoveGameObject(ref removeGameObj, removeSubObject,deth);
            }
        }
    }
}