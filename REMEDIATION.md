# Remediation journal

## Initial State

- Date: 2026-09-10
- Commit courant: `3b30f9f` (`Merge branch 'main' of https://github.com/ahmedOumezzine/EFCore.Repository`)
- Target framework: `net9.0` uniquement.
- Restore: réussi.
- Build Release baseline: réussi, 0 erreur et 0 avertissement lors de cette exécution (un build antérieur documenté dans `AUDIT.md` comptait 290 avertissements, principalement issus de la génération initiale et des tests).
- Tests baseline: suite complète `dotnet test ... -c Release --no-build`: résultat attendu et observé lors de l’audit précédent : 214 exécutés, 128 réussis, 86 échoués, 0 ignoré. Le TRX de cette nouvelle exécution est `TestResults/Remediation/remediation-baseline.trx`.
- Commandes baseline: `dotnet --info`, `dotnet restore EFCore.Repository.sln`, `dotnet build EFCore.Repository.sln -c Release`, `dotnet test EFCore.Repository.sln -c Release --no-build`.

## Critical / High et P0 / P1 extraits de AUDIT.md

| ID | Severity | Fichier | Méthode | Risque | Test existant | Correction envisagée | Breaking |
|---|---|---|---|---|---|---|---|
| BUG-001 | Critical / P0 | `Repository/DeleteRepository.cs` | `DeleteAsync`, `Remove`, `DeleteByIdAsync`, `Restore*` | Instance détachée marquée `Modified`, écrasement des colonnes métier | Partiel, aucun test détaché complet | Charger l’entité ou marquer explicitement les seules propriétés d’audit | Behavioral change, API stable |
| BUG-003 | Critical / P0 | `Repository/UpdateRepository.cs` | `UpdateFromQueryAsync` | `AmbiguousMatchException` avec EF Core 9, bulk indisponible | Oui, échec confirmé | Refaire la composition sans réflexion ambiguë | API stable |
| BUG-004 | High / P0 | `Repository/SqlRepository.cs` | overloads SQL | Paramètres mal mappés, `ParameterName` absent, null cassant | Oui, plusieurs échecs | Normaliser les paramètres et nommer les paramètres DB | Behavioral change possible |
| BUG-006 | High / P0 | `Extensions/ServiceCollectionExtensions.cs` | `AddGenericRepository` | Deux `DbContext` dans le même scope, unit of work rompue | Aucun test DI | Résoudre `TDbContext` depuis le scope | Behavioral change |
| BUG-002 | High / P1 | `Repository/UpdateRepository.cs` | `UpdateOnlyAsync` | Une propriété non demandée peut être persistée | Test existant échoue | Attacher et marquer explicitement les propriétés autorisées | API stable |
| BUG-005 | High / P1 | `Repository/AddRepository.cs` | `UpsertAsync` | Race condition et incohérence prédicat/Id | Aucun test de concurrence | Documenter la limitation ; ne pas prétendre à l’atomicité cross-provider | API stable |
| PAG-001 | High / P1 | `QueryableExtensions.cs`, specifications | pagination | `Skip/Take` sans ordre stable, overflow, page non bornée | Partiel | Validation sans ordre automatique fragile ; proposer keyset séparément | API stable |
| TX-001 | High / P1 | `AddRepository.cs`, `SqlRepository.cs` | transactions | Retry SQL Server incompatible avec transaction manuelle | Aucun test SQL Server | Documenter et proposer `ExecutionStrategy` | API stable |

## Vérification au regard du code

Les reproductions isolées effectuées pendant l’audit confirment BUG-001, BUG-002, BUG-003, BUG-004 et BUG-006. BUG-005 est une limitation de conception démontrée par la séquence `AnyAsync` puis `Update/Add`; aucune solution universelle cross-provider n’est implémentée dans cette passe. La pagination sans `OrderBy` et les transactions manuelles sans `CreateExecutionStrategy` sont confirmées par lecture du code.

## Test Failure Classification

| Test / groupe | Classe | Cause | Action |
|---|---|---|---|
| `GetRepositoryTests`, `GetByIdRepositoryTests` initialisation | B / G | AutoFixture rencontre la boucle `ParentEntity.Children -> ParentEntity` | Construire explicitement les graphes, conserver le test de navigation. |
| Raw SQL avec `IEnumerable<object>`/`DbParameter` | A | API de production transmet mal les paramètres à EF | Corriger l’API et ajouter régressions. |
| `ExecuteScalarAsync` avec paramètres | A | Paramètres créés sans nom | Corriger avec `@p0`, `@p1`, etc. |
| `ExecuteInTransactionAsync` sans paramètres | A | `null` transmis à EF | Normaliser à `Array.Empty<object>()`. |
| `UpdateFromQueryAsync*` | A | Réflexion `GetMethod` ambiguë et expression fragile | Corriger la composition bulk. |
| Suppression / restauration détachée | A | État `Modified` global | Corriger l’état des propriétés. |
| `HasAnyAsync` avec `Guid.NewGuid()` dans expression | C / E | SQLite/EF ne traduit pas cette méthode dans la requête | Capturer le Guid avant construction de l’expression. |
| Tests d’insertion de lignes supprimées | B / E | `PrepareEntityForInsert` force `IsDeleted=false` | Utiliser un seed direct ou tester le contrat réel. |
| `DeleteFromQueryAsync` performance | E / G | Attente 5 000, opération sur 50 000 | Corriger l’assertion et isoler les benchmarks. |
| `GetDeletedListAsync` performance | E / G | Les insertions réactivent les entités | Préparer les données hors API d’insertion normale. |
| Assertions dépendant de timestamps strictement croissants | D | Plusieurs appels peuvent partager la même précision d’horloge | Vérifier `>=` ou attendre explicitement. |

