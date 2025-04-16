# **Nom du projet**

> Prototype de jeu web tactique inspiré de **RoboStrike**, développé en **C# / ASP.NET Core**. Le dépôt illustre la conception d’un moteur tour‑par‑tour, l’architecture client‑serveur en temps quasi‑réel, ainsi que les bonnes pratiques de développement modernes en .NET.

---

## 1. Aperçu / Introduction

Ce dépôt rassemble le code source d’un **prototype académique** réalisé dans le cadre d’un module de développement logiciel. L’objectif pédagogique est triple :

1. **Modéliser** un jeu tour‑par‑tour (gestion des états, ordres différés, résolution simultanée).
2. **Mettre en œuvre** les patterns fondamentaux client‑serveur avec ASP.NET Core (API REST + WebSocket/SignalR pour le temps réel).
3. **Appliquer** un cycle de développement professionnel : tests automatisés, intégration continue, documentation versionnée.

> **Public cible :** étudiants et enseignants évaluant la conception logicielle. Ce prototype n’est **pas** destiné à une exploitation en production.

---

## 2. Fonctionnalités clés

| État | Fonctionnalité                | Description                                                                 |
| ---- | ----------------------------- | --------------------------------------------------------------------------- |
| ✅    | Système de comptes            | Inscription, connexion et gestion de session JWT                            |
| ✅    | File d’attente de matchmaking | Aligne les joueurs puis démarre la partie dès que le quorum est atteint     |
| ✅    | Moteur de jeu côté serveur    | Agrège les inputs, résout la logique du tour, renvoie l’état consolidé      |
| ✅    | Transmission temps réel       | API REST + WebSocket/SignalR pour diffuser le nouvel état à chaque tour     |
| ✅    | Rendu & animations client     | Script JS interprétant l’état reçu et jouant les animations Canvas          |
| 🔧   | Fin de partie                 | **Conditions de victoire alternatives** (score, élimination…) au‑delà des 6 tours |
| 🔧   | IA basique                    | Robot adversaire pour jouer hors‑ligne                                      |
| 🔧   | Tableau de scores persistant  | Stockage SQLite / MariaDB + endpoint `/api/scores`                          |

---

## 3. Installation & Prérequis

### 3.1 Prérequis système

- **.NET SDK ≥ 9.0 (pré‑version)** – téléchargeable sur <https://dotnet.microsoft.com>.
- **MariaDB ≥ 10.6** ou **MySQL ≥ 8** (serveur local ou conteneur Docker).  
  _Un script SQL est fourni pour créer la base et les comptes._

### 3.2 Initialisation de la base de données

```sql
-- scripts/setup_db.sql
CREATE DATABASE IF NOT EXISTS robostrike;
USE robostrike;

CREATE TABLE IF NOT EXISTS Users (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  Username VARCHAR(255) NOT NULL UNIQUE,
  Email VARCHAR(255) NOT NULL UNIQUE,
  Is_email_validated BOOLEAN NOT NULL DEFAULT FALSE,
  PasswordHash VARCHAR(255) NOT NULL,
  Salt VARCHAR(255) NOT NULL,
  Points INT NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS Sessions (
  SessionId VARCHAR(100) PRIMARY KEY,
  UserId INT NOT NULL,
  CreatedAt DATETIME NOT NULL,
  ExpiresAt DATETIME NOT NULL,
  IsActive BOOLEAN NOT NULL DEFAULT TRUE,
  FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE USER IF NOT EXISTS 'user'@'localhost' IDENTIFIED BY 'password123';
GRANT SELECT, INSERT, UPDATE, DELETE ON robostrike.* TO 'user'@'localhost';
FLUSH PRIVILEGES;
```

Exécution :

```bash
mariadb -u root -p < scripts/setup_db.sql
```

> **Sécurité :** changez immédiatement le mot de passe `password123` pour votre environnement.

### 3.3 Installation rapide du projet

```bash
# Cloner le dépôt
git clone https://github.com/<organisation>/<repo>.git
cd <repo>

# Restauration des dépendances (obligatoire)
dotnet restore

# Lancement direct du serveur API
cd src/GameServer
dotnet run
```

> ⚠️ La compilation explicite (`dotnet build`) est optionnelle ; **Visual Studio / VS Code** ou **Rider** géreront la build à l’exécution. En ligne de commande, `dotnet run` restaure, compile et exécute d’un seul coup.

