using System;
using System.IO;
public class CPHInline
{
    private const string DOSSIER_JOUEURS = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\joueurs";
    private const string CONFIG_LEVEL    = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\config_level.json";
    private const string CONFIG_ALLIES   = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\config_allies.json";

    public bool Execute()
    {
        string nomJoueur    = args["user"].ToString();
        string cheminFichier = Path.Combine(DOSSIER_JOUEURS, nomJoueur.ToLower() + ".json");

        if (!File.Exists(cheminFichier))
        {
            CPH.SendMessage(nomJoueur + ", tape !rejoindre pour t'inscrire dans l'Antre de Pointu !");
            return true;
        }

        string json           = File.ReadAllText(cheminFichier);
        string offreEnAttente = LireValeurString(json, "offreEnAttente");
        long   offreExpire    = long.Parse(LireValeur(json, "offreExpire"));
        long   maintenant     = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        if (offreEnAttente == "")
        {
            CPH.SendMessage(nomJoueur + ", tu n'as aucune offre en attente !");
            return true;
        }

        if (offreExpire > 0 && maintenant > offreExpire)
        {
            json = ModifierValeur(json, "offreEnAttente", "", true);
            json = ModifierValeur(json, "offreValeur",    "0", false);
            json = ModifierValeur(json, "offreExpire",    "0", false);
            File.WriteAllText(cheminFichier, json);
            CPH.SendMessage(nomJoueur + ", l'offre a expiré avant que tu puisses l'accepter !");
            return true;
        }

        int valeur = int.Parse(LireValeur(json, "offreValeur"));

        if (offreEnAttente == "vieux_sage")
        {
            string cfgA      = File.ReadAllText(CONFIG_ALLIES);
            int    chanceXP  = int.Parse(LireValeur(cfgA, "vieux_sage_chance_xp"));
            int    chanceItem = int.Parse(LireValeur(cfgA, "vieux_sage_chance_perte_item"));
            Random rng       = new Random();
            bool   gagne     = rng.Next(100) < chanceXP;
            bool   perd      = rng.Next(100) < chanceItem;

            json = ModifierValeur(json, "offreEnAttente", "", true);
            json = ModifierValeur(json, "offreValeur",    "0", false);
            json = ModifierValeur(json, "offreExpire",    "0", false);

            string msgFinal = "";

            // XP si gagné
            if (gagne)
            {
                json     = AjouterValeur(json, "experience", valeur);
                json     = VerifierMonteeNiveau(json, nomJoueur);
                msgFinal += nomJoueur + ", tu acceptes le marché du Vieux Sage. Il te transmet son savoir ! +" + valeur + " XP !";
            }

            // Perte item si perdu
            if (perd)
            {
                string   inv   = LireValeurString(json, "inventaire");
                string[] items = inv == "" ? new string[0] : inv.Split(',');
                if (items.Length > 0)
                {
                    int    idx      = rng.Next(items.Length);
                    string itemPerdu = items[idx].Trim();
                    string nouvInv  = "";
                    bool   retire   = false;
                    foreach (string it in items)
                    {
                        if (!retire && it.Trim() == itemPerdu) { retire = true; continue; }
                        if (nouvInv != "") nouvInv += ",";
                        nouvInv += it.Trim();
                    }
                    json     = ModifierValeurString(json, "inventaire", nouvInv);
                    // Si XP déjà gagné → commence par "Mais" pour enchaîner
                    // Sinon → commence par le nom du joueur (premier message)
                    msgFinal += (msgFinal != "" ? " Mais" : nomJoueur + ", tu acceptes le marché —")
                              + " le Vieux Sage s'empare de " + itemPerdu + " en guise de paiement...";
                }
            }

            // Rien
            if (!gagne && !perd)
                msgFinal = nomJoueur + ", tu acceptes le marché — le Vieux Sage sourit et s'éclipse sans rien donner ni prendre...";

            File.WriteAllText(cheminFichier, json);
            CPH.SendMessage(msgFinal);
        }
        
        else if (offreEnAttente == "marchand_soin")
        {
            int pvActuels = int.Parse(LireValeur(json, "pvActuels"));
            int pvMax     = int.Parse(LireValeur(json, "pvMax"));
            int soin      = Math.Min(valeur, pvMax - pvActuels);
            json = ModifierValeur(json, "offreEnAttente", "", true);
            json = ModifierValeur(json, "offreValeur",    "0", false);
            json = ModifierValeur(json, "offreExpire",    "0", false);
            if (soin > 0)
            {
                json = AjouterValeur(json, "pvActuels", soin);
                File.WriteAllText(cheminFichier, json);
                CPH.SendMessage(nomJoueur + ", le marchand te soigne ! +" + soin + " PV → " + (pvActuels + soin) + "/" + pvMax + " PV !");
            }
            else
            {
                File.WriteAllText(cheminFichier, json);
                CPH.SendMessage(nomJoueur + ", tu acceptes les soins... mais tu as déjà tous tes PV !");
            }
        }

        return true;
    }

