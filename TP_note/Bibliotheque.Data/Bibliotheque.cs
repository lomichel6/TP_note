using System;
using System.Collections.Generic;
using TP_note.Bibliotheque.Data.Models;

namespace TP_note.Bibliotheque.Data
{
    /// <summary>
    /// Conteneur principal pour toutes les données de la bibliothéque.
    /// Contient les collections de livres, catégories et utilisateurs.
    /// </summary>
    [Serializable]
    public class Bibliotheque
    {
        /// <summary>
        /// Collection de tous les livres dans la bibliothéque
        /// </summary>
        public List<Livre> Livres { get; set; } = new List<Livre>();

        /// <summary>
        /// Collection de toutes les catégories de livres
        /// </summary>
        public List<Categorie> Categories { get; set; } = new List<Categorie>();

        /// <summary>
        /// Collection de tous les utilisateurs enregistrés
        /// </summary>
        public List<Utilisateur> Utilisateurs { get; set; } = new List<Utilisateur>();
    }
}
