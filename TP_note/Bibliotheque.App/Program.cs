using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TP_note.Bibliotheque.Data;
using TP_note.Bibliotheque.Data.Models;
using TP_note.Bibliotheque.Data.Services;
using TP_note.Bibliotheque.Serialization;
using TP_note.Bibliotheque.Serialization.Services;

namespace TP_note
{
    internal class Program
    {
        private static readonly BibliothequeService Service = new();
        private static readonly BibliothequePersistenceService Persistence = new();

        /// <summary>
        /// Point d'entrée principal de l'application. Affiche un menu et traite les choix de l'utilisateur.
        /// </summary>
        private static void Main()
        {
            var quitter = false;

            while (!quitter)
            {
                AfficherMenu();
                var choix = Console.ReadLine()?.Trim();
                Console.WriteLine();

                switch (choix)
                {
                    case "1":
                        AjouterLivre();
                        break;
                    case "2":
                        AjouterUtilisateur();
                        break;
                    case "3":
                        AjouterCategorie();
                        break;
                    case "4":
                        ListerBibliotheque();
                        break;
                    case "5":
                        EmprunterLivre();
                        break;
                    case "6":
                        RendreLivre();
                        break;
                    case "7":
                        SauvegarderBibliotheque();
                        break;
                    case "8":
                        ChargerBibliotheque();
                        break;
                    case "0":
                        quitter = true;
                        break;
                    default:
                        Console.WriteLine("Choix invalide.");
                        break;
                }

                Console.WriteLine();
            }
        }

        /// <summary>
        /// Affiche le menu principal avec toutes les options disponibles et le chemin par défaut de sauvegarde.
        /// </summary>
        private static void AfficherMenu()
        {
            Console.WriteLine("=== Gestion de la bibliotheque ===");
            Console.WriteLine("1. Ajouter un livre");
            Console.WriteLine("2. Ajouter un utilisateur");
            Console.WriteLine("3. Ajouter une categorie");
            Console.WriteLine("4. Lister les elements");
            Console.WriteLine("5. Emprunter un livre");
            Console.WriteLine("6. Rendre un livre");
            Console.WriteLine("7. Sauvegarder la bibliotheque");
            Console.WriteLine("8. Charger la bibliotheque");
            Console.WriteLine("0. Quitter");
            Console.WriteLine($"Chemin par defaut : {Persistence.GetDefaultDirectory()}");
            Console.Write("Choix : ");
        }

        /// <summary>
        /// Demande à l'utilisateur les informations d'un livre et l'ajoute à la bibliothèque.
        /// </summary>
        private static void AjouterLivre()
        {
            try
            {
                var titre = LireTexte("Titre");
                var auteur = LireTexte("Auteur");
                var isbn = LireTexte("ISBN");
                var categorie = LireTexte("Categorie");
                var datePublication = LireDate("Date de publication (JJ/MM/AAAA)", DateTime.Today);

                var livre = Service.AjouterLivre(titre, auteur, datePublication, isbn, categorie);
                Console.WriteLine($"Livre ajoute : {livre.Titre} ({livre.Auteur})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Impossible d'ajouter le livre : {ex.Message}");
            }
        }

        /// <summary>
        /// Demande les informations d'un utilisateur et l'enregistre dans la bibliothèque.
        /// </summary>
        private static void AjouterUtilisateur()
        {
            try
            {
                var nom = LireTexte("Nom");
                var prenom = LireTexte("Prenom");
                var email = LireTexte("Email");

                var utilisateur = Service.AjouterUtilisateur(nom, prenom, email);
                Console.WriteLine($"Utilisateur ajoute : {utilisateur.Prenom} {utilisateur.Nom}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Impossible d'ajouter l'utilisateur : {ex.Message}");
            }
        }

        /// <summary>
        /// Demande le nom d'une catégorie et l'ajoute à la bibliothèque.
        /// </summary>
        private static void AjouterCategorie()
        {
            try
            {
                var nom = LireTexte("Nom de la categorie");
                var categorie = Service.AjouterCategorie(nom);
                Console.WriteLine($"Categorie enregistree : {categorie.Nom}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Impossible d'ajouter la categorie : {ex.Message}");
            }
        }

        /// <summary>
        /// Affiche la liste complète de tous les livres, utilisateurs et catégories avec leurs statuts.
        /// </summary>
        private static void ListerBibliotheque()
        {
            Console.WriteLine("=> Livres");
            if (!Service.Livres.Any())
            {
                Console.WriteLine("  Aucun livre enregistre.");
            }
            else
            {
                foreach (var livre in Service.Livres)
                {
                    var emprunteur = Service.ObtenirEmprunteur(livre);
                    var statut = emprunteur == null ? "disponible" : $"emprunte par {emprunteur.Prenom} {emprunteur.Nom}";
                    Console.WriteLine($"  - {livre.Titre} ({livre.Auteur}) [{livre.Categorie?.Nom}] - {statut}");
                }
            }

            Console.WriteLine("=> Utilisateurs");
            if (!Service.Utilisateurs.Any())
            {
                Console.WriteLine("  Aucun utilisateur.");
            }
            else
            {
                foreach (var utilisateur in Service.Utilisateurs)
                {
                    var emprunts = utilisateur.LivresEmpruntes?.Select(l => l.Titre).ToList() ?? new List<string>();
                    Console.WriteLine($"  - {utilisateur.Prenom} {utilisateur.Nom} ({utilisateur.Email}) - {emprunts.Count} emprunt(s)");
                    if (emprunts.Count > 0)
                        Console.WriteLine($"    Livres empruntes : {string.Join(", ", emprunts)}");
                }
            }

            Console.WriteLine("=> Categories");
            if (!Service.Categories.Any())
            {
                Console.WriteLine("  Aucune categorie.");
            }
            else
            {
                foreach (var categorie in Service.Categories)
                {
                    Console.WriteLine($"  - {categorie.Nom} ({categorie.Livres.Count} livre(s))");
                }
            }
        }