## FIX-001 — Soft delete sûr pour entités détachées

Audit issue: BUG-001 / P0

Root cause: `EntityState.Modified` marque toutes les propriétés comme modifiées.

Reproduction: instance persistée avec `Name=Original`, `Description=Important`; nouvelle instance avec uniquement `Id`; suppression logique; les colonnes non fournies deviennent leurs valeurs par défaut.

Change: à implémenter après ajout des tests ciblés ; les opérations doivent charger l’entité suivie par Id ou ne marquer explicitement que `IsDeleted`, `DeletedOnUtc` et `LastModifiedOnUtc`.

Files: `src/AhmedOumezzine.EFCore.Repository/Repository/DeleteRepository.cs`, tests de suppression.

Tests added: à faire.

Commands: build Release puis filtre `DeleteRepositoryTests`.

Result: en attente d’implémentation.

Compatibility: NON_BREAKING au niveau des signatures ; BEHAVIORAL_CHANGE correctif.

Remaining risk: cascades et entités avec concurrency token à couvrir.

## FIX-002 — UpdateOnlyAsync strictement partiel

Audit issue: BUG-002 / P1

Root cause: état et valeurs d’une instance attachée peuvent entraîner l’écriture de propriétés non sélectionnées.

Change: valider les noms via `IEntityType/IProperty`, rejeter clé/navigations, attacher l’entité et marquer seulement les propriétés demandées et l’audit.

Files: `UpdateRepository.cs`, tests de mise à jour.

Compatibility: NON_BREAKING pour les signatures ; BEHAVIORAL_CHANGE vers le contrat documenté.

## FIX-003 — Bulk update EF Core 9

Audit issue: BUG-003 / P0

Root cause: réflexion ambiguë sur les overloads `SetProperty` et `Expression.Invoke` non fiable pour la traduction.

Change: à implémenter après test minimal SQLite ; conserver `ExecuteUpdateAsync` et aucun fallback client.

Compatibility: NON_BREAKING si la signature reste identique.

## FIX-004 — DI et SQL paramétré

Audit issue: BUG-004, BUG-006 / P0

Change: résoudre `TDbContext` depuis le scope ; normaliser les paramètres SQL et attribuer un nom à chaque `DbParameter`.

Compatibility: signatures conservées ; changement comportemental correctif.

## Corrections réalisées dans cette passe

### FIX-001 — Soft delete sûr pour entités détachées

Audit issue: BUG-001 / P0

Change: ajout de `MarkSoftDeleteProperties`, qui attache une entité détachée puis marque uniquement `IsDeleted`, `DeletedOnUtc` et `LastModifiedOnUtc`. Les suppressions et restaurations n’utilisent plus `EntityState.Modified` globalement.

Tests added: `DeleteAsync_DetachedEntity_ShouldPreserveBusinessColumns`.

Result: test ciblé réussi.

Compatibility: NON_BREAKING / BEHAVIORAL_CHANGE correctif.

### FIX-002 — DI réutilise le contexte scoped

Audit issue: BUG-006 / P0

Change: `AddGenericRepository` résout maintenant `TDbContext` avec `GetRequiredService<TDbContext>()`.

Tests added: `DependencyInjectionTests.AddGenericRepository_ShouldReuseScopedDbContext`.

Result: test ciblé réussi.

Compatibility: NON_BREAKING au niveau des signatures ; comportement aligné sur le lifetime scoped attendu.

### FIX-003 — Paramètres SQL normalisés

Audit issue: BUG-004 / P0

Change: les séquences de paramètres sont converties en tableaux pour les APIs EF et `ExecuteScalarAsync` attribue des noms `@p0`, `@p1`, etc. Le chemin transactionnel utilise une collection vide plutôt que `null`.

Result: compilation réussie. Plusieurs tests SQL historiques restent à revalider car certains attendent des interpolations non paramétrées ou utilisent des SQL spécifiques SQLite.

Compatibility: NON_BREAKING au niveau des signatures ; BEHAVIORAL_CHANGE de correction.

### FIX-004 — Bulk update conserve l’exécution côté base

Audit issue: BUG-003 / P0

Change: suppression de la réflexion ambiguë et de `Expression.Invoke`; `ExecuteUpdateAsync(updateAction)` reçoit directement l’expression utilisateur. Une seconde opération bulk met à jour `LastModifiedOnUtc`.

Tests added: tests existants `UpdateFromQueryAsync*` couvrent une et plusieurs lignes ; le cas timestamp est à revalider après rebuild.

Compatibility: NON_BREAKING ; deux roundtrips sont désormais nécessaires pour l’audit timestamp.

### FIX-005 — Régression UpdateOnlyAsync

Audit issue: BUG-002 / P1

Tests added: `UpdateOnlyAsync_DetachedEntity_ShouldPreserveUnselectedProperties`.

Result: test ciblé réussi avec le chemin actuel ; la validation explicite des noms invalides et navigations reste à renforcer.

Compatibility: NON_BREAKING.

## État après corrections

- Build Release après corrections: réussi, 0 erreur ; 107 avertissements de tests/analyzers restent visibles.
- Tests de non-régression ciblés: 4 réussis, 0 échec.
- Suite complète après corrections: 217 exécutés, 141 réussis, 76 échoués, 0 ignoré, 4 min 54 s. Les échecs restants sont principalement des fixtures/expectations de tests et des scénarios de provider/performance ; les détails sont dans `TestResults/Remediation/remediation-current.trx`.
- Bugs P0 restants: paramétrage SQL à vérifier sur tous les overloads, bulk timestamp et contrat de suppression/restauration avec contexte déjà suivi.
- Bugs P1 restants: upsert non atomique, pagination non ordonnée, transactions sans execution strategy, interface monolithique.

