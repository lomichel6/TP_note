using System.IO;

namespace TP_note.Bibliotheque.Serialization
{
    /// <summary>
    /// Interface pour implémenter différents formats de sérialisation.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Sérialise un objet dans le flux spécifié.
        /// </summary>
        /// <typeparam name="T">Type de l'objet à sérialiser</typeparam>
        /// <param name="data">Objet à sérialiser</param>
        /// <param name="stream">Flux de destination</param>
        void Serialize<T>(T data, Stream stream);

        /// <summary>
        /// Désérialise un objet depuis le flux spécifié.
        /// </summary>
        /// <typeparam name="T">Type de l'objet à désérialiser</typeparam>
        /// <param name="stream">Flux source</param>
        /// <returns>Objet désérialisé</returns>
        T Deserialize<T>(Stream stream);
    }
}
