using System;
using System.IO;
public class CPHInline
{
    private const string DOSSIER_JOUEURS = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\joueurs";

    public bool Execute()
    {
        string nomJoueur = args["user"].ToString();
        string cheminFichier = Path.Combine(DOSSIER_JOUEURS, $"{nomJoueur}.json");
// Vérifie si le joueur exist dans le système est à quel stade.
        if (!File.Exists(cheminFichier))
        {
            CPH.SendMessage(nomJoueur + ", tape !rejoindre pour commencer ton parcous d'aventurier dans l'Antre de Pointu. Tu pourras ensuite choisir ta classe et commencer à faire des quêtes pour gagner de l'expérience et monter en niveau !");
            return true;
        }

        string json = File.ReadAllText(cheminFichier);

        if (LireValeur(json, "classeChoisie") != "true")
        {
            CPH.SendMessage(nomJoueur + ", choisis ta classe avant de commencer une quête. Il te suffit de taper !choisirclasse dans le chat. suivi du nom de la classe que tu souhaites (ex: !choisirclasse hexadécimeur).");
            return true;
        }

        if (LireValeur(json, "enQuete") == "true")
        {
            string queteEnCours = LireValeur(json, "queteId");
            int ticks = int.Parse(LireValeur(json, "queteTicksRestants"));
            CPH.SendMessage(nomJoueur + ", tu as déjà une quête en cours : " + queteEnCours + ". Il te reste " + (ticks * 5)+ " minutes de chemin.");
            return true;
        }
         if (LireValeur(json, "enCombat") == "true")
        {
            CPH.SendMessage(nomJoueur + ", tu es actuellement en combat. Termine ton combat avant de commencer une nouvelle quête.");
            return true;
        }

        Random rng = new Random();
        string[] quetes = { "artefact_01", "artefact_02", "artefact_03", "artefact_04", "artefact_05", "service_01", "service_02", "service_03", "service_04", "service_05", "entretien_01", "entretien_02", "entretien_03" };
        string queteId = quetes[rng.Next(quetes.Length)];
        string[] data = GetQueteData(queteId);

// MAJ du joueur
        json = ModifierValeur(json, "enQuete", "true", false);
        json = ModifierValeur(json, "queteId", queteId, true);
        json = ModifierValeur(json, "queteTicksRestants", data[1], false);

        File.WriteAllText(cheminFichier, json);
        CPH.SendMessage(nomJoueur + ", tu as commencé la quête : " + data[0] + ". Il te reste " + (int.Parse(data[1]) * 5) + " minutes pour la terminer. Bonne chance aventurier !");
        return true;
    }

// Descriptif quetes, ticks, xpRecompense, ramRecompemse
    private string[] GetQueteData(string id)
    {
        switch (id)
        {
            case "artefact_01":
                return new string[]
                {
                    "Direction le nord ! Dans une cabane perchée, " +
                    "un artefact de Pointu t'attend. Durée estimée : 30 min.",
                    "6", "100", "10"
                };    
            case "artefact_02":
                return new string[]
                {
                    "Direction le sud ! Dans une grotte sombre, " +
                    "un artefact de Pointu t'attend. Durée estimée : 20 min.",
                    "5", "80", "8"
                };
            case "artefact_03":
                return new string[]
                {
                    "Direction l'est ! Dans une tour abandonnée, " +
                    "un artefact de Pointu t'attend. Durée estimée : 15 min.",
                    "3", "50", "5"
                };
            case "artefact_04":
                return new string[]
                {
                    "Direction l'ouest ! Dans une forêt mystérieuse, " +
                    "un artefact de Pointu t'attend. Durée estimée : 10 min.",
                    "2", "30", "3"
                };
            case "artefact_05":
                return new string[]
                {
                    "Direction le centre ! Dans une caverne secrète, " +
                    "un artefact de Pointu t'attend. Durée estimée : 5 min.",
                    "1", "10", "1"
                };
            case "service_01":
                return new string[]
                {
                    "Un villageois a besoin d'aide pour récolter des ressources. " +
                    "Durée estimée : 15 min.",
                    "3", "50", "5"
                };
            case "service_02":
                return new string[]
                {
                    "Un marchand cherche un aventurier pour escorter sa caravane. " +
                    "Durée estimée : 20 min.",
                    "4", "70", "7"
                };
            case "service_03":
                return new string[]
                {
                    "Un forgeron a besoin de matériaux rares pour fabriquer une arme. " +
                    "Durée estimée : 25 min.",
                    "5", "90", "9"
                };
            case "service_04":
                return new string[]
                {
                    "Un noble cherche un aventurier pour une mission diplomatique. " +
                    "Durée estimée : 30 min.",
                    "6", "120", "12"
                };
            case "service_05":
                return new string[]
                {
                    "Un mage a besoin d'aide pour collecter des ingrédients magiques. " +
                    "Durée estimée : 10 min.",
                    "2", "40", "4"
                };
            case "entretien_01":
                return new string[]
                {
                    "Un villageois a besoin d'aide pour la sécurité de son réseau. " +
                    "Durée estimée : 15 min.",
                    "3", "50", "5"
                };
            case "entretien_02":
                return new string[]
                {
                    "Un marchand cherche un aventurier pour renforcer sa bourique. " +
                    "Durée estimée : 20 min.",
                    "4", "70", "7"
                };
            case "entretien_03":
                return new string[]
                {
                    "L'arbre serveur a besoin d'être entretenue. " +
                    "Durée estimée : 25 min.",
                    "5", "90", "9"
                };

            default:
                return new string[] { "Quête inconnue", "1", "0", "0" };
        }
    }

    private string LireValeur(string json, string cle)
    {
        string marqueur = "\"" + cle + "\": ";
        int posDebut    = json.IndexOf(marqueur);
        if (posDebut == -1) return "0";
        posDebut       += marqueur.Length;
        int posFin      = json.IndexOfAny(new char[] { ',', '\n', '}' }, posDebut);
        return json.Substring(posDebut, posFin - posDebut).Trim().Trim('"');
    }

    private string ModifierValeur(string json, string cle, string val, bool estTexte)
    {
        string marqueur = "\"" + cle + "\": ";
        int posDebut    = json.IndexOf(marqueur);
        if (posDebut == -1) return json;
        posDebut       += marqueur.Length;
        int posFin      = json.IndexOfAny(new char[] { ',', '\n', '}' }, posDebut);
        string ancienne = json.Substring(posDebut, posFin - posDebut);
        string nouvelle = estTexte ? "\"" + val + "\"" : val;
        return json.Substring(0, posDebut) + nouvelle + json.Substring(posDebut + ancienne.Length);
    }

}