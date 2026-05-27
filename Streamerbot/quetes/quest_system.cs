using System;
using System.IO;
public class CPHInline
{
    private const string DOSSIER_JOUEURS = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\joueurs";
    private const string CONFIG_QUETES   = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\config_quetes.json";

    public bool Execute()
    {
        string nomJoueur = args["user"].ToString();
        string cheminFichier = Path.Combine(DOSSIER_JOUEURS, nomJoueur.ToLower() + ".json");

        if (!File.Exists(cheminFichier))
        {
            CPH.SendMessage(nomJoueur + ", tape !rejoindre pour commencer ton parcours d'aventurier dans l'Antre de Pointu. Tu pourras ensuite choisir ta classe et commencer à faire des quêtes !");
            return true;
        }

        string json = File.ReadAllText(cheminFichier);
        long maintenant = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        long cooldownFin = long.Parse(LireValeur(json, "queteCooldownFin"));
        if (cooldownFin > 0 && maintenant < cooldownFin)
        {
            int minutesRestantes = (int)Math.Ceiling((cooldownFin - maintenant) / 60.0);
            CPH.SendMessage(nomJoueur + ", tu récupères dans l'Antre après ta défaite. Encore " + minutesRestantes + " minute(s) avant de pouvoir repartir en quête !");
            return true;
        }

        if (LireValeur(json, "classeChoisie") != "true")
        {
            CPH.SendMessage(nomJoueur + ", choisis ta classe avant de commencer une quête. Tape !choisirclasse suivi du nom de la classe (ex: !choisirclasse hexadécimeur).");
            return true;
        }

        if (LireValeur(json, "enCombat") == "true")
        {
            CPH.SendMessage(nomJoueur + ", tu es actuellement en combat. Termine ton combat avant de commencer une nouvelle quête.");
            return true;
        }

        int seed = (nomJoueur.GetHashCode() ^ (int)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) & int.MaxValue;
        Random rng = new Random(seed);

        if (LireValeur(json, "enQuete") == "true")
        {
            if (LireValeur(json, "enRencontre") == "true")
            {
                string ennemNom = LireValeur(json, "ennemiNom");
                CPH.SendMessage(nomJoueur + ", tu es en pleine rencontre contre un " + ennemNom + " ! Concentre-toi sur le combat !");
                return true;
            }

            string queteEnCours = LireValeur(json, "queteId");
            int ticksRequis = int.Parse(LireValeur(json, "queteTicksRestants"));
            long debutTimestamp = long.Parse(LireValeur(json, "queteDernierTick"));
            long totalPause = long.Parse(LireValeur(json, "queteTotalPause"));
            long secondesEcoulees = (maintenant - debutTimestamp) - totalPause;
            long secondesRequises = ticksRequis * 5 * 60L;

            if (secondesEcoulees < secondesRequises)
            {
                int minutesRestantes = (int)Math.Ceiling((secondesRequises - secondesEcoulees) / 60.0);
                CPH.SendMessage(nomJoueur + ", ta quête est en cours (" + queteEnCours + "). Il te reste environ " + minutesRestantes + " minute(s).");
                return true;
            }

            // Le temps est écoulé : résoudre la quête
            string[] data = GetQueteData(queteEnCours);
            bool succes = rng.Next(100) >= 20;

            json = ModifierValeur(json, "enQuete", "false", false);
            json = ModifierValeur(json, "queteTicksRestants", "0", false);

            if (succes)
            {
                int xp = int.Parse(data[2]);
                int ram = int.Parse(data[3]);
                json = AjouterValeur(json, "experience", xp);
                json = AjouterValeur(json, "ram", ram);
                File.WriteAllText(cheminFichier, json);
                CPH.SendMessage(nomJoueur + ", ta quête est terminée ! Succès ! Tu gagnes " + xp + " XP et " + ram + " RAM. Bien joué aventurier !");
            }
            else
            {
                File.WriteAllText(cheminFichier, json);
                CPH.SendMessage(nomJoueur + ", ta quête est terminée... Échec. Le destin ne t'a pas souri cette fois. Retente ta chance bientôt !");
            }
            return true;
        }

        // Lancer une nouvelle quête
        string[] quetes = { "artefact_01", "artefact_02", "artefact_03", "artefact_04", "artefact_05", "service_01", "service_02", "service_03", "service_04", "service_05", "entretien_01", "entretien_02", "entretien_03" };
        string queteId = quetes[rng.Next(quetes.Length)];
        string[] questData = GetQueteData(queteId);
        int ticks = int.Parse(questData[1]);

        json = ModifierValeur(json, "enQuete", "true", false);
        json = ModifierValeur(json, "queteId", queteId, true);
        json = ModifierValeur(json, "queteTicksRestants", questData[1], false);
        json = ModifierValeur(json, "queteDernierTick", maintenant.ToString(), false);
        json = ModifierValeur(json, "enRencontre", "false", false);
        json = ModifierValeur(json, "rencontreType", "", true);
        json = ModifierValeur(json, "quetePauseDebut", "0", false);
        json = ModifierValeur(json, "queteTotalPause", "0", false);
        json = ModifierValeur(json, "queteCooldownFin", "0", false);
        json = ModifierValeur(json, "dernierCheckRencontre", maintenant.ToString(), false);
        json = ModifierValeur(json, "queteEventsUsed", "0", false);

        File.WriteAllText(cheminFichier, json);
        CPH.SendMessage(nomJoueur + ", tu pars en quête : " + questData[0] + " !" );
        CPH.EnableTimer("QuestCheck");
        return true;
    }