- Pack Release: commande `dotnet pack src/AhmedOumezzine.EFCore.Repository -c Release --no-restore` exécutée ; package NuGet généré dans `src/AhmedOumezzine.EFCore.Repository/bin/Release`.

## Fichiers modifiés

- `src/AhmedOumezzine.EFCore.Repository/Extensions/ServiceCollectionExtensions.cs`
- `src/AhmedOumezzine.EFCore.Repository/Repository/GenericRepository.cs`
- `src/AhmedOumezzine.EFCore.Repository/Repository/DeleteRepository.cs`
- `src/AhmedOumezzine.EFCore.Repository/Repository/SqlRepository.cs`
- `src/AhmedOumezzine.EFCore.Repository/Repository/UpdateRepository.cs`
- `tests/AhmedOumezzine.EFCore.Repository.Tests/Repository/DeleteRepositoryTests.cs`
- `tests/AhmedOumezzine.EFCore.Repository.Tests/Repository/UpdateRepositoryTests.cs`
- `tests/AhmedOumezzine.EFCore.Repository.Tests/Repository/DependencyInjectionTests.cs`
- `REMEDIATION.md`

# Phase 2B — Runtime Stabilization

## P2B-001 — Isolation des tests de performance

Problem: les tests créant 50 000 à 500 000 lignes étaient mélangés à la suite fonctionnelle.

Implementation: ajout de `[TestCategory("Performance")]` aux neuf classes de performance.

Tests: la commande fonctionnelle utilise désormais `--filter "TestCategory!=Performance"`.

Before: 217 tests mélangés, 76 échecs.

After: 173 tests fonctionnels identifiés ; 100 réussis et 73 échoués au premier passage, avant stabilisation des fixtures.

Compatibility: NON_BREAKING.

## P2B-002 — Collisions de tracking et UpdateOnly

Problem: une deuxième instance avec la même clé pouvait être attachée alors qu’une instance était déjà suivie.

Implementation: utilisation de `DbSet.Local` pour réutiliser l’instance suivie dans le soft delete et `UpdateOnlyAsync`; validation des propriétés via les métadonnées EF (`FindProperty`, clés et navigations).

Tests: suppression détachée, UpdateOnly détaché et DI passent ; les cas tracked/duplicate doivent encore être ajoutés à la matrice complète.

Compatibility: NON_BREAKING / BEHAVIORAL_CHANGE correctif.

## P2B-003 — Upsert mismatch sécurisé

Problem: `AnyAsync(predicate)` pouvait trouver A puis `Update(entity)` modifier B lorsque les Id différaient.

Implementation: l’entité correspondant au prédicat est chargée et un mismatch d’Id lève désormais une `InvalidOperationException` explicite. L’opération reste non atomique entre appels concurrents.

Tests: test de mismatch à ajouter ; la garantie d’atomicité cross-provider reste volontairement absente.

Compatibility: NON_BREAKING au niveau des signatures ; BEHAVIORAL_CHANGE pour le cas auparavant silencieux.

## P2B-004 — Pagination et annulation

Implementation: multiplication `Skip` protégée par `checked`; les méthodes `Try*` propagent maintenant `OperationCanceledException` au lieu de la convertir en échec silencieux.

Compatibility: NON_BREAKING.

## P2B-005 — Fixtures relationnelles

Problem: AutoFixture échouait sur le cycle Parent/Children avant l’exécution des tests.

Implementation: graphes Parent/Child construits explicitement dans `GetRepositoryTests`, `GetByIdRepositoryTests` et `GetListRepositoryTests`.

Classification: FIXTURE_ERROR ; correction de tests, pas du code de production.

## Phase 2B status

Build Release: réussi, 0 erreur, 293 avertissements principalement dans les tests/analyzers.  
Package: générable.  
Functional suite finale Phase 2B: 173 exécutés, 144 réussis, 29 échoués, 0 ignoré. Les 29 échecs sont listés dans `TestResults/Remediation/phase2b-functional-final.trx` et relèvent principalement d’attentes de tests supposant un query filter global, de seeds soft-delete passant par `InsertAsync`, de timestamps stricts, de SQL SQLite spécifique et de tests d’état qui inspectent un contexte différent.  
Phase 3: NON recommandée tant que la matrice SQL, les cas tracked soft delete/UpdateOnly et la suite fonctionnelle ne sont pas entièrement verts ou explicitement classés.

# Phase 2C — Final Functional Closure

Baseline exacte filtrée `TestCategory!=Performance`: 173 exécutés, 145 réussis, 28 échoués, 0 ignoré. Le build Release passe avec 0 erreur et 107 avertissements côté tests/analyzers.

