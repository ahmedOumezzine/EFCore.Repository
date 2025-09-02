# 📦 AhmedOumezzine.EFCore.Repository

> **Un repository générique léger, moderne et extensible pour Entity Framework Core (EF Core 6/7/8/9)**

Éliminez la répétition du code CRUD et accélérez votre développement avec une couche repository **prête à l’emploi**, **100 % async**, et **compatible avec le soft delete, la pagination, les projections et les specifications**.

🔧 Parfait pour les projets ASP.NET Core, Minimal APIs, et applications console.  
📦 Facile à intégrer, simple à étendre.

---

## 🌟 Pourquoi utiliser ce Repository ?

Le pattern Repository divise, mais **bien conçu, il devient un levier de productivité**.

Avec `AhmedOumezzine.EFCore.Repository`, vous obtenez :

- ✅ Réduction drastique de la duplication de code
- ✅ Support du **soft delete** (`IsDeleted`, `DeletedOnUtc`)
- ✅ Pagination native avec `PaginatedList<T>`
- ✅ Projections (`Select` vers DTO)
- ✅ Specifications réutilisables
- ✅ Requêtes SQL brutes sécurisées
- ✅ Bulk operations (EF Core 7+)
- ✅ 100 % **async/await**
- ✅ Testable (avec SQLite en mémoire)
- ✅ Léger, modulaire, facile à étendre

> 💡 **Vous n’êtes pas obligé de tout utiliser** :  
> Intégrez seulement les parties dont vous avez besoin.  
> Combinez avec vos propres repositories métier (`IProductService`, etc.).

---

## 🚀 Démarrage rapide

### 1. Installez le package

```bash
dotnet add package AhmedOumezzine.EFCore.Repository 
```
### 2. Enregistrez le repository dans Program.cs (ou Startup.cs)
```csharp 
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
 
// 🔹 Ajout du Generic Repository
builder.Services.AddGenericRepository<AppDbContext>(); 
```
### 3. Injectez IRepository dans vos services ou contrôleurs
```csharp 
public class ProductsController : ControllerBase
{
    private readonly IRepository _repository;

    public ProductsController(IRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var products = await _repository.GetListAsync<Product>();
        return Ok(products);
    }
}
```
## 📚 Documentation
📘 Consultez le Wiki complet pour :
- Méthodes CRUD (Add, Update, Delete)
- Requêtes (Get, GetList, Pagination)
- SQL brut
- Configuration DI
- Bonnes pratiques

## ⭐ Donnez une étoile ! ✨
Si vous trouvez cette bibliothèque utile, donnez une étoile ⭐ pour m’encourager à continuer à créer des projets open source !
Merci pour votre soutien ❤️

## 📄 Licence
Ce projet est sous licence MIT — libre d’utilisation, modification et distribution, même à des fins commerciales.
[Consultez la licence.](https://github.com/ahmedOumezzine/EFCore.Repository/blob/main/LICENSE.txt "Consultez la licence.")

## 📬 Contact & Contributions
📧 Email : ahmedoumezzine@outlook.fr

🐞 Issues : [Signaler un bug ou demander une fonctionnalité](https://github.com/ahmedOumezzine/EFCore.Repository/issues "Signaler un bug ou demander une fonctionnalité")

🤝 Contributions : Les PR sont les bienvenues ! (Respectez les conventions de code)
