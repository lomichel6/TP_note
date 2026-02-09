using System;
using System.Collections.Generic;

namespace TP_note.Bibliotheque.Data.Models
{
    /// <summary>
    /// Représente un utilisateur enregistré dans la bibliothéque.
    /// </summary>
    [Serializable]
    public class Utilisateur
    {
        /// <summary>
        /// Nom de famille de l'utilisateur
        /// </summary>
        public string Nom { get; set; }

        /// <summary>
        /// Prénom de l'utilisateur
        /// </summary>
        public string Prenom { get; set; }

        /// <summary>
        /// Email unique de l'utilisateur
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Date d'inscription de l'utilisateur à la bibliothéque
        /// </summary>
        public DateTime DateInscription { get; set; }

        /// <summary>
        /// Liste des livres actuellement empruntés par l'utilisateur
        /// </summary>
        public List<Livre> LivresEmpruntes { get; set; } = new List<Livre>();

        /// <summary>
        /// Constructeur par défaut avec date d'inscription actuelle.
        /// </summary>
        public Utilisateur()
        {
            DateInscription = DateTime.Now;
        }

        /// <summary>
        /// Constructeur complet avec tous les paramètres.
        /// </summary>
        public Utilisateur(string nom, string prenom, string email, DateTime dateInscription, List<Livre> livresEmpruntes)
        {
            Nom = nom;
            Prenom = prenom;
            Email = email;
            DateInscription = dateInscription;
            LivresEmpruntes = livresEmpruntes ?? new List<Livre>();
        }

        /// <summary>
        /// Constructeur simplifié avec initialisation de la date à aujourd'hui.
        /// </summary>
        public Utilisateur(string nom, string prenom, string email)
            : this(nom, prenom, email, DateTime.Today, new List<Livre>())
        {
        }
    }
}