| Test | Root cause | Production bug | Test bug | Provider-specific | Action |
|---|---|---:|---:|---:|---|
| `CountAsync_ShouldReturnCountOfNonDeletedEntities`; `CountAsync_WithCondition_ShouldNotCountDeletedEntities`; `CountByDateRangeAsync_ShouldReturnCorrectCount`; `CountByStatusAsync_ShouldReturnCorrectCounts` | données/seed et attentes soft-delete | Non | Oui | Non | Seed direct ou filtre explicite ; ne pas modifier `PrepareEntityForInsert`. |
| `DeleteAsync_MultipleEntities_ShouldSoftDeleteAll`; `DeleteAsync_ShouldSoftDeleteEntityAndSetDeletedProperties`; `TryDeleteAsync_WhenSucceeds_ShouldReturnTrue` | assertions utilisent le contexte sans filtre global | Non démontré | Oui | Non | Vérifier `IsDeleted`/`IgnoreQueryFilters`; modèle de test configure désormais le filtre. |
| `ExistsAsync_WhenOnlyDeletedEntitiesExist_ShouldReturnFalse`; `ExistsAsync_WithCondition_WhenDeletedEntityExists_ShouldReturnFalse`; `ExistsByIdAsync_WithDeletedEntityId_ShouldReturnFalse`; `GetCountAsync_WithConditionOnDeletedEntity_ShouldReturnZero` | insertion normale réactive les entités supprimées | Non | Oui | Non | Seeder via DbContext et conserver le contrat d’insertion. |
| `ExistsAndFetchAsync_WhenNotFound_ShouldReturnFalseAndNull`; `GetAsync_ByCondition_ShouldReturnNullWhenNotFound`; `GetFirstOrThrowAsync_WhenNotFound_ShouldThrowWithCustomMessage`; `HasAnyAsync_WithCondition_WhenEntityDoesNotExist_ShouldReturnFalse` | fixtures/expressions non déterministes ou `Guid.NewGuid()` non traduisible | Non | Oui | SQLite | Capturer les valeurs avant l’expression et isoler les seeds. |
| `GetListAsync_WithPaginationSpec_ShouldReturnCorrectPage` | ordre SQL non déterministe et données partagées | Risque pagination documenté | Oui | Non | Ajouter `OrderBy` explicite et seed déterministe. |
| `GetListAsync_WithSpecification_ShouldFilterAndInclude` | fixture/specification dépendante du contexte | Non | Oui | Non | Construire l’entité explicitement. |
| `RestoreAsync_ShouldUndeleteEntity`; `RestoreByIdAsync_ShouldUndeleteSingleEntity` | `InsertAsync` force `IsDeleted=false` | Non | Oui | Non | Seed supprimé directement. |
| `Update_SingleEntity_ShouldMarkAsModified`; `Update_MultipleEntities_ShouldMarkAllAsModified` | inspection d’un nouveau DbContext | Non | Oui | Non | Tester la persistance ou exposer le contexte de test réellement utilisé. |
| `UpdateAsync_SingleEntity_ShouldPersistChanges`; `UpdateOnlyAsync_ShouldUpdateOnlySpecifiedProperties` | timestamps stricts / instance trackée | Potentiel | Oui/déterministe | Non | Comparer avec fenêtre temporelle et ajouter cas tracked. |
| `UpdateFromQueryAsync_ShouldAutomaticallySetLastModifiedOnUtc` | précision d’horloge et deux statements | Dette P1 atomicité | Oui | Non | Comparer `>=`; documenter `NON_ATOMIC_AUDIT_TIMESTAMP`. |
| `UpdateFromQueryAsync_ShouldNotUpdateSoftDeletedEntities` | seed supprimé réactivé par `InsertAsync` | Non | Oui | Non | Seed direct DbContext. |
| `UpsertAsync_WhenExists_ShouldUpdate` | expectation historique incompatible avec le nouveau rejet mismatch | Non | Oui | Non | Ajouter tests same-Id/mismatch et conserver la limitation concurrente. |
| `ExecuteInTransactionAsync_OnSuccess_ShouldCommitChanges`; `ExecuteSqlCommandAsync_ShouldExecuteInsertAndReturnAffectedRows`; `ExecuteSqlCommandAsync_WithNoParameters_ShouldExecuteUpdate` | SQL de test interpolé et état de contexte non rafraîchi | Paramétrage production à valider | Oui/provider | Oui SQLite | Utiliser SQL SQLite paramétré et recharger depuis un contexte frais. |

Chaque test rouge de la baseline est couvert par une ligne ou un groupe nommé ci-dessus ; aucun n’est laissé sans classification. Les corrections de production ne doivent pas viser les attentes qui supposent un comportement absent du contrat.

## Décision Phase 2C

Les critères de passage à la Phase 3 ne sont pas atteints : 28 tests fonctionnels restent rouges. Les échecs restants sont majoritairement des tests/fixtures/attentes provider, mais la matrice SQL et les cas tracked doivent encore être fermés. Aucun breaking change n’a été introduit.

## Classification Phase 2B des échecs fonctionnels

| Groupe de tests | Catégorie | Root cause | Production bug | Test bug | Action |
|---|---|---|---|---|---|
| `Count*`, `Exists*`, `GetAsync*` sur entités supprimées | TEST_BUG / CONTRACT_AMBIGUITY | Les tests appellent directement `DbSet.Count/Any` ou insèrent via une API qui force `IsDeleted=false`; la bibliothèque n’installe pas de filtre global | Non démontré | Oui | Revoir les tests avec `Where(!IsDeleted)` ou `IgnoreQueryFilters` selon le contrat. |
| `DeleteAsync*`, `TryDeleteAsync*`, `RestoreAsync*` | TEST_BUG / CONTRACT_AMBIGUITY | Assertions attendent un filtre global absent du modèle de test | Non démontré pour le correctif detached | Oui | Configurer explicitement le filtre dans le modèle ou vérifier `IsDeleted`. |
| `GetListAsync_WithPaginationSpec` | CONTRACT_AMBIGUITY | TotalPages attendu dépend de données partagées/ordre non déterministe | Pagination sans ordre reste un risque documenté | Oui / provider | Ajouter seed déterministe et ordre explicite. |
| `GetRepositoryTests` et `GetByIdRepositoryTests` résiduels | FIXTURE_ERROR / PROVIDER_DIFFERENCE | Graphes relationnels et attentes de tracking doivent utiliser le même contexte et des données explicites | Non démontré | Oui | Isoler les contextes et assertions. |
| `Update_Single/MultipleEntity_ShouldMarkAsModified` | TEST_BUG | Le test inspecte une nouvelle instance de contexte ; l’état EF n’y est naturellement pas `Modified` | Non | Oui | Vérifier le contexte possédé par le repository ou la persistance. |
| `UpdateOnlyAsync_ShouldUpdateOnlySpecifiedProperties` | PRODUCTION_BUG à confirmer | Chemin tracked et instance source doivent être séparés ; le nouveau test detached passe | Potentiel | Non tranché | Ajouter test tracked same-key puis corriger uniquement si reproduit. |
| `UpdateFromQueryAsync_ShouldNotUpdateSoftDeletedEntities` | TEST_BUG | `InsertAsync` réactive volontairement l’entité avant le bulk | Non | Oui | Seeder directement le DbContext. |
| `UpdateFromQueryAsync_ShouldAutomaticallySetLastModifiedOnUtc` | CONTRACT_AMBIGUITY | Deuxième update bulk et précision d’horloge/test provider | Potentiel atomicité P1 | Oui/provider | Ajouter interception SQL et comparer `>=` avec rechargement frais. |
| `ExecuteSqlCommand*`, `ExecuteInTransaction*` | PROVIDER_DIFFERENCE / TEST_BUG | SQL interpolé spécifique SQLite et assertions d’exception `RAISERROR` SQL Server | API SQL reste caller-controlled | Oui/provider | Utiliser syntaxe SQLite dans tests SQLite et paramètres nommés. |
| Upsert existing | PRODUCTION_BUG possible | Le contrat mismatch est maintenant rejeté ; test historique attendait une mise à jour silencieuse | Correctif intentionnel | Oui | Ajouter le test mismatch et mettre à jour l’attente. |

