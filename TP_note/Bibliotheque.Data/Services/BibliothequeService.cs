using System;
using System.Collections.Generic;
using System.Linq;
using TP_note.Bibliotheque.Data.Models;

namespace TP_note.Bibliotheque.Data.Services
{
    /// <summary>
    /// Service principal de gestion de la bibliothèque. Gère les livres, utilisateurs et catégories.
    /// Enforce les règles de gestion : limite d'emprunts, ISBN et emails uniques, etc.
    /// </summary>
    public class BibliothequeService
    {
        private Bibliotheque _bibliotheque;
        private readonly int _dureeEmpruntJours;
        private readonly int _nbMaxEmpruntsParUtilisateur;

        /// <summary>
        /// Initialise le service avec une bibliothèque optionnelle et les paramètres d'emprunt.
        /// </summary>
        public BibliothequeService(Bibliotheque bibliotheque = null, int dureeEmpruntJours = 21, int nbMaxEmpruntsParUtilisateur = 5)
        {
            _bibliotheque = bibliotheque ?? new Bibliotheque();
            _dureeEmpruntJours = dureeEmpruntJours;
            _nbMaxEmpruntsParUtilisateur = nbMaxEmpruntsParUtilisateur;
            RebuildIndexes();
        }

        public Bibliotheque Bibliotheque => _bibliotheque;

        public IReadOnlyList<Livre> Livres => _bibliotheque.Livres?.AsReadOnly() ?? new List<Livre>().AsReadOnly();
        public IReadOnlyList<Utilisateur> Utilisateurs => _bibliotheque.Utilisateurs?.AsReadOnly() ?? new List<Utilisateur>().AsReadOnly();
        public IReadOnlyList<Categorie> Categories => _bibliotheque.Categories?.AsReadOnly() ?? new List<Categorie>().AsReadOnly();

        /// <summary>
        /// Remplace la bibliothéque en cours par une nouvelle et reconstruit les index internes.
        /// </summary>
        public void RemplacerBibliotheque(Bibliotheque bibliotheque)
        {
            _bibliotheque = bibliotheque ?? throw new ArgumentNullException(nameof(bibliotheque));
            RebuildIndexes();
        }

        /// <summary>
        /// Ajoute une nouvelle catégorie à la bibliothéque. Retourne la catégorie existante si elle existe déjà.
        /// </summary>
        public Categorie AjouterCategorie(string nom)
        {
            if (string.IsNullOrWhiteSpace(nom))
                throw new ArgumentException("Le nom de la categorie est obligatoire.", nameof(nom));

            var trimmed = nom.Trim();
            var existante = _bibliotheque.Categories.FirstOrDefault(c => string.Equals(c.Nom, trimmed, StringComparison.OrdinalIgnoreCase));
            if (existante != null)
                return existante;

            var categorie = new Categorie { Nom = trimmed };
            _bibliotheque.Categories.Add(categorie);
            return categorie;
        }

        /// <summary>
        /// Enregistre un nouvel utilisateur dans la bibliothèque. L'email doit être unique.
        /// </summary>
        public Utilisateur AjouterUtilisateur(string nom, string prenom, string email)
        {
            if (string.IsNullOrWhiteSpace(nom)) throw new ArgumentException("Le nom est obligatoire.", nameof(nom));
            if (string.IsNullOrWhiteSpace(prenom)) throw new ArgumentException("Le prenom est obligatoire.", nameof(prenom));
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("L'email est obligatoire.", nameof(email));

            var normalizedEmail = email.Trim();
            if (ChercherUtilisateurParEmail(normalizedEmail) != null)
                throw new InvalidOperationException("Un utilisateur avec cet email existe deja.");

            var utilisateur = new Utilisateur
            {
                Nom = nom.Trim(),
                Prenom = prenom.Trim(),
                Email = normalizedEmail,
                DateInscription = DateTime.Today,
                LivresEmpruntes = new List<Livre>()
            };

            _bibliotheque.Utilisateurs.Add(utilisateur);
            return utilisateur;
        }

        /// <summary>
        /// Ajoute un nouveau livre à la bibliothéque avec les informations fournies. L'ISBN doit être unique.
        /// Crée la catégorie si elle n'existe pas.
        /// </summary>
        public Livre AjouterLivre(string titre, string auteur, DateTime dateDePublication, string isbn, string nomCategorie)
        {
            if (string.IsNullOrWhiteSpace(titre)) throw new ArgumentException("Le titre est obligatoire.", nameof(titre));
            if (string.IsNullOrWhiteSpace(auteur)) throw new ArgumentException("L'auteur est obligatoire.", nameof(auteur));
            if (string.IsNullOrWhiteSpace(isbn)) throw new ArgumentException("L'ISBN est obligatoire.", nameof(isbn));
            if (string.IsNullOrWhiteSpace(nomCategorie)) throw new ArgumentException("La categorie est obligatoire.", nameof(nomCategorie));

            if (ChercherLivreParIsbn(isbn) != null)
                throw new InvalidOperationException("Un livre avec cet ISBN existe deja.");

            var categorie = AjouterCategorie(nomCategorie);

            var livre = new Livre
            {
                Titre = titre.Trim(),
                Auteur = auteur.Trim(),
                DateDePublication = dateDePublication,
                ISBN = isbn.Trim(),
                Categorie = categorie,
                DateAjout = DateTime.Today
            };

            _bibliotheque.Livres.Add(livre);
            categorie.Livres.Add(livre);
            return livre;
        }

