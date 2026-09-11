# Audit technique — AhmedOumezzine.EFCore.Repository

Date de l’audit : 2026-09-10  
Périmètre : solution, code source, tests, documentation, packaging et CI présents dans le dépôt.  
Méthode : lecture du code réel, compilation Release, exécution des tests, inspection du package NuGet généré et reproductions isolées hors dépôt.

## Executive Summary

Le dépôt est une bibliothèque EF Core 9 ciblant uniquement `net9.0`, organisée autour d’un `Repository<TDbContext>` partiel qui implémente une interface publique unique de 96 méthodes. Elle couvre les lectures, projections, specifications, pagination, insertions, mises à jour, suppressions logiques/physiques, bulk operations, transactions et SQL brut.

La base est exploitable comme prototype avancé, mais le package n’est pas prêt pour une consommation professionnelle sans corrections. Les risques les plus élevés concernent les opérations sur entités détachées, l’API SQL brute, l’upsert non atomique, l’enregistrement DI qui crée un deuxième contexte et la méthode bulk `UpdateFromQueryAsync`, actuellement cassée avec EF Core 9.

| Domaine | Note /10 | Constat |
|---|---:|---|
| Architecture | 4 | Un repository monolithique et une interface trop large ; séparation en fichiers partiels seulement. |
| API Design | 3 | 96 méthodes, overloads nombreux, types EF exposés et comportements de persistance incohérents. |
| EF Core correctness | 3 | Plusieurs bugs reproductibles et dépendance implicite à des conventions du consommateur. |
| Performance | 4 | Bulk APIs présentes, mais pagination sans ordre, listes matérialisées et tests de performance non fiables. |
| Security | 4 | Les overloads paramétrés sont utilisables, mais plusieurs APIs acceptent du SQL arbitraire et l’usage non paramétré est facile. |
| Reliability | 3 | Exceptions avalées par `Try*`, upsert en course, transactions incompatibles avec une execution strategy de retry. |
| Testing | 4 | 214 tests exécutés : 128 réussis, 86 échoués, aucun ignoré ; beaucoup de tests sont mal isolés ou contradictoires. |
| Documentation | 3 | README incomplet/inexact, wiki référencé mais non présent dans le dépôt, XML docs très incomplets. |
| NuGet packaging | 2 | Métadonnées essentielles absentes ; seul `net9.0` est packagé. |
| CI/CD | 1 | Aucun workflow GitHub Actions exploitable trouvé. |
| Maintainability | 3 | Interface et repository difficiles à faire évoluer ; nullabilité et analyzers produisent un bruit important. |

## Cartographie

### Structure et projets

- `EFCore.Repository.sln` contient une bibliothèque et un projet de tests ; les dossiers de solution `demo` et `src/tests` ne correspondent pas à des projets présents dans les fichiers suivis.
- `src/AhmedOumezzine.EFCore.Repository` contient 20 fichiers C#.
- `tests/AhmedOumezzine.EFCore.Repository.Tests` contient 23 fichiers C#.
- Aucun `Directory.Build.*`, `Directory.Packages.*`, `global.json`, `NuGet.config`, `Directory.Build.targets` ou fichier Dependabot/Renovate n’a été trouvé.
- `.github/workflows` existe comme dossier, mais aucun workflow n’est présent.

### Target frameworks et dépendances

La bibliothèque et les tests ciblent uniquement `net9.0`. La bibliothèque référence `Microsoft.EntityFrameworkCore` 9.0.9, `Microsoft.EntityFrameworkCore.Relational` 9.0.9 et `System.Linq.Dynamic.Core` 1.6.8. Les tests ajoutent SQLite, MSTest et AutoFixture en versions preview.

Le README annonce EF Core 6/7/8/9, mais le projet ne multi-cible pas ces versions et ses APIs `ExecuteUpdateAsync`/`ExecuteDeleteAsync` exigent EF Core 7+. Il n’existe donc pas de preuve de compatibilité EF 6–8. Au 2026-09-10, .NET 9 est en maintenance jusqu’au 10 novembre 2026, tandis que .NET 10 est LTS ; la cible unique `net9.0` réduit la durée de support du package ([cycle de support .NET](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core)).

### API publique réellement exposée

Types publics : `IRepository`, `Repository<TDbContext>`, `BaseEntity`, `AuditLog`, `SpecificationBase<T>`, `Specification<T>`, `PaginationSpecification<T>`, `PaginatedList<T>`, `QueryableExtensions` et `ServiceCollectionExtensions`.

`IRepository` mélange lecture, écriture, bulk, transactions et SQL brut. `Repository<TDbContext>` expose en plus des méthodes absentes de l’interface, notamment `DeleteFromQueryAsync`, `SoftDeleteFromQueryAsync`, `ExistsByCompositeKeyAsync` et plusieurs aliases/count methods. Les consommateurs injectés via DI voient uniquement l’interface non générique.

## Build et tests

Commandes exécutées :

```text
dotnet --info
dotnet restore EFCore.Repository.sln
dotnet build EFCore.Repository.sln -c Release --no-restore
dotnet test EFCore.Repository.sln -c Release --no-build --no-restore
dotnet test EFCore.Repository.sln -c Release --no-build --no-restore --filter "FullyQualifiedName!~Performance"
dotnet list EFCore.Repository.sln package --vulnerable --include-transitive --no-restore
```

Résultats :

- Restore : réussi après autorisation d’accès à `NuGet.Config`.
- Build Release : réussi, 0 erreur et 290 avertissements.
- Suite complète : 214 tests, 128 réussis, 86 échoués, 0 ignoré, 6 min 20 s.
- Rejeu fonctionnel hors performance : 170 tests, 91 réussis, 79 échoués.
- Le test `HasAnyAsync_WithCondition_WhenEntityDoesNotExist_ShouldReturnFalse` échoue avec `Guid.NewGuid()` non traduisible ; il est donc instable selon le provider/contexte de test.
- Les avertissements comprennent 149 `CS1591`, 44 `CS8618`, 28 `CS8625`, 17 `CS8602`, 3 `CS1710`, 3 `CS8603`, 1 `CS8631`, 2 `CS0618`, ainsi que `MSTEST0001` et 40 `MSTEST0037`.

Les tests utilisent SQLite en mémoire, ce qui est préférable à EF InMemory pour les requêtes relationnelles, mais ne couvre pas SQL Server/PostgreSQL. Plusieurs tests de performance sont des tests fonctionnels lourds (50 000 à 500 000 lignes), utilisent des attentes exactes incohérentes et ne doivent pas bloquer une CI standard.

## Bugs confirmés

### BUG-001 — Suppression détachée pouvant effacer les colonnes métier

Severity: Critical  
Fichier : `src/AhmedOumezzine.EFCore.Repository/Repository/DeleteRepository.cs`  
Symboles : `DeleteAsync`, `Remove`, `DeleteByIdAsync`, `Restore*`

Les méthodes marquent une instance détachée `EntityState.Modified`. EF envoie alors toutes les propriétés de l’instance, y compris les valeurs par défaut. Une reproduction avec une nouvelle instance portant seulement l’Id a remplacé `Name`, `Description` et les dates par leurs valeurs par défaut lors du soft delete.

Correction : charger l’entité suivie par Id, ou attacher l’instance en ne marquant explicitement que `IsDeleted`, `DeletedOnUtc` et `LastModifiedOnUtc` comme modifiées. Ajouter un test de suppression détachée qui vérifie la conservation de chaque colonne métier.

### BUG-002 — `UpdateOnlyAsync` persiste des propriétés non demandées

Severity: High  
Fichier : `Repository/UpdateRepository.cs`  
Symbole : `UpdateOnlyAsync`

Après `Attach`, la méthode modifie l’état de propriétés demandées mais `SetLastModifiedOnUtc` est appelé sans garantir que les autres valeurs reçues ne seront jamais écrites dans tous les chemins de tracking. Une reproduction avec `Name` demandé et `Description` non demandé a persisté `Description`.

Correction : attacher une instance minimale, marquer uniquement la clé, les propriétés autorisées et l’audit comme modifiées ; valider les noms de propriétés et rejeter les navigations/indexeurs.

### BUG-003 — `UpdateFromQueryAsync` casse sur EF Core 9

Severity: Critical  
Fichier : `Repository/UpdateRepository.cs:173-184`  
Symbole : `UpdateFromQueryAsync`

`GetMethod(nameof(SetPropertyCalls<TEntity>.SetProperty))` rencontre plusieurs overloads et lève `AmbiguousMatchException`. La construction avec `Expression.Invoke` est en outre fragile pour la traduction SQL. Les tests fonctionnels et de performance échouent sur ce chemin.

Correction : composer l’expression sans réflexion ambiguë, ou exposer directement deux opérations bulk explicites ; tester au minimum SQLite et SQL Server avec une vraie expression `SetProperty`.

### BUG-004 — Paramètres SQL incompatibles dans plusieurs overloads

Severity: High  
Fichier : `Repository/SqlRepository.cs`  
Symboles : `GetFromRawSqlAsync`, `GetSingleFromSqlAsync`, `ExecuteScalarAsync`, `ExecuteInTransactionAsync`

Le passage d’une `IEnumerable<object>` à `FromSqlRaw` est interprété comme une valeur unique dans certains overloads et produit une erreur de mapping `List<object>`/`object[]`. `ExecuteScalarAsync` crée des paramètres sans `ParameterName`, ce qui échoue avec SQLite. `ExecuteInTransactionAsync` transmet `null` à `ExecuteSqlRawAsync`, qui exige une collection non nulle.

Correction : normaliser tous les paramètres en `object[]`, créer des paramètres nommés (`@p0`, `@p1`, …) ou accepter `DbParameter` explicitement, et utiliser `Array.Empty<object>()` lorsqu’il n’y a aucun paramètre.

### BUG-005 — Upsert non atomique et incohérent avec le prédicat

Severity: High  
Fichier : `Repository/AddRepository.cs:172-201`  
Symbole : `UpsertAsync`

La méthode fait `AnyAsync(predicate)` puis `Update(entity)`. Si le prédicat trouve une ligne A alors que `entity.Id` désigne B, B est modifiée. Deux appels concurrents peuvent également insérer deux lignes. Le commentaire reconnaît le caractère non atomique mais l’API reste dangereusement générale.

Correction : imposer une clé/contrainte unique, utiliser une opération provider-specific ou documenter explicitement les garanties ; ne jamais utiliser un Id différent de la ligne chargée par le prédicat.

### BUG-006 — DI crée un second `DbContext`

Severity: High  
Fichier : `Extensions/ServiceCollectionExtensions.cs:27-34`

`AddGenericRepository` appelle `ActivatorUtilities.CreateInstance<TDbContext>` au lieu de réutiliser `GetRequiredService<TDbContext>`. La reproduction confirme `sameContext=False`. Cela casse le unit of work, peut ouvrir une seconde connexion et rend la disposition/lifetime ambigus. EF Core recommande généralement un contexte scoped par unité de travail ([lifetime DbContext](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/)).

Correction : injecter le contexte enregistré ; proposer séparément une factory si plusieurs unités de travail sont nécessaires.

### BUG-007 — Les insertions réinitialisent les valeurs soft-delete fournies

Severity: Medium  
Fichier : `Repository/GenericRepository.cs:35-43`  
Symbole : `PrepareEntityForInsert`

Toute insertion force `IsDeleted=false` et `DeletedOnUtc=null`. C’est cohérent pour une création normale, mais contradictoire avec les tests qui tentent d’insérer des données déjà supprimées et avec un éventuel import/restauration. La reproduction confirme que les valeurs fournies sont écrasées.

Correction : documenter la règle ou fournir une API d’import explicite ; ne pas réutiliser `InsertRangeAsync` pour restaurer des snapshots.

## Audit EF Core et sécurité

Les lectures standard filtrent `!IsDeleted`, mais aucun filtre global n’est ajouté par la bibliothèque : la garantie dépend de chaque méthode et les consommateurs peuvent appeler directement leur `DbSet`. `GetDeletedListAsync` ne fonctionne que si le modèle contient réellement des entités supprimées ; les tests avec `HasQueryFilter` masquent les lignes et n’utilisent pas `IgnoreQueryFilters`.

La pagination utilise `Skip/Take` sans imposer `OrderBy`. EF Core recommande un ordre totalement unique pour éviter doublons et éléments manquants, et la pagination offset devient coûteuse sur de grands offsets ([pagination EF Core](https://learn.microsoft.com/en-us/ef/core/querying/pagination)). `pageIndex * pageSize` peut déborder un `int`; aucun plafond de page n’est imposé. Une reproduction à grand index ne lève pas d’erreur mais ne fournit aucune garantie utile.

Les projections sont généralement appliquées avant matérialisation, ce qui est positif. Les méthodes de liste par défaut trackent les entités ; `AsNoTracking` existe mais n’est pas la valeur par défaut. Les `Includes` sont délégués au consommateur sans split query ni contrôle de cardinalité.

Les bulk operations sont immédiates, ne synchronisent pas le change tracker et ne démarrent pas implicitement une transaction. Elles ne fournissent pas de contrôle de concurrence automatique ([ExecuteUpdate/ExecuteDelete](https://learn.microsoft.com/en-us/ef/core/saving/execute-insert-update-delete)). `InsertWithTransactionAsync` et `ExecuteInTransactionAsync` commencent une transaction manuelle sans passer par `CreateExecutionStrategy`; avec SQL Server et `EnableRetryOnFailure`, ce scénario peut échouer, comme le documente EF Core ([transactions et execution strategy](https://learn.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency)).

Surface SQL :

| API | Classement | Motif |
|---|---|---|
| `FromSqlRaw(sql)` sans paramètre | RISKY | SQL arbitraire contrôlé par le consommateur ; injection possible si une entrée est concaténée. |
| `FromSqlRaw(sql, parameters)` | SAFE WITH CONDITIONS | Sûr si le texte SQL reste constant et que les valeurs passent par des paramètres nommés. |
| `ExecuteSqlRawAsync` | SAFE WITH CONDITIONS | Même règle ; l’API permet aussi d’exécuter DELETE/DDL arbitraires. |
| `ExecuteScalarAsync` | RISKY | Paramétrage manuel actuellement incorrect et absence de validation du type de requête. |
| `ExecuteInTransactionAsync` | RISKY | SQL arbitraire, paramètres null cassants et retry/transaction non traité. |

L’audit n’a pas trouvé de concaténation interne de paramètres utilisateurs, mais l’API rend le mauvais usage très facile. Les tests eux-mêmes utilisent des interpolations SQL non paramétrées.

## Specifications, concurrence et robustesse

`SpecificationBase<T>` est mutable, stocke des delegates `Func<IQueryable<T>, ...>` et expose directement des listes publiques. Elle n’offre ni composition immuable, ni `ThenBy`, ni projection native, ni validation centralisée. `OrderByDynamic` construit une chaîne passée à Dynamic LINQ : le nom de colonne et la direction doivent être whitelistés par le consommateur.

`TryGet*`, `TryUpdate*`, `TryDelete*`, `TryRestore*` avalent toutes les exceptions, y compris annulation, erreurs de connexion et erreurs de programmation. Cela détruit le diagnostic et rend impossible la distinction « absent », « annulé » et « panne ».

Le contexte est stocké dans un champ et le repository n’est pas thread-safe ; il ne doit pas être utilisé concurremment. Aucun token de concurrence (`rowversion`/concurrency token) n’est défini. Les bulk operations ne peuvent donc pas garantir une mise à jour optimiste.

## Tests : matrice et lacunes

| Fonctionnalité | Tests existants | Qualité | Cas manquants | Priorité |
|---|---|---|---|---|
| CRUD de base | Oui | Moyenne | Entités détachées, contexte partagé | P1 |
| Specifications | Oui | Moyenne | Composition, ordre unique, projection complexe | P1 |
| Pagination | Oui | Faible | Sans ordre, overflow, page max, keyset | P1 |
| Soft delete | Oui | Faible | Entité détachée, navigation/cascade, modèle avec filtre global | P0 |
| Hard delete/bulk | Oui | Moyenne | Contraintes FK et concurrence | P1 |
| Transactions | Oui | Faible | SQL Server retry, transaction imbriquée, annulation | P1 |
| Raw SQL | Oui | Faible | Paramètres nommés, null, provider SQL Server | P0 |
| Concurrence | Non | Absente | RowVersion et conflits | P1 |
| DI | Non | Absente | Réutilisation du contexte et lifetime | P0 |
| CancellationToken | Partiel | Faible | Annulation pendant chaque opération et propagation | P2 |
| Packaging/API | Non | Absente | `dotnet pack`, API compatibility, README inclus | P1 |

Les tests de performance contiennent des erreurs de données attendues : `DeleteFromQueryAsync` attend 5 000 lignes mais en supprime 50 000 ; certains tests comptent des entités que `PrepareEntityForInsert` réactive ; le test de liste supprimée attend 1 000 alors que les insertions les réactivent. Ils devraient être déplacés vers BenchmarkDotNet ou un job séparé.

## Packaging NuGet

Le package généré est `AhmedOumezzine.EFCore.Repository` version `1.0.3`, avec DLL/XML `lib/net9.0`. Le nuspec contient `Description=Package Description`, sans `RepositoryUrl`, `PackageReadmeFile`, `PackageLicenseExpression`, tags, symbol package ou validation d’API. SourceLink est produit dans les artefacts de build, mais n’est pas configuré comme metadata de publication complète. `GeneratePackageOnBuild=true` rend les builds locaux plus surprenants et ne remplace pas une étape de pack Release explicite.

Recommandations : metadata complète, README inclus dans le nupkg, licence SPDX `MIT`, `RepositoryUrl`, `PackageReleaseNotes`, `.snupkg`, `EnablePackageValidation`, validation API et ranges de dépendances compatibles avec la matrice de frameworks réellement supportée.

## CI/CD et documentation

Aucune pipeline n’est présente. Ajouter une matrice `net8.0/net9.0` seulement si les références EF sont adaptées, sinon documenter clairement `net9.0`; exécuter restore, build Release, tests fonctionnels, couverture, analyseurs, `dotnet pack`, validation de package et scan de vulnérabilités. Les tests de performance doivent être séparés.

Le README annonce EF 6/7/8/9, « 100 % async », bulk operations et transactions sans expliquer les garanties réelles. Son exemple injecte `IRepository<AppDbContext>`, type inexistant : l’interface n’est pas générique. L’URL Wiki est référencée mais aucune documentation Wiki n’est versionnée dans le dépôt. Ajouter des exemples paramétrés et documenter explicitement soft delete, hard delete, tracking, transactions, concurrence et ownership du `DbContext`.

## Backlog priorisé

| ID | Priorité | Catégorie | Problème | Fichiers | Correction | Breaking change | Effort |
|---|---|---|---|---|---|---|---|
| BL-001 | P0 | Data integrity | Suppression détachée écrase les colonnes | `DeleteRepository.cs` | Charger suivi ou modifier seulement les propriétés de soft delete | Potentiellement | M |
| BL-002 | P0 | EF correctness | Bulk update lève `AmbiguousMatchException` | `UpdateRepository.cs` | Refaire la composition EF Core 9 sans réflexion ambiguë | Non-breaking | M |
| BL-003 | P0 | DI | Le repository crée un second contexte | `ServiceCollectionExtensions.cs` | Résoudre `TDbContext` depuis le scope | Potentiellement | S |
| BL-004 | P0 | SQL | Paramètres object/DbParameter mal transmis | `SqlRepository.cs` | Normaliser paramètres et nommer `DbParameter` | Potentiellement | M |
| BL-005 | P1 | API | Interface monolithique de 96 méthodes | `IRepository.cs` | Interfaces lecture/écriture/sql séparées ; façade de compatibilité | Breaking à terme | L |
| BL-006 | P1 | Data integrity | Upsert prédicat/Id non cohérent et non atomique | `AddRepository.cs` | Contrat par clé unique et stratégie atomique/provider | Potentiellement | L |
| BL-007 | P1 | Pagination | Pas d’ordre obligatoire ni plafond | `QueryableExtensions.cs`, specifications | Exiger ordre ou documenter ; `checked`, limites, keyset | Potentiellement | M |
| BL-008 | P1 | Reliability | Transactions incompatibles avec retry | `AddRepository.cs`, `SqlRepository.cs` | `CreateExecutionStrategy().ExecuteAsync` | Non-breaking | M |
| BL-009 | P1 | Packaging | Métadonnées NuGet absentes | `.csproj` | Metadata, README, licence, symbols, validation | Non-breaking | S |
| BL-010 | P1 | Tests | 86 échecs et tests partagés non isolés | `tests/**` | Fixtures indépendantes, tests provider réalistes | Non-breaking | L |
| BL-011 | P2 | API | Exceptions avalées par `Try*` | repository partiels | Capturer seulement erreurs prévues ou retourner un résultat typé | Potentiellement | M |
| BL-012 | P2 | Security | SQL brut et Dynamic LINQ trop permissifs | `SqlRepository.cs`, `SpecificationEvaluator.cs` | API paramétrées, whitelist et documentation de sécurité | Potentiellement | M |
| BL-013 | P2 | Concurrency | Aucun concurrency token | modèle/documentation | Supporter token fourni par l’application et vérifier rows affected | Non-breaking | M |
| BL-014 | P3 | Maintainability | Nullability/XML docs/analyzers bruyants | `src/**` | Corriger annotations, docs et warnings réels | Non-breaking | M |

## Top 10 des changements à meilleur ratio

1. Réutiliser le `DbContext` scoped existant dans DI.
2. Corriger les paramètres SQL et le cas `null`.
3. Réécrire `UpdateFromQueryAsync` pour EF Core 9.
4. Sécuriser les suppressions d’entités détachées.
5. Corriger `UpdateOnlyAsync` et ajouter son test de conservation.
6. Ajouter une contrainte de page et un ordre déterministe.
7. Rendre l’upsert cohérent avec la clé effectivement modifiée.
8. Séparer tests fonctionnels et benchmarks.
9. Corriger README et métadonnées NuGet.
10. Ajouter une CI Release avec tests, pack, validation et scan.

## Roadmap

### Release patch

Corriger DI, paramètres SQL, suppression détachée, `UpdateOnlyAsync`, exceptions de paramètres null, README de démarrage et métadonnées NuGet sans retirer les APIs existantes.

### Release minor

Ajouter interfaces spécialisées, pagination validée avec tri déterministe, tests SQL Server/SQLite, stratégie d’exécution transactionnelle, APIs bulk stables et documentation complète.

### Release major

Remplacer l’interface monolithique par des contrats spécialisés, réduire l’exposition de `IQueryable`/EF-specific types, revoir les signatures SQL et supprimer les overloads ambigus. Fournir des adapters et un guide de migration depuis `IRepository`.

## Conclusion opérationnelle

L’audit est terminé. `AUDIT.md` est le seul fichier ajouté ; aucune source, configuration ou test existant n’a été modifié. Les corrections doivent attendre une instruction explicite, conformément au périmètre demandé.
