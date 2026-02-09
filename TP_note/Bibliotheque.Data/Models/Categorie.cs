using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace TP_note.Bibliotheque.Data.Models
{
    /// <summary>
    /// Représente une catégorie de livres dans la bibliothéque.
    /// </summary>
    [Serializable]
    public class Categorie
    {
        /// <summary>
        /// Nom de la catégorie
        /// </summary>
        public string Nom { get; set; }

        /// <summary>
        /// Liste des livres appartenant à cette catégorie.
        /// Cette propriété est ignorée lors de la sérialisation XML pour éviter les références circulaires.
        /// </summary>
        [XmlIgnore]
        public List<Livre> Livres { get; set; }

        /// <summary>
        /// Constructeur par défaut initialisant une liste de livres vide.
        /// </summary>
        public Categorie()
        {
            Livres = new List<Livre>();
        }
    }
}
