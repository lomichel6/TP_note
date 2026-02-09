using System;

namespace TP_note.Bibliotheque.Data.Models
{
    /// <summary>
    /// Représente un livre dans la bibliothéque avec ses informations bibliographiques.
    /// </summary>
    [Serializable]
    public class Livre
    {
        /// <summary>
        /// Titre du livre
        /// </summary>
        public string Titre { get; set; }

        /// <summary>
        /// Auteur du livre
        /// </summary>
        public string Auteur { get; set; }

        /// <summary>
        /// Date de publication du livre
        /// </summary>
        public DateTime DateDePublication { get; set; }

        /// <summary>
        /// Identifiant ISBN unique du livre
        /// </summary>
        public string ISBN { get; set; }

        /// <summary>
        /// Catégorie associée au livre
        /// </summary>
        public Categorie Categorie { get; set; }

        /// <summary>
        /// Date d'ajout du livre à la bibliothéque
        /// </summary>
        public DateTime DateAjout { get; set; }

        /// <summary>
        /// Constructeur par défaut.
        /// </summary>
        public Livre()
        {
        }

        /// <summary>
        /// Constructeur avec paramètres pour initialiser toutes les propriétés du livre.
        /// </summary>
        public Livre(string titre, string auteur, DateTime dateDePublication, string isbn, Categorie categorie, DateTime dateAjout)
        {
            Titre = titre;
            Auteur = auteur;
            DateDePublication = dateDePublication;
            ISBN = isbn;
            Categorie = categorie;
            DateAjout = dateAjout;
        }
    }
}
