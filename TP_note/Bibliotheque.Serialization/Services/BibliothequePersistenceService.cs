using System;
using System.IO;
using System.Security.Principal;
using TP_note.Bibliotheque.Serialization;
using BibliothequeModel = TP_note.Bibliotheque.Data.Bibliotheque;

namespace TP_note.Bibliotheque.Serialization.Services
{
    public class BibliothequePersistenceService
    {
        private const int MaxPasswordAttempts = 3;

        /// <summary>
        /// Retourne le répertoire par défaut pour stocker les fichiers de sauvegarde (Mes Documents\\Bibliotheque).
        /// Crée le répertoire s'il n'existe pas.
        /// </summary>
        public string GetDefaultDirectory()
        {
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var dir = Path.Combine(documents, "Bibliotheque");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return dir;
        }

        /// <summary>
        /// Génère le chemin du fichier par défaut en fonction du format de sérialisation et du SID utilisateur.
        /// Format : Bibliotheque_{SID}.{extension}.enc
        /// </summary>
        public string GetDefaultFilePath(SerializerType serializerType)
        {
            var sid = WindowsIdentity.GetCurrent().User?.Value ?? "DefaultUser";
            var extension = serializerType == SerializerType.Binary ? "bin" : "xml";
            var fileName = $"Bibliotheque_{sid}.{extension}.enc";
            return Path.Combine(GetDefaultDirectory(), fileName);
        }

        /// <summary>
        /// Sauvegarde la bibliothèque de manière chiffrée dans un fichier avec le format spécifié.
        /// </summary>
        /// <param name="bibliotheque">La bibliothèque à sauvegarder</param>
        /// <param name="serializerType">Format de sérialisation (XML ou Binary)</param>
        /// <param name="filePath">Chemin optionnel du fichier (utilise le chemin par défaut si null)</param>
        /// <param name="password">Mot de passe optionnel pour le chiffrement</param>
        public void Sauvegarder(BibliothequeModel bibliotheque, SerializerType serializerType, string filePath = null, string password = null)
        {
            if (bibliotheque == null) throw new ArgumentNullException(nameof(bibliotheque));

            filePath ??= GetDefaultFilePath(serializerType);
            var serializer = SerializerFactory.Create(serializerType);

            try
            {
                CryptoHelper.EncryptToFile(
                    filePath,
                    stream => serializer.Serialize(bibliotheque, stream),
                    password);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Impossible d'enregistrer la bibliotheque.", ex);
            }
        }

        /// <summary>
        /// Charge la bibliothèque depuis un fichier chiffré et déchiffré avec le mot de passe.
        /// Limite les tentatives de déchiffrement à 3 et supprime le fichier si le mot de passe est erroné 3 fois.
        /// </summary>
        /// <param name="serializerType">Format de sérialisation (XML ou Binary)</param>
        /// <param name="filePath">Chemin optionnel du fichier (utilise le chemin par défaut si null)</param>
        /// <param name="demanderMotDePasse">Fonction callback pour demander le mot de passe à l'utilisateur</param>
        /// <returns>La bibliothèque chargée</returns>
        public BibliothequeModel Charger(SerializerType serializerType, string filePath = null, Func<int, string> demanderMotDePasse = null)
        {
            filePath ??= GetDefaultFilePath(serializerType);

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Fichier de bibliotheque introuvable.", filePath);

            var serializer = SerializerFactory.Create(serializerType);
            int attempts = 0;

            while (true)
            {
                var password = demanderMotDePasse?.Invoke(attempts + 1);

                try
                {
                    BibliothequeModel result = null;
                    CryptoHelper.DecryptFromFile(
                        filePath,
                        stream => { result = serializer.Deserialize<BibliothequeModel>(stream); },
                        password);

                    return result;
                }
                catch (Exception ex)
                {
                    attempts++;

                    if (demanderMotDePasse == null)
                        throw new InvalidOperationException("Erreur de chargement de la bibliotheque.", ex);

                    if (attempts >= MaxPasswordAttempts)
                    {
                        TryDeleteFile(filePath);
                        throw new InvalidOperationException("Mot de passe errone troisieme fois, le fichier a ete supprime.", ex);
                    }
                }
            }
        }

        // Essaie de supprimer le fichier. Ignore les erreurs de suppression.
        private static void TryDeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch
            {
                // ignore deletion errors
            }
        }
    }
}
