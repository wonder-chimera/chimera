using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace Kemo.DataIO
{
    /*
     
        ToDo
        

    */

    /// <summary>
    /// ファイルの読み込み結果
    /// </summary>
    public enum LoadState
    {
        /// <summary>
        /// ファイルが存在しなかった
        /// </summary>
        FileNotFound,
        /// <summary>
        /// ファイルをデータに変換できなかった
        /// </summary>
        FileNotConverted,
        /// <summary>
        /// データのに異常があった
        /// </summary>
        ConsistencyError,
        /// <summary>
        /// ロードに成功した
        /// </summary>
        Complete,
        /// <summary>
        /// エラー確認をしなかった、又は予期していない状態
        /// </summary>
        None
    }

    /// <summary>
    /// オブジェクトの整合性を確認するメソッドを定義します。
    /// </summary>
    interface IConsistencyCheck
    {
        bool ConsistencyCheck();
    }

    /// <summary>
    /// 
    /// <para>
    /// データの読み書き・および暗号化を簡単に行えるクラスです。
    /// データは一つのファイルとして保存されます。
    /// </para>
    /// 
    /// <para>
    /// 複数のデータを扱いたい場合は、複数の<see cref="DataFileManager{Type}"/>を使用するか、
    /// DataFileListManagerを使ってくださ（未実装）
    /// </para>
    /// 
    /// </summary>
    /// <typeparam name="Type">保存・読み込みするデータの型</typeparam>
    public class DataFileManager<Type>
    {
        /// <summary>
        /// ファイルが暗号化されているかのチェック文字列
        /// </summary>
        readonly byte[] cryptoTag = Encoding.UTF8.GetBytes("[ISCRYPTO]");

        /// <summary>
        /// 暗号化の鍵
        /// </summary>
        byte[] key = new byte[0];

        /// <summary>
        /// ファイルを保存するパスを表します。
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// このマネージャーがファイルを読み書きするとき、暗号化・復号をするかを表します。
        /// </summary>
        public bool IsCrypto { get; }

        /// <summary>
        /// このマネージャーがオブジェクトをファイル化する時のシリアライズ法を表します。
        /// </summary>
        public SerializeType SerializeType { get { return serializer.SerializeType; } }

        private Serializer serializer { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath">ファイルを保存するパス</param>
        /// <param name="serializeType">ファイル化に使用するシリアライズ法</param>
        public DataFileManager(string filePath, SerializeType serializeType = SerializeType.XML)
        {
            serializer = new Serializer(SerializeType.XML);

            FilePath = filePath;
            IsCrypto = false;

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath">ファイルを保存するパス</param>
        /// <param name="key">暗号化・復号に使用するパスワード</param>
        /// <param name="isCrypto">暗号化・復号のON・OFF（trueでON）</param>
        /// <param name="serializeType">ファイル化に使用するシリアライズ法</param>
        public DataFileManager(string filePath, string key, bool isCrypto = true, SerializeType serializeType = SerializeType.XML)
            : this(filePath, serializeType)
        {
            IsCrypto = isCrypto;

            List<byte> keyB = Encoding.UTF8.GetBytes(key).ToList();
            while (keyB.Count < 24)
            {
                keyB.AddRange(keyB);
            }

            this.key = keyB.Take(24).ToArray();
        }

        /// <summary>
        /// <paramref name="saveObject"/>を<see cref="FilePath"/>にセーブします。
        /// </summary>
        /// <param name="saveObject"></param>
        public void Save(Type saveObject)
        {
            Save(saveObject, IsCrypto);
        }
        
        /// <summary>
        /// エラーチェック無しのロード。ロードしたデータを返します。
        /// 暗号化されていた場合は、自動的に複合します。
        /// 読み込めなかった場合、エラーが発生します。
        /// </summary>
        /// <returns></returns>
        public Type Load()
        {
            Type ret;
            Load(FileIsCrypto(), out ret, false);

            return ret;
        }

        /// <summary>
        /// エラーをチェックし、<paramref name="loadData"/>にファイルをロードします。エラーチェックの結果を<see cref="LoadState"/>で返します。
        /// 暗号化されていた場合は、自動的に複合します。
        /// </summary>
        /// <param name="loadData">データを格納する変数</param>
        /// <returns></returns>
        public LoadState TryLoad(out Type loadData)
        {
            return Load(FileIsCrypto(), out loadData, true);
        }

        protected bool FileIsCrypto()
        {
            if (!File.Exists(FilePath))
            {
                return false;
            }

            FileStream fileStream = File.Open(
                FilePath,
                FileMode.Open
            );

            byte[] streamTag = new byte[cryptoTag.Length];

            fileStream.Read(streamTag, 0, cryptoTag.Length);

            fileStream.Close();

            return cryptoTag.SequenceEqual(streamTag);
        }

        protected void Save(Type saveObject, bool crypto)
        {
            // 書き込むファイルを確保
            FileStream fileStream = File.Open(
                FilePath,
                FileMode.Create
            );

            if (crypto)
            {
                byte[] iv = SymmetricAlgorithm.Create().IV;

                // ファイルへの暗号化込みのストリーム
                CryptoStream cryptoStream = new CryptoStream(
                    fileStream,
                    new TripleDESCryptoServiceProvider().CreateEncryptor(key, iv),
                    CryptoStreamMode.Write
                );

                //暗号化タグとIVを挿入
                fileStream.Write(cryptoTag, 0, cryptoTag.Length);
                fileStream.Write(iv, 0, 8);

                // saveObjectをシリアルにして、暗号化ストリームに書き込み
                serializer.Serialize(cryptoStream, saveObject);
                // 暗号化ストリームくろーず
                cryptoStream.Close();
            }
            else
            {
                // saveObjectをシリアルにして、ストリームに書き込み
                serializer.Serialize(fileStream, saveObject);
            }
            // くろーず
            fileStream.Close();
        }

        protected LoadState Load(bool crypto, out Type loadData, bool tryLoad)
        {
            if (tryLoad && !File.Exists(FilePath))
            {
                loadData = default(Type);
                return LoadState.FileNotFound;
            }

            LoadState returnState = LoadState.None;

            // 読み込むファイルを確保
            using (FileStream fileStream = File.Open(FilePath, FileMode.Open))
            {
                if (crypto)
                {
                    fileStream.Seek(cryptoTag.Length, SeekOrigin.Begin);
                    CryptoStream cryptoStream;
                    byte[] iv = new byte[8];

                    fileStream.Read(iv, 0, 8);

                    // 復号込みのストリーム
                    cryptoStream = new CryptoStream(
                        fileStream,
                        new TripleDESCryptoServiceProvider().CreateDecryptor(key, iv),
                        CryptoStreamMode.Read
                    );

                    if (tryLoad)
                    {
                        if (serializer.TryDeserialize(cryptoStream, out loadData))
                        {
                            returnState = LoadState.Complete;
                        }
                        else
                        {
                            returnState = LoadState.FileNotConverted;
                        }
                    }
                    else
                    {
                        loadData = serializer.Deserialize<Type>(cryptoStream);
                    }
                    // くろーず
                    cryptoStream.Close();
                }
                else
                {
                    // ストリームから読み込み。Typeオブジェクトへ変換

                    if (tryLoad)
                    {
                        if (serializer.TryDeserialize(fileStream, out loadData))
                        {
                            returnState = LoadState.Complete;
                        }
                        else
                        {
                            returnState = LoadState.FileNotConverted;
                        }
                    }
                    else
                    {
                        loadData = serializer.Deserialize<Type>(fileStream);
                    }
                }
            }

            if (returnState == LoadState.Complete && loadData is IConsistencyCheck cObj && !cObj.ConsistencyCheck())
            {
                return LoadState.ConsistencyError;
            }

            return returnState;
        }

    }



}
