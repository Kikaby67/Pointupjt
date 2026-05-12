using System;
using System.IO;

public class CPHInline
{

    // CONFIGURATION — le seul endroit où tu touches les chemins

    private const string DOSSIER_JOUEURS = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\joueurs";

    public bool Execute()
    {
    
        // 1. On récupère le nom du viewer qui a tapé !rejoindre
    
        string nomJoueur = args["user"].ToString();

     
        // 2. On construit le chemin complet vers son fichier
    
    
        string cheminFichier = Path.Combine(DOSSIER_JOUEURS, nomJoueur.ToLower() + ".json");

    
        // 3. On vérifie si ce joueur est déjà inscrit
    
        if (File.Exists(cheminFichier))
        {
            CPH.SendMessage(nomJoueur + ", tu es déjà inscrit dans l'Antre, Aventurier !");
            return true;
        }

      
        // 4. Le joueur est nouveau — on crée son profil JSON On écrit le JSON "à la main" pour bien comprendre ce qu'on stocke
        
        string dateAujourdhui = DateTime.Now.ToString("dd-MM-yyyy");

       string contenuJson =
            "{\n" +
            "  \"nomJoueur\": \"" + nomJoueur + "\",\n" +
            "  \"dateInscription\": \"" + dateAujourdhui + "\",\n" +
            "  \"ram\": 10,\n" +
            "  \"niveau\": 1,\n" +
            "  \"experience\": 0,\n" +
            // --- Classe ---
            "  \"classeChoisie\": false,\n" +
            "  \"classe\": \"\",\n" +
            "  \"sousClasse\": \"\",\n" +
            // --- Stats de combat ---
            "  \"pvMax\": 0,\n" +
            "  \"pvActuels\": 0,\n" +
            "  \"classeArmure\": 0,\n" +
            "  \"bonusAttaque\": 0,\n" +
            // --- État ---
            "  \"zoneActuelle\": \"Foret-Memoire\",\n" +
            "  \"enQuete\": false,\n" +
            "  \"enCombat\": false,\n" +
            "  \"inventaire\": [],\n" +
            // --- Statistiques ---
            "  \"statistiques\": {\n" +
            "    \"combatsGagnes\": 0,\n" +
            "    \"combatsPerdus\": 0,\n" +
            "    \"quetesTerminees\": 0\n" +
            "  }\n" +
            "}";

        // 5. On s'assure que le dossier existe (sécurité : si le dossier a été supprimé par erreur)
     
        Directory.CreateDirectory(DOSSIER_JOUEURS);

    
        // 6. On écrit le fichier sur le disque
   
        File.WriteAllText(cheminFichier, contenuJson);

     
        // 7. Message de bienvenue dans le chat Twitch
      
        CPH.SendMessage(nomJoueur + " Bien, le bonjour Aventurier de l'Antre ! " +
            "Ton fragment de Carapace est prêt. " +
            "Grâce à lui tu pourras voir tes informations d'aventurier comme ceci : " +
            "Niveau 1 | XP : 0 | Ram : 10");

        return true;
    }
}