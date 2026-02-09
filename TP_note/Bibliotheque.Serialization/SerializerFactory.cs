namespace TP_note.Bibliotheque.Serialization
{
    /// <summary>
    /// Factory statique pour créer des instances de sérialiseurs en fonction du type requis.
    /// Implémente le pattern Factory pour une extensibilité facile.
    /// </summary>
    public static class SerializerFactory
    {
        /// <summary>
        /// Crée une instance de sérialiseur en fonction du type spécifié.
        /// </summary>
        /// <param name="type">Type de format de sérialisation (XML ou Binary)</param>
        /// <returns>Instance de sérialiseur correspondant au type</returns>
        public static ISerializer Create(SerializerType type)
        {
            return type switch
            {
                SerializerType.Binary => new BinarySerializerImpl(),
                SerializerType.Xml => new XmlSerializerImpl(),
                _ => new XmlSerializerImpl()
            };
        }
    }
}