        /// <summary>
        /// Permet à un utilisateur d'emprunter un livre en demandant l'ISBN et l'email.
        /// </summary>
        private static void EmprunterLivre()
        {
            var isbn = LireTexte("ISBN du livre");
            var email = LireTexte("Email de l'utilisateur");

            var livre = Service.ChercherLivreParIsbn(isbn);
            var utilisateur = Service.ChercherUtilisateurParEmail(email);

            if (livre == null)
            {
                Console.WriteLine("Livre introuvable.");
                return;
            }

            if (utilisateur == null)
            {
                Console.WriteLine("Utilisateur introuvable.");
                return;
            }

            try
            {
                Service.EmprunterLivre(livre, utilisateur);
                Console.WriteLine($"{utilisateur.Prenom} {utilisateur.Nom} emprunte {livre.Titre}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Impossible d'emprunter : {ex.Message}");
            }
        }

        /// <summary>
        /// Enregistre la restitution d'un livre emprunté par un utilisateur.
        /// </summary>
        private static void RendreLivre()
        {
            var isbn = LireTexte("ISBN du livre");
            var email = LireTexte("Email de l'utilisateur");

            var livre = Service.ChercherLivreParIsbn(isbn);
            var utilisateur = Service.ChercherUtilisateurParEmail(email);

            if (livre == null)
            {
                Console.WriteLine("Livre introuvable.");
                return;
            }

            if (utilisateur == null)
            {
                Console.WriteLine("Utilisateur introuvable.");
                return;
            }

            try
            {
                Service.RendreLivre(livre, utilisateur);
                Console.WriteLine($"{utilisateur.Prenom} {utilisateur.Nom} a rendu {livre.Titre}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Impossible de rendre : {ex.Message}");
            }
        }

        /// <summary>
        /// Demande le format de sérialisation (XML ou Binaire) et un mot de passe, puis sauvegarde de manière chiffrée.
        /// </summary>
        private static void SauvegarderBibliotheque()
        {
            var serializerType = ChoisirSerializerType();
            var motDePasse = DemanderMotDePasse("Mot de passe (vide = SID Windows)");

            try
            {
                Persistence.Sauvegarder(Service.Bibliotheque, serializerType, password: motDePasse);
                Console.WriteLine("Sauvegarde effectuee.");
                Console.WriteLine($"Fichier : {Persistence.GetDefaultFilePath(serializerType)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur de sauvegarde : {ex.Message}");
            }
        }

        /// <summary>
        /// Demande le format de sérialisation et charge la bibliothèque depuis le fichier chiffré.
        /// </summary>
        private static void ChargerBibliotheque()
        {
            var serializerType = ChoisirSerializerType();
            try
            {
                var bibliotheque = Persistence.Charger(serializerType, demanderMotDePasse: EssayerMotDePasse);
                Service.RemplacerBibliotheque(bibliotheque);
                Console.WriteLine("Chargement reussi.");
                Console.WriteLine($"Fichier utilise : {Persistence.GetDefaultFilePath(serializerType)}");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Aucun fichier enregistre trouve.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur de chargement : {ex.Message}");
            }
        }

        /// <summary>
        /// Demande à l'utilisateur de choisir entre un format XML ou Binaire pour la sérialisation.
        /// </summary>
        private static SerializerType ChoisirSerializerType()
        {
            while (true)
            {
                Console.Write("Format des donnees (1=XML, 2=Binaire) [1] : ");
                var saisie = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(saisie) || saisie == "1")
                    return SerializerType.Xml;
                if (saisie == "2")
                    return SerializerType.Binary;
                Console.WriteLine("Choix invalide.");
            }
        }

        /// <summary>
        /// Demande à l'utilisateur d'entrer un mot de passe avec un message personnalisé.
        /// </summary>
        private static string DemanderMotDePasse(string message)
        {
            Console.Write(message + ": ");
            var saisie = Console.ReadLine();
            return string.IsNullOrWhiteSpace(saisie) ? null : saisie;
        }

        /// <summary>
        /// Demande le mot de passe lors d'une tentative de chargement (affiche le numéro de tentative).
        /// </summary>
        private static string EssayerMotDePasse(int tentative)
        {
            Console.Write($"Mot de passe (essai {tentative}/3, vide = SID) : ");
            var saisie = Console.ReadLine();
            return string.IsNullOrWhiteSpace(saisie) ? null : saisie;
        }

        /// <summary>
        /// Demande à l'utilisateur d'entrer un texte avec le label spécifié. Valide que le champ n'est pas vide.
        /// </summary>
        private static string LireTexte(string label)
        {
            while (true)
            {
                Console.Write($"{label} : ");
                var saisie = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(saisie))
                    return saisie.Trim();
                Console.WriteLine("La valeur est requise.");
            }
        }

        /// <summary>
        /// Demande à l'utilisateur d'entrer une date. Utilise la date par défaut si l'entrée est invalide.
        /// </summary>
        private static DateTime LireDate(string label, DateTime fallback)
        {
            Console.Write($"{label} : ");
            var saisie = Console.ReadLine();
            return DateTime.TryParse(saisie, out var date) ? date : fallback;
        }
    }
}