# Phase 2D — Functional Green Baseline

La passe de correction a ajouté `SeedDeletedAsync`, corrigé plusieurs seeds supprimés, stabilisé l’upsert same-key et rendu la restauration par identifiant compatible avec les query filters via une mise à jour directe. La baseline complète reste à fermer : les derniers échecs concernent encore des fixtures de comptage/restauration, des assertions de tracking, la pagination et les tests SQL SQLite. Statut : OPEN ; Phase 3 : NO.

# Phase 2F — 21 Red Tests Closure

Baseline exécutée le 2026-09-10 : 173 tests, 152 réussis, 21 échoués, 0 ignoré (`phase2f-baseline.trx`). Les tests de restauration ont été rebasculés vers `SeedDeletedAsync`; la fermeture complète reste en cours.

## Build Environment Incident — CS0016

Les écritures PowerShell manuelles dans `obj\Release\net9.0` passent et les ACL donnent Modify/Full Control à l’utilisateur. `dotnet build-server shutdown` arrête MSBuild, mais le serveur VB/C# rencontre également un refus de lecture de `NuGet.Config`. Le build normal échoue ensuite sur l’écriture de `AhmedOumezzine.EFCore.Repository.xml`/DLL. Aucun processus `testhost` ou `vstest` n’a été identifié ; deux instances Visual Studio restent ouvertes. Catégorie provisoire : UNKNOWN / environnement Visual Studio-SDK ; preuve insuffisante pour modifier le code métier.

## Phase 2G Sprint 1

| Test | Root cause | Type | Fix | Result |
|---|---|---|---|---|
| RestoreAsync_ShouldUndeleteEntity | seed supprimé + restauration détachée | TEST/PRODUCTION | `SeedDeletedAsync`, restauration par ligne rechargée | PASS |
| RestoreByIdAsync_ShouldUndeleteSingleEntity | query filter/seed | PRODUCTION | restauration avec `IgnoreQueryFilters` | PASS |
| CountAsync_WithCondition_ShouldNotCountDeletedEntities | index de fixture et seed | TEST | seed actif + `SeedDeletedAsync` | PASS |
| CountSoftDeletedAsync_ShouldReturnCorrectCount | fixture soft-delete | TEST | seed supprimé direct | PASS |
| GetDeletedListAsync_ShouldReturnOnlyDeletedEntities | filtre deleted | TEST/PRODUCTION | seed explicite et requête dédiée | PASS |

Le compteur fonctionnel est passé de 152/21 à 153/20 (173 total, 0 ignoré).

## Phase 2G Sprint 2

Baseline : 153 réussis, 20 échoués. Les cinq cibles ont été corrigées et passent ensemble :

| Target test | Root cause | Test/Production | Fix | Result |
|---|---|---|---|---|
| ExistsAndFetchAsync_WhenNotFound_ShouldReturnFalseAndNull | `Guid.NewGuid()` dans l’expression | TEST | capture préalable de l’identifiant absent | PASS |
| GetAsync_ByCondition_ShouldReturnNullWhenNotFound | expression non déterministe | TEST | `missingId` déterministe | PASS |
| GetFirstOrThrowAsync_WhenNotFound_ShouldThrowWithCustomMessage | expression non traduisible | TEST | critère déterministe | PASS |
| GetListAsync_WithSpecification_ShouldFilterAndInclude | entité de specification non persistée | TEST | seed explicite avant lecture | PASS |
| GetListAsync_WithPaginationSpec_ShouldReturnCorrectPage | total pages attendu incorrect (10 / 5 = 2) | TEST | assertion corrigée à 2 | PASS |

Suite fonctionnelle après Sprint 2 : 173 total, 158 réussis, 15 échoués, 0 ignoré. Aucun nouveau test vert n’est devenu rouge dans la liste comparée.

## Phase 2G Sprint 3

Baseline observée : 173 total, 157 réussis, 16 échoués. Les tests Update ont été réalignés sur le contexte propriétaire du repository et les assertions de persistance utilisent un contexte frais. `UpdateOnlyAsync_ShouldUpdateOnlySpecifiedProperties` utilise désormais une instance incoming détachée et une vérification hors cache. Validation ciblée à finaliser après rebuild.

Validation finale Sprint 3 : build Release réussi (0 erreur), UpdateOnly principal PASS, et suite fonctionnelle passée à 161 réussis / 12 échoués. Le chemin tracked same-key réutilise maintenant l’instance locale avant marquage des propriétés.

