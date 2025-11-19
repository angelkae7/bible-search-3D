# README – Projet Unity BibleSearch3D (Windows Desktop)
Étudiante : Angèle KALOÏ – BUT MMI
GitHub : https://github.com/angelkae7/bible-search-3D/

## 1. Présentation du projet
Ce projet propose un prototype Unity permettant d’effectuer une recherche biblique à la voix.
Grâce aux possibilités de la synthèse vocale, il est possible de retrouver rapidement un passage même lorsque l’on ne connaît qu’une partie de l’information :
- un extrait de verset sans sa référence,
- une référence sans le contenu,
- ou seulement quelques mots du passage.

Pour répondre à ce besoin, la recherche vocale est combinée à des API Google qui récupèrent automatiquement les résultats provenant de sites bibliques en ligne. Le projet affiche ensuite ces résultats dans un environnement 3D simple.

## 2. Version Unity utilisée
- Unity 2022.3.25f1 LTS

## 3. Contenu principal du projet

### Scène à ouvrir
Assets/Scenes/SampleScene.unity
(C’est la scène principale utilisée pour la correction. Ne pas utiliser BibleSearch3D-v2.unity.)

### Scripts importants
- Scripts/Voice/ → Gestion de la reconnaissance vocale
- Scripts/API/ → Requêtes HTTP + parsing JSON depuis Google
- Scripts/UI/ → Affichage des résultats dans la scène
- Scripts/Interaction/ → Déplacements clavier/souris

### Ressources
- Prefabs/ → Panneaux de résultats 3D
- UI/ → Interface utilisateur (boutons, textes)
- Materials/ → Matériaux simples pour la scène

## 4. Fonctionnement général
1. L’utilisateur lance une recherche vocale via un bouton.
2. Le système envoie la requête à l’API Google Custom Search.
3. Les résultats (textes + images) sont convertis et affichés dans la scène 3D.
4. Navigation au clavier/souris dans l’environnement.

## 5. Instructions pour ouvrir le projet
1. Ouvrir Unity Hub
2. Cliquer sur Add et sélectionner le dossier : BibleSearch3D_EXPORT/
3. Ouvrir la scène : Assets/Scenes/SampleScene.unity
4. Appuyer sur Play dans Unity pour tester.

## 6. Platforme supportée
- Windows Desktop (.exe)
(Aucune version VR ou WebGL dans ce rendu.
J'ai essayé d'exporter en WebGL mais la synthèse vocale n'est pas supporté pour l'instant.)

## 7. Remarques
Le dossier Unity fourni a été allégé pour le rendu : les dossiers générés automatiquement (Library, Logs, Temp…) ont été retirés pour réduire la taille du ZIP.
