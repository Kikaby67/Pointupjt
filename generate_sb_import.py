"""
Generateur d'import Streamer.bot - Pointu-PJT
Format exact decode depuis export reel SB 1.0.4 :
  - type C# = 99999
  - code = base64 dans byteCode
  - trigger type 401 avec commandId
  - SBAE + deflate (sans CRC) + base64
"""
import json, uuid, os, base64, zlib, struct

BASE = os.path.dirname(os.path.abspath(__file__))
SB   = os.path.join(BASE, "Streamerbot")
OUT  = os.path.join(BASE, "Pointu-PJT_SB_Import.txt")

QUEUE_DEFAULT = "00000000-0000-0000-0000-000000000000"
MSCORLIB      = "C:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\mscorlib.dll"

def guid():
    return str(uuid.uuid4())

def lire_cs(chemin_relatif):
    chemin = os.path.join(SB, chemin_relatif)
    if not os.path.exists(chemin):
        print("  [!] MANQUANT : " + chemin_relatif)
        return None
    with open(chemin, encoding="utf-8") as f:
        return f.read()

def make_csharp_subaction(code):
    byte_code = base64.b64encode(code.encode("utf-8")).decode("ascii")
    return {
        "name":                None,
        "description":         None,
        "references":          [MSCORLIB],
        "byteCode":            byte_code,
        "precompile":          False,
        "delayStart":          False,
        "saveResultToVariable":False,
        "saveToVariable":      None,
        "id":                  guid(),
        "weight":              0.0,
        "type":                99999,
        "parentId":            None,
        "enabled":             True,
        "index":               0
    }

def make_command(nom_commande):
    cmd_id = guid()
    cmd = {
        "permittedUsers":     [],
        "permittedGroups":    [],
        "id":                 cmd_id,
        "name":               nom_commande,
        "enabled":            True,
        "include":            False,
        "mode":               0,
        "command":            nom_commande,
        "regexExplicitCapture":False,
        "location":           0,
        "ignoreBotAccount":   True,
        "ignoreInternal":     True,
        "sources":            1,
        "persistCounter":     False,
        "persistUserCounter": False,
        "caseSensitive":      False,
        "globalCooldown":     0,
        "userCooldown":       0,
        "group":              None,
        "grantType":          0
    }
    return cmd, cmd_id

def make_action(nom, groupe, code, cmd_id=None):
    triggers = []
    if cmd_id:
        triggers.append({
            "commandId":  cmd_id,
            "id":         guid(),
            "type":       401,
            "enabled":    True,
            "exclusions": []
        })
    return {
        "id":                  guid(),
        "queue":               QUEUE_DEFAULT,
        "enabled":             True,
        "excludeFromHistory":  False,
        "excludeFromPending":  False,
        "name":                nom,
        "group":               groupe,
        "alwaysRun":           False,
        "randomAction":        False,
        "concurrent":          False,
        "triggers":            triggers,
        "subActions":          [make_csharp_subaction(code)],
        "collapsedGroups":     []
    }

# ── Définition des actions ────────────────────────────────────────────────────
ACTIONS_DEF = [
    ("!bonjour",         "!bonjour",         "Joueurs",    "Commandes/Bonjour/Commande_bonjour.cs"),
    ("!rejoindre",       "!rejoindre",       "Joueurs",    "Commandes/Rejoindre/Commande_rejoindre.cs"),
    ("!profil",          "!profil",          "Joueurs",    "Commandes/Profil/commande_!profil.cs"),
    ("!choisirclasse",   "!choisirclasse",   "Joueurs",    "Commandes/Choisirclasse/commande_choisirclasse.cs"),
    ("!sousclasse",      "!sousclasse",      "Joueurs",    "Commandes/ChoisirSousClasse/commande_sousclasse.cs"),
    ("!repos",           "!repos",           "Joueurs",    "Commandes/Repos/commande_repos.cs"),
    ("!arbonet",         "!arbonet",         "Joueurs",    "Commandes/Arbonet/commande_arbonet.cs"),
    ("!inventaire",      "!inventaire",      "Items",      "Commandes/Inventaire/commande_inventaire.cs"),
    ("!equiper",         "!equiper",         "Items",      "Commandes/Equiper/commande_equiper.cs"),
    ("!equipement",      "!equipement",      "Items",      "Commandes/Equipement/commande_equipement.cs"),
    ("!vendre",          "!vendre",          "Items",      "Commandes/Vendre/commande_vendre.cs"),
    ("!utiliser",        "!utiliser",        "Items",      "Commandes/Utiliser/commande_utiliser.cs"),
    ("!quete",           "!quete",           "Quetes",     "quetes/quest_system.cs"),
    ("!abandon",         "!abandon",         "Quetes",     "Commandes/Abandon/commande_abandon.cs"),
    ("!accepter",        "!accepter",        "Quetes",     "Commandes/Accepter/commande_accepter.cs"),
    ("!refuser",         "!refuser",         "Quetes",     "Commandes/Refuser/commande_refuser.cs"),
    ("!attaque",         "!attaque",         "Combat",     "combat/combat_attaque.cs"),
    ("!soin",            "!soin",            "Combat",     "combat/combat_soin.cs"),
    ("!defense",         "!defense",         "Combat",     "combat/combat_defense.cs"),
    ("!fuir",            "!fuir",            "Combat",     "combat/combat_fuir.cs"),
    ("!hexadecimeur",    "!hexadecimeur",    "InfoClasses","Commandes/InfoClasses/commande_hexadecimeur.cs"),
    ("!cryptolame",      "!cryptolame",      "InfoClasses","Commandes/InfoClasses/commande_cryptolame.cs"),
    ("!hackmancien",     "!hackmancien",     "InfoClasses","Commandes/InfoClasses/commande_hackmancien.cs"),
    ("!firewaller",      "!firewaller",      "InfoClasses","Commandes/InfoClasses/commande_firewaller.cs"),
    ("!algorythmancien", "!algorythmancien", "InfoClasses","Commandes/InfoClasses/commande_algorythmancien.cs"),
    ("!classement",      "!classement",      "Broadcaster","Commandes/Classement/commande_classement.cs"),
    ("!racine",          "!racine",          "Broadcaster","Commandes/Secret/commande_secret.cs"),
    ("QuestCheck",       None,               "Timers",     "quetes/quest_timer.cs"),
    ("Timer XP Visionnage", None,            "Timers",     "Timer_Xp/Timer_XP_visionnage.cs"),
]

actions_list  = []
commands_list = []
manquants     = []

for (nom, commande, groupe, fichier) in ACTIONS_DEF:
    code = lire_cs(fichier)
    if code is None:
        manquants.append(fichier)
        continue

    cmd_id = None
    if commande is not None:
        cmd, cmd_id = make_command(commande)
        commands_list.append(cmd)

    actions_list.append(make_action(nom, groupe, code, cmd_id))

# ── Structure finale ──────────────────────────────────────────────────────────
export = {
    "meta": {
        "name":           "Pointu-PJT",
        "author":         "kikabygaming",
        "version":        "1.0.0",
        "description":    "Commandes RPG Pointu-PJT",
        "autoRunAction":  None,
        "minimumVersion": None
    },
    "data": {
        "actions":          actions_list,
        "queues":           [],
        "commands":         commands_list,
        "websocketServers": [],
        "websocketClients": [],
        "timers":           []
    },
    "version":        23,
    "exportedFrom":   "1.0.4",
    "minimumVersion": "1.0.0-alpha.1"
}

# ── Encodage SBAE + deflate (sans trailer CRC) + base64 ───────────────────────
json_bytes  = json.dumps(export, ensure_ascii=False).encode("utf-8")
# Gzip sans CRC valide (comme SB le fait)
compressed  = zlib.compress(json_bytes, level=9)
# Construire header gzip minimal + deflate payload (sans CRC/size trailer)
gzip_header = bytes([0x1f, 0x8b, 0x08, 0x00,  # magic + deflate + flags=0
                     0x00, 0x00, 0x00, 0x00,   # mtime = 0
                     0x00, 0x00])              # xfl=0, os=0
# zlib compress includes 2-byte header + 4-byte adler32 trailer — strip them for raw deflate
raw_deflate = compressed[2:-4]
# Gzip trailer: CRC32 + size mod 2^32
import binascii
crc   = binascii.crc32(json_bytes) & 0xFFFFFFFF
isize = len(json_bytes) & 0xFFFFFFFF
gzip_trailer = struct.pack("<II", crc, isize)

full_gz    = gzip_header + raw_deflate + gzip_trailer
sbae_bytes = b"SBAE" + full_gz
b64_string = base64.b64encode(sbae_bytes).decode("ascii")

with open(OUT, "w", encoding="ascii") as f:
    f.write(b64_string)

print("=" * 60)
print("Fichier : " + OUT)
print("Actions : " + str(len(actions_list)))
print("Commandes : " + str(len(commands_list)))
if manquants:
    print("Manquants (" + str(len(manquants)) + ") :")
    for m in manquants: print("  - " + m)
print("=" * 60)
print("IMPORT : Actions > clic droit > Import > coller le contenu du .txt")
print("Timers a configurer manuellement apres import :")
print("  - QuestCheck : 30s, repete, desactive au demarrage")
print("  - Timer XP Visionnage : 900s, repete, active au demarrage")