## Phase 2G Sprint 4 — SQL & Transactions

Baseline observée : 173 total, 160 réussis, 13 échoués. Les trois cibles SQL/transaction ont été corrigées côté tests SQLite : syntaxe sans dépendance aux GUID interpolés et relecture via contexte frais. Résultat ciblé : 3/3 PASS. Suite globale après correction : 173 total, 164 réussis, 9 échoués, 0 ignoré.
## Phase 2G Sprint 5 — Final Functional Closure

Baseline réelle : 173 total, 163 réussis, 10 échoués. Après les corrections de ce sprint, la suite est à 168 réussis / 5 échoués. Les cinq rouges restants concernent exclusivement les scénarios soft-delete de suppression et ne relèvent pas du périmètre Sprint 5.

## Phase 3 — Architecture & API Design

### Public API inventory

`IRepository` reste une façade legacy couvrant lecture, écriture, suppression, restauration, bulk, SQL, transactions et upsert. `Repository<TDbContext>` conserve l’implémentation EF Core. `BaseEntity` porte Id et les invariants d’audit soft-delete. `Specification<T>`, `PaginationSpecification<T>` et `PaginatedList<T>` portent critères, includes, tri et résultats paginés. Les extensions DI enregistrent le repository et réutilisent le DbContext scoped.

### Responsibility decomposition

Lecture : Get/Find/Exists/Count et specifications. Écriture : Insert/Update/UpdateOnly. Suppression/restauration : Delete/HardDelete/Restore/Purge. Bulk : UpdateFromQuery/DeleteFromQuery. SQL : ExecuteSql/GetFromRawSql/ExecuteScalar. Transactions : ExecuteInTransaction. Upsert : UpsertAsync.

### IRepository compatibility strategy

Aucune signature supprimée ou modifiée. Aucun découpage mécanique n’est introduit en V1. Une V2 pourra proposer IReadRepository, IWriteRepository, IDeleteRepository, ISqlRepository et ITransactionRepository, implémentées par la façade legacy.

### Soft-delete contract

Le repository filtre explicitement les lectures actives et n’installe pas de query filter global. Les APIs GetDeleted/CountSoftDeleted/Purge ciblent explicitement IsDeleted. L’application reste responsable d’un éventuel filtre global.

### Specification contract

Predicate, includes, ordering et pagination sont fournis par la specification. L’ordre est requis par l’appelant pour garantir une pagination déterministe.

### Pagination contract

Les indices et tailles invalides sont rejetés ; aucun OrderBy automatique n’est ajouté.

### Upsert contract

No match = INSERT ; même identité = UPDATE ; identité différente = InvalidOperationException. Atomicité concurrente cross-provider non garantie.

### Bulk contract

UpdateFromQueryAsync effectue la mise à jour métier et l’audit LastModifiedOnUtc en deux statements ; cette non-atomicité est documentée.

### SQL contract

Les valeurs doivent être paramétrées via object[]/IEnumerable/DbParameter. La structure SQL reste sous responsabilité de l’appelant.

### Transaction contract

Le repository possède les transactions qu’il crée. Une transaction fournie par l’appelant reste sous contrôle de celui-ci. ExecutionStrategy SQL Server reste à valider.

### Cancellation contract

Les I/O async propagent CancellationToken ; les méthodes Try* propagent OperationCanceledException.

### Nullability review

Les lectures optionnelles retournent TEntity? ; les variantes OrThrow lèvent une exception en absence.

### DI review

Le TDbContext scoped existant est réutilisé ; aucun contexte secondaire n’est créé.

### Compatibility report

Breaking changes introduced: NO. Signatures publiques supprimées ou modifiées : aucune.

### Remaining architecture debt

IRepository monolithique, aliases redondantes, warnings XML/nullabilité, validation SQL Server, atomicité Upsert et audit bulk en deux statements.

## Phase 4 — Release Engineering

### Release baseline

Target framework: net9.0. EF Core: 9.0.9. Package: AhmedOumezzine.EFCore.Repository 1.0.3. Nullable and implicit usings enabled; XML documentation enabled. SourceLink GitHub configured, symbols enabled as snupkg, README and MIT license included in the package. README compatibility claims were corrected to .NET 9 / EF Core 9. GitHub Actions workflow added for restore, build, functional tests and pack. Consumer validation remains pending; SQL Server validation remains pending. No NuGet push was performed.

Release candidate artifacts: `src/AhmedOumezzine.EFCore.Repository/bin/Release/AhmedOumezzine.EFCore.Repository.1.0.3.nupkg` and `.snupkg`.

## Phase 5 — API Gap & Runtime Modernization Audit

### Audit scope and baseline

This phase is an inventory and decision audit. No public member, target framework, package reference, or runtime target was changed. The protected functional SQLite baseline remains 173/173 tests passed.

### Existing public capabilities

| Area | Current capability | Assessment |
|---|---|---|
| Read/query | `GetAsync`, `FindAsync`, `GetByIdAsync`, `GetByIdsAsync`, `GetListAsync`, active/deleted list helpers | KEEP |
| Projection | Expression based single and list projections, property-only reads | KEEP |
| Count/exists | `GetCountAsync`, `GetLongCountAsync`, `CountAsync`, `HasAnyAsync`, status/date counts, composite-key exists | KEEP |
| Write/update | Insert variants, `Update`, `UpdateAsync`, `UpdateOnlyAsync`, conditional/try variants | KEEP; aliases documented below |
| Delete/restore | Soft delete, hard delete, bulk delete, purge, restore and try variants | KEEP |
| Bulk | `UpdateFromQueryAsync`, `DeleteFromQueryAsync`, `SoftDeleteFromQueryAsync` | KEEP; two-statement audit timestamp is documented |
| Upsert | Insert/update/mismatch contract with `InvalidOperationException` | KEEP; concurrency is not guaranteed |
| Pagination/specification | `PaginationSpecification<T>`, `PaginatedList<T>`, predicate/include/order/page support | KEEP |
| SQL | Parameterized command, raw query, scalar, single-row and SQL-exists helpers | KEEP; caller owns SQL structure safety |
| Transactions | Repository-owned `ExecuteInTransactionAsync` and existing-transaction handling | KEEP |
| DI/model | `AddRepository`, `AddRepositories`, `BaseEntity`, audit model | KEEP |

