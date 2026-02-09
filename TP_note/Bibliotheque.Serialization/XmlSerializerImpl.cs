using System;
using System.IO;
using System.Xml.Serialization;

namespace TP_note.Bibliotheque.Serialization
{
    /// <summary>
    /// Implémentation de la sérialisation XML utilisant XmlSerializer.
    /// </summary>
    public class XmlSerializerImpl : ISerializer
    {
        /// <summary>
        /// Sérialise l'objet en format XML et l'écrit dans le flux fourni.
        /// </summary>
        public void Serialize<T>(T data, Stream stream)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            var serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(stream, data);
        }

        /// <summary>
        /// Désérialise un objet depuis un flux contenant des données XML.
        /// </summary>
        public T Deserialize<T>(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            var serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(stream);
        }
    }
}