        /// <summary>
        /// Enregistre l'emprunt d'un livre par un utilisateur. Vérifie que le livre est disponible 
        /// et que l'utilisateur n'a pas atteint le nombre maximum d'emprunts.
        /// </summary>
        public void EmprunterLivre(Livre livre, Utilisateur utilisateur)
        {
            if (livre == null) throw new ArgumentNullException(nameof(livre));
            if (utilisateur == null) throw new ArgumentNullException(nameof(utilisateur));

            if (!_bibliotheque.Livres.Contains(livre))
                throw new InvalidOperationException("Ce livre n'appartient pas a la bibliotheque.");

            if (!_bibliotheque.Utilisateurs.Contains(utilisateur))
                throw new InvalidOperationException("Cet utilisateur n'existe pas.");

            VerifierPeutEmprunter(livre, utilisateur);

            if (utilisateur.LivresEmpruntes == null)
                utilisateur.LivresEmpruntes = new List<Livre>();

            utilisateur.LivresEmpruntes.Add(livre);
        }

        /// <summary>
        /// Enregistre la restitution d'un livre emprunté par un utilisateur.
        /// </summary>
        public void RendreLivre(Livre livre, Utilisateur utilisateur)
        {
            if (livre == null) throw new ArgumentNullException(nameof(livre));
            if (utilisateur == null) throw new ArgumentNullException(nameof(utilisateur));

            if (utilisateur.LivresEmpruntes == null || !utilisateur.LivresEmpruntes.Contains(livre))
                throw new InvalidOperationException("Ce livre n'est pas emprunte par cet utilisateur.");

            utilisateur.LivresEmpruntes.Remove(livre);
        }

        /// <summary>
        /// Recherche un livre par son ISBN (case-insensitive). Retourne null si non trouvé.
        /// </summary>
        public Livre ChercherLivreParIsbn(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
                return null;

            var normalized = isbn.Trim();
            return _bibliotheque.Livres.FirstOrDefault(l => string.Equals(l.ISBN, normalized, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Recherche un utilisateur par son email (case-insensitive). Retourne null si non trouvé.
        /// </summary>
        public Utilisateur ChercherUtilisateurParEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var normalized = email.Trim();
            return _bibliotheque.Utilisateurs.FirstOrDefault(u => string.Equals(u.Email, normalized, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Retourne l'utilisateur qui a emprunté le livre, ou null si le livre est disponible.
        /// </summary>
        public Utilisateur ObtenirEmprunteur(Livre livre)
        {
            if (livre == null) return null;

            return _bibliotheque.Utilisateurs.FirstOrDefault(u => u.LivresEmpruntes != null && u.LivresEmpruntes.Contains(livre));
        }

        // Vérifie que le livre est disponible et que l'utilisateur peut emprunter.
        private void VerifierPeutEmprunter(Livre livre, Utilisateur utilisateur)
        {
            if (EstLivreEmprunte(livre))
                throw new InvalidOperationException("Le livre est deja emprunte.");

            if (utilisateur.LivresEmpruntes.Count >= _nbMaxEmpruntsParUtilisateur)
                throw new InvalidOperationException("L'utilisateur a atteint le nombre maximum d'emprunts autorises.");
        }

        // Vérifie si un livre est actuellement emprunté par quelqu'un.
        private bool EstLivreEmprunte(Livre livre)
        {
            return _bibliotheque.Utilisateurs.Any(u => u.LivresEmpruntes != null && u.LivresEmpruntes.Contains(livre));
        }

        // Reconstitue les index internes de la bibliothéque après un chargement.
        // Assure la cohérence des données : matérialise les références de livres, reconstruit les associations catégorie-livre.
        private void RebuildIndexes()
        {
            _bibliotheque.Livres ??= new List<Livre>();
            _bibliotheque.Categories ??= new List<Categorie>();
            _bibliotheque.Utilisateurs ??= new List<Utilisateur>();

            var categoriesParNom = new Dictionary<string, Categorie>(StringComparer.OrdinalIgnoreCase);
            foreach (var categorie in _bibliotheque.Categories.Where(c => !string.IsNullOrWhiteSpace(c.Nom)))
            {
                var nomNormalized = categorie.Nom.Trim();
                categorie.Nom = nomNormalized;
                categorie.Livres = new List<Livre>();
                categoriesParNom[nomNormalized] = categorie;
            }

            var livresParIsbn = _bibliotheque.Livres
                .Where(l => !string.IsNullOrWhiteSpace(l.ISBN))
                .ToDictionary(l => l.ISBN.Trim(), StringComparer.OrdinalIgnoreCase);

            foreach (var livre in _bibliotheque.Livres)
            {
                if (livre == null)
                    continue;

                var nomCategorie = (livre.Categorie?.Nom ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(nomCategorie))
                    continue;

                if (!categoriesParNom.TryGetValue(nomCategorie, out var categorie))
                {
                    categorie = new Categorie { Nom = nomCategorie, Livres = new List<Livre>() };
                    categoriesParNom[nomCategorie] = categorie;
                }

                livre.Categorie = categorie;
                categorie.Livres.Add(livre);
            }

            _bibliotheque.Categories = categoriesParNom.Values.ToList();

            foreach (var utilisateur in _bibliotheque.Utilisateurs)
            {
                if (utilisateur.LivresEmpruntes == null)
                {
                    utilisateur.LivresEmpruntes = new List<Livre>();
                    continue;
                }

                var resolved = new List<Livre>();
                foreach (var livre in utilisateur.LivresEmpruntes.Where(l => l != null))
                {
                    var isbn = (livre.ISBN ?? string.Empty).Trim();
                    if (!string.IsNullOrWhiteSpace(isbn) && livresParIsbn.TryGetValue(isbn, out var matched))
                    {
                        resolved.Add(matched);
                        continue;
                    }

                    resolved.Add(livre);
                }

                utilisateur.LivresEmpruntes = resolved;
            }
        }
    }
}