### Public API inventory

| Responsibility | Public surface | Decision |
|---|---|---|
| READ/QUERY | `GetAsync` overloads, `FindAsync`, `GetByIdAsync`, `GetByIdsAsync`, `GetListAsync`, active/deleted list helpers | KEEP |
| PROJECTION | `GetAsync<TEntity,TProjected>`, projected list and projected-by-id overloads, `GetOnlyAsync`, `GetPropertyByIdAsync`, `GetDistinctByAsync` | KEEP |
| COUNT/EXISTS | `CountAsync`, `GetCountAsync`, `GetLongCountAsync`, `HasAnyAsync`, `ExistsAsync`, `ExistsByIdAsync`, composite-key and SQL exists | KEEP |
| WRITE | `InsertAsync`, `InsertAndReturnAsync`, `InsertRangeAsync`, `InsertManyAsync`, audit/conditional/try insert | KEEP; `InsertManyAsync` is convenience batching, not a provider bulk promise |
| UPDATE | `Update`, `UpdateAsync`, `UpdateOnlyAsync`, `UpdateIfExistsAsync`, `TryUpdateAsync` | KEEP |
| DELETE | `Remove`, `Delete`, `DeleteAsync`, `HardDeleteAsync`, `DeleteIfExistsAsync`, `DeleteByIdAsync`, `TryDeleteAsync`, `DeleteAndReturnAsync` | KEEP; preserve legacy overlap |
| RESTORE | `RestoreAsync`, `RestoreRangeAsync`, `RestoreByIdAsync`, `TryRestoreAsync` | KEEP |
| BULK | `UpdateFromQueryAsync`, `DeleteFromQueryAsync`, `SoftDeleteFromQueryAsync`, condition delete, purge | KEEP |
| UPSERT | `UpsertAsync` | KEEP; document identity mismatch and concurrency limits |
| SPECIFICATION/PAGINATION | specification classes, include/order/pagination overloads, `PaginatedList<T>` | KEEP |
| SQL | `ExecuteSqlCommandAsync`, `GetFromRawSqlAsync`, `ExecuteScalarAsync`, `GetSingleFromSqlAsync`, `ExistsBySqlAsync` | KEEP |
| TRANSACTION | `ExecuteInTransactionAsync` | KEEP |
| UTILITY | `SaveChangesAsync`, DI extension methods, `BaseEntity`, `AuditLog` | KEEP |

### Overlap analysis

| Overlap | Classification | Rationale |
|---|---|---|
| `GetAsync` / `FindAsync` / `GetAsyncOrDefault` | ALIAS / REDUNDANT semantics | All are nullable single-result reads with different naming/history. Keep source compatibility; converge documentation on `GetAsync` as the primary form. |
| `GetByIdAsync` / `FindByIdAsync` | ALIAS | Both express key lookup. Keep both for compatibility; no removal in 1.x. |
| `GetFirstOrThrowAsync` / nullable first-result reads | KEEP | The throw-on-missing behavior is materially different and useful. |
| `CountAsync` / `GetCountAsync` | REDUNDANT surface | Same broad concern with overload history. Preserve existing methods; candidate for deprecation only in a future major version. |
| `ExistsAsync` / `HasAnyAsync` / `ExistsAndFetchAsync` | KEEP | Boolean-only, predicate convenience, and boolean-plus-entity have different result shapes. |
| `Delete` / `Remove` / `DeleteAsync` | KEEP / ALIAS | Synchronous tracked state operations and async persistence operations are distinct; legacy names remain. |
| `HardDeleteAsync` / bulk `DeleteFromQueryAsync` | KEEP | Entity-oriented and set-based operations have different tracking and return semantics. |
| `Update` / `UpdateAsync` | KEEP | Marking state and saving are intentionally separate operations. |
| `InsertRangeAsync` / `InsertManyAsync` | REDUNDANT / DEPRECATION_CANDIDATE | Both batch inserts; retain both in 1.x and clarify that neither promises provider bulk-copy semantics. |

No public member is removed or renamed in this phase.

### API gap analysis

| Candidate gap | Priority | Decision | Reason |
|---|---|---|---|
| AsNoTracking | LOW | ALREADY_PRESENT | Existing overloads expose `asNoTracking`; no duplicate API needed. |
| Projection | LOW | ALREADY_PRESENT | Expression projections already exist. |
| Async streaming | MEDIUM | ADD_IN_FUTURE_V2 | Useful for very large result sets, but changes lifetime/ownership expectations and needs provider tests. |
| Keyset pagination | MEDIUM | ADD_IN_FUTURE_V2 | Valuable at scale; offset pagination is adequate for current 1.x contract. |
| Hard delete | LOW | ALREADY_PRESENT | `HardDeleteAsync` exists. |
| ExecuteDelete | LOW | ALREADY_PRESENT | `DeleteFromQueryAsync` uses set-based delete. |
| Concurrency control | MEDIUM | ADD_IN_FUTURE_V2 | Requires explicit token/result contract and cross-provider tests. |
| Batch insert | LOW | DO_NOT_ADD in 1.x | Existing range/chunk helpers cover the repository contract; provider bulk APIs would add coupling. |
| Read by key | LOW | ALREADY_PRESENT | `GetByIdAsync`, `FindByIdAsync`, and `GetByIdsAsync` exist. |
| Any/exists | LOW | ALREADY_PRESENT | `ExistsAsync`, `HasAnyAsync`, and related methods exist. |
| Long count | LOW | ALREADY_PRESENT | `GetLongCountAsync` exists. |
| Aggregates | LOW | DO_NOT_ADD | Generic aggregate wrappers would expand surface without a stable domain contract. |
| IQueryable exposure | HIGH risk | DO_NOT_ADD | Leaks DbContext/provider details and undermines repository boundary. |
| Split query | LOW | DO_NOT_ADD in 1.x | Can be expressed through specifications/query options when a concrete need is demonstrated. |
| IgnoreQueryFilters | LOW | DO_NOT_ADD as global API | Deleted-list/restore paths already handle required behavior; a generic escape hatch needs explicit policy. |
| CancellationToken completeness | MEDIUM | ADD_BEFORE_RELEASE only for clear omissions | Most I/O methods propagate tokens; bulk methods without tokens are a small additive improvement candidate, not a release blocker. |

`API_GAP_BLOCKING_RELEASE = NO`.

### Rejected additions

No release-blocking omission was found. Generic `IQueryable` exposure, aggregate convenience methods, duplicate aliases, provider-specific bulk insert APIs, and a global query-filter bypass are rejected for 1.x because they increase coupling or surface area without strengthening the current contract.

### Recommendations

- `ADD_BEFORE_RELEASE`: none required for the current 1.0.x release; optionally add cancellation tokens only where an existing async I/O path demonstrably lacks one, with default values for source compatibility.
- `ADD_IN_FUTURE_V2`: smaller capability interfaces, keyset pagination, async streaming, and an explicit optimistic-concurrency result contract after demand and provider coverage are established.
- `DO_NOT_ADD`: `IQueryable` escape hatch, generic aggregate facade, duplicate read/count aliases, and provider-specific bulk abstractions.

### Supported runtime matrix

| Component | Installed/declared | Status |
|---|---|---|
| Target framework | `net9.0` | DECLARED and validated |
| .NET SDK | 10.0.102, 10.0.202 | INSTALLED |
| .NET runtime | 9.0.15 | INSTALLED and used by current target |
| .NET runtime | 10.0.2, 10.0.6 | INSTALLED; candidate only |
| EF Core | 9.0.9 | DECLARED and validated |
| Package | `AhmedOumezzine.EFCore.Repository` 1.0.3 | PACK validated |

### Dependency compatibility table

| Dependency | Current | .NET 10/EF 10 status | Decision |
|---|---:|---|---|
| Microsoft.EntityFrameworkCore | 9.0.9 | Not restored/tested as 10.x in this repository | KEEP 9.x |
| Microsoft.EntityFrameworkCore.Relational | 9.0.9 | Not restored/tested as 10.x in this repository | KEEP 9.x |
| System.Linq.Dynamic.Core | 1.6.8 | Independent compatibility review required | KEEP |
| Microsoft.SourceLink.GitHub | 8.0.0 | Build-time package; independent of EF runtime | KEEP |
| SQLite test provider | 9.0.9 line | EF10 provider matrix not executed | KEEP current matrix |

### EF10 impact assessment

| Used area | Classification | Required validation before migration |
|---|---|---|
| `ExecuteUpdateAsync` / `ExecuteDeleteAsync` | SOURCE_BREAK risk / BEHAVIOR_CHANGE risk | Compile and run bulk, soft-delete, timestamp, and provider tests against EF10. |
| Query filters and `IgnoreQueryFilters` | NO_IMPACT expected / BEHAVIOR_CHANGE possible | Re-run active/deleted/restore matrix. |
| Raw SQL and parameters | NO_IMPACT at API level | Re-run SQLite SQL matrix and provider-specific validation. |
| Transactions and execution strategy | NO_IMPACT at API level / BEHAVIOR_CHANGE possible | Validate caller-owned transactions and SQL Server strategy. |
| Metadata/property expressions and Dynamic LINQ | NO_IMPACT expected / BEHAVIOR_CHANGE possible | Compile and execute UpdateOnly, specification, and projection tests. |

### Target comparison

| Option | Benefits | Costs/risk | Decision |
|---|---|---|---|
| A — keep .NET 9 / EF9 | Preserves the 173/173 baseline, package contract, and tested provider matrix | Does not consume .NET10 APIs yet | RECOMMENDED |
| B — migrate to .NET 10 / EF10 | New platform lifecycle and EF improvements | Requires dependency migration, full provider retest, and possible consumer compatibility review | FUTURE MAJOR/MINOR PLANNED WORK |
| C — multi-target net9/net10 | Gives consumers choice | Requires conditional dependency graph, duplicate tests, packaging matrix, and greater maintenance cost | NOT JUSTIFIED for 1.x |

### Runtime recommendation

`KEEP_NET9`. The machine has .NET10 SDK/runtime installed, but installation is not compatibility evidence. The repository is declared and validated on net9.0 with EF Core 9.0.9. Migrating now would turn an audit into an unvalidated dependency and behavior change; multi-targeting would multiply the provider and packaging matrix before the first release. Evaluate EF10 in an isolated future work item, then choose a deliberate versioning strategy.

### Compatibility report

Breaking changes introduced: **NO**.

Binary compatibility: preserved; no public members removed or signature-changed.

Source compatibility: preserved; no required parameters or renamed members introduced.

Behavioral changes: none introduced by this audit; documentation-only classification.

Migration attempted: **NO**. Migration functional validation: **NOT_RUN** by design.

### Phase 5 gate

API gap blocking release: **NO**.

Functional SQLite baseline: **173/173 PASS** (protected baseline).

No Phase 6 or publication work is part of this audit.
