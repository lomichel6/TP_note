using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TP_note.Bibliotheque.Serialization
{
    /// <summary>
    /// Implémentation de la sérialisation binaire utilisant BinaryFormatter.
    /// </summary>
    public class BinarySerializerImpl : ISerializer
    {
        /// <summary>
        /// Sérialise l'objet en format binaire et l'écrit dans le flux fourni.
        /// </summary>
        public void Serialize<T>(T data, Stream stream)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, data);
        }

        /// <summary>
        /// Désérialise un objet depuis un flux contenant des données binaires.
        /// </summary>
        public T Deserialize<T>(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            var formatter = new BinaryFormatter();
            var result = formatter.Deserialize(stream);
            return (T)result;
        }
    }
}
