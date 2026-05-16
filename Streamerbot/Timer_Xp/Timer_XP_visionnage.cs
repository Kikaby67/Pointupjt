using System;
using System.IO;

public class CPHInline
{
    private const string DOSSIER_JOUEURS  = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\joueurs";
    private const int    XP_PAR_PERIODE   = 5;

    // XP requis pour chaque niveau (index = niveau)
    // Niveau 1 = 0 XP, Niveau 2 = 300 XP, etc.
    private static readonly int[] XP_SEUILS =
        { 0, 0, 300, 900, 2700, 6500, 14000, 23000, 34000, 48000, 64000 };

    public bool Execute()
    {
        // ── Récupère tous les profils joueurs ─────────────────
        string[] fichiers = Directory.GetFiles(DOSSIER_JOUEURS, "*.json");

        foreach (string cheminFichier in fichiers)
        {
            string json = File.ReadAllText(cheminFichier);

            // Pas de classe choisie → pas d'XP
            if (LireValeur(json, "classeChoisie") != "true") continue;

            // ── Ajout des 5 XP ────────────────────────────────
            int xpActuel     = int.Parse(LireValeur(json, "experience"));
            int niveauActuel = int.Parse(LireValeur(json, "niveau"));
            int nouvelXP     = xpActuel + XP_PAR_PERIODE;

            json = ModifierValeur(json, "experience", nouvelXP.ToString(), false);

            // ── Vérification de montée de niveau ──────────────
            int nouveauNiveau = CalculerNiveau(nouvelXP);

            if (nouveauNiveau > niveauActuel && nouveauNiveau <= 10)
            {
                // Mise à jour du niveau
                json = ModifierValeur(json, "niveau", nouveauNiveau.ToString(), false);

                // Application des bonus du nouveau niveau
                json = AppliquerBonusNiveau(json, nouveauNiveau);

                // Annonce dans le chat
                string nomJoueur = LireValeur(json, "nomJoueur");
                CPH.SendMessage(MessageNiveau(nomJoueur, nouveauNiveau));
            }

            File.WriteAllText(cheminFichier, json);
        }

        return true;
    }

    // ── Calcule le niveau selon l'XP total ───────────────────
    // Parcourt le tableau à l'envers et retourne le premier
    // niveau dont le seuil est atteint
    private int CalculerNiveau(int xp)
    {
        for (int i = XP_SEUILS.Length - 1; i >= 1; i--)
        {
            if (xp >= XP_SEUILS[i]) return i;
        }
        return 1;
    }

    // ── Applique le bonus selon le niveau atteint ─────────────
    private string AppliquerBonusNiveau(string json, int niveau)
    {
        switch (niveau)
        {
            case 2:  // +3 PV
                json = AjouterValeur(json, "pvMax",      3);
                json = AjouterValeur(json, "pvActuels",  3);
                break;

            case 3:  // +1 CA +1 PV
                json = AjouterValeur(json, "classeArmure", 1);
                json = AjouterValeur(json, "pvMax",        1);
                json = AjouterValeur(json, "pvActuels",    1);
                break;

            case 4:  // +3 PV
                json = AjouterValeur(json, "pvMax",      3);
                json = AjouterValeur(json, "pvActuels",  3);
                break;

            case 5:  // Sous-classe → aucun bonus automatique
                break;

            case 6:  // +3 PV
                json = AjouterValeur(json, "pvMax",      3);
                json = AjouterValeur(json, "pvActuels",  3);
                break;

            case 7:  // +1 CA +1 PV
                json = AjouterValeur(json, "classeArmure", 1);
                json = AjouterValeur(json, "pvMax",        1);
                json = AjouterValeur(json, "pvActuels",    1);
                break;

            case 8:  // +100 Ram
                json = AjouterValeur(json, "ram", 100);
                break;

            case 9:  // +3 PV
                json = AjouterValeur(json, "pvMax",      3);
                json = AjouterValeur(json, "pvActuels",  3);
                break;

            case 10: // +2 Charisme
                json = AjouterValeur(json, "charisme", 2);
                break;
        }
        return json;
    }

    // ── Message d'annonce selon le niveau ─────────────────────
    private string MessageNiveau(string nomJoueur, int niveau)
    {
        string bonus;
        switch (niveau)
        {
            case 2:  bonus = "+3 PV max";                                          break;
            case 3:  bonus = "+1 CA · +1 PV max";                                 break;
            case 4:  bonus = "+3 PV max";                                          break;
            case 5:  bonus = "Sous-classe débloquée ! Tape !choisirSousClasse !";  break;
            case 6:  bonus = "+3 PV max";                                          break;
            case 7:  bonus = "+1 CA · +1 PV max";                                 break;
            case 8:  bonus = "+100 Ram";                                           break;
            case 9:  bonus = "+3 PV max";                                          break;
            case 10: bonus = "+2 Charisme · Niveau maximum atteint !";             break;
            default: bonus = "";                                                    break;
        }
        return "🎉 " + nomJoueur + " passe au niveau " + niveau + " ! " + bonus;
    }

    // ── Lit une valeur et ajoute un montant ───────────────────
    // Raccourci pour éviter de répéter LireValeur + ModifierValeur
    private string AjouterValeur(string json, string cle, int montant)
    {
        int valeurActuelle = int.Parse(LireValeur(json, cle));
        return ModifierValeur(json, cle, (valeurActuelle + montant).ToString(), false);
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