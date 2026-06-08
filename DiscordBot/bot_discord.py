import os
import json
import sys
import traceback
import datetime
import discord
from discord.ext import commands
from discord import app_commands
from dotenv import load_dotenv
from calendrier_image import generer_image_calendrier, trouver_banner, slug

# ─── LOG ERREURS ──────────────────────────────────────
def log_error(exc_type, exc_value, exc_tb):
    trace = "".join(traceback.format_exception(exc_type, exc_value, exc_tb))
    print(f"{datetime.datetime.now()} - {trace}", flush=True)
    with open("error.log", "a") as f:
        f.write(f"{datetime.datetime.now()} - {trace}\n")

sys.excepthook = log_error

# ─── CONFIG ───────────────────────────────────────────
# Discloud ne charge pas .env automatiquement → on le lit nous-mêmes
ENV_PATH = os.path.join(os.path.dirname(os.path.abspath(__file__)), ".env")
load_dotenv(ENV_PATH)
print(f".env trouvé: {os.path.exists(ENV_PATH)}", flush=True)
DISCORD_TOKEN = os.getenv("DISCORD_TOKEN")

# Chemins (à côté du script pour partir avec le zip Discloud)
BASE_DIR     = os.path.dirname(os.path.abspath(__file__))
STREAMS_FILE = os.path.join(BASE_DIR, "streams.json")
JOURS_FR     = ["Lun", "Mar", "Mer", "Jeu", "Ven", "Sam", "Dim"]

# ─── PLANNING (lecture/écriture synchrone) ────────────

def charger_streams() -> dict:
    """Charge streams.json de façon synchrone (évite les race conditions)."""
    if not os.path.exists(STREAMS_FILE):
        return {}
    with open(STREAMS_FILE, "r", encoding="utf-8") as f:
        try:
            return json.load(f)
        except json.JSONDecodeError:
            return {}

def sauver_streams(data: dict) -> None:
    """Écrit streams.json de façon synchrone."""
    with open(STREAMS_FILE, "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)

# ─── BOT (slash uniquement) ───────────────────────────

intents = discord.Intents.default()
bot = commands.Bot(command_prefix="!", intents=intents, help_command=None)

@bot.event
async def on_ready():
    # Sync par-serveur = apparition instantanée (le sync global met jusqu'à ~1h)
    try:
        total = 0
        for guild in bot.guilds:
            bot.tree.copy_global_to(guild=guild)
            synced = await bot.tree.sync(guild=guild)
            total += len(synced)
            print(f"Slash sync serveur {guild.name} ({guild.id}) : {len(synced)} commandes")
        if not bot.guilds:
            print("⚠️ Le bot n'est dans AUCUN serveur — réinvite-le avec le scope applications.commands")
        print(f"Slash commands synchronisées (total) : {total}")
    except Exception as e:
        print(f"Erreur sync slash commands : {type(e).__name__} : {e}")
    print(f"PointuBot connecté → {bot.user} (id: {bot.user.id})")

# ─── SLASH COMMANDS — PLANNING ────────────────────────

def _embed_calendrier(data: dict, lundi) -> discord.Embed:
    """Repli texte si la génération d'image échoue."""
    lignes = []
    for i in range(7):
        jour    = lundi + datetime.timedelta(days=i)
        streams = sorted(data.get(jour.isoformat(), []),
                         key=lambda s: s.get("time", ""))
        contenu = (", ".join(f"{s['time']} {s['game']}" for s in streams)
                   if streams else "—")
        lignes.append(f"**{JOURS_FR[i]} {jour.day}** → {contenu}")
    embed = discord.Embed(title="📅 Planning stream de la semaine",
                          description="\n".join(lignes), color=0x39FF14)
    embed.set_footer(text="Mis à jour • /calendrier pour rafraîchir")
    embed.timestamp = datetime.datetime.now()
    return embed


@bot.tree.command(name="calendrier",
                  description="Affiche le planning stream de la semaine en cours")
async def calendrier(interaction: discord.Interaction):
    await interaction.response.defer()
    data  = charger_streams()
    today = datetime.date.today()
    lundi = today - datetime.timedelta(days=today.weekday())
    try:
        buf = generer_image_calendrier(data, lundi)
        await interaction.followup.send(
            file=discord.File(buf, filename="calendrier.png"))
    except Exception as e:
        print(f"Image calendrier KO, repli embed : {type(e).__name__} : {e}",
              flush=True)
        await interaction.followup.send(embed=_embed_calendrier(data, lundi))


@bot.tree.command(name="addstream",
                  description="Ajoute un stream au planning (admin)")
@app_commands.describe(jour="Date au format YYYY-MM-DD",
                       heure="Heure au format HH:MM",
                       jeu="Nom du jeu / catégorie")
@app_commands.default_permissions(administrator=True)
async def addstream(interaction: discord.Interaction,
                    jour: str, heure: str, jeu: str):
    try:
        datetime.datetime.strptime(jour, "%Y-%m-%d")
        datetime.datetime.strptime(heure, "%H:%M")
    except ValueError:
        await interaction.response.send_message(
            "❌ Format invalide — jour : `YYYY-MM-DD`, heure : `HH:MM`.",
            ephemeral=True)
        return

    data = charger_streams()
    data.setdefault(jour, []).append({"time": heure, "game": jeu})
    data[jour].sort(key=lambda s: s["time"])
    sauver_streams(data)

    statut_img = ("🖼️ bannière trouvée" if trouver_banner(jeu)
                  else f"📭 pas de bannière — ajoute `banners/{slug(jeu)}.png`")
    await interaction.response.send_message(
        f"✅ Ajouté : **{jour}** → {heure} {jeu}\n{statut_img}", ephemeral=True)


@bot.tree.command(name="delstream",
                  description="Supprime un stream du planning (admin)")
@app_commands.describe(jour="Date au format YYYY-MM-DD",
                       index="Numéro du stream (0 = premier de la journée)")
@app_commands.default_permissions(administrator=True)
async def delstream(interaction: discord.Interaction,
                    jour: str, index: int):
    data    = charger_streams()
    streams = data.get(jour)
    if not streams or index < 0 or index >= len(streams):
        await interaction.response.send_message(
            "❌ Aucun stream à cet index pour ce jour.", ephemeral=True)
        return

    retire = streams.pop(index)
    if not streams:
        del data[jour]
    sauver_streams(data)
    await interaction.response.send_message(
        f"🗑️ Supprimé : **{jour}** → {retire['time']} {retire['game']}",
        ephemeral=True)


@bot.tree.error
async def on_app_command_error(interaction: discord.Interaction, error):
    if isinstance(error, app_commands.MissingPermissions):
        msg = "⛔ Commande réservée aux admins."
    else:
        msg = f"⚠️ Erreur : {error}"
    if interaction.response.is_done():
        await interaction.followup.send(msg, ephemeral=True)
    else:
        await interaction.response.send_message(msg, ephemeral=True)


if __name__ == "__main__":
    print(f"Démarrage PointuBot — token présent: {bool(DISCORD_TOKEN)}", flush=True)
    try:
        bot.run(DISCORD_TOKEN)
    except Exception as e:
        print(f"CRASH bot.run : {type(e).__name__} : {e}", flush=True)
        traceback.print_exc()
        raise