    private string VerifierMonteeNiveau(string json, string nomJoueur)
    {
        int niveauActuel  = int.Parse(LireValeur(json, "niveau"));
        int nouvelXP      = int.Parse(LireValeur(json, "experience"));
        int nouveauNiveau = CalculerNiveau(nouvelXP);
        if (nouveauNiveau > niveauActuel)
        {
            json = ModifierValeur(json, "niveau", nouveauNiveau.ToString(), false);
            json = AppliquerBonusNiveau(json, nouveauNiveau);
            CPH.SendMessage(MessageNiveau(nomJoueur, nouveauNiveau));
        }
        return json;
    }

    private int CalculerNiveau(int xp)
    {
        string cfg    = File.ReadAllText(CONFIG_LEVEL);
        int niveauMax = int.Parse(LireValeur(cfg, "niveauMax"));
        for (int i = niveauMax; i >= 2; i--)
            if (xp >= int.Parse(LireValeur(cfg, "niveau_" + i + "_xp"))) return i;
        return 1;
    }

    private string AppliquerBonusNiveau(string json, int niveau)
    {
        string cfg        = File.ReadAllText(CONFIG_LEVEL);
        int pvBonus       = int.Parse(LireValeur(cfg, "niveau_" + niveau + "_pvBonus"));
        int caBonus       = int.Parse(LireValeur(cfg, "niveau_" + niveau + "_caBonus"));
        int ramBonus      = int.Parse(LireValeur(cfg, "niveau_" + niveau + "_ramBonus"));
        int charismeBonus = int.Parse(LireValeur(cfg, "niveau_" + niveau + "_charismeBonus"));
        if (pvBonus > 0) { json = AjouterValeur(json, "pvMax", pvBonus); json = AjouterValeur(json, "pvActuels", pvBonus); }
        if (caBonus       > 0) json = AjouterValeur(json, "classeArmure", caBonus);
        if (ramBonus      > 0) json = AjouterValeur(json, "ram",          ramBonus);
        if (charismeBonus > 0) json = AjouterValeur(json, "charisme",     charismeBonus);
        return json;
    }

    private string MessageNiveau(string nomJoueur, int niveau)
    {
        string cfg   = File.ReadAllText(CONFIG_LEVEL);
        string bonus = LireValeur(cfg, "niveau_" + niveau + "_message");
        return "🎉 " + nomJoueur + " passe au niveau " + niveau + " ! " + bonus;
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

    private string ModifierValeurString(string json, string cle, string val)
    {
        string marqueur = "\"" + cle + "\": \"";
        int posDebut    = json.IndexOf(marqueur);
        if (posDebut == -1) return json;
        posDebut       += marqueur.Length;
        int posFin      = json.IndexOf("\"", posDebut);
        if (posFin == -1) return json;
        return json.Substring(0, posDebut) + val + json.Substring(posFin);
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
