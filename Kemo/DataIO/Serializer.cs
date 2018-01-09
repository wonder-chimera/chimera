using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Security.Cryptography;

namespace Kemo.DataIO
{
    /*
     
         ToDo
          
          Binary方式を実装
         
         
         */

    /// <summary>
    /// <see cref="Serializer{Type}"/>が使用するシリアライズ法を表します。
    /// </summary>
    public enum SerializeType
    {
        XML,
        Binary
    }

    /// <summary>
    /// 指定の方法で色々な型のobjectと<see cref="Stream"/>の内容を相互変換するクラスです。
    /// </summary>
    /// <typeparam name="Type"></typeparam>
    public class Serializer
    {
        /// <summary>
        /// <paramref name="serializeType"/>で指定した方法で色々な型のobjectと<see cref="Stream"/>の内容を相互変換します。
        /// </summary>
        /// <param name="serializeType">使用するシリアライズ方法</param>
        public Serializer(SerializeType serializeType)
        {
            this.SerializeType = serializeType;
        }

        /// <summary>
        /// 使用するシリアライズ方法
        /// </summary>
        public SerializeType SerializeType { get; }

        /// <summary>
        /// <paramref name="stream"/>を<see cref="SerializeType"/>方式で<typeparamref name="Type"/>型に変換して<paramref name="returnObj"/>に代入します。
        /// 成功した場合trueを、そうでない場合falseを返します。
        /// </summary>
        /// <typeparam name="Type">取得するオブジェクトの型</typeparam>
        /// <param name="stream">入力ストリーム</param>
        /// <param name="returnObj">取得したオブジェクトを代入する変数</param>
        /// <returns></returns>
        public bool TryDeserialize<Type>(Stream stream, out Type returnObj)
        {
            try
            {
                returnObj = Deserialize<Type>(stream);
            }
            catch (System.InvalidOperationException ex )
            {
                if (ex.InnerException is XmlException)
                {
                    returnObj = default(Type);
                    return false;
                }

                throw;
            }

            return true;
        }

        /// <summary>
        /// <paramref name="stream"/>を<see cref="SerializeType"/>方式で<typeparamref name="Type"/>型に変換して返します
        /// 読み込めなかった場合、エラーが発生します。
        /// </summary>
        /// <typeparam name="Type">取得するオブジェクトの型</typeparam>
        /// <param name="stream">入力ストリーム</param>
        /// <returns></returns>
        public Type Deserialize<Type>(Stream stream)
        {
            Type ret = default(Type);

            switch (SerializeType)
            {
                case SerializeType.XML:
                    // XMLとしてストリームから読み込み。Typeオブジェクトへ変換
                    XmlSerializer serializer = new XmlSerializer(typeof(Type));
                    ret = (Type)serializer.Deserialize(stream);
                    break;

                case SerializeType.Binary:
                    throw new System.NotImplementedException();

                default:
                    throw new System.NotImplementedException();
            }

            return ret;
        }

        /// <summary>
        /// <paramref name="typeObj"/>を<see cref="SerializeType"/>方式で変換し、<paramref name="stream"/>に書き込みます。
        /// </summary>
        /// <typeparam name="Type">変換元のオブジェクトの型</typeparam>
        /// <param name="stream">出力ストリーム</param>
        /// <param name="typeObj">変換するデータ</param>
        public void Serialize<Type>(Stream stream, Type typeObj)
        {
            try
            {
                switch (SerializeType)
                {
                    case SerializeType.XML:

                        // saveObjectをXMLにして、ストリームに書き込み
                        XmlSerializer serializer = new XmlSerializer(typeof(Type));
                        serializer.Serialize(stream, typeObj);
                        break;

                    case SerializeType.Binary:

                        throw new System.NotImplementedException();

                    default:

                        throw new System.NotImplementedException();
                }
            }
            catch (System.Exception)
            {

                throw;
            }
        }

    }
}