---

## 4. Configuration

Chaque projet possède un fichier `appsettings.json`. Adaptez‑le à votre moteur de base de données :

```json
{
  "ConnectionStrings": {
    "Main": "Server=localhost;Port=3306;Database=robostrike;User=user;Password=PASSWORD_ICI;"
  },
  "GameOptions": {
    "Port": 5181
  }
}
```

### Problème de port : API ≠ 5181

Sur certains systèmes, le **serveur API** peut se lancer sur un port aléatoire si **5181** est occupé. Avant de démarrer le client web :

1. Lancez `dotnet run --project src/GameServer` et notez le port réel affiché dans la console.
2. Si le port n’est pas **5181**, modifiez les chemins suivants pour le refléter :
    - `WebServer/Components/Pages/Game.razor`
    - `WebServer/Components/Pages/Hub.razor`

> Une amélioration future consistera à externaliser ce port dans un fichier de config global.

---

## 5. Guide d’utilisation

### Démarrage rapide (locaux)

```bash
# Server
cd src/GameServer
dotnet run

# Client (Blazor WebAssembly)
cd src/GameClient
npm install && npm run dev
```

### Scénario principal

1. **Créer un lobby** :
   ```http
   POST /api/lobbies { "name": "Demo" }
   ```
2. **Rejoindre** et **envoyer des ordres** via WebSocket :
   ```js
   const socket = new WebSocket("ws://localhost:5173/ws/lobby/1");
   socket.send(JSON.stringify({ command: "Move", direction: "North" }));
   ```

---

## 6. API / Référence

> Tous les endpoints ci‑dessous exigent l’entête HTTP `Authorization: Bearer <token>` ; le **Middleware** extrait `UserId` depuis le JWT et l’injecte dans `HttpContext.Items`.

### 6.1 Matchmaking

| Méthode | Route                           | Description                                   | Corps / Query | Réponse 200 |
|---------|---------------------------------|-----------------------------------------------|---------------|-------------|
| `GET`   | `/api/matchmaking/join`         | Place le joueur dans la file d’attente ou renvoie l’état de la partie en cours. | — | `{ Message:"Queued successfully" }` ou `{ Status: { … } }` |
| `GET`   | `/api/matchmaking/leave`        | Retire le joueur de la file d’attente.        | — | `{ Message:"Dequeued successfully" }` |
| `GET`   | `/api/matchmaking/status`       | Long‑polling : notifie quand la partie démarre ou expire après 30 s. | — | `{ Status:"<gameId>" }` ou `{ Status:"No updates" }` |

### 6.2 Jeu

| Méthode | Route                                   | Description                                       | Corps JSON                   | Réponse 200 |
|---------|-----------------------------------------|---------------------------------------------------|------------------------------|-------------|
| `PUT`   | `/api/game/{gameId}/inputs`             | Soumet les **6 premiers** inputs du joueur pour le tour en cours. | "FFLRRB" (string) | "Moves Submitted" |
| `GET`   | `/api/game/{gameId}/status`             | Récupère l’état courant : round, drapeau `GameOver`. | — | `{ Status:{ Round:{ … }, GameOver:false } }` |

### 6.3 Carte

| Méthode | Route  | Description                               | Corps JSON (extrait)   | Réponse 200 |
|---------|--------|-------------------------------------------|------------------------|-------------|
| `PUT`   | `/map` | Crée ou met à jour une carte personnalisée. | `{ "Width":10, "Height":10, "tiles":[…] }` | `{ Message:"Map received successfully", MapDetails:{ … } }` |

---

## 7. Tests

### Exécuter la suite de tests

Le projet de tests se trouve dans **`TestFinalCSharpProject/`** ; exécutez‑le ainsi :

```bash
# Depuis la racine du dépôt
cd TestFinalCSharpProject

dotnet test --filter FullyQualifiedName~UnitTest1
```

> `UnitTest1.cs` contient les tests unitaires de référence ; vous pouvez lancer **tous** les tests du projet simplement avec `dotnet test`.

### Analyse statique & couverture

- **dotnet format** : style de code C# (`dotnet format` à la racine)
- **Coverlet** : rapport de couverture (`dotnet test /p:CollectCoverage=true`)

---

## Licence & Crédits


> © 2025 — Li Léo, Boukaouma Mokrane, Ciorba Antonio, Barois Ulysse