    private string[] GetQueteData(string id)
    {
        string cfg   = File.ReadAllText(CONFIG_QUETES);
        string ticks = LireValeur(cfg, id + "_ticks");
        if (ticks == "0") return new string[] { "Quête inconnue", "1", "0", "0" };
        string desc = LireValeurString(cfg, id + "_description");
        string xp   = LireValeur(cfg, id + "_xp");
        string ram  = LireValeur(cfg, id + "_ram");
        return new string[] { desc, ticks, xp, ram };
    }

    private string LireValeur(string json, string cle)
    {
        string marqueur = "\"" + cle + "\": ";
        int posDebut = json.IndexOf(marqueur);
        if (posDebut == -1) return "0";
        posDebut += marqueur.Length;
        int posFin = json.IndexOfAny(new char[] { ',', '\n', '}' }, posDebut);
        return json.Substring(posDebut, posFin - posDebut).Trim().Trim('"');
    }

    private string LireValeurString(string json, string cle)
    {
        string marqueur = "\"" + cle + "\": \"";
        int posDebut    = json.IndexOf(marqueur);
        if (posDebut == -1) return "";
        posDebut       += marqueur.Length;
        int posFin      = json.IndexOf("\"", posDebut);
        if (posFin == -1) return "";
        return json.Substring(posDebut, posFin - posDebut);
    }

    private string AjouterValeur(string json, string cle, int montant)
    {
        string marqueur = "\"" + cle + "\": ";
        int posDebut = json.IndexOf(marqueur);
        if (posDebut == -1) return json;
        posDebut += marqueur.Length;
        int posFin = json.IndexOfAny(new char[] { ',', '\n', '}' }, posDebut);
        string ancienneStr = json.Substring(posDebut, posFin - posDebut).Trim().Trim('"');
        int ancienne = int.TryParse(ancienneStr, out int v) ? v : 0;
        return json.Substring(0, posDebut) + (ancienne + montant).ToString() + json.Substring(posDebut + (posFin - posDebut));
    }

    private string ModifierValeur(string json, string cle, string val, bool estTexte)
    {
        string marqueur = "\"" + cle + "\": ";
        int posDebut = json.IndexOf(marqueur);
        if (posDebut == -1) return json;
        posDebut += marqueur.Length;
        int posFin = json.IndexOfAny(new char[] { ',', '\n', '}' }, posDebut);
        string ancienne = json.Substring(posDebut, posFin - posDebut);
        string nouvelle = estTexte ? "\"" + val + "\"" : val;
        return json.Substring(0, posDebut) + nouvelle + json.Substring(posDebut + ancienne.Length);
    }

}