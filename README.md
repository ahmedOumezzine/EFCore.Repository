# 📦 AhmedOumezzine.EFCore.Repository

> **Un repository générique léger, moderne et extensible pour Entity Framework Core (EF Core 9)**

Éliminez la répétition du code CRUD et accélérez votre développement avec une couche repository **prête à l’emploi**, **asynchrone**, et **optimisée pour le soft delete, la pagination, les projections et les specifications.**

---

## 🌟 Fonctionnalités

Le pattern Repository divise, mais **bien conçu, il devient un levier de productivité.**

Avec `AhmedOumezzine.EFCore.Repository`, vous obtenez :

- ✅ **Productivité** : Réduction drastique de la duplication de code CRUD.
- ✅ **Performance** : Support natif des **Bulk Operations** (EF Core 7+) pour des mises à jour et suppressions ultra-rapides.
- ✅ **Design** : Pagination native (`PaginatedList<T>`) et Projections (`Select` vers DTO) pour des API efficaces.
- ✅ **Testabilité** : Parfaitement compatible avec SQLite en mémoire et les frameworks de *mocking*.
- ✅ **Confort** : Gestion native du **soft delete** (`IsDeleted`, `DeletedOnUtc`) et des transactions.
- ✅ **Flexibilité** : `Specifications` réutilisables et support des requêtes SQL brutes sécurisées.

> 💡 **Intégration Modulaire** : Incorporez seulement les méthodes dont vous avez besoin. Combinez ce dépôt générique avec vos propres services métier spécifiques (ex: `IProductService`) sans conflit.

---

## 🚀 Démarrage rapide

### 1. Installez le package

```bash
dotnet add package AhmedOumezzine.EFCore.Repository
```
2. Enregistrez le repository via l'extension DI (Program.cs)Utilisez la méthode d'extension pour simplifier l'enregistrement de votre DbContext et du dépôt générique.

```C#
// 1. Configurez le DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
 
// 2. 🔹 Enregistrement du Generic Repository
 
builder.Services.AddGenericRepository<AppDbContext>();
```
3. Injectez IRepository dans vos services ou contrôleursLe dépôt générique est injecté dans le conteneur de services et prêt à l'emploi.

```C#
public class ProductsController : ControllerBase
{
    // Injecte IRepository<TDbContext>
    private readonly IRepository<AppDbContext> _repository; 

    public ProductsController(IRepository<AppDbContext> repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        // Récupération de la liste de l'entité Product
        var products = await _repository.GetListAsync<Product>(); 
        return Ok(products);
    }
}
```
## 📚 Documentation Complète
Pour des exemples détaillés sur les Bulk Operations, le Soft Delete et la Pagination, consultez le Wiki.

📘 Consulter le [Wiki de AhmedOumezzine.EFCore.Repository](https://github.com/ahmedOumezzine/EFCore.Repository/wiki)

## 🤝 Contribuer & ContactVotre feedback est essentiel. 

Toute contribution (PR, rapport de bug, suggestion) est la bienvenue.

⭐ Soutenez: Donnez une étoile ⭐ sur GitHub si ce projet vous est utile !

🐞 Issues : Signaler un bug ou demander une fonctionnalité

📧 Contact :  ahmedoumezzine@outlook.fr

## 📄 Licence
Ce projet est publié sous licence MIT — libre d’utilisation, modification et distribution, même à des fins commerciales.

## Compatibilité

Cette version cible .NET 9 et EF Core 9. SQLite en mémoire est utilisé pour les tests fonctionnels. La validation SQL Server et l’atomicité concurrente de l’upsert restent des limitations documentées.
