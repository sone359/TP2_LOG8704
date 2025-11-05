# AR Project — Instructions de Build & Tests

## Cloner Repo

Il faut en premier lieu cloner le repo github.
Une fois le repo git cloné, il faut s'assuré d'être sur la branche dev-no-vuforia, du à changement de library et de dépendance, la main est brisé.
Par la suite, ouvrir Unity Hub, Add -> Add project from disk et sélectionner le dossier TP2_LOG8704.

La editor version utilisé dans ce projet est 6000.0.54f1

## 🎯 Scène à builder
La scène à sélectionner pour tester le projet est :

**`Main1`**

## 📱 Plateforme testée
Le projet a principalement été testé sur :

- **iOS**
  - Appareil : **iPhone 15 Pro**
  - Build effectué sur **macOS** via **Xcode**
  - Version iOS utilisée : **26.1.0**

## 🛠️ Instructions de build iOS (Unity + Xcode)

1. **Sélectionner la scène**
   - File → Build Settings → cocher **Main1**

2. **Sélectionner la plateforme iOS**
   - File → Build Settings → **iOS**

3. **Lancer le build Unity**
   - Cliquer **Build**
   - Choisir/créer un dossier `build`  
   Unity va générer un projet Xcode.

4. **Préparer le projet dans Xcode**
   - Ouvrir le dossier `build` Unity dans **Xcode**
   - Aller dans **Signing & Capabilities**
   - Cocher **Automatically manage signing**
   - Sélectionner votre **Personal Team** (compte Apple développeur gratuit)

> ⚠️ Un compte développeur gratuit limite l'installation à **3 apps** sur l’iPhone.

5. **Nettoyer et recompiler**
   - `Product → Clean Build Folder`
   - Relancer le build

### ✅ Fix d'erreur possible
Si une erreur liée au linker apparaît: 

<img width="850" height="180" alt="image" src="https://github.com/user-attachments/assets/0223b544-1db7-4a93-ab72-5fb41ded5694" />

, enlever le flag `-ld64` dans :

- **Unity-iPhone → Build Settings**
- **UnityFramework → Build Settings**

6. **Installer sur iPhone**
   - Brancher l’iPhone au Mac
   - L'installation se fait automatiquement après avoir relancer le build

## ℹ️ Notes utiles
- L'app doit être déployée depuis Xcode sur l’iPhone
- Si Xcode refuse de compiler, refaire **Clean Build Folder**